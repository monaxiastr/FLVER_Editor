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
                Text = "Mesh"
            };
            Panel p = new Panel();
            int sizeY = 50;
            int currentY = 10;
            var boneNameList = new List<TextBox>();
            parentList = new List<TextBox>();
            childList = new List<TextBox>();
            p.AutoScroll = true;
            f.Controls.Add(p);

            List<CheckBox> cbList = new List<CheckBox>(); //List for deleting
            List<TextBox> tbList = new List<TextBox>();
            List<CheckBox> affectList = new List<CheckBox>();

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
                    Location = new System.Drawing.Point(500, currentY)
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
                    Size = new System.Drawing.Size(15, 15),
                    Location = new System.Drawing.Point(320, currentY)
                };
                p.Controls.Add(cb);
                cbList.Add(cb);

                CheckBox cb2 = new CheckBox
                {//choose
                    Checked = true,
                    Size = new System.Drawing.Size(15, 15),
                    Location = new System.Drawing.Point(390, currentY)
                };
                p.Controls.Add(cb2);
                affectList.Add(cb2);
                Button buttonCheck = new Button();
                int btnI = i;
                buttonCheck.Text = "Check";
                buttonCheck.Size = new System.Drawing.Size(70, 20);
                buttonCheck.Location = new System.Drawing.Point(420, currentY);

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
                    UpdateVertices();
                };

                p.Controls.Add(buttonCheck);

                Button buttonTBF = new Button
                {
                    Text = "TBF",
                    Size = new System.Drawing.Size(70, 20),
                    Location = new System.Drawing.Point(500, currentY)
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
                        continue;
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
                                for (int j = 0; j < v.Positions.Count; j++)
                                {
                                    v.Positions[j] = new Vector3(0, 0, 0);
                                    if (v.BoneWeights == null)
                                        continue;
                                    for (int k = 0; k < v.BoneWeights.Length; k++)
                                        v.BoneWeights[k] = 0;
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
