using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;

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

        public static string orgFileName = "";

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

        public static string[] argments = { };

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            argments = args;
            Console.WriteLine("Hello!");
            string assemblyPath = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location
            );
            IniParser settingFile = new IniParser(assemblyPath + "\\MySFformat.ini");
            show3D = settingFile.GetSetting("FLVER", "show3D").Trim() != "0";

            ModelAdjModule();
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
                    {
                        c.B = 0;
                    }
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
            {
                ans.Add(ans[ans.Count - 1]);
            }

            for (int i = 0; i < bonePosList.Length; i++)
            {
                bonePosList[i] = null;
            }

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
            if (!File.Exists(orgFileName + ".bak"))
                File.Copy(orgFileName, orgFileName + ".bak", false);
        }

        /// <summary>
        /// Start window
        /// </summary>
        static void ModelAdjModule()
        {
            OpenFileDialog openFileDialog1;
            openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Title = "Choose fromsoftware .flver model file. by Forsaknsilver"
            };

            if (argments.Length > 0)
            {
                openFileDialog1.FileName = argments[0];
                orgFileName = argments[0];
            }
            else if (openFileDialog1.ShowDialog() == DialogResult.OK)
                orgFileName = openFileDialog1.FileName;
            else
                return;
            string fname = openFileDialog1.FileName;
            FLVER b = null;
            if (fname.Length > 4)
            {
                if (openFileDialog1.FileName.Substring(fname.Length - 4) == ".dcx")
                {
                    //遇到不是DS3,BB的情况会报错，这时候进入DCX状态
                    BND4 bnds = null;
                    List<BinderFile> flverFiles = new List<BinderFile>();
                    try
                    {
                        //Support BND4(DS2,DS3,BB) only
                        bnds = SoulsFile<BND4>.Read(openFileDialog1.FileName);
                    }
                    catch (Exception e) //进入dcx状态
                    {
                        Console.WriteLine("Is not BND4... Try DCX decompress");
                        var fileName = openFileDialog1.FileName;
                        byte[] bytes = DCX.Decompress(fileName, out DCX.Type compression);
                        if (BND4.Is(bytes))
                        {
                            Console.WriteLine($"Unpacking BND4: {fileName}...");
                            bnds = SoulsFile<BND4>.Read(bytes);
                        }
                        //throw e;
                    }
                    if (bnds == null)
                    {
                        MessageBox.Show("Read error.");
                        Application.Exit();
                    }
                    Form cf = new Form
                    {
                        Size = new System.Drawing.Size(520, 400),
                        Text = "Select the flver file you want to view",
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                    };

                    ListBox lv = new ListBox
                    {
                        Size = new System.Drawing.Size(490, 330),
                        Location = new System.Drawing.Point(10, 10),
                        MultiColumn = false,
                    };

                    foreach (var bf in bnds.Files)
                    {
                        if (bf.Name.Contains(".flver"))
                        {
                            flverFiles.Add(bf);
                            lv.Items.Add(bf.Name);
                        }
                        else if (bf.Name.Length >= 4 && loadTexture)
                        {
                            if (bf.Name.Substring(bf.Name.Length - 4) == ".tpf")
                            {
                                try
                                {
                                    targetTPF = TPF.Read(bf.Bytes);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("Unsupported tpf file");
                                }
                            }
                        }
                    }

                    Button select = new Button
                    {
                        Text = "Select",
                        Size = new System.Drawing.Size(490, 20),
                        Location = new System.Drawing.Point(10, 340),
                    };
                    select.Click += (s, e) =>
                    {
                        if (lv.SelectedIndices.Count == 0)
                        {
                            return;
                        }
                        b = FLVER.Read(flverFiles[lv.SelectedIndices[0]].Bytes);
                        openFileDialog1.FileName =
                            openFileDialog1.FileName
                            + "."
                            + FindFileName(flverFiles[0].Name)
                            + ".flver";
                        flverName = openFileDialog1.FileName;
                        cf.Close();
                    };
                    cf.Controls.Add(lv);
                    cf.Controls.Add(select);

                    if (flverFiles.Count == 0)
                    {
                        MessageBox.Show("No FLVER files found!");

                        return;
                    }
                    else if (flverFiles.Count == 1)
                    {
                        b = FLVER.Read(flverFiles[0].Bytes);
                        openFileDialog1.FileName =
                            openFileDialog1.FileName
                            + "."
                            + FindFileName(flverFiles[0].Name)
                            + ".flver";
                        flverName = openFileDialog1.FileName;
                    }
                    else
                        cf.ShowDialog();
                }
            }

            if (b == null)
            {
                b = FLVER.Read(openFileDialog1.FileName);
                flverName = openFileDialog1.FileName;
            }

            targetFlver = b;

            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;
                mono = new Mono3D();
                if (show3D)
                {
                    UpdateVertices();
                    mono.Run();
                }
            }).Start();

            Form f = new Form
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = "FLVER Bones - " + openFileDialog1.FileName,
            };
            Panel p = new Panel();
            int currentY = 10;
            boneNameList = new List<DataGridViewTextBoxCell>();
            parentList = new List<TextBox>();
            childList = new List<TextBox>();

            var boneParentList = new List<DataGridViewTextBoxCell>();
            var boneChildList = new List<DataGridViewTextBoxCell>();
            p.AutoScroll = true;
            f.Controls.Add(p);

            DataGridView dg = new DataGridView();
            var bindingList = new System.ComponentModel.BindingList<FLVER.Bone>(b.Bones);

            dg.Columns.Add("Index", "Index");
            dg.Columns[0].Width = 50;
            dg.Columns.Add("Name", "Name");
            dg.Columns.Add("ParentID", "ParentID");
            dg.Columns[2].Width = 70;
            dg.Columns.Add("ChildID", "ChildID");
            dg.Columns[3].Width = 70;
            dg.Columns.Add("Position", "Position");
            dg.Columns.Add("Scale", "Scale");
            dg.Columns.Add("Rotation", "Rotation");

            foreach (DataGridViewColumn column in dg.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dg.Location = new System.Drawing.Point(10, 10);
            dg.Size = new System.Drawing.Size(380, 450);
            dg.RowHeadersVisible = false;

            for (int i = 0; i < b.Bones.Count; i++)
            {
                FLVER.Bone bn = b.Bones[i];

                DataGridViewRow row = new DataGridViewRow();
                {
                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell
                    {
                        Value = "[" + i + "]"
                    };

                    row.Cells.Add(textboxcell);
                    textboxcell.ReadOnly = true;
                }
                {
                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell
                    {
                        Value = bn.Name
                    };

                    row.Cells.Add(textboxcell);
                    boneNameList.Add(textboxcell);
                }
                {
                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell
                    {
                        Value = bn.ParentIndex + ""
                    };

                    row.Cells.Add(textboxcell);
                    boneParentList.Add(textboxcell);
                }
                {
                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell
                    {
                        Value = bn.ChildIndex + ""
                    };

                    row.Cells.Add(textboxcell);
                    boneChildList.Add(textboxcell);
                }
                {
                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell
                    {
                        Value =
                        bn.Translation.X + "," + bn.Translation.Y + "," + bn.Translation.Z
                    };

                    row.Cells.Add(textboxcell);
                }
                {
                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell
                    {
                        Value = bn.Scale.X + "," + bn.Scale.Y + "," + bn.Scale.Z
                    };

                    row.Cells.Add(textboxcell);
                }
                {
                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell
                    {
                        Value =
                        bn.Rotation.X + "," + bn.Rotation.Y + "," + bn.Rotation.Z
                    };

                    row.Cells.Add(textboxcell);
                }
                dg.Rows.Add(row);
            }

            f.Size = new System.Drawing.Size(550, 700);
            p.Size = new System.Drawing.Size(400, 530);

            currentY += 450;

            Button button = new Button();
            ButtonTips("保存你在Bones部分做出的修改。(改骨骼名称以及父骨骼ID)", button);
            button.Text = "Modify";
            button.Location = new System.Drawing.Point(435, 50);
            button.Click += (s, e) =>
            {
                for (int i2 = 0; i2 < b.Bones.Count; i2++)
                {
                    if (boneNameList.Count < b.Bones.Count)
                    {
                        MessageBox.Show(
                            "Bone does not match, something modified?\nWill not save bone info but will save other things."
                        );
                        break;
                    }
                    b.Bones[i2].Name = boneNameList[i2].Value.ToString();
                    b.Bones[i2].ParentIndex = short.Parse(boneParentList[i2].Value.ToString()); //parentList[i2].Text
                    b.Bones[i2].ChildIndex = short.Parse(boneChildList[i2].Value.ToString());
                }
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Modification finished");
            };

            var serializer = new JavaScriptSerializer();
            string serializedResult = serializer.Serialize(b.Bones);

            Button button2 = new Button();
            ButtonTips("打开材质编辑窗口。", button2);
            button2.Text = "Material";
            button2.Location = new System.Drawing.Point(435, 100);
            button2.Click += (s, e) =>
            {
                ModelMaterial();
            };

            Button button3 = new Button();
            ButtonTips("打开面片编辑(Mesh)窗口。", button3);
            button3.Text = "Mesh";
            button3.Location = new System.Drawing.Point(435, 150);
            button3.Click += (s, e) =>
            {
                ModelMesh();
            };

            Button button_dummy = new Button();
            ButtonTips(
                "打开辅助点(Dummy)窗口。辅助点包含了武器的一些剑风位置，伤害位置之类的信息。",
                button_dummy
            );
            button_dummy.Text = "Dummy";
            button_dummy.Location = new System.Drawing.Point(435, 200);
            button_dummy.Click += (s, e) =>
            {
                Dummies();
            };

            Button button_importModel = new Button();
            ButtonTips(
                "导入外部模型文件，比如Fbx,Dae,Obj。但注意只有Fbx文件可以支持导入骨骼权重。\n"
                    + "可以保留UV贴图坐标，切线法线的信息，但你还是得手动修改贴图信息的。\n"
                    + "另外，实验性质的加入了导入超过65535个顶点的面片集的功能。",
                button_importModel
            );
            button_importModel.Text = "ImportModel";
            button_importModel.Font = new System.Drawing.Font(button.Font.FontFamily, 8);
            button_importModel.Location = new System.Drawing.Point(435, 250);
            button_importModel.Click += (s, e) =>
            {
                importFBX();
            };

            Label thanks = new Label
            {
                Text = "FLVER Editor " + version,
                Location = new System.Drawing.Point(10, f.Size.Height - 60),
                Size = new System.Drawing.Size(700, 50)
            };

            f.Resize += (s, e) =>
            {
                p.Size = new System.Drawing.Size(f.Size.Width - 150, f.Size.Height - 70);
                button.Location = new System.Drawing.Point(f.Size.Width - 115, 50);
                button2.Location = new System.Drawing.Point(f.Size.Width - 115, 100);
                button3.Location = new System.Drawing.Point(f.Size.Width - 115, 150);
                button_dummy.Location = new System.Drawing.Point(f.Size.Width - 115, 200);
                button_importModel.Location = new System.Drawing.Point(f.Size.Width - 115, 250);

                thanks.Location = new System.Drawing.Point(10, f.Size.Height - 60);
                dg.Size = new System.Drawing.Size(f.Size.Width - 200, 450);
            };
            p.Size = new System.Drawing.Size(f.Size.Width - 150, f.Size.Height - 70);

            p.Controls.Add(dg);
            f.Controls.Add(button);
            f.Controls.Add(button2);
            f.Controls.Add(button3);
            f.Controls.Add(button_dummy);
            f.Controls.Add(button_importModel);
            f.Controls.Add(thanks);
            f.BringToFront();
            f.WindowState = FormWindowState.Normal;
            Application.Run(f);
        }

        private static void Select_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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

            //Console.WriteLine(dummyStr);

            f.Controls.Add(p);
            {
                Label l = new Label
                {
                    Text = "Choose # to translate:",
                    Size = new System.Drawing.Size(150, 15),
                    Location = new System.Drawing.Point(10, currentY2 + 5)
                };
                p.Controls.Add(l);
            }
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
            ButtonTips(
                "Fix external weapon's weapon trail/lighting reversal problem in Sekiro by adding kusabimaru's dummy information.\n"
                    + "写入契丸的辅助点信息以解决武器在只狼内没有剑风以及无法雷闪的问题。",
                buttonFix
            );
            buttonFix.Text = "SekiroFix";
            buttonFix.Location = new System.Drawing.Point(650, 200);
            buttonFix.Click += (s, e) =>
            {
                for (int i = 0; i < refDummy.Count; i++)
                {
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

        static void ModelMaterial()
        {
            Form f = new Form
            {
                Text = "Material"
            };
            Panel p = new Panel();
            int sizeY = 50;
            int currentY = 10;
            var boneNameList = new List<TextBox>();
            parentList = new List<TextBox>();
            childList = new List<TextBox>();
            //p.AutoSize = true;
            p.AutoScroll = true;
            f.Controls.Add(p);

            p.Controls.Add(new Label
            {
                Text = "index",
                Size = new System.Drawing.Size(50, 15),
                Location = new System.Drawing.Point(10, currentY + 5)
            });
            p.Controls.Add(new Label
            {
                Text = "name",
                Size = new System.Drawing.Size(150, 15),
                Location = new System.Drawing.Point(70, currentY + 5)
            });
            p.Controls.Add(new Label
            {
                Text = "type",
                Size = new System.Drawing.Size(150, 15),
                Location = new System.Drawing.Point(270, currentY + 5)
            });
            p.Controls.Add(new Label
            {
                Text = "texture path",
                Size = new System.Drawing.Size(150, 15),
                Location = new System.Drawing.Point(340, currentY + 5)
            });
            currentY += 20;

            for (int i = 0; i < targetFlver.Materials.Count; i++)
            {
                FLVER.Material bn = targetFlver.Materials[i];

                p.Controls.Add(new TextBox
                {
                    Size = new System.Drawing.Size(200, 15),
                    Location = new System.Drawing.Point(70, currentY),
                    Text = bn.Name
                });
                p.Controls.Add(new Label
                {
                    Text = "[" + i + "]",
                    Size = new System.Drawing.Size(50, 15),
                    Location = new System.Drawing.Point(10, currentY + 5)
                });
                p.Controls.Add(new TextBox
                {
                    Size = new System.Drawing.Size(70, 15),
                    Location = new System.Drawing.Point(270, currentY),
                    Text = bn.Flags + ",GX" + bn.GXBytes + ",Unk" + bn.Unk18
                });

                Button buttonCheck = new Button();
                int btnI = i;
                buttonCheck.Text = "Edit";
                ButtonTips("快速编辑此材质的贴图路径以及基础信息。", buttonCheck);
                buttonCheck.Size = new System.Drawing.Size(70, 20);
                buttonCheck.Location = new System.Drawing.Point(350, currentY);

                buttonCheck.Click += (s, e) =>
                {
                    MaterialQuickEdit(targetFlver.Materials[btnI]);
                };

                p.Controls.Add(buttonCheck);

                currentY += 20;
                sizeY += 20;
            }

            var serializer = new JavaScriptSerializer();
            string serializedResult = serializer.Serialize(targetFlver.Materials);

            int btnY = 50;

            Button button = new Button();
            ButtonTips("保存对材质的修改至Flver文件中。", button);
            button.Text = "Modify";
            button.Location = new System.Drawing.Point(650, btnY);
            button.Click += (s, e) =>
            {
                AutoBackUp();
                targetFlver.Write(flverName);
            };

            btnY += 50;

            Button button3 = new Button();
            ButtonTips("导入外部的Json文本并保存至Flver文件中。", button3);
            button3.Text = "LoadJson";
            button3.Location = new System.Drawing.Point(650, btnY);
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
                        targetFlver.Materials = serializer.Deserialize<List<FLVER.Material>>(res);
                        AutoBackUp();
                        targetFlver.Write(flverName);
                        MessageBox.Show(
                            "Material change completed! Please exit the program!",
                            "Info"
                        );
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
            Button button3ex = new Button();
            btnY += 50;

            button3ex.Text = "ExportJson";
            ButtonTips("导出当前材质信息到一个json文件内。", button3ex);
            button3ex.Location = new System.Drawing.Point(650, btnY);
            button3ex.Click += (s, e) =>
            {
                exportJson(
                    FormatOutput(serializer.Serialize(targetFlver.Materials)),
                    "Material.json",
                    "Material json text exported!"
                );
            };
            btnY += 50;

            Button buttonA = new Button
            {
                Text = "M[A]_e",
                Location = new System.Drawing.Point(650, btnY)
            };
            ButtonTips("替换所有的材质(mtd)为M[A]_e材质。", buttonA);
            buttonA.Click += (s, e) =>
            {
                foreach (FLVER.Material m in targetFlver.Materials)
                {
                    m.MTD = "M[A]_e.mtd";
                    foreach (FLVER.Texture t in m.Textures)
                    {
                        if (t.Type == "g_BumpmapTexture")
                            t.Path = "N:\\FDP\\data\\Model\\parts\\FullBody\\FB_M_8200\\LG_M_8200\\tex\\LG_M_8200_n.tif";
                        else if (t.Type == "g_SpecularTexture")
                            t.Path = "N:\\FDP\\data\\Model\\parts\\FullBody\\FB_M_8200\\LG_M_8200\\tex\\LG_M_8200_r.tif";
                    }
                }
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Material change completed! Please exit the program!", "Info");
            };
            btnY += 50;

            Button tpfXmlEdit = new Button();
            ButtonTips("自动编辑.tpf贴图文件用WitchyBND解包出来的xml文件。", tpfXmlEdit);
            tpfXmlEdit.Text = "Xml Edit";
            tpfXmlEdit.Location = new System.Drawing.Point(650, btnY);
            tpfXmlEdit.Click += (s, e) =>
            {
                XmlEdit();
            };
            btnY += 50;

            Button mtdConvert = new Button();
            ButtonTips("自动转换所有材质路径为你输入的值。", mtdConvert);
            mtdConvert.Text = "M. Rename";
            mtdConvert.Location = new System.Drawing.Point(650, btnY);
            mtdConvert.Click += (s, e) =>
            {
                string res = "M[ARSN].mtd";
                DialogResult dr = BasicTools.ShowInputDialog(ref res);
                if (dr == DialogResult.Cancel)
                    return;
                foreach (var v in targetFlver.Materials)
                    v.MTD = res;
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Material change completed! Please exit the program!", "Info");
            };
            btnY += 50;

            f.Size = new System.Drawing.Size(750, 600);
            p.Size = new System.Drawing.Size(600, 530);
            f.Resize += (s, e) =>
            {
                p.Size = new System.Drawing.Size(f.Size.Width - 150, f.Size.Height - 70);
                button.Location = new System.Drawing.Point(f.Size.Width - 100, 50);
                button3.Location = new System.Drawing.Point(f.Size.Width - 100, 150);
                button3ex.Location = new System.Drawing.Point(f.Size.Width - 100, 200);
                buttonA.Location = new System.Drawing.Point(f.Size.Width - 100, 250);
                tpfXmlEdit.Location = new System.Drawing.Point(f.Size.Width - 100, 300);
                mtdConvert.Location = new System.Drawing.Point(f.Size.Width - 100, 350);
            };

            f.Controls.Add(button);
            f.Controls.Add(button3);
            f.Controls.Add(button3ex);
            f.Controls.Add(buttonA);
            f.Controls.Add(tpfXmlEdit);
            f.Controls.Add(mtdConvert);
            f.ShowDialog();
        }

        private static void XmlEdit()
        {
            OpenFileDialog openFileDialog1;
            openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Title = "Choose .xml file depacked from .tpf file by Yabber"
            };
            string targetXml;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                targetXml = openFileDialog1.FileName;
            else
                return;
            string parentDir = Path.GetDirectoryName(targetXml);
            string[] fileArray = Directory.GetFiles(parentDir, "*.dds");
            Console.Write(fileArray);
            string[] orgContent = File.ReadLines(targetXml).ToArray();

            string newContent = "";
            for (int i = 0; i < 7; i++)
            {
                newContent += orgContent[i] + "\r\n";
            }

            for (int i = 0; i < fileArray.Length; i++)
            {
                newContent += "    <texture>" + "\r\n";
                newContent += "      <name>" + Path.GetFileName(fileArray[i]) + "</name>" + "\r\n";

                string xmlFormat = "0";
                if (
                    MessageBox.Show(
                        "Is " + Path.GetFileName(fileArray[i]) + " a normal texture?",
                        "Set",
                        MessageBoxButtons.YesNo
                    ) == DialogResult.Yes
                )
                    xmlFormat = "106";

                newContent += "      <format>" + xmlFormat + "</format>" + "\r\n";
                newContent += "      <flags1>0x00</flags1>" + "\r\n";
                newContent += "    </texture>" + "\r\n";
            }

            newContent += "  </textures> \r\n   </tpf> ";
            File.WriteAllText(targetXml, newContent);

            MessageBox.Show("Xml auto edited!");
        }

        static void ModelMesh()
        {
            int[] tests = { 0, 0, 0 };

            Form f = new Form
            {
                Text = "Mesh"
            };
            Panel p = new Panel();
            int sizeY = 50;
            int currentY = 10;
            var boneNameList = new List<TextBox>();
            parentList = new List<TextBox>();
            childList = new List<TextBox>();
            //p.AutoSize = true;
            p.AutoScroll = true;
            f.Controls.Add(p);

            List<CheckBox> cbList = new List<CheckBox>(); //List for deleting
            List<TextBox> tbList = new List<TextBox>();
            List<CheckBox> affectList = new List<CheckBox>();

            TextBox meshInfo = new TextBox
            {
                ReadOnly = true,
                Multiline = true
            };
            p.Controls.Add(new Label
            {
                Text = "index",
                Size = new System.Drawing.Size(50, 15),
                Location = new System.Drawing.Point(10, currentY + 5)
            });
            p.Controls.Add(new Label
            {
                Text = "name",
                Size = new System.Drawing.Size(150, 15),
                Location = new System.Drawing.Point(70, currentY + 5)
            });
            p.Controls.Add(new Label
            {
                Text = "Delete?",
                Size = new System.Drawing.Size(50, 15),
                Location = new System.Drawing.Point(270, currentY + 5)
            });
            p.Controls.Add(new Label
            {
                Text = "Chosen",
                Size = new System.Drawing.Size(50, 15),
                Location = new System.Drawing.Point(340, currentY + 5)
            });
            {
                Button dA = new Button
                {//delete all
                    Text = "A",
                    Size = new System.Drawing.Size(15, 15),
                    Location = new System.Drawing.Point(320, currentY + 5)
                };
                dA.Click += (s, e) =>
                {
                    bool allSelected = true;
                    foreach (var item in cbList)
                        if (item.Checked == false)
                            allSelected = false;
                    foreach (var item in cbList)
                        item.Checked = !allSelected;
                };
                ButtonTips("全选/全不选", dA);
                p.Controls.Add(dA);
            }
            {
                Button dA = new Button
                {//choose all
                    Text = "A",
                    Size = new System.Drawing.Size(15, 15),
                    Location = new System.Drawing.Point(390, currentY + 5)
                };
                dA.Click += (s, e) =>
                {
                    bool allSelected = true;
                    foreach (var item in affectList)
                        if (item.Checked == false)
                            allSelected = false;
                    foreach (var item in affectList)
                        item.Checked = !allSelected;
                };
                ButtonTips("全选/全不选", dA);
                p.Controls.Add(dA);
            }
            {
                Button dA = new Button
                {
                    Text = "TBF All",
                    Size = new System.Drawing.Size(70, 20),
                    Location = new System.Drawing.Point(480, currentY)
                };
                dA.Click += (s, e) =>
                {
                    for (int i = 0; i < affectList.Count; i++)
                    {
                        if (affectList[i].Checked == false)
                        {
                            continue;
                        }
                        foreach (var fs in targetFlver.Meshes[i].FaceSets)
                            fs.CullBackfaces = !fs.CullBackfaces;
                    }
                    AutoBackUp();
                    targetFlver.Write(flverName);
                    MessageBox.Show("Finished toggling all back face rendering!", "Info");
                };
                ButtonTips("开关选择的双面渲染", dA);
                p.Controls.Add(dA);
            }

            currentY += 20;

            for (int i = 0; i < targetFlver.Meshes.Count; i++)
            {
                FLVER.Mesh bn = targetFlver.Meshes[i];

                TextBox t = new TextBox
                {
                    Size = new System.Drawing.Size(200, 15),
                    Location = new System.Drawing.Point(70, currentY),
                    ReadOnly = true,
                    Text = "[M:" + targetFlver.Materials[bn.MaterialIndex].Name + "],Unk1:" + bn.Unk1 + ",Dyna:" + bn.Dynamic
                };
                p.Controls.Add(t);

                p.Controls.Add(new Label
                {
                    Text = "[" + i + "]",
                    Size = new System.Drawing.Size(50, 15),
                    Location = new System.Drawing.Point(10, currentY + 5)
                });

                CheckBox cb = new CheckBox
                {//delete
                    Checked = false,
                    Size = new System.Drawing.Size(70, 15),
                    Location = new System.Drawing.Point(320, currentY)
                };
                p.Controls.Add(cb);
                cbList.Add(cb);

                CheckBox cb2 = new CheckBox
                {//choose
                    Checked = true,
                    Size = new System.Drawing.Size(70, 15),
                    Location = new System.Drawing.Point(390, currentY)
                };
                p.Controls.Add(cb2);
                affectList.Add(cb2);
                Button buttonCheck = new Button();
                int btnI = i;
                buttonCheck.Text = "Check";
                buttonCheck.Size = new System.Drawing.Size(70, 20);
                buttonCheck.Location = new System.Drawing.Point(380, currentY);

                buttonCheck.Click += (s, e) =>
                {
                    useCheckingMesh = true;
                    checkingMeshNum = btnI;
                    FLVER.Mesh mes = targetFlver.Meshes[btnI];
                    JavaScriptSerializer jse = new JavaScriptSerializer();

                    FLVER.Mesh m2 = new FLVER.Mesh
                    {
                        Vertices = new List<FLVER.Vertex>(),
                        VertexBuffers = mes.VertexBuffers,
                        Unk1 = mes.Unk1,
                        MaterialIndex = mes.MaterialIndex,
                        FaceSets = jse.Deserialize<List<FLVER.FaceSet>>(jse.Serialize(mes.FaceSets)),
                        Dynamic = mes.Dynamic,
                        DefaultBoneIndex = mes.DefaultBoneIndex,
                        BoundingBoxUnk = mes.BoundingBoxUnk,
                        BoundingBoxMin = mes.BoundingBoxMin,
                        BoundingBoxMax = mes.BoundingBoxMax,
                        BoneIndices = mes.BoneIndices
                    };
                    foreach (FLVER.FaceSet fs in m2.FaceSets)
                    {
                        fs.Vertices = null;
                    }
                    meshInfo.Text = jse.Serialize(m2);
                    UpdateVertices();
                };

                p.Controls.Add(buttonCheck);

                Button buttonTBF = new Button
                {
                    Text = "TBF",
                    Size = new System.Drawing.Size(70, 20),
                    Location = new System.Drawing.Point(480, currentY)
                };
                buttonTBF.Click += (s, e) =>
                {
                    FLVER.Mesh mes = targetFlver.Meshes[btnI];
                    foreach (var vfs in mes.FaceSets)
                    {
                        vfs.CullBackfaces = !vfs.CullBackfaces;
                    }
                    UpdateVertices();
                    AutoBackUp();
                    targetFlver.Write(flverName);
                    MessageBox.Show("Finished toggling back face rendering!", "Info");
                };
                ButtonTips("Toggle back face rendering or not", buttonTBF);
                p.Controls.Add(buttonTBF);
                currentY += 20;
                sizeY += 20;
            }

            p.Controls.Add(new Label
            {
                Text = "Chosen meshes operation---",
                Size = new System.Drawing.Size(250, 15),
                Location = new System.Drawing.Point(10, currentY + 5)
            });

            currentY += 20;

            CheckBox rotCb = new CheckBox
            {
                Size = new System.Drawing.Size(80, 15),
                Text = "rotation",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(rotCb);

            TextBox rotX = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(90, currentY),
                Text = "0"
            };
            p.Controls.Add(rotX);

            TextBox rotY = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(150, currentY),
                Text = "0"
            };
            p.Controls.Add(rotY);

            TextBox rotZ = new TextBox
            {
                Size = new System.Drawing.Size(70, 15),
                Location = new System.Drawing.Point(210, currentY),
                Text = "0"
            };
            p.Controls.Add(rotZ);

            currentY += 20;

            CheckBox transCb = new CheckBox
            {
                Size = new System.Drawing.Size(80, 15),
                Text = "translation",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(transCb);

            TextBox transX = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(90, currentY),
                Text = "0"
            };
            p.Controls.Add(transX);

            TextBox transY = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(150, currentY),
                Text = "0"
            };
            p.Controls.Add(transY);

            TextBox transZ = new TextBox
            {
                Size = new System.Drawing.Size(70, 15),
                Location = new System.Drawing.Point(210, currentY),
                Text = "0"
            };
            p.Controls.Add(transZ);

            currentY += 20;

            CheckBox scaleCb = new CheckBox
            {
                Size = new System.Drawing.Size(80, 15),
                Text = "scale",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(scaleCb);

            TextBox scaleX = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(90, currentY),
                Text = "1"
            };
            p.Controls.Add(scaleX);

            TextBox scaleY = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(150, currentY),
                Text = "1"
            };
            p.Controls.Add(scaleY);

            TextBox scaleZ = new TextBox
            {
                Size = new System.Drawing.Size(70, 15),
                Location = new System.Drawing.Point(210, currentY),
                Text = "1"
            };
            p.Controls.Add(scaleZ);

            Button buttonN = new Button
            {
                Text = "N. Flip",
                Size = new System.Drawing.Size(70, 20),
                Location = new System.Drawing.Point(280, currentY)
            };
            ButtonTips("按你输入的数值调整法线数值。", buttonN);
            buttonN.Click += (s, e) =>
            {
                for (int i = 0; i < cbList.Count; i++)
                {
                    if (affectList[i].Checked == false)
                        continue;
                    float x = float.Parse(scaleX.Text);
                    float y = float.Parse(scaleY.Text);
                    float z = float.Parse(scaleZ.Text);
                    foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                    {
                        for (int j = 0; j < v.Positions.Count; j++)
                        {
                            int xs = 1;
                            int ys = 1;
                            int zs = 1;

                            //1.62: fixed scaling don't change normal error.
                            if (x < 0)
                                xs = -1;
                            if (y < 0)
                                ys = -1;
                            if (z < 0)
                                zs = -1;
                            v.Normals[j] = new Vector4(
                                v.Normals[j].X * xs,
                                v.Normals[j].Y * ys,
                                v.Normals[j].Z * zs,
                                v.Normals[j].W
                            );
                        }
                    }
                }
                MessageBox.Show("Normal flip completed.");
                AutoBackUp();
                targetFlver.Write(flverName);
            };
            p.Controls.Add(buttonN);

            currentY += 20;

            CheckBox rotDg = new CheckBox
            {
                Size = new System.Drawing.Size(160, 15),
                Text = "Rotate in degrees",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(rotDg);

            currentY += 20;

            CheckBox dummyCb = new CheckBox
            {
                Size = new System.Drawing.Size(160, 15),
                Text = "Affect dummy",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(dummyCb);

            currentY += 20;

            CheckBox bonesCb = new CheckBox
            {
                Size = new System.Drawing.Size(160, 15),
                Text = "Affect bones",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(bonesCb);

            currentY += 20;

            CheckBox facesetCb = new CheckBox
            {
                Size = new System.Drawing.Size(160, 15),
                Text = "Delete faceset only",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(facesetCb);

            currentY += 20;

            CheckBox scaleBoneWeight = new CheckBox
            {
                Size = new System.Drawing.Size(200, 15),
                Text = "Convert bone weight index:",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(scaleBoneWeight);

            TextBox boneF = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(210, currentY),
                Text = "0"
            };
            p.Controls.Add(boneF);

            TextBox boneT = new TextBox
            {
                Size = new System.Drawing.Size(60, 15),
                Location = new System.Drawing.Point(270, currentY),
                Text = "0"
            };
            p.Controls.Add(boneT);

            currentY += 20;
            meshInfo.Size = new System.Drawing.Size(360, 300);
            meshInfo.Location = new System.Drawing.Point(10, currentY);
            p.Controls.Add(meshInfo);

            Button button = new Button
            {
                Text = "Modify",
                Location = new System.Drawing.Point(650, 50)
            };
            ButtonTips("修改面片并保存至Flver文件中。", button);
            button.Click += (s, e) =>
            {
                for (int i = 0; i < cbList.Count; i++)
                {
                    if (affectList[i].Checked == false)
                    {
                        continue;
                    }
                    if (cbList[i].Checked == true)
                    {
                        //if only delete facesets.... but keep vertices.
                        //trick used in some physics case.
                        if (facesetCb.Checked)
                            foreach (var mf in targetFlver.Meshes[i].FaceSets)
                                for (uint facei = 0; facei < mf.Vertices.Length; facei++)
                                    mf.Vertices[facei] = 1;
                        else
                        {
                            foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                            {
                                for (int j = 0; j < v.Positions.Count; j++)
                                {
                                    v.Positions[j] = new Vector3(0, 0, 0);
                                    if (v.BoneWeights == null)
                                        continue;
                                    for (int k = 0; k < v.BoneWeights.Length; k++)
                                        v.BoneWeights[k] = 0;
                                }
                            }
                            foreach (var mf in targetFlver.Meshes[i].FaceSets)
                                mf.Vertices = new uint[0] { };
                        }
                    }
                    int i2 = int.Parse(tbList[i].Text);
                    if (i2 >= 0)
                    {
                        foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                        {
                            if (v.Positions == null)
                            {
                                v.Positions = new List<Vector3>();
                            }
                            for (int j = 0; j < v.Positions.Count; j++)
                            {
                                if (v.BoneWeights == null)
                                {
                                    v.BoneWeights = new float[4];
                                    v.BoneIndices = new int[4];
                                }
                                //v.Positions[j] = new System.Numerics.Vector3(0, 0, 0);
                                for (int k = 0; k < v.BoneWeights.Length; k++)
                                    v.BoneWeights[k] = 0;
                                v.BoneIndices[0] = i2;
                                v.BoneWeights[0] = 1;
                            }
                        }
                        if (!targetFlver.Meshes[i].BoneIndices.Contains(i2))
                            targetFlver.Meshes[i].BoneIndices.Add(i2);
                        targetFlver.Meshes[i].Dynamic = true;
                    }

                    if (transCb.Checked)
                    {
                        float x = float.Parse(transX.Text);
                        float y = float.Parse(transY.Text);
                        float z = float.Parse(transZ.Text);
                        foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                            for (int j = 0; j < v.Positions.Count; j++)
                                v.Positions[j] = new Vector3(
                                    v.Positions[j].X + x,
                                    v.Positions[j].Y + y,
                                    v.Positions[j].Z + z
                                );
                    }

                    if (rotCb.Checked)
                    {
                        float roll = float.Parse(rotX.Text);
                        float pitch = float.Parse(rotY.Text);

                        float yaw = float.Parse(rotZ.Text);
                        if (rotDg.Checked)
                        {
                            roll = (float)(roll / 180f * Math.PI);
                            pitch = (float)(pitch / 180f * Math.PI);
                            yaw = (float)(yaw / 180f * Math.PI);
                        }

                        foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                        {
                            for (int j = 0; j < v.Positions.Count; j++)
                                v.Positions[j] = RotatePoint(v.Positions[j], pitch, roll, yaw);
                            for (int j2 = 0; j2 < v.Normals.Count; j2++)
                                v.Normals[j2] = RotatePoint(v.Normals[j2], pitch, roll, yaw);
                            for (int j2 = 0; j2 < v.Tangents.Count; j2++)
                                v.Tangents[j2] = RotatePoint(v.Tangents[j2], pitch, roll, yaw);
                        }
                    }

                    if (scaleCb.Checked)
                    {
                        float x = float.Parse(scaleX.Text);
                        float y = float.Parse(scaleY.Text);
                        float z = float.Parse(scaleZ.Text);
                        foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                        {
                            for (int j = 0; j < v.Positions.Count; j++)
                            {
                                v.Positions[j] = new Vector3(
                                    v.Positions[j].X * x,
                                    v.Positions[j].Y * y,
                                    v.Positions[j].Z * z
                                );
                                int xs = 1;
                                int ys = 1;
                                int zs = 1;

                                //1.62: fixed scaling don't change normal error.
                                if (x < 0)
                                {
                                    xs = -1;
                                }
                                if (y < 0)
                                {
                                    ys = -1;
                                }
                                if (z < 0)
                                {
                                    zs = -1;
                                }
                                v.Normals[j] = new Vector4(
                                    v.Normals[j].X * xs,
                                    v.Normals[j].Y * ys,
                                    v.Normals[j].Z * zs,
                                    v.Normals[j].W
                                );
                                v.Tangents[j] = new Vector4(
                                    v.Tangents[j].X * xs,
                                    v.Tangents[j].Y * ys,
                                    v.Tangents[j].Z * zs,
                                    v.Tangents[j].W
                                );
                            }
                        }
                    }

                    if (scaleBoneWeight.Checked == true)
                    {
                        int fromBone = int.Parse(boneF.Text);
                        int toBone = int.Parse(boneT.Text);

                        foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                            for (int j = 0; j < v.Positions.Count; j++)
                                if (v.BoneIndices != null)
                                    for (int k = 0; k < v.BoneIndices.Length; k++)
                                        if (v.BoneIndices[k] == fromBone)
                                            v.BoneIndices[k] = toBone;
                    }
                }
                if (dummyCb.Checked)
                {
                    foreach (FLVER.Dummy d in targetFlver.Dummies)
                    {
                        if (transCb.Checked)
                        {
                            float x = float.Parse(transX.Text);
                            float y = float.Parse(transY.Text);
                            float z = float.Parse(transZ.Text);

                            d.Position.X += x;
                            d.Position.Y += y;
                            d.Position.Z += z;
                        }
                        if (rotCb.Checked)
                        {
                            float roll = float.Parse(rotX.Text);
                            float pitch = float.Parse(rotY.Text);
                            float yaw = float.Parse(rotZ.Text);
                            d.Position = RotatePoint(d.Position, pitch, roll, yaw);
                        }
                        if (scaleCb.Checked)
                        {
                            float x = float.Parse(scaleX.Text);
                            float y = float.Parse(scaleY.Text);
                            float z = float.Parse(scaleZ.Text);

                            d.Position.X *= x;
                            d.Position.Y *= y;
                            d.Position.Z *= z;
                        }
                    }
                }

                //if affect bones were checked
                if (bonesCb.Checked)
                {
                    float x = float.Parse(scaleX.Text);
                    float y = float.Parse(scaleY.Text);
                    float z = float.Parse(scaleZ.Text);
                    //1.67: update affect bone functionality
                    foreach (FLVER.Bone bs in targetFlver.Bones)
                    {
                        if (true)
                        {
                            bs.Translation.X = x * bs.Translation.X;
                            bs.Translation.Y = y * bs.Translation.Y;
                            bs.Translation.Z = z * bs.Translation.Z;
                        }
                    }
                }
                AutoBackUp();
                targetFlver.Write(flverName);
                UpdateVertices();
                MessageBox.Show("Modificiation successful!");
            };

            Button button2 = new Button();
            ButtonTips("把另一个Flver文件合并到当前的Flver文件内。", button2);
            button2.Text = "Attach";
            button2.Location = new System.Drawing.Point(650, 100);
            button2.Click += (s, e) =>
            {
                var openFileDialog1 = new OpenFileDialog
                {
                    Title = "Choose the flver file you want to attach to the scene"
                };
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FLVER sekiro = FLVER.Read(openFileDialog1.FileName);
                        int materialOffset = targetFlver.Materials.Count;
                        int layoutOffset = targetFlver.BufferLayouts.Count;

                        Dictionary<int, int> sekiroToTarget = new Dictionary<int, int>();
                        for (int i2 = 0; i2 < sekiro.Bones.Count; i2++)
                        {
                            FLVER.Bone attachBone = sekiro.Bones[i2];
                            for (int i3 = 0; i3 < targetFlver.Bones.Count; i3++)
                                if (attachBone.Name == targetFlver.Bones[i3].Name)
                                {
                                    sekiroToTarget.Add(i2, i3);
                                    break;
                                }
                        }

                        foreach (FLVER.Mesh m in sekiro.Meshes)
                        {
                            m.MaterialIndex += materialOffset;
                            foreach (FLVER.VertexBuffer vb in m.VertexBuffers)
                            {
                                vb.LayoutIndex += layoutOffset;
                            }

                            foreach (FLVER.Vertex v in m.Vertices)
                            {
                                if (v.BoneIndices == null)
                                {
                                    continue;
                                }
                                for (int i5 = 0; i5 < v.BoneIndices.Length; i5++)
                                    if (sekiroToTarget.ContainsKey(v.BoneIndices[i5]))
                                        v.BoneIndices[i5] = sekiroToTarget[v.BoneIndices[i5]];
                            }
                        }

                        targetFlver.BufferLayouts = targetFlver
                            .BufferLayouts.Concat(sekiro.BufferLayouts)
                            .ToList();

                        targetFlver.Meshes = targetFlver.Meshes.Concat(sekiro.Meshes).ToList();

                        targetFlver.Materials = targetFlver
                            .Materials.Concat(sekiro.Materials)
                            .ToList();
                        //sekiro.Meshes[0].MaterialIndex

                        //targetFlver.Materials =  new JavaScriptSerializer().Deserialize<List<FLVER.Material>>(res);
                        AutoBackUp();
                        targetFlver.Write(flverName);
                        MessageBox.Show(
                            "Attaching new flver file completed! Please exit the program!",
                            "Info"
                        );
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

            Button buttonFlip = new Button();
            ButtonTips("翻转模型的YZ轴，有些外部模型需要这么做。", buttonFlip);
            buttonFlip.Text = "Switch YZ";
            buttonFlip.Location = new System.Drawing.Point(650, 150);
            buttonFlip.Click += (s, e) =>
            {
                for (int i = 0; i < cbList.Count; i++)
                {
                    if (affectList[i].Checked == false)
                        continue;
                    float roll = (float)(Math.PI * -0.5f); //X
                    float pitch = (float)(Math.PI); //Y

                    float yaw = 0;
                    foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                    {
                        for (int j = 0; j < v.Positions.Count; j++)
                            v.Positions[j] = RotatePoint(v.Positions[j], pitch, roll, yaw);
                        for (int j2 = 0; j2 < v.Normals.Count; j2++)
                            v.Normals[j2] = RotatePoint(v.Normals[j2], pitch, roll, yaw);
                        for (int j2 = 0; j2 < v.Tangents.Count; j2++)
                            v.Tangents[j2] = RotatePoint(v.Tangents[j2], pitch, roll, yaw);
                    }
                }

                UpdateVertices();

                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("YZ axis switched!", "Info");
            };

            Button reverseFaceset = new Button();
            ButtonTips("模型翻面。有些特殊情况需要这么做。", reverseFaceset);
            reverseFaceset.Text = "Rev. Mesh";
            reverseFaceset.Location = new System.Drawing.Point(650, 200);
            reverseFaceset.Click += (s, e) =>
            {
                for (int i = 0; i < cbList.Count; i++)
                {
                    if (affectList[i].Checked == false)
                        continue;
                    foreach (FLVER.FaceSet fs in targetFlver.Meshes[i].FaceSets)
                        for (int ifs = 0; ifs < fs.Vertices.Length; ifs += 3)
                            (fs.Vertices[ifs + 2], fs.Vertices[ifs + 1]) = (fs.Vertices[ifs + 1], fs.Vertices[ifs + 2]);
                }
                UpdateVertices();
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Faceset switched!", "Info");
            };

            Button reverseNormal = new Button();
            ButtonTips("反向模型法线&切线。有些特殊情况需要这么做。", reverseNormal);
            reverseNormal.Text = "Rev. Norm.";
            reverseNormal.Location = new System.Drawing.Point(650, 250);
            reverseNormal.Click += (s, e) =>
            {
                for (int i = 0; i < cbList.Count; i++)
                {
                    if (affectList[i].Checked == false)
                        continue;
                    foreach (FLVER.Vertex v in targetFlver.Meshes[i].Vertices)
                    {
                        for (int j2 = 0; j2 < v.Normals.Count; j2++)
                            v.Normals[j2] = new Vector4(
                                -v.Normals[j2].X,
                                -v.Normals[j2].Y,
                                -v.Normals[j2].Z,
                                v.Normals[j2].W
                            );
                        for (int j2 = 0; j2 < v.Tangents.Count; j2++)
                            v.Tangents[j2] = new Vector4(
                                -v.Tangents[j2].X,
                                -v.Tangents[j2].Y,
                                -v.Tangents[j2].Z,
                                v.Tangents[j2].W
                            );
                    }
                }

                UpdateVertices();

                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Normals reversed!", "Info");
            };

            Button meshReset = new Button();
            ButtonTips("部分重置面片信息，主要用于导入DS2flver文件至DS3之中。", meshReset);
            meshReset.Text = "M. Reset";
            meshReset.Location = new System.Drawing.Point(650, 300);
            meshReset.Click += (s, e) =>
            {
                SetMeshInfoToDefault();

                UpdateVertices();

                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Meshs resetted!", "Info");
            };

            f.Size = new System.Drawing.Size(750, 600);
            p.Size = new System.Drawing.Size(600, 530);
            f.Resize += (s, e) =>
            {
                p.Size = new System.Drawing.Size(f.Size.Width - 150, f.Size.Height - 70);
                button.Location = new System.Drawing.Point(f.Size.Width - 100, 50);
                button2.Location = new System.Drawing.Point(f.Size.Width - 100, 100);
                buttonFlip.Location = new System.Drawing.Point(f.Size.Width - 100, 150);
                reverseFaceset.Location = new System.Drawing.Point(f.Size.Width - 100, 200);
                reverseNormal.Location = new System.Drawing.Point(f.Size.Width - 100, 250);
                meshReset.Location = new System.Drawing.Point(f.Size.Width - 100, 300);
            };

            f.Controls.Add(button);
            f.Controls.Add(button2);
            f.Controls.Add(buttonFlip);
            f.Controls.Add(reverseFaceset);
            f.Controls.Add(reverseNormal);
            f.Controls.Add(meshReset);

            f.ShowDialog();
            //Application.Run(f);
        }

        static void MaterialQuickEdit(FLVER.Material m)
        {
            Form f = new Form
            {
                Text = "Material quick editor : <" + m.Name + ">"
            };
            Panel p = new Panel();
            List<TextBox> typeList = new List<TextBox>();
            List<TextBox> pathList = new List<TextBox>();
            int currentY = 10;

            Button btnOk = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(500, 50)
            };

            f.Controls.Add(btnOk);

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(500, 100)
            };
            btnCancel.Click += (s, e) =>
            {
                f.Close();
            };
            f.Controls.Add(btnCancel);

            Label tName = new Label
            {
                Size = new System.Drawing.Size(90, 15),
                Location = new System.Drawing.Point(10, currentY),
                Text = "Material Name"
            };
            p.Controls.Add(tName);

            TextBox tName2 = new TextBox
            {
                Size = new System.Drawing.Size(200, 15),
                Location = new System.Drawing.Point(100, currentY),
                Text = m.Name
            };
            p.Controls.Add(tName2);

            currentY += 20;

            Label lMTD = new Label
            {
                Size = new System.Drawing.Size(80, 15),
                Location = new System.Drawing.Point(10, currentY),
                Text = "Mtd path:"
            };
            p.Controls.Add(lMTD);

            TextBox tMTD = new TextBox
            {
                Size = new System.Drawing.Size(200, 15),
                Location = new System.Drawing.Point(100, currentY),
                Text = m.MTD
            };
            p.Controls.Add(tMTD);

            currentY += 20;

            btnOk.Click += (s, e) =>
            {
                m.Name = tName2.Text;
                m.MTD = tMTD.Text;

                for (int i2 = 0; i2 < m.Textures.Count; i2++)
                {
                    m.Textures[i2].Path = pathList[i2].Text;
                    m.Textures[i2].Type = typeList[i2].Text;
                }
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Modification saved! Please exit the material window!");
                f.Close();
            };

            for (int i = 0; i < m.Textures.Count; i++)
            {
                currentY += 20;

                Label lTYPE = new Label
                {
                    Size = new System.Drawing.Size(40, 15),
                    Location = new System.Drawing.Point(10, currentY),
                    Text = "Type:"
                };
                p.Controls.Add(lTYPE);

                TextBox tTYPE = new TextBox
                {
                    Size = new System.Drawing.Size(340, 15),
                    Location = new System.Drawing.Point(60, currentY),
                    Text = m.Textures[i].Type
                };
                p.Controls.Add(tTYPE);
                typeList.Add(tTYPE);

                currentY += 20;

                Label lPATH = new Label
                {
                    Size = new System.Drawing.Size(40, 15),
                    Location = new System.Drawing.Point(10, currentY),
                    Text = "Path:"
                };
                p.Controls.Add(lPATH);

                TextBox tPATH = new TextBox
                {
                    Size = new System.Drawing.Size(340, 15),
                    Location = new System.Drawing.Point(60, currentY),
                    Text = m.Textures[i].Path
                };
                p.Controls.Add(tPATH);
                pathList.Add(tPATH);

                Button btnBrowse = new Button
                {
                    Text = "Browse",
                    Size = new System.Drawing.Size(60, 20),
                    Location = new System.Drawing.Point(410, currentY)
                };
                p.Controls.Add(btnBrowse);

                btnBrowse.Click += (s, e) =>
                {
                    var openFileDialog2 = new OpenFileDialog
                    {
                        Filter = "DDS Texture Files (DDS)|*.DDS"
                    };

                    if (openFileDialog2.ShowDialog() == DialogResult.OK)
                    {
                        string fn = openFileDialog2.FileName;
                        string fnn = Path.GetFileNameWithoutExtension(fn);
                        //MessageBox.Show("Opened:" + fnn);
                        tPATH.Text = fnn + ".tif";
                    }
                };

                currentY += 20;
            }

            p.AutoScroll = true;
            f.Controls.Add(p);

            f.Size = new System.Drawing.Size(600, 600);
            p.Size = new System.Drawing.Size(500, 580);
            f.Resize += (s, e) =>
            {
                p.Size = new System.Drawing.Size(500, f.Size.Height - 70);
            };
            f.ShowDialog();
        }

        //1.73 New
        /// <summary>
        /// Dummy Text
        /// </summary>
        /// <param name="newBones">The new bones list</param>
        public static void BoneWeightShift(List<FLVER.Bone> newBones)
        {
            //Step 1 build a int table to map old bone index -> new bone index
            int[] boneMapTable = new int[targetFlver.Bones.Count];
            for (int i = 0; i < targetFlver.Bones.Count; i++)
                boneMapTable[i] = FindNewIndex(newBones, i);

            //Step 2 according to the table, change all the vertices' bone weights
            foreach (var v in vertices)
                for (int i = 0; i < v.BoneIndices.Length; i++)
                    v.BoneIndices[i] = boneMapTable[v.BoneIndices[i]];
        }

        //Find Bone index, if no such bone find its parent's index
        public static int FindNewIndex(List<FLVER.Bone> newBones, int oldBoneIndex)
        {
            int ans;
            string oldBoneName = targetFlver.Bones[oldBoneIndex].Name;
            for (int i = 0; i < 5; i++)
            {
                ans = FindNewIndexByName(newBones, oldBoneName);
                if (ans >= 0)
                {
                    return ans;
                }
                oldBoneIndex = targetFlver.Bones[oldBoneIndex].ParentIndex;
                if (oldBoneIndex < 0)
                {
                    return 0;
                }
                oldBoneName = targetFlver.Bones[oldBoneIndex].Name;
            }

            return 0;
        }

        public static int FindNewIndexByName(List<FLVER.Bone> newBones, string oldBoneName)
        {
            for (int i = 0; i < newBones.Count; i++)
                if (oldBoneName == newBones[i].Name)
                    return i;
            return -1;
        }

        public static void ButtonTips(string tips, Button btn)
        {
            ToolTip ToolTip1 = new ToolTip();
            ToolTip1.SetToolTip(btn, tips);
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
            {
                startIndex = altStartIndex;
            }

            int endIndex = arg.LastIndexOf('.');
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            if (endIndex >= 0)
            {
                string res = arg.Substring(startIndex, endIndex - startIndex);
                if ((res.ToCharArray())[0] == '\\' || (res.ToCharArray())[0] == '/')
                    res = res.Substring(1);
                return res;
            }

            return arg;
        }

        public static void SetMeshInfoToDefault()
        {
            int layoutCount = targetFlver.BufferLayouts.Count;
            FLVER.BufferLayout newBL = new FLVER.BufferLayout
            {
                new FLVER.BufferLayout.Member(
                    0,
                    0,
                    FLVER.BufferLayout.MemberType.Float3,
                    FLVER.BufferLayout.MemberSemantic.Position,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    12,
                    FLVER.BufferLayout.MemberType.Byte4B,
                    FLVER.BufferLayout.MemberSemantic.Normal,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    16,
                    FLVER.BufferLayout.MemberType.Byte4B,
                    FLVER.BufferLayout.MemberSemantic.Tangent,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    20,
                    FLVER.BufferLayout.MemberType.Byte4B,
                    FLVER.BufferLayout.MemberSemantic.Tangent,
                    1
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    24,
                    FLVER.BufferLayout.MemberType.Byte4B,
                    FLVER.BufferLayout.MemberSemantic.BoneIndices,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    28,
                    FLVER.BufferLayout.MemberType.Byte4C,
                    FLVER.BufferLayout.MemberSemantic.BoneWeights,
                    0
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    32,
                    FLVER.BufferLayout.MemberType.Byte4C,
                    FLVER.BufferLayout.MemberSemantic.VertexColor,
                    1
                ),
                new FLVER.BufferLayout.Member(
                    0,
                    36,
                    FLVER.BufferLayout.MemberType.UVPair,
                    FLVER.BufferLayout.MemberSemantic.UV,
                    0
                )
            };

            targetFlver.BufferLayouts.Add(newBL);

            foreach (FLVER.Mesh mn in targetFlver.Meshes)
            {
                mn.BoundingBoxMax = new Vector3(1, 1, 1);
                mn.BoundingBoxMin = new Vector3(-1, -1, -1);
                mn.BoundingBoxUnk = new Vector3();
                mn.Unk1 = 0;

                mn.DefaultBoneIndex = 0;
                mn.Dynamic = true;
                mn.VertexBuffers = new List<FLVER.VertexBuffer>
                {
                    new FLVER.VertexBuffer(0, layoutCount, -1)
                };
                var varray = mn.FaceSets[0].Vertices;

                mn.FaceSets = new List<FLVER.FaceSet>();

                for (int i = 0; i < mn.Vertices.Count; i++)
                {
                    FLVER.Vertex vit = mn.Vertices[i];

                    mn.Vertices[i] = generateVertex(
                        new Vector3(vit.Positions[0].X, vit.Positions[0].Y, vit.Positions[0].Z),
                        vit.UVs[0],
                        vit.UVs[0],
                        vit.Normals[0],
                        vit.Tangents[0],
                        1
                    );
                    mn.Vertices[i].BoneIndices = vit.BoneIndices;
                    mn.Vertices[i].BoneWeights = vit.BoneWeights;
                }

                mn.FaceSets.Add(generateBasicFaceSet());
                mn.FaceSets[0].Vertices = varray;
                mn.FaceSets[0].CullBackfaces = false;
                if (mn.FaceSets[0].Vertices.Length > 65534)
                {
                    MessageBox.Show(
                        "There are more than 65535 vertices in a mesh , switch to 32 bits index size mode."
                    );
                    mn.FaceSets[0].IndexSize = 32;
                }
            }
        }

        public static void SetFlverMatPath(FLVER.Material m, string typeName, string newPath)
        {
            for (int i = 0; i < m.Textures.Count; i++)
            {
                if (m.Textures[i].Type == typeName)
                {
                    m.Textures[i].Path = newPath;
                    return;
                }
            }

            FLVER.Texture tn = new FLVER.Texture
            {
                Type = typeName,
                Path = newPath,
                ScaleX = 1,
                ScaleY = 1,
                Unk10 = 1,
                Unk11 = true
            };
            m.Textures.Add(tn);
        }

        public static DataTable ToDataTable<T>(IList<T> data)
        {
            System.ComponentModel.PropertyDescriptorCollection props =
                System.ComponentModel.TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                System.ComponentModel.PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = props[i].GetValue(item);
                table.Rows.Add(values);
            }
            return table;
        }
    }
}
