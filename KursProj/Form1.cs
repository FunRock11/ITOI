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
            Images.Add(BeginImg);
            Harris = new List<Harris>();
            FindFiles(Path.GetFullPath(BasePath));
            foreach (Img img in Images)
            {
                Harris.Add(new Harris(img, 2, 0.1));
            }


            int NumPoints = 50;
            foreach (Harris h in Harris)
            {
                NumPoints = Math.Min(NumPoints, h.NPoints);
            }

            foreach (Harris h in Harris)
            {
                h.MS(NumPoints);
            }
            
            foreach(Harris h in Harris)
            {
                h.Hashed();
            }

            int[] P = new int[Harris.Count];
            for (int i = 0; i < Harris.Count; i++)
            {
                P[i] = Harris[0].HemmingRast(Harris[i], 5);
            }

            Harris[0].ImageWithMS.Draw(pictureBox1);
            Harris[2].ImageWithMS.Draw(pictureBox2);

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
