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

namespace KursProj
{
    public partial class Form1 : Form
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        string BasePath = "../../TestFiles/";

        Img BeginImg;
        string BeginPath;
        List<Img> Images;
        List<Harris> Harris;

        public Form1()
        {
            InitializeComponent();
            BeginPath = Path.GetFullPath(BasePath + "BeginImage1.png");
            BeginImg = new Img(BeginPath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Images = new List<Img>();
            Harris = new List<Harris>();
            FindFiles(Path.GetFullPath(BasePath));

            int[] P = new int[Images.Count];
            int ii = 0;
            foreach (Img img in Images)
            {
                int size = Math.Min(Math.Min(BeginImg.Width, BeginImg.Height), Math.Min(img.Width, img.Height));
                Harris BeginHarris = new Harris(BeginImg, 2, 0.5, size);
                Harris.Add(new Harris(img, 2, 0.5, size));

                int NumPoints = 100;
                NumPoints = Math.Min(NumPoints, BeginHarris.NPoints);
                NumPoints = Math.Min(NumPoints, Harris[Harris.Count - 1].NPoints);

                BeginHarris.MS(NumPoints);
                Harris[Harris.Count - 1].MS(NumPoints);

                P[ii] = BeginHarris.PointComparisonMS(Harris[ii], 2);

                ii++;
            }

            int a = 0;

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
    }
}
