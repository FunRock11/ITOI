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
        Img BeginImg1;
        Img BeginImg2;
        Img BeginImg3;
        string BeginPath = "";
        string DirPath = "";
        List<Img> Images;
        Harris[] Harris;

        public Form1()
        {
            InitializeComponent();
        }

        private void ThreadProc1()
        {
            int i11 = 0;
            int i12 = Convert.ToInt32(Math.Round((double)Images.Count / 4));
            for (int i1 = i11; i1 < i12; i1++)
            {
                int size1 = Math.Min(Math.Min(BeginImg1.Width, BeginImg1.Height), Math.Min(Images[i1].Width, Images[i1].Height));
                Harris BeginHarris1 = new Harris(BeginImg1, 2, 0.1, size1);
                Harris[i1] = new Harris(Images[i1], 2, 0.1, size1);

                int NumPoints1 = 100;
                NumPoints1 = Math.Min(NumPoints1, BeginHarris1.NPoints);
                NumPoints1 = Math.Min(NumPoints1, Harris[i1].NPoints);

                BeginHarris1.MS(NumPoints1);
                Harris[i1].MS(NumPoints1);

                Harris[i1].PointComparisonMS(BeginHarris1, 2);
            }
        }

        private void ThreadProc2()
        {
            int i21 = Convert.ToInt32(Math.Round((double)Images.Count / 4));
            int i22 = Convert.ToInt32(Math.Round((double)Images.Count / 2));
            for (int i2 = i21; i2 < i22; i2++)
            {
                int size2 = Math.Min(Math.Min(BeginImg2.Width, BeginImg2.Height), Math.Min(Images[i2].Width, Images[i2].Height));
                Harris BeginHarris2 = new Harris(BeginImg2, 2, 0.1, size2);
                Harris[i2] = new Harris(Images[i2], 2, 0.1, size2);

                int NumPoints2 = 100;
                NumPoints2 = Math.Min(NumPoints2, BeginHarris2.NPoints);
                NumPoints2 = Math.Min(NumPoints2, Harris[i2].NPoints);

                BeginHarris2.MS(NumPoints2);
                Harris[i2].MS(NumPoints2);

                Harris[i2].PointComparisonMS(BeginHarris2, 2);
            }
        }

        private void ThreadProc3()
        {
            int i31 = Convert.ToInt32(Math.Round((double)Images.Count / 2));
            int i32 = Images.Count - Convert.ToInt32(Math.Round((double)Images.Count / 4));
            for (int i3 = i31; i3 < i32; i3++)
            {
                int size3 = Math.Min(Math.Min(BeginImg3.Width, BeginImg3.Height), Math.Min(Images[i3].Width, Images[i3].Height));
                Harris BeginHarris3 = new Harris(BeginImg3, 2, 0.1, size3);
                Harris[i3] = new Harris(Images[i3], 2, 0.1, size3);

                int NumPoints3 = 100;
                NumPoints3 = Math.Min(NumPoints3, BeginHarris3.NPoints);
                NumPoints3 = Math.Min(NumPoints3, Harris[i3].NPoints);

                BeginHarris3.MS(NumPoints3);
                Harris[i3].MS(NumPoints3);

                Harris[i3].PointComparisonMS(BeginHarris3, 2);
            }
        }

        private void ThreadProc4()
        {
            int i41 = Images.Count - Convert.ToInt32(Math.Round((double)Images.Count / 4));
            int i42 = Images.Count;
            for (int i4 = i41; i4 < i42; i4++)
            {
                int size4 = Math.Min(Math.Min(BeginImg.Width, BeginImg.Height), Math.Min(Images[i4].Width, Images[i4].Height));
                Harris BeginHarris4 = new Harris(BeginImg, 2, 0.1, size4);
                Harris[i4] = new Harris(Images[i4], 2, 0.1, size4);

                int NumPoints4 = 100;
                NumPoints4 = Math.Min(NumPoints4, BeginHarris4.NPoints);
                NumPoints4 = Math.Min(NumPoints4, Harris[i4].NPoints);

                BeginHarris4.MS(NumPoints4);
                Harris[i4].MS(NumPoints4);

                Harris[i4].PointComparisonMS(BeginHarris4, 2);
            }
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
            FindFiles(DirPath);

            Harris = new Harris[Images.Count];
            if (Images.Count >= 4)
            {
                BeginImg1 = new Img(BeginImg.Bitmap);
                BeginImg2 = new Img(BeginImg.Bitmap);
                BeginImg3 = new Img(BeginImg.Bitmap);

                Thread thread1 = new Thread(ThreadProc1);
                Thread thread2 = new Thread(ThreadProc2);
                Thread thread3 = new Thread(ThreadProc3);

                thread1.Start();
                thread2.Start();
                thread3.Start();
                ThreadProc4();

                thread1.Join();
                thread2.Join();
                thread3.Join();
            }
            else
            {
                for (int i = 0; i < Images.Count; i++)
                {
                    int size = Math.Min(Math.Min(BeginImg.Width, BeginImg.Height), Math.Min(Images[i].Width, Images[i].Height));
                    Harris BeginHarris = new Harris(BeginImg, 2, 0.1, size);
                    Harris[i] = new Harris(Images[i], 2, 0.1, size);

                    int NumPoints = 100;
                    NumPoints = Math.Min(NumPoints, BeginHarris.NPoints);
                    NumPoints = Math.Min(NumPoints, Harris[i].NPoints);

                    BeginHarris.MS(NumPoints);
                    Harris[i].MS(NumPoints);

                    Harris[i].PointComparisonMS(BeginHarris, 2);
                }
            }

            int ii = 1;
            for (int i = 0; i < Images.Count; i++)
            {
                double R = (double)Harris[i].P / Harris[i].NewPoints * 100.0;
                if (R >= 50)
                {
                    imageList1.Images.Add(Images[i].ColourBitmap);
                    listView1.Items.Add(Images[i].Path + " (P = " + Convert.ToString(Math.Round(R, 2)) + "%)", ii - 1);
                    ii++;
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
            textBox1.Text = Path.GetFileName(BeginPath);
            textBox1.Visible = true;
            label4.Visible = true;
            BeginImg = new Img(BeginPath);
            pictureBox1.Image = BeginImg.ColourBitmap;
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
            textBox2.Text = DirPath;
            label5.Visible = true;
            textBox2.Visible = true;
        }
    }
}
