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
        string BasePath = "../../../files/";

        Img BeginImg;
        List<Img> Images;

        public Form1()
        {
            InitializeComponent();
            BeginImg = new Img(BasePath + "Begin/BeginImage0.png");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Images = new List<Img>();
            FindFiles("C:\\Users\\6\\Desktop\\");
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
                        Images.Add(new Img(f));
                    }
                }
                catch { }
            }
        }
    }
}
