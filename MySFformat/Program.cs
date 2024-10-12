using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MySFformat
{
    public class VertexInfo
    {
        public int meshIndex = 0;
        public uint vertexIndex = 0;
    }

    static partial class Program
    {
        public static FLVER targetFlver;
        public static TPF targetTPF = null;
        public static string flverName;
        public static List<DataGridViewTextBoxCell> boneNameList;
        public static List<TextBox> parentList;
        public static List<TextBox> childList;
        public static List<VertexInfo> verticesInfo = new List<VertexInfo>();

        public static Vector3D[] bonePosList = new Vector3D[1000];

        public static Dictionary<string, string> boneParentList;
        public static List<FLVER.Vertex> vertices = new List<FLVER.Vertex>();
        public static Mono3D mono;

        public static Vector3 checkingPoint;
        public static Vector3 checkingPointNormal;
        public static bool useCheckingPoint = false;

        public static int checkingMeshNum = 0;
        public static bool useCheckingMesh = false;

        /***settings***/
        public static bool loadTexture = true;
        public static bool show3D = false;
        public static int boneFindParentTimes = 15; //if cannot find bone, find if its parent bone matches flver bone name

        public static bool boneDisplay = true;
        public static bool dummyDisplay = true;

        public static bool setVertexPos = false;
        public static float setVertexX = 0;
        public static float setVertexY = 1.75f;
        public static float setVertexZ = 0;

        public static RotationOrder rotOrder = RotationOrder.YZX;

        public static string version = "2.0";

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //加载配置
            string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            IniParser settingFile = new IniParser(assemblyPath + "\\MySFformat.ini");
            show3D = settingFile.GetSetting("FLVER", "show3D").Trim() != "0";

            //读取文件
            if (args.Length > 0)
                flverName = args[0];
            else
            {
                OpenFileDialog openFileDialog1;
                openFileDialog1 = new OpenFileDialog
                {
                    Title = "Choose fromsoftware .flver model file.",
                    Filter = "Flver File|*.flver"
                };
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    flverName = openFileDialog1.FileName;
                else
                    return;
            }
            targetFlver = FLVER.Read(flverName);

            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;
                mono = new Mono3D();
                if (show3D)
                {//启动3D视图
                    UpdateVertices();
                    mono.Run();
                }
            }).Start();

            //显示窗口
            ShowMainForm();
        }

        public static void UpdateVertices()
        {
            List<VertexPositionColor> ans = new List<VertexPositionColor>();
            List<VertexPositionColor> triangles = new List<VertexPositionColor>();
            List<VertexPositionColorTexture> textureTriangles =
                new List<VertexPositionColorTexture>();
            vertices.Clear();
            verticesInfo.Clear();
            List<MeshInfos> mis = new List<MeshInfos>();

            for (int i = 0; i < targetFlver.Meshes.Count; i++)
            {
                if (targetFlver.Meshes[i] == null)
                    continue;

                bool renderBackFace = false;
                if (targetFlver.Meshes[i].FaceSets.Count > 0)
                    if (targetFlver.Meshes[i].FaceSets[0].CullBackfaces == false)
                        renderBackFace = true;
                foreach (FLVER.Vertex[] vl in targetFlver.Meshes[i].GetFaces())
                {
                    Microsoft.Xna.Framework.Color cline = Microsoft.Xna.Framework.Color.Black;
                    if (useCheckingMesh && checkingMeshNum == i)
                    {
                        cline.G = 255;
                        cline.R = 255;
                    }
                    cline.A = 125;
                    ans.Add(new VertexPositionColor(toXnaV3XZY(vl[0].Positions[0]), cline));
                    ans.Add(new VertexPositionColor(toXnaV3XZY(vl[1].Positions[0]), cline));
                    ans.Add(new VertexPositionColor(toXnaV3XZY(vl[0].Positions[0]), cline));
                    ans.Add(new VertexPositionColor(toXnaV3XZY(vl[2].Positions[0]), cline));
                    ans.Add(new VertexPositionColor(toXnaV3XZY(vl[1].Positions[0]), cline));
                    ans.Add(new VertexPositionColor(toXnaV3XZY(vl[2].Positions[0]), cline));

                    Microsoft.Xna.Framework.Color c = new Microsoft.Xna.Framework.Color();

                    Microsoft.Xna.Framework.Vector3 va =
                        toXnaV3(vl[1].Positions[0]) - toXnaV3(vl[0].Positions[0]);
                    Microsoft.Xna.Framework.Vector3 vb =
                        toXnaV3(vl[2].Positions[0]) - toXnaV3(vl[0].Positions[0]);
                    Microsoft.Xna.Framework.Vector3 vnromal = crossPorduct(va, vb);
                    vnromal.Normalize();
                    Microsoft.Xna.Framework.Vector3 light = new Microsoft.Xna.Framework.Vector3(
                        mono.lightX,
                        mono.lightY,
                        mono.lightZ
                    );
                    light.Normalize();
                    float theta = dotProduct(vnromal, light);
                    int value = 125 + (int)(125 * theta);
                    if (value > 255)
                        value = 255;
                    if (value < 0)
                        value = 0;
                    if (mono.flatShading)
                        value = 255;
                    c.R = (byte)value;
                    c.G = (byte)value;
                    c.B = (byte)value;
                    c.A = 255;
                    if (useCheckingMesh && checkingMeshNum == i)
                        c.B = 0;
                    triangles.Add(new VertexPositionColor(toXnaV3XZY(vl[0].Positions[0]), c));
                    triangles.Add(new VertexPositionColor(toXnaV3XZY(vl[2].Positions[0]), c));
                    triangles.Add(new VertexPositionColor(toXnaV3XZY(vl[1].Positions[0]), c));

                    if (loadTexture)
                    {
                        textureTriangles.Add(
                            new VertexPositionColorTexture(
                                toXnaV3XZY(vl[0].Positions[0]),
                                c,
                                new Microsoft.Xna.Framework.Vector2(vl[0].UVs[0].X, vl[0].UVs[0].Y)
                            )
                        );
                        textureTriangles.Add(
                            new VertexPositionColorTexture(
                                toXnaV3XZY(vl[2].Positions[0]),
                                c,
                                new Microsoft.Xna.Framework.Vector2(vl[2].UVs[0].X, vl[2].UVs[0].Y)
                            )
                        );
                        textureTriangles.Add(
                            new VertexPositionColorTexture(
                                toXnaV3XZY(vl[1].Positions[0]),
                                c,
                                new Microsoft.Xna.Framework.Vector2(vl[1].UVs[0].X, vl[1].UVs[0].Y)
                            )
                        );
                    }

                    if (renderBackFace)
                    {
                        triangles.Add(new VertexPositionColor(toXnaV3XZY(vl[0].Positions[0]), c));
                        triangles.Add(new VertexPositionColor(toXnaV3XZY(vl[1].Positions[0]), c));
                        triangles.Add(new VertexPositionColor(toXnaV3XZY(vl[2].Positions[0]), c));

                        if (loadTexture)
                        {
                            textureTriangles.Add(
                                new VertexPositionColorTexture(
                                    toXnaV3XZY(vl[0].Positions[0]),
                                    c,
                                    new Microsoft.Xna.Framework.Vector2(
                                        vl[0].UVs[0].X,
                                        vl[0].UVs[0].Y
                                    )
                                )
                            );
                            textureTriangles.Add(
                                new VertexPositionColorTexture(
                                    toXnaV3XZY(vl[1].Positions[0]),
                                    c,
                                    new Microsoft.Xna.Framework.Vector2(
                                        vl[1].UVs[0].X,
                                        vl[1].UVs[0].Y
                                    )
                                )
                            );
                            textureTriangles.Add(
                                new VertexPositionColorTexture(
                                    toXnaV3XZY(vl[2].Positions[0]),
                                    c,
                                    new Microsoft.Xna.Framework.Vector2(
                                        vl[2].UVs[0].X,
                                        vl[2].UVs[0].Y
                                    )
                                )
                            );
                        }
                    }
                }

                for (uint j = 0; j < targetFlver.Meshes[i].Vertices.Count; j++)
                {
                    FLVER.Vertex v = targetFlver.Meshes[i].Vertices[(int)j];
                    vertices.Add(v);
                    VertexInfo vi = new VertexInfo
                    {
                        meshIndex = i,
                        vertexIndex = j
                    };
                    verticesInfo.Add(vi);
                }

                MeshInfos mi = new MeshInfos();
                var tName = targetFlver
                    .Materials[targetFlver.Meshes[i].MaterialIndex]
                    .Textures[0]
                    .Path;
                tName = FindFileName(tName);
                mi.textureName = tName;
                //MessageBox.Show("Found texture name:" + mi.textureName);
                mi.triTextureVertices = textureTriangles.ToArray();
                textureTriangles.Clear();
                mis.Add(mi);
            }
            if (ans.Count % 2 != 0)
                ans.Add(ans[ans.Count - 1]);

            for (int i = 0; i < bonePosList.Length; i++)
                bonePosList[i] = null;

            //Calcaulte bone global space

            //bone space X,Y,Z axis
            if (boneDisplay)
            {
                Transform3D[] boneTrans = new Transform3D[targetFlver.Bones.Count];
                for (int i = 0; i < targetFlver.Bones.Count; i++)
                {
                    boneTrans[i] = new Transform3D
                    {
                        rotOrder = rotOrder,
                        position = new Vector3D(targetFlver.Bones[i].Translation)
                    };
                    boneTrans[i].setRotationInRad(new Vector3D(targetFlver.Bones[i].Rotation));
                    boneTrans[i].scale = new Vector3D(targetFlver.Bones[i].Scale);

                    if (targetFlver.Bones[i].ParentIndex >= 0)
                    {
                        boneTrans[i].parent = boneTrans[targetFlver.Bones[i].ParentIndex];

                        Vector3D actPos = boneTrans[i].getGlobalOrigin();

                        if (boneTrans[targetFlver.Bones[i].ParentIndex] != null)
                        {
                            Vector3D parentPos = boneTrans[targetFlver.Bones[i].ParentIndex]
                                .getGlobalOrigin();

                            ans.Add(
                                new VertexPositionColor(
                                    new Microsoft.Xna.Framework.Vector3(
                                        parentPos.X - 0.005f,
                                        parentPos.Z - 0.005f,
                                        parentPos.Y
                                    ),
                                    Microsoft.Xna.Framework.Color.Purple
                                )
                            );
                            ans.Add(
                                new VertexPositionColor(
                                    new Microsoft.Xna.Framework.Vector3(
                                        actPos.X,
                                        actPos.Z,
                                        actPos.Y
                                    ),
                                    Microsoft.Xna.Framework.Color.Purple
                                )
                            );

                            ans.Add(
                                new VertexPositionColor(
                                    new Microsoft.Xna.Framework.Vector3(
                                        parentPos.X + 0.005f,
                                        parentPos.Z + 0.005f,
                                        parentPos.Y
                                    ),
                                    Microsoft.Xna.Framework.Color.Purple
                                )
                            );
                            ans.Add(
                                new VertexPositionColor(
                                    new Microsoft.Xna.Framework.Vector3(
                                        actPos.X,
                                        actPos.Z,
                                        actPos.Y
                                    ),
                                    Microsoft.Xna.Framework.Color.Purple
                                )
                            );
                        }
                    }
                }
            }

            for (int i = 0; i < targetFlver.Dummies.Count && dummyDisplay; i++)
            {
                FLVER.Dummy d = targetFlver.Dummies[i];

                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            d.Position.X - 0.025f,
                            d.Position.Z,
                            d.Position.Y
                        ),
                        Microsoft.Xna.Framework.Color.Purple
                    )
                );
                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            d.Position.X + 0.025f,
                            d.Position.Z,
                            d.Position.Y
                        ),
                        Microsoft.Xna.Framework.Color.Purple
                    )
                );

                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            d.Position.X,
                            d.Position.Z - 0.025f,
                            d.Position.Y
                        ),
                        Microsoft.Xna.Framework.Color.Purple
                    )
                );
                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            d.Position.X,
                            d.Position.Z + 0.025f,
                            d.Position.Y
                        ),
                        Microsoft.Xna.Framework.Color.Purple
                    )
                );

                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            d.Position.X,
                            d.Position.Z,
                            d.Position.Y
                        ),
                        Microsoft.Xna.Framework.Color.Green
                    )
                );
                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            d.Position.X + d.Forward.X,
                            d.Position.Z + d.Forward.Z,
                            d.Position.Y + d.Forward.Y
                        ),
                        Microsoft.Xna.Framework.Color.Green
                    )
                );
            }

            if (useCheckingPoint)
            {
                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            checkingPoint.X - 0.05f,
                            checkingPoint.Z - 0.05f,
                            checkingPoint.Y
                        ),
                        Microsoft.Xna.Framework.Color.AntiqueWhite
                    )
                );
                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            checkingPoint.X + 0.05f,
                            checkingPoint.Z + 0.05f,
                            checkingPoint.Y
                        ),
                        Microsoft.Xna.Framework.Color.AntiqueWhite
                    )
                );

                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            checkingPoint.X - 0.05f,
                            checkingPoint.Z + 0.05f,
                            checkingPoint.Y
                        ),
                        Microsoft.Xna.Framework.Color.AntiqueWhite
                    )
                );
                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            checkingPoint.X + 0.05f,
                            checkingPoint.Z - 0.05f,
                            checkingPoint.Y
                        ),
                        Microsoft.Xna.Framework.Color.AntiqueWhite
                    )
                );

                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            checkingPoint.X,
                            checkingPoint.Z,
                            checkingPoint.Y
                        ),
                        Microsoft.Xna.Framework.Color.Blue
                    )
                );
                ans.Add(
                    new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(
                            checkingPoint.X + 0.2f * checkingPointNormal.X,
                            checkingPoint.Z + 0.2f * checkingPointNormal.Z,
                            checkingPoint.Y + 0.2f * checkingPointNormal.Y
                        ),
                        Microsoft.Xna.Framework.Color.Blue
                    )
                );

                useCheckingPoint = false;
            }
            useCheckingMesh = false;
            mono.vertices = ans.ToArray();
            // mono.triTextureVertices = textureTriangles.ToArray();
            mono.meshInfos = mis.ToArray();
            mono.triVertices = triangles.ToArray();
        }

        static void AutoBackUp()
        {
            if (!File.Exists(flverName + ".bak"))
                File.Copy(flverName, flverName + ".bak", false);
        }

        static void Dummies()
        {
            Form f = new Form
            {
                Text = "Dummies"
            };
            Panel p = new Panel();
            int currentY2 = 10;
            p.AutoScroll = true;
            string assemblyPath = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location
            );
            string dummyStr = File.ReadAllText(assemblyPath + "\\dummyInfo.dll");
            List<FLVER.Dummy> refDummy = new JavaScriptSerializer().Deserialize<List<FLVER.Dummy>>(
                dummyStr
            );


            f.Controls.Add(p);
            p.Controls.Add(new Label
            {
                Text = "Choose # to translate:",
                Size = new System.Drawing.Size(150, 15),
                Location = new System.Drawing.Point(10, currentY2 + 5)
            });
            currentY2 += 20;

            TextBox t = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(10, currentY2 + 5),
                Text = "-1"
            };
            p.Controls.Add(t);

            TextBox tref = new TextBox
            {
                Size = new System.Drawing.Size(100, 15),
                Location = new System.Drawing.Point(150, currentY2 + 5),
                Text = "",
                ReadOnly = true
            };
            p.Controls.Add(tref);

            Button buttonCheck = new Button();
            ButtonTips("按照你输入的序列数找到对应的辅助点，辅助点会以白色的X显示。",
                buttonCheck
            );
            buttonCheck.Text = "Check";
            buttonCheck.Location = new System.Drawing.Point(70, currentY2 + 5);
            buttonCheck.Click += (s, e) =>
            {
                int i = int.Parse(t.Text);
                if (i >= 0 && i < targetFlver.Dummies.Count)
                {
                    useCheckingPoint = true;
                    checkingPoint = new Vector3(
                        targetFlver.Dummies[i].Position.X,
                        targetFlver.Dummies[i].Position.Y,
                        targetFlver.Dummies[i].Position.Z
                    );
                    checkingPointNormal = new Vector3(
                        targetFlver.Dummies[i].Forward.X * 0.2f,
                        targetFlver.Dummies[i].Forward.Y * 0.2f,
                        targetFlver.Dummies[i].Forward.Z * 0.2f
                    );

                    tref.Text = "RefID:" + targetFlver.Dummies[i].ReferenceID;
                    UpdateVertices();
                }
                else
                    MessageBox.Show("Invalid modification value!");
            };
            p.Controls.Add(buttonCheck);

            currentY2 += 25;

            Label ltip = new Label
            {
                Location = new System.Drawing.Point(10, currentY2 + 5),
                Size = new System.Drawing.Size(200, 15),
                Text = "Translate value (x,y,z):"
            };
            p.Controls.Add(ltip);

            currentY2 += 20;

            TextBox tX = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(10, currentY2 + 5),
                Text = "0"
            };
            p.Controls.Add(tX);

            TextBox tY = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(70, currentY2 + 5),
                Text = "0"
            };
            p.Controls.Add(tY);

            TextBox tZ = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(130, currentY2 + 5),
                Text = "0"
            };
            p.Controls.Add(tZ);

            currentY2 += 20;

            var serializer = new JavaScriptSerializer();
            string serializedResult = serializer.Serialize(targetFlver.Dummies);

            Button button = new Button();
            ButtonTips("移动你所选择的辅助点，然后保存移动后的信息至Flver文件内。", button);
            button.Text = "Modify";
            button.Location = new System.Drawing.Point(650, 50);
            button.Click += (s, e) =>
            {
                int i = int.Parse(t.Text);
                if (i >= 0 && i < targetFlver.Dummies.Count)
                {
                    targetFlver.Dummies[i].Position.X += float.Parse(tX.Text);
                    targetFlver.Dummies[i].Position.Y += float.Parse(tY.Text);
                    targetFlver.Dummies[i].Position.Z += float.Parse(tZ.Text);
                    AutoBackUp();
                    targetFlver.Write(flverName);
                    UpdateVertices();
                }
                else
                    MessageBox.Show("Invalid modification value!");
            };

            Button button3 = new Button();
            ButtonTips(
                "Import external json file's dummy information and save to the flver file.\n"
                    + "读取外部json文本并存储至Flver文件中。",
                button3
            );
            button3.Text = "LoadJson";
            button3.Location = new System.Drawing.Point(650, 150);
            button3.Click += (s, e) =>
            {
                var openFileDialog1 = new OpenFileDialog();
                string res = "";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sr = new StreamReader(openFileDialog1.FileName);
                        res = sr.ReadToEnd();
                        sr.Close();
                        targetFlver.Dummies = serializer.Deserialize<List<FLVER.Dummy>>(res);
                        AutoBackUp();
                        targetFlver.Write(flverName);
                        UpdateVertices();
                        MessageBox.Show("Dummy change completed! Please exit the program!", "Info");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Security error.\n\nError message: {ex.Message}\n\n"
                                + $"Details:\n\n{ex.StackTrace}"
                        );
                    }
                }
            };

            Button buttonFix = new Button();
            ButtonTips("写入契丸的辅助点信息以解决武器在只狼内没有剑风以及无法雷闪的问题。",
                buttonFix
            );
            buttonFix.Text = "SekiroFix";
            buttonFix.Location = new System.Drawing.Point(650, 200);
            buttonFix.Click += (s, e) =>
            {
                for (int i = 0; i < refDummy.Count; i++)
                    for (int j = 0; j < targetFlver.Dummies.Count; j++)
                    {
                        if (targetFlver.Dummies[j].ReferenceID == refDummy[i].ReferenceID)
                            break;
                        else if (j == targetFlver.Dummies.Count - 1)
                        {
                            targetFlver.Dummies.Add(refDummy[i]);
                            break;
                        }
                    }
                AutoBackUp();
                targetFlver.Write(flverName);

                UpdateVertices();
                MessageBox.Show("Dummy change fixed! Please exit the program!", "Info");
            };

            f.Size = new System.Drawing.Size(750, 600);
            p.Size = new System.Drawing.Size(600, 530);
            f.Resize += (s, e) =>
            {
                p.Size = new System.Drawing.Size(f.Size.Width - 150, f.Size.Height - 70);
                button.Location = new System.Drawing.Point(f.Size.Width - 100, 50);
                button3.Location = new System.Drawing.Point(f.Size.Width - 100, 150);
                buttonFix.Location = new System.Drawing.Point(f.Size.Width - 100, 200);
            };

            f.Controls.Add(button);
            f.Controls.Add(button3);
            f.Controls.Add(buttonFix);
            f.ShowDialog();
        }

        static int FindFLVER_Bone(FLVER f, string name)
        {
            for (int flveri = 0; flveri < f.Bones.Count; flveri++)
                if (f.Bones[flveri].Name == name)
                    return flveri;
            return -1;
        }


        /// <summary>
        /// Dummy Text
        /// </summary>
        /// <param name="newBones">The new bones list</param>
        public static void BoneWeightShift(List<FLVER.Bone> newBones)
        {
            //Step 1 build a int table to map old bone index -> new bone index
            int[] boneMapTable = new int[targetFlver.Bones.Count];
            for (int i = 0; i < targetFlver.Bones.Count; i++)
                boneMapTable[i] = FindNewBoneIndex(newBones, i);

            //Step 2 according to the table, change all the vertices' bone weights
            foreach (var v in vertices)
                for (int i = 0; i < v.BoneIndices.Length; i++)
                    v.BoneIndices[i] = boneMapTable[v.BoneIndices[i]];
        }

        //Find Bone index, if no such bone find its parent's index
        public static int FindNewBoneIndex(List<FLVER.Bone> newBones, int oldBoneIndex)
        {
            while (oldBoneIndex >= 0)
            {
                string oldBoneName = targetFlver.Bones[oldBoneIndex].Name;
                for (int i = 0; i < newBones.Count; i++)
                    if (oldBoneName == newBones[i].Name)
                        return i;
                oldBoneIndex = targetFlver.Bones[oldBoneIndex].ParentIndex;
            }
            return 0;
        }

        public static void ButtonTips(string tips, Button btn)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(btn, tips);
        }

        /// <summary>
        /// Find the file name without its path name and extension name.
        /// </summary>
        /// <param name="arg">Input.</param>
        /// <returns></returns>
        public static string FindFileName(string arg)
        {
            int startIndex = arg.LastIndexOf('/');

            int altStartIndex = arg.LastIndexOf('\\');

            if (altStartIndex > startIndex)
                startIndex = altStartIndex;

            int endIndex = arg.LastIndexOf('.');
            if (startIndex < 0)
                startIndex = 0;
            if (endIndex >= 0)
            {
                string res = arg.Substring(startIndex, endIndex - startIndex);
                if ((res.ToCharArray())[0] == '\\' || (res.ToCharArray())[0] == '/')
                    res = res.Substring(1);
                return res;
            }

            return arg;
        }


    }
}
