using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MySFformat
{
    static partial class Program
    {
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
                ExportJson(
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
                button3.Location = new System.Drawing.Point(f.Size.Width - 100, 100);
                button3ex.Location = new System.Drawing.Point(f.Size.Width - 100, 150);
                buttonA.Location = new System.Drawing.Point(f.Size.Width - 100, 200);
                tpfXmlEdit.Location = new System.Drawing.Point(f.Size.Width - 100, 250);
                mtdConvert.Location = new System.Drawing.Point(f.Size.Width - 100, 300);
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
                newContent += orgContent[i] + "\r\n";

            for (int i = 0; i < fileArray.Length; i++)
            {
                newContent += "    <texture>\r\n";
                newContent += $"      <name>{Path.GetFileName(fileArray[i])}</name>\r\n";

                string xmlFormat = "0";
                if (MessageBox.Show($"Is {Path.GetFileName(fileArray[i])} a normal texture?",
                    "Set", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    xmlFormat = "106";

                newContent += $"      <format>{xmlFormat}</format>\r\n"
                    + "      <flags1>0x00</flags1>\r\n"
                    + "    </texture>\r\n";
            }

            newContent += "  </textures> \r\n   </tpf> ";
            File.WriteAllText(targetXml, newContent);

            MessageBox.Show("Xml auto edited!");
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

    }
}
