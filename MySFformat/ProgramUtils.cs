using Assimp;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using static SoulsFormats.FLVER.FaceSet;

namespace MySFformat
{
    static partial class Program
    {
        public static Microsoft.Xna.Framework.Vector3 toXnaV3(Vector3 v)
        {
            return new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);
        }

        public static Microsoft.Xna.Framework.Vector3 toXnaV3XZY(Vector3 v)
        {
            return new Microsoft.Xna.Framework.Vector3(v.X, v.Z, v.Y);
        }

        public static Vector3 findBoneTrans(List<FLVER.Bone> b, int index)
        {
            if (bonePosList[index] != null)
                return bonePosList[index].toNumV3();

            if (b[index].ParentIndex == -1)
            {
                bonePosList[index] = new Vector3D(b[index].Translation);
                return b[index].Translation;
            }

            Vector3 ans = b[index].Translation + findBoneTrans(b, b[index].ParentIndex);
            bonePosList[index] = new Vector3D(ans);
            return ans;
        }

        static void printNodeStruct(Node n, int depth = 0, String parent = null)
        {
            if (n.ChildCount == 0)
            {
                string pred = "";
                for (int i = 0; i < depth; i++)
                {
                    pred += "\t";
                }
                if (!n.Name.Contains("$AssimpFbx$"))
                {
                    if (!boneParentList.ContainsKey(n.Name))
                        boneParentList.Add(n.Name, parent);
                    if (parent == null)
                    {
                        parent = "";
                    }
                    Console.Write(pred + parent + "->" + n.Name + "\n");
                }
            }
            else
            {
                string pred = "";
                for (int i = 0; i < depth; i++)
                {
                    pred += "\t";
                }
                string nextParent = parent;
                int increase = 0;
                if (!n.Name.Contains("$AssimpFbx$"))
                {
                    nextParent = n.Name;
                    if (!boneParentList.ContainsKey(n.Name))
                        boneParentList.Add(n.Name, parent);

                    if (parent == null)
                    {
                        parent = "";
                    }
                    increase = 1;
                    Console.Write(pred + parent + "->" + n.Name + "\n");
                }

                foreach (Node nn in n.Children)
                {
                    printNodeStruct(nn, depth + increase, nextParent);
                }
            }
        }

        static Vector3D getMyV3D(Assimp.Vector3D v)
        {
            return new Vector3D(v.X, v.Y, v.Z);
        }

        static FLVER.FaceSet generateBasicFaceSet(FSFlags flags = FSFlags.None, int indexSize = 16, uint[] vertices = null)
        {
            return new FLVER.FaceSet
            {
                CullBackfaces = true,
                TriangleStrip = false,
                Unk06 = 1,
                Unk07 = 0,
                IndexSize = indexSize,
                Flags = flags,
                Vertices = vertices
            };
        }

        static FLVER.Vertex generateVertex(
            Vector3 pos,
            Vector3 uv1,
            Vector3 uv2,
            Vector3 normal,
            Vector3 tangets,
            int tangentW = -1
        )
        {
            FLVER.Vertex ans = new FLVER.Vertex();
            ans.Positions = new List<Vector3> { pos };
            ans.BoneIndices = new int[4] { 0, 0, 0, 0 };
            ans.BoneWeights = new float[4] { 1, 0, 0, 0 };
            ans.UVs = new List<Vector3> { uv1, uv2 };
            ans.Normals = new List<Vector4> { new Vector4(normal.X, normal.Y, normal.Z, -1f) };
            ans.Tangents = new List<Vector4> { new Vector4(tangets.X, tangets.Y, tangets.Z, tangentW), new Vector4(tangets.X, tangets.Y, tangets.Z, tangentW) };
            ans.Colors = new List<FLVER.Vertex.Color> { new FLVER.Vertex.Color(255, 255, 255, 255) };

            return ans;
        }

        static FLVER.Vertex generateVertex(
            Vector3 pos,
            Vector3 uv1,
            Vector3 uv2,
            Vector4 normal,
            Vector4 tangets
        )
        {
            return new FLVER.Vertex
            {
                Positions = new List<Vector3> { pos },
                BoneIndices = new int[4] { 0, 0, 0, 0 },
                BoneWeights = new float[4] { 1, 0, 0, 0 },
                UVs = new List<Vector3> { uv1, uv2 },
                Normals = new List<Vector4> { normal },
                Tangents = new List<Vector4> { tangets, tangets },
                Colors = new List<FLVER.Vertex.Color> { new FLVER.Vertex.Color(255, 255, 255, 255) }
            };
        }

        /*************** Basic Tools section *****************/

        public static Vector3 RotatePoint(Vector3 p, float pitch, float roll, float yaw)
        {
            Vector3 ans = new Vector3(0, 0, 0);

            var cosa = Math.Cos(yaw);
            var sina = Math.Sin(yaw);

            var cosb = Math.Cos(pitch);
            var sinb = Math.Sin(pitch);

            var cosc = Math.Cos(roll);
            var sinc = Math.Sin(roll);

            var Axx = cosa * cosb;
            var Axy = cosa * sinb * sinc - sina * cosc;
            var Axz = cosa * sinb * cosc + sina * sinc;

            var Ayx = sina * cosb;
            var Ayy = sina * sinb * sinc + cosa * cosc;
            var Ayz = sina * sinb * cosc - cosa * sinc;

            var Azx = -sinb;
            var Azy = cosb * sinc;
            var Azz = cosb * cosc;

            var px = p.X;
            var py = p.Y;
            var pz = p.Z;

            ans.X = (float)(Axx * px + Axy * py + Axz * pz);
            ans.Y = (float)(Ayx * px + Ayy * py + Ayz * pz);
            ans.Z = (float)(Azx * px + Azy * py + Azz * pz);

            return ans;
        }

        public static Vector4 RotatePoint(Vector4 p, float pitch, float roll, float yaw)
        {
            Vector4 ans = new Vector4(0, 0, 0, p.W);

            var cosa = Math.Cos(yaw);
            var sina = Math.Sin(yaw);

            var cosb = Math.Cos(pitch);
            var sinb = Math.Sin(pitch);

            var cosc = Math.Cos(roll);
            var sinc = Math.Sin(roll);

            var Axx = cosa * cosb;
            var Axy = cosa * sinb * sinc - sina * cosc;
            var Axz = cosa * sinb * cosc + sina * sinc;

            var Ayx = sina * cosb;
            var Ayy = sina * sinb * sinc + cosa * cosc;
            var Ayz = sina * sinb * cosc - cosa * sinc;

            var Azx = -sinb;
            var Azy = cosb * sinc;
            var Azz = cosb * cosc;

            var px = p.X;
            var py = p.Y;
            var pz = p.Z;

            ans.X = (float)(Axx * px + Axy * py + Axz * pz);
            ans.Y = (float)(Ayx * px + Ayy * py + Ayz * pz);
            ans.Z = (float)(Azx * px + Azy * py + Azz * pz);

            return ans;
        }

        public static Vector3 RotateLine(Vector3 p, Vector3 org, Vector3 direction, double theta)
        {
            double x = p.X;
            double y = p.Y;
            double z = p.Z;

            double a = org.X;
            double b = org.Y;
            double c = org.Z;

            double nu = direction.X / direction.Length();
            double nv = direction.Y / direction.Length();
            double nw = direction.Z / direction.Length();

            double[] rP = new double[3];

            rP[0] =
                (a * (nv * nv + nw * nw) - nu * (b * nv + c * nw - nu * x - nv * y - nw * z))
                    * (1 - Math.Cos(theta))
                + x * Math.Cos(theta)
                + (-c * nv + b * nw - nw * y + nv * z) * Math.Sin(theta);
            rP[1] =
                (b * (nu * nu + nw * nw) - nv * (a * nu + c * nw - nu * x - nv * y - nw * z))
                    * (1 - Math.Cos(theta))
                + y * Math.Cos(theta)
                + (c * nu - a * nw + nw * x - nu * z) * Math.Sin(theta);
            rP[2] =
                (c * (nu * nu + nv * nv) - nw * (a * nu + b * nv - nu * x - nv * y - nw * z))
                    * (1 - Math.Cos(theta))
                + z * Math.Cos(theta)
                + (-b * nu + a * nv - nv * x + nu * y) * Math.Sin(theta);

            Vector3 ans = new Vector3((float)rP[0], (float)rP[1], (float)rP[2]);
            return ans;
        }

        public static Microsoft.Xna.Framework.Vector3 crossPorduct(
            Microsoft.Xna.Framework.Vector3 a,
            Microsoft.Xna.Framework.Vector3 b
        )
        {
            float x1 = a.X;
            float y1 = a.Y;
            float z1 = a.Z;
            float x2 = b.X;
            float y2 = b.Y;
            float z2 = b.Z;
            return new Microsoft.Xna.Framework.Vector3(
                y1 * z2 - z1 * y2,
                z1 * x2 - x1 * z2,
                x1 * y2 - y1 * x2
            );
        }

        public static float dotProduct(
            Microsoft.Xna.Framework.Vector3 a,
            Microsoft.Xna.Framework.Vector3 b
        )
        {
            float x1 = a.X;
            float y1 = a.Y;
            float z1 = a.Z;
            float x2 = b.X;
            float y2 = b.Y;
            float z2 = b.Z;
            return x1 * x2 + y1 * y2 + z1 * z2;
        }

        public static void ModelSwapModule()
        {
            OpenFileDialog openFileDialog1;
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog1.Title = "Choose template seikiro model file.";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                Console.WriteLine(openFileDialog1.FileName);
            else
                return;

            FLVER b = FLVER.Read(openFileDialog1.FileName);

            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            openFileDialog2.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog2.Title = "Choose source DS/BB model file.";

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
                Console.WriteLine(openFileDialog2.FileName);
            else
                return;
            FLVER src = FLVER.Read(openFileDialog2.FileName);

            Form f = new Form();

            Label l = new Label();
            l.Text = "x,y,z offset? Y= weapon length axis,Y+=Closer to hand";
            l.Size = new System.Drawing.Size(150, 15);
            l.Location = new System.Drawing.Point(10, 20);
            f.Controls.Add(l);

            TextBox t = new TextBox();
            t.Size = new System.Drawing.Size(70, 15);
            t.Location = new System.Drawing.Point(10, 60);
            t.Text = "0";
            f.Controls.Add(t);

            TextBox t2 = new TextBox();
            t2.Size = new System.Drawing.Size(70, 15);
            t2.Location = new System.Drawing.Point(10, 100);
            t2.Text = "0";
            f.Controls.Add(t2);

            TextBox t3 = new TextBox();
            t3.Size = new System.Drawing.Size(70, 15);
            t3.Location = new System.Drawing.Point(10, 140);
            t3.Text = "0";
            f.Controls.Add(t3);

            CheckBox cb1 = new CheckBox();
            cb1.Size = new System.Drawing.Size(70, 15);
            cb1.Location = new System.Drawing.Point(10, 160);
            cb1.Text = "Copy Material";
            f.Controls.Add(cb1);

            CheckBox cb2 = new CheckBox();
            cb2.Size = new System.Drawing.Size(150, 15);
            cb2.Location = new System.Drawing.Point(10, 180);
            cb2.Text = "Copy Bones";
            f.Controls.Add(cb2);

            CheckBox cb3 = new CheckBox();
            cb3.Size = new System.Drawing.Size(150, 15);
            cb3.Location = new System.Drawing.Point(10, 200);
            cb3.Text = "Copy Dummy";
            f.Controls.Add(cb3);

            CheckBox cb4 = new CheckBox();
            cb4.Size = new System.Drawing.Size(350, 15);
            cb4.Location = new System.Drawing.Point(10, 220);
            cb4.Text = "All vertex weight to first bone";
            f.Controls.Add(cb4);

            f.ShowDialog();

            float x = float.Parse(t.Text);
            float y = float.Parse(t2.Text);
            float z = float.Parse(t3.Text);

            b.Meshes = src.Meshes;

            if (cb1.Checked)
                b.Materials = src.Materials;

            if (cb2.Checked)
                b.Bones = src.Bones;

            if (cb3.Checked)
                b.Dummies = src.Dummies;

            if (cb4.Checked)
                for (int i = 0; i < b.Meshes.Count; i++)
                {
                    b.Meshes[i].BoneIndices = new List<int>();
                    b.Meshes[i].BoneIndices.Add(0);
                    b.Meshes[i].BoneIndices.Add(1);
                    b.Meshes[i].DefaultBoneIndex = 1;
                    foreach (FLVER.Vertex v in b.Meshes[i].Vertices)
                        for (int j = 0; j < v.Positions.Count; j++)
                        {
                            if (v.BoneWeights == null)
                            {
                                continue;
                            }
                            v.Positions[j] = new Vector3(0, 0, 0);
                            for (int k = 0; k < v.BoneWeights.Length; k++)
                            {
                                v.BoneWeights[k] = 0;
                                v.BoneIndices[k] = 0;
                            }
                            v.BoneIndices[0] = 1;
                            v.BoneWeights[0] = 1;
                        }
                }

            foreach (FLVER.Mesh m in b.Meshes)
                foreach (FLVER.Vertex v in m.Vertices)
                    for (int i = 0; i < v.Positions.Count; i++)
                        v.Positions[i] = new Vector3(
                            v.Positions[i].X + x,
                            v.Positions[i].Y + y,
                            v.Positions[i].Z + z
                        );
            b.Write(openFileDialog1.FileName + "n");
            MessageBox.Show("Swap completed!", "Info");
        }

        public static void ExportJson(
            string content,
            string fileName = "export.json",
            string endMessage = ""
        )
        {
            var openFileDialog2 = new SaveFileDialog();
            openFileDialog2.FileName = fileName;
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sw = new StreamWriter(openFileDialog2.FileName);
                    sw.Write(content);
                    sw.Close();
                    MessageBox.Show(endMessage, "Info");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Security error.\n\nError message: {ex.Message}\n\n"
                            + $"Details:\n\n{ex.StackTrace}"
                    );
                }
            }
        }

        public static string FormatOutput(string jsonString)
        {
            var stringBuilder = new StringBuilder();

            bool escaping = false;
            bool inQuotes = false;
            int indentation = 0;

            foreach (char character in jsonString)
            {
                if (escaping)
                {
                    escaping = false;
                    stringBuilder.Append(character);
                }
                else
                {
                    if (character == '\\')
                    {
                        escaping = true;
                        stringBuilder.Append(character);
                    }
                    else if (character == '\"')
                    {
                        inQuotes = !inQuotes;
                        stringBuilder.Append(character);
                    }
                    else if (!inQuotes)
                    {
                        if (character == ',')
                        {
                            stringBuilder.Append(character);
                            stringBuilder.Append("\r\n");
                            stringBuilder.Append('\t', indentation);
                        }
                        else if (character == '[' || character == '{')
                        {
                            stringBuilder.Append(character);
                            stringBuilder.Append("\r\n");
                            stringBuilder.Append('\t', ++indentation);
                        }
                        else if (character == ']' || character == '}')
                        {
                            stringBuilder.Append("\r\n");
                            stringBuilder.Append('\t', --indentation);
                            stringBuilder.Append(character);
                        }
                        else if (character == ':')
                        {
                            stringBuilder.Append(character);
                            stringBuilder.Append('\t');
                        }
                        else
                        {
                            stringBuilder.Append(character);
                        }
                    }
                    else
                    {
                        stringBuilder.Append(character);
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}
