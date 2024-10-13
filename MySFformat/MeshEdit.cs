using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MySFformat
{
    static partial class Program
    {
        static void ModelMesh()
        {
            Form f = new Form
            {
                Text = "MeshEdit",
                Size = new System.Drawing.Size(650, 600)
            };

            Panel p = new Panel
            {
                AutoScroll = true,
                Size = new System.Drawing.Size(f.Size.Width - 150, f.Size.Height - 70)
            };
            parentList = new List<TextBox>();
            childList = new List<TextBox>();
            f.Controls.Add(p);

            List<CheckBox> chosenList = new List<CheckBox>();

            p.Controls.Add(new Label
            {
                Text = "name",
                Size = new System.Drawing.Size(150, 15),
                Location = new System.Drawing.Point(15, 15)
            });
            Button buttonChooseAll = new Button
            {//choose all
                Text = "Choose All",
                Size = new System.Drawing.Size(80, 20),
                Location = new System.Drawing.Point(265, 10)
            };
            buttonChooseAll.Click += (s, e) =>
            {
                bool allSelected = true;
                foreach (var item in chosenList)
                    if (item.Checked == false)
                        allSelected = false;
                foreach (var item in chosenList)
                    item.Checked = !allSelected;
            };
            ButtonTips("全选/全不选", buttonChooseAll);
            p.Controls.Add(buttonChooseAll);

            int currentY = 30;
            //每一个mesh的属性与操作
            for (int i = 0; i < targetFlver.Meshes.Count; i++)
            {
                FLVER.Mesh bn = targetFlver.Meshes[i];
                TextBox t = new TextBox
                {
                    Size = new System.Drawing.Size(250, 15),
                    Location = new System.Drawing.Point(15, currentY),
                    ReadOnly = true,
                    Text = "[M:" + targetFlver.Materials[bn.MaterialIndex].Name + "],Unk1:" + bn.Unk1 + ",Dyna:" + bn.Dynamic
                };
                p.Controls.Add(t);

                CheckBox checkBox = new CheckBox
                {//choose
                    Checked = true,
                    Size = new System.Drawing.Size(15, 15),
                    Location = new System.Drawing.Point(300, currentY)
                };
                p.Controls.Add(checkBox);
                chosenList.Add(checkBox);

                int btnIndex = i;
                Button buttonDelete = new Button
                {
                    Text = "Delete",
                    Size = new System.Drawing.Size(70, 20),
                    Location = new System.Drawing.Point(350, currentY)
                };
                buttonDelete.Click += (s, e) =>
                {
                    foreach (FLVER.Vertex v in targetFlver.Meshes[btnIndex].Vertices)
                    {
                        for (int j = 0; j < v.Positions.Count; j++)
                            v.Positions[j] = new Vector3(0, 0, 0);
                        for (int j = 0; j < v.BoneWeights.Length; j++)
                            v.BoneWeights[j] = 0;
                    }
                    foreach (var mf in targetFlver.Meshes[btnIndex].FaceSets)
                        mf.Vertices = new uint[0];
                    UpdateVertices();
                };
                p.Controls.Add(buttonDelete);

                Button buttonCheck = new Button
                {
                    Text = "Check",
                    Size = new System.Drawing.Size(70, 20),
                    Location = new System.Drawing.Point(420, currentY)
                };
                buttonCheck.Click += (s, e) =>
                {
                    useCheckingMesh = true;
                    checkingMeshNum = btnIndex;
                    FLVER.Mesh mes = targetFlver.Meshes[btnIndex];
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
                        fs.Vertices = null;
                    UpdateVertices();
                };

                p.Controls.Add(buttonCheck);
                currentY += 20;
            }

            #region Chosen meshes operation
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

            currentY += 20;

            CheckBox rotateRadius = new CheckBox
            {
                Size = new System.Drawing.Size(160, 15),
                Text = "Rotate in radius",
                Location = new System.Drawing.Point(10, currentY),
                Checked = false
            };
            p.Controls.Add(rotateRadius);

            #endregion

            Button button = new Button
            {
                Text = "修改",
                Location = new System.Drawing.Point(f.Size.Width - 100, 50)
            };
            ButtonTips("修改面片并保存至Flver文件中。", button);
            button.Click += (s, e) =>
            {
                for (int i = 0; i < chosenList.Count; i++)
                {
                    if (chosenList[i].Checked == false)
                        continue;

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
                        if (!rotateRadius.Checked)
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
                            for (int j = 0; j < v.Positions.Count; j++)
                            {
                                v.Positions[j] = new Vector3(
                                    v.Positions[j].X * x,
                                    v.Positions[j].Y * y,
                                    v.Positions[j].Z * z
                                );
                                int xs = x >= 0 ? 1 : -1;
                                int ys = y >= 0 ? 1 : -1;
                                int zs = z >= 0 ? 1 : -1;
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
                AutoBackUp();
                targetFlver.Write(flverName);
                UpdateVertices();
                MessageBox.Show("Modificiation successful!");
            };

            Button button2 = new Button();
            ButtonTips("把另一个Flver文件合并到当前的Flver文件内。", button2);
            button2.Text = "合并Flver";
            button2.Location = new System.Drawing.Point(f.Size.Width - 100, 100);
            button2.Click += (s, e) =>
            {
                var openFileDialog1 = new OpenFileDialog
                {
                    Title = "选择需要合并的Flver文件",
                    Filter = "Flver files (*.flver)|*.flver",
                };
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
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
                                    continue;
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
                        AutoBackUp();
                        targetFlver.Write(flverName);
                        MessageBox.Show("Attaching new flver file completed!", "Info");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\nDetails:\n\n{ex.StackTrace}");
                    }
            };

            Button buttonFlip = new Button();
            ButtonTips("翻转模型的YZ轴，有些外部模型需要这么做。", buttonFlip);
            buttonFlip.Text = "翻转YZ";
            buttonFlip.Location = new System.Drawing.Point(f.Size.Width - 100, 150);
            buttonFlip.Click += (s, e) =>
            {
                for (int i = 0; i < chosenList.Count; i++)
                {
                    if (chosenList[i].Checked == false)
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
            reverseFaceset.Text = "模型翻面";
            reverseFaceset.Location = new System.Drawing.Point(f.Size.Width - 100, 200);
            reverseFaceset.Click += (s, e) =>
            {
                for (int i = 0; i < chosenList.Count; i++)
                    if (chosenList[i].Checked == true)
                        foreach (FLVER.FaceSet fs in targetFlver.Meshes[i].FaceSets)
                            for (int ifs = 0; ifs < fs.Vertices.Length; ifs += 3)
                                (fs.Vertices[ifs + 2], fs.Vertices[ifs + 1]) = (fs.Vertices[ifs + 1], fs.Vertices[ifs + 2]);
                UpdateVertices();
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Faceset switched!", "Info");
            };

            Button reverseNormal = new Button();
            ButtonTips("反向模型法线&切线。有些特殊情况需要这么做。", reverseNormal);
            reverseNormal.Text = "反向法切";
            reverseNormal.Location = new System.Drawing.Point(f.Size.Width - 100, 250);
            reverseNormal.Click += (s, e) =>
            {
                for (int i = 0; i < chosenList.Count; i++)
                {
                    if (chosenList[i].Checked == false)
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
            meshReset.Text = "重置面片";
            meshReset.Location = new System.Drawing.Point(f.Size.Width - 100, 300);
            meshReset.Click += (s, e) =>
            {
                SetMeshInfoToDefault();
                UpdateVertices();
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Meshs resetted!", "Info");
            };

            Button buttonTBF = new Button
            {
                Text = "双面渲染",
                Location = new System.Drawing.Point(f.Size.Width - 100, 350)
            };
            buttonTBF.Click += (s, e) =>
            {
                for (int i = 0; i < chosenList.Count; i++)
                    if (chosenList[i].Checked == true)
                        foreach (var fs in targetFlver.Meshes[i].FaceSets)
                            fs.CullBackfaces = !fs.CullBackfaces;
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("成功开关双面渲染!", "Info");
            };
            ButtonTips("开关选择的双面渲染", buttonTBF);

            f.Resize += (s, e) =>
            {
                p.Size = new System.Drawing.Size(f.Size.Width - 150, f.Size.Height - 70);
                button.Location = new System.Drawing.Point(f.Size.Width - 100, 50);
                button2.Location = new System.Drawing.Point(f.Size.Width - 100, 100);
                buttonFlip.Location = new System.Drawing.Point(f.Size.Width - 100, 150);
                reverseFaceset.Location = new System.Drawing.Point(f.Size.Width - 100, 200);
                reverseNormal.Location = new System.Drawing.Point(f.Size.Width - 100, 250);
                meshReset.Location = new System.Drawing.Point(f.Size.Width - 100, 300);
                buttonTBF.Location = new System.Drawing.Point(f.Size.Width - 100, 350);
            };

            f.Controls.Add(button);
            f.Controls.Add(button2);
            f.Controls.Add(buttonFlip);
            f.Controls.Add(reverseFaceset);
            f.Controls.Add(reverseNormal);
            f.Controls.Add(meshReset);
            f.Controls.Add(buttonTBF);

            f.ShowDialog();
        }

        public static void SetMeshInfoToDefault()
        {
            int layoutCount = targetFlver.BufferLayouts.Count;

            targetFlver.BufferLayouts.Add(new FLVER.BufferLayout
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
            });

            foreach (FLVER.Mesh mesh in targetFlver.Meshes)
            {
                mesh.BoundingBoxMax = new Vector3(1, 1, 1);
                mesh.BoundingBoxMin = new Vector3(-1, -1, -1);
                mesh.BoundingBoxUnk = new Vector3();
                mesh.Unk1 = 0;

                mesh.DefaultBoneIndex = 0;
                mesh.Dynamic = true;
                mesh.VertexBuffers = new List<FLVER.VertexBuffer>
                {
                    new FLVER.VertexBuffer(0, layoutCount, -1)
                };
                var varray = mesh.FaceSets[0].Vertices;

                mesh.FaceSets = new List<FLVER.FaceSet>();

                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    FLVER.Vertex vit = mesh.Vertices[i];

                    mesh.Vertices[i] = generateVertex(
                        new Vector3(vit.Positions[0].X, vit.Positions[0].Y, vit.Positions[0].Z),
                        vit.UVs[0],
                        vit.UVs[0],
                        vit.Normals[0],
                        vit.Tangents[0]
                    );
                    mesh.Vertices[i].BoneIndices = vit.BoneIndices;
                    mesh.Vertices[i].BoneWeights = vit.BoneWeights;
                }

                mesh.FaceSets.Add(generateBasicFaceSet());
                mesh.FaceSets[0].Vertices = varray;
                mesh.FaceSets[0].CullBackfaces = false;
                if (mesh.FaceSets[0].Vertices.Length > 65534)
                {
                    Console.WriteLine("More than 65535 vertices in a mesh, switch to 32 bits index size mode.");
                    mesh.FaceSets[0].IndexSize = 32;
                }
            }
        }

    }
}
