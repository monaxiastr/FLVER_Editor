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
            };
            Panel p = new Panel();
            p.AutoScroll = true;
            f.Controls.Add(p);

            int currentY = 10;
            boneNameList = new List<DataGridViewTextBoxCell>();
            parentList = new List<TextBox>();
            childList = new List<TextBox>();

            var boneParentList = new List<DataGridViewTextBoxCell>();
            var boneChildList = new List<DataGridViewTextBoxCell>();

            DataGridView dg = new DataGridView();
            var bindingList = new System.ComponentModel.BindingList<FLVER.Bone>(targetFlver.Bones);

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

            for (int i = 0; i < targetFlver.Bones.Count; i++)
            {
                FLVER.Bone bn = targetFlver.Bones[i];

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
                for (int i2 = 0; i2 < targetFlver.Bones.Count; i2++)
                {
                    if (boneNameList.Count < targetFlver.Bones.Count)
                    {
                        MessageBox.Show(
                            "Bone does not match, something modified?\nWill not save bone info but will save other things."
                        );
                        break;
                    }
                    targetFlver.Bones[i2].Name = boneNameList[i2].Value.ToString();
                    targetFlver.Bones[i2].ParentIndex = short.Parse(boneParentList[i2].Value.ToString()); //parentList[i2].Text
                    targetFlver.Bones[i2].ChildIndex = short.Parse(boneChildList[i2].Value.ToString());
                }
                AutoBackUp();
                targetFlver.Write(flverName);
                MessageBox.Show("Modification finished");
            };

            var serializer = new JavaScriptSerializer();
            string serializedResult = serializer.Serialize(targetFlver.Bones);

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
                ImportFBX();
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
            Application.Run(f);
        }
    }
}
