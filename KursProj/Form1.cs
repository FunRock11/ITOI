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
        string TempPath = "../../TempFiles/";

        Img BeginImg;
        Img BeginImg1;
        Img BeginImg2;
        Img BeginImg3;
        string BeginPath = "";
        string DirPath = "";
        List<Img> Images;
        Harris[] Harris;
        string[] Status;
        int NumNotCompletedProcess;

        public Form1()
        {
            InitializeComponent();
        }

        private void ThreadProc(object x)
        {
            Img begimg = (Img)x;
            while (NumNotCompletedProcess > 0)
            {
                for (int i = 0; i < Status.Length; i++)
                {
                    if (Status[i] == "Not completed")
                    {
                        Status[i] = "In progress";
                        NumNotCompletedProcess--;

                        int size = Math.Min(Math.Min(begimg.Width, begimg.Height), Math.Min(Images[i].Width, Images[i].Height));
                        Harris BeginHarris = new Harris(begimg, 2, 0.1, size);
                        Harris[i] = new Harris(Images[i], 2, 0.1, size);

                        int NumPoints = 100;
                        NumPoints = Math.Min(NumPoints, BeginHarris.NPoints);
                        NumPoints = Math.Min(NumPoints, Harris[i].NPoints);

                        BeginHarris.MS(NumPoints);
                        Harris[i].MS(NumPoints);

                        Harris[i].PointComparisonMS(BeginHarris);

                        Status[i] = "Completed";
                        /*
                        Harris[i].ImageWithMS.Save(TempPath + i + ".png");
                        BeginHarris.ImageWithMS.Save(TempPath + "Begin" + i + ".png");
                        */
                    }
                }
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

            Status = new string[Images.Count];
            NumNotCompletedProcess = Status.Length;
            for (int i = 0; i < Status.Length; i++)
            {
                Status[i] = "Not completed";
            }

            BeginImg1 = new Img(BeginImg.Bitmap);
            BeginImg2 = new Img(BeginImg.Bitmap);
            BeginImg3 = new Img(BeginImg.Bitmap);

            Thread thread1 = new Thread(ThreadProc);
            Thread thread2 = new Thread(ThreadProc);
            Thread thread3 = new Thread(ThreadProc);

            thread1.Start(BeginImg1);
            thread2.Start(BeginImg2);
            thread3.Start(BeginImg3);
            ThreadProc(BeginImg);

            thread1.Join();
            thread2.Join();
            thread3.Join();

            imageList1.Images.Clear();
            listView1.Items.Clear();

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
