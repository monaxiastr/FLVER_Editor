using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
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
        public static string flverName;
        public static List<FLVER.Vertex> vertices = new List<FLVER.Vertex>();
        public static List<VertexInfo> verticesInfo = new List<VertexInfo>();
        public static Vector3D[] bonePosList = new Vector3D[1000];
        public static Dictionary<string, string> boneParentList;

        //3D view related
        public static Mono3D mono;
        public static Vector3 checkingPoint;
        public static Vector3 checkingPointNormal;
        public static bool useCheckingPoint = false;
        public static int checkingMeshNum = 0;
        public static bool useCheckingMesh = false;
        public static bool boneDisplay = true;
        public static bool dummyDisplay = true;
        public static bool setVertexPos = false;
        public static float setVertexX = 0;
        public static float setVertexY = 1.75f;
        public static float setVertexZ = 0;

        /***settings***/
        public static bool loadTexture = true;
        public static bool show3D = false;

        public static string version = "2.0";

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //加载配置
            string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            IniParser settingFile = new IniParser(assemblyPath + "\\settings.ini");
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

        static void AutoBackUp()
        {
            if (!File.Exists(flverName + ".bak"))
                File.Copy(flverName, flverName + ".bak", false);
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
            int startIndex = arg.LastIndexOf('/') > arg.LastIndexOf('\\') ? arg.LastIndexOf('/') : arg.LastIndexOf('\\');

            int endIndex = arg.LastIndexOf('.');
            if (startIndex < 0)
                startIndex = 0;
            if (endIndex >= 0)
            {
                string res = arg.Substring(startIndex, endIndex - startIndex);
                if (res.ToCharArray()[0] == '\\' || res.ToCharArray()[0] == '/')
                    res = res.Substring(1);
                return res;
            }

            return arg;
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
                        rotOrder = RotationOrder.YZX,
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
            mono.meshInfos = mis.ToArray();
            mono.triVertices = triangles.ToArray();
        }
    }
}
