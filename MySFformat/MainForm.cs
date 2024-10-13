using SoulsFormats;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MySFformat
{
    static partial class Program
    {
        static void ShowMainForm()
        {
            Form f = new Form
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = "FLVER Bones - " + flverName,
                Size = new System.Drawing.Size(750, 700)
            };
            boneNameList = new List<DataGridViewTextBoxCell>();
            parentList = new List<TextBox>();
            childList = new List<TextBox>();

            var boneParentList = new List<DataGridViewTextBoxCell>();
            var boneChildList = new List<DataGridViewTextBoxCell>();

            DataGridView dataGridView = new DataGridView
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(600, 600),
                RowHeadersVisible = false,
            };

            dataGridView.Columns.Add("Index", "Index");
            dataGridView.Columns.Add("Name", "Name");
            dataGridView.Columns.Add("ParentID", "ParentID");
            dataGridView.Columns.Add("ChildID", "ChildID");
            dataGridView.Columns.Add("Position", "Position");
            dataGridView.Columns.Add("Scale", "Scale");
            dataGridView.Columns.Add("Rotation", "Rotation");
            dataGridView.Columns[0].Width = 50;
            dataGridView.Columns[2].Width = 65;
            dataGridView.Columns[3].Width = 65;

            foreach (DataGridViewColumn column in dataGridView.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

            for (int i = 0; i < targetFlver.Bones.Count; i++)
            {
                FLVER.Bone bn = targetFlver.Bones[i];

                DataGridViewRow row = new DataGridViewRow();
                DataGridViewTextBoxCell indexCell = new DataGridViewTextBoxCell
                {
                    Value = "[" + i + "]"
                };
                row.Cells.Add(indexCell);
                indexCell.ReadOnly = true;

                DataGridViewTextBoxCell nameCell = new DataGridViewTextBoxCell
                {
                    Value = bn.Name
                };
                row.Cells.Add(nameCell);
                boneNameList.Add(nameCell);

                DataGridViewTextBoxCell parentIndexCell = new DataGridViewTextBoxCell
                {
                    Value = bn.ParentIndex + ""
                };
                row.Cells.Add(parentIndexCell);
                boneParentList.Add(parentIndexCell);

                DataGridViewTextBoxCell childIndexCell = new DataGridViewTextBoxCell
                {
                    Value = bn.ChildIndex + ""
                };
                row.Cells.Add(childIndexCell);
                boneChildList.Add(childIndexCell);

                row.Cells.Add(new DataGridViewTextBoxCell
                {
                    Value = bn.Translation.X + "," + bn.Translation.Y + "," + bn.Translation.Z
                });
                row.Cells.Add(new DataGridViewTextBoxCell
                {
                    Value = bn.Scale.X + "," + bn.Scale.Y + "," + bn.Scale.Z
                });
                row.Cells.Add(new DataGridViewTextBoxCell
                {
                    Value = bn.Rotation.X + "," + bn.Rotation.Y + "," + bn.Rotation.Z
                });
                dataGridView.Rows.Add(row);
            }

            Label versionLabel = new Label
            {
                Text = "FLVER Editor " + version,
                Location = new System.Drawing.Point(10, f.Size.Height - 60),
                Size = new System.Drawing.Size(300, 50)
            };

            Panel panel = new Panel
            {
                AutoScroll = true,
                Location = new System.Drawing.Point(f.Size.Width - 125, 0),
                Size = new System.Drawing.Size(125, f.Size.Height),
            };
            f.Controls.Add(panel);

            Button buttonModify = new Button
            {
                Text = "保存修改",
                Location = new System.Drawing.Point(20, 50)
            };
            ButtonTips("保存你在Bones部分做出的修改。(改骨骼名称以及父骨骼ID)", buttonModify);
            buttonModify.Click += (s, e) =>
            {
                for (int i = 0; i < targetFlver.Bones.Count; i++)
                {
                    if (boneNameList.Count < targetFlver.Bones.Count)
                    {
                        MessageBox.Show("骨骼不匹配，将存储除骨骼外的其他修改");
                        break;
                    }
                    targetFlver.Bones[i].Name = boneNameList[i].Value.ToString();
                    targetFlver.Bones[i].ParentIndex = short.Parse(boneParentList[i].Value.ToString()); //parentList[i].Text
                    targetFlver.Bones[i].ChildIndex = short.Parse(boneChildList[i].Value.ToString());
                }
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("修改完成");
            };

            var serializer = new JavaScriptSerializer();
            string serializedResult = serializer.Serialize(targetFlver.Bones);

            Button buttonMaterial = new Button
            {
                Text = "材质编辑",
                Location = new System.Drawing.Point(20, 100)
            };
            ButtonTips("打开材质编辑窗口。", buttonMaterial);
            buttonMaterial.Click += (s, e) =>
            {
                ModelMaterial();
            };

            Button buttonMesh = new Button
            {
                Text = "面片编辑",
                Location = new System.Drawing.Point(20, 150)
            };
            ButtonTips("打开面片编辑窗口。", buttonMesh);
            buttonMesh.Click += (s, e) =>
            {
                ModelMesh();
            };

            Button buttonDummy = new Button
            {
                Text = "辅助点",
                Location = new System.Drawing.Point(20, 200)
            };
            ButtonTips("打开辅助点(Dummy)窗口。辅助点包含了武器的一些剑风位置，伤害位置之类的信息。", buttonDummy);
            buttonDummy.Click += (s, e) =>
            {
                Dummies();
            };

            Button buttonImportModel = new Button
            {
                Text = "导入模型",
                Location = new System.Drawing.Point(20, 250)
            };
            ButtonTips("导入外部模型文件，比如Fbx,Dae,Obj。但注意只有Fbx文件可以支持导入骨骼权重。\n"
                    + "可以保留UV贴图坐标，切线法线的信息，但你还是得手动修改贴图信息的。\n"
                    + "另外，实验性质的加入了导入超过65535个顶点的面片集的功能。",
                buttonImportModel
            );
            buttonImportModel.Click += (s, e) =>
            {
                ImportFBX();
            };

            Button buttonDeleteAll = new Button
            {
                Text = "DeleteAll",
                Location = new System.Drawing.Point(20, 300)
            };
            buttonDeleteAll.Click += (s, e) =>
            {
                targetFlver.Meshes.Clear();
                targetFlver.Materials.Clear();
                AutoBackUp();
                targetFlver.Write(flverName);
                UpdateVertices();
            };
            ButtonTips("删除所有面片和材质，并保存修改。", buttonDeleteAll);

            f.Resize += (s, e) =>
            {
                versionLabel.Location = new System.Drawing.Point(10, f.Size.Height - 60);
                panel.Location = new System.Drawing.Point(f.Size.Width - 125, 0);
                panel.Size = new System.Drawing.Size(125, f.Size.Height);
            };

            f.Controls.Add(dataGridView);
            f.Controls.Add(versionLabel);
            panel.Controls.Add(buttonModify);
            panel.Controls.Add(buttonMaterial);
            panel.Controls.Add(buttonMesh);
            panel.Controls.Add(buttonDummy);
            panel.Controls.Add(buttonImportModel);
            panel.Controls.Add(buttonDeleteAll);
            f.BringToFront();
            Application.Run(f);
        }
    }
}
