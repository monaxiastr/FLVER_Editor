using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MySFformat
{
    static partial class Program
    {
        static void Dummies()
        {
            Form f = new Form
            {
                Text = "Dummies",
                Size = new System.Drawing.Size(750, 600)
            };

            Panel p = new Panel
            {
                Size = new System.Drawing.Size(600, 530),
                AutoScroll = true
            };
            f.Controls.Add(p);

            string assemblyPath = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location
            );

            int currentY2 = 10;
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
            ButtonTips("读取外部json文本并存储至Flver文件中。", button3);
            button3.Text = "LoadJson";
            button3.Location = new System.Drawing.Point(650, 100);
            button3.Click += (s, e) =>
            {
                var openFileDialog1 = new OpenFileDialog
                {
                    Filter = "Json files (*.json)|*.json"
                };
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sr = new StreamReader(openFileDialog1.FileName);
                        string res = sr.ReadToEnd();
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
                            $"Security error.\n\nError message: {ex.Message}\n\nDetails:\n\n{ex.StackTrace}"
                        );
                    }
                }
            };

            f.Resize += (s, e) =>
            {
                p.Size = new System.Drawing.Size(f.Size.Width - 150, f.Size.Height - 70);
                button.Location = new System.Drawing.Point(f.Size.Width - 100, 50);
                button3.Location = new System.Drawing.Point(f.Size.Width - 100, 100);
            };

            f.Controls.Add(button);
            f.Controls.Add(button3);
            f.ShowDialog();
        }

    }
}
