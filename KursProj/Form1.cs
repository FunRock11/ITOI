using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace KursProj
{
    public partial class Form1 : Form
    {
        OpenFileDialog openFileDialog;
        FolderBrowserDialog folderBrowserDialog;
        string BasePath = "../../TestFiles/";

        Img BeginImg;
        string BeginPath = "";
        string DirPath = "";
        List<Img> Images;
        List<Harris> Harris;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (BeginPath == "")
            {
                MessageBox.Show("Для начала выберите исходное изображение!", "Информация!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (DirPath == "")
            {
                MessageBox.Show("Для начала выберите каталог для поиска!", "Информация!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Images = new List<Img>();
            Harris = new List<Harris>();
            FindFiles(DirPath);

            foreach (Img img in Images)
            {
                int size = Math.Min(Math.Min(BeginImg.Width, BeginImg.Height), Math.Min(img.Width, img.Height));
                Harris BeginHarris = new Harris(BeginImg, 2, 0.1, size);
                Harris.Add(new Harris(img, 2, 0.1, size));

                int NumPoints = 100;
                NumPoints = Math.Min(NumPoints, BeginHarris.NPoints);
                NumPoints = Math.Min(NumPoints, Harris[Harris.Count - 1].NPoints);

                BeginHarris.MS(NumPoints);
                Harris[Harris.Count - 1].MS(NumPoints);

                Harris[Harris.Count - 1].PointComparisonMS(BeginHarris, 2);
            }

            foreach(Harris harris in Harris)
            {
                double R = (double)harris.P / harris.NewPoints * 100.0;
                if (R >= 50)
                {
                    listBox1.Items.Add(harris.Path);
                }
            }

        }

        private void FindFiles(string CoreDir)
        {
            if (Directory.Exists(CoreDir))
            {
                try
                {
                    string[] dirs = Directory.GetDirectories(CoreDir);
                    string[] files = Directory.GetFiles(CoreDir, "*.png");

                    foreach (string d in dirs)
                    {
                        FindFiles(d + "\\");
                    }

                    foreach (string f in files)
                    {
                        if (f != BeginPath)
                        {
                            Images.Add(new Img(f));
                        }
                    }
                }
                catch { }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения(*.png)|*.png";
            openFileDialog.InitialDirectory = Path.GetFullPath(BasePath);
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            BeginPath = openFileDialog.FileName;
            label1.Text = "Исходное изображение: " + BeginPath;
            label1.Visible = true;
            BeginImg = new Img(BeginPath);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Path.GetFullPath(BasePath);
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            DirPath = folderBrowserDialog.SelectedPath;
            label2.Text = "Каталог для поиска: " + DirPath;
            label2.Visible = true;
        }
    }
}
