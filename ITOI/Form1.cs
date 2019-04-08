using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ITOI
{
    public partial class Form1 : Form
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        Func F = new Func();
        string BasePath = "../../../files/";

        Img BeginImg;
        Img GrayImg;
        
        int IWidth;
        int IHeight;

        double[,] DerivativeX;
        double[,] DerivativeY;
        double[,] Sobel;

        Img DerivativeXImg;
        Img DerivativeYImg;
        Img SobelImg;

        Moravek Moravek;
        Harris Harris;

        private void KeyPress1(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 127)
            {
                e.Handled = true;
            }
        }

        private void KeyPress2(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 44 && number != 127)
            {
                e.Handled = true;
            }
        }

        public Form1()
        {
            InitializeComponent();
            /* Очищаем предыдущие результаты */
            F.ClearDir(BasePath + "Result");
            F.ClearDir(BasePath + "Lab 1");
            F.ClearDir(BasePath + "Lab 2");
            F.ClearDir(BasePath + "Lab 2/Core");
            F.ClearDir(BasePath + "Lab 3");
            /*-------------------------------*/
            /*
            openFileDialog.InitialDirectory = "C:/Users/Nick/Desktop/Обработка изображений/ITOI/files/Begin";
            openFileDialog.Filter = "";
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                Close();

            */
            BeginImg = new Img(BasePath + "Begin/BeginImage5.png");
            GrayImg = new Img(BeginImg.GrayMatrix, BeginImg.Width, BeginImg.Height);
            GrayImg.Save(BasePath + "Result/GrayImage.png");

            IWidth = BeginImg.Width;
            IHeight = BeginImg.Height;

            BeginImg.Draw(pictureBox1);
            GrayImg.Draw(pictureBox2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int k = 1;
            double[,] maskX = new double[,]
            {
                {1, 0, -1},
                {2, 0, -2},
                {1, 0, -1}
            };
            double[,] maskY = new double[,]
            {
                {1, 2, 1},
                {0, 0, 0},
                {-1, -2, -1}
            };

            DerivativeX = F.Svertka(GrayImg.GrayMatrix, IWidth, IHeight, maskX, k, 1);
            DerivativeXImg = new Img(DerivativeX, IWidth, IHeight);
            DerivativeXImg.Draw(pictureBox3);
            DerivativeXImg.Save(BasePath + "Lab 1/DerivativeX.png");

            DerivativeY = F.Svertka(GrayImg.GrayMatrix, IWidth, IHeight, maskY, k, 1);
            DerivativeYImg = new Img(DerivativeY, IWidth, IHeight);
            DerivativeYImg.Draw(pictureBox4);
            DerivativeYImg.Save(BasePath + "Lab 1/DerivativeY.png");

            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GrayImg.Draw(pictureBox6);

            Sobel = new double[IHeight, IWidth];
            double dx = 0.0;
            double dy = 0.0;

            for (int y = 0; y < IHeight; y++)
            {
                for (int x = 0; x < IWidth; x++)
                {
                    dx = Math.Pow(DerivativeX[y, x], 2);
                    dy = Math.Pow(DerivativeY[y, x], 2);
                    Sobel[y, x] = Math.Sqrt(dx + dy);
                }
            }

            SobelImg = new Img(Sobel, IWidth, IHeight);
            SobelImg.Draw(pictureBox5);
            SobelImg.Save(BasePath + "Lab 1/Sobel.png");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool mTr = true;
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                mTr = false;
                MessageBox.Show("Введите данные", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (mTr)
            {
                F.ClearDir(BasePath + "Lab 2");
                F.ClearDir(BasePath + "Lab 2/Core");

                double sigma0 = Convert.ToDouble(textBox1.Text); // Сигма 0
                int S = Convert.ToInt32(textBox2.Text);  // Число масштабов в октаве
                double k = Math.Pow(2.0, (1.0 / (double)S)); // Интервал между масштабами

                int minr = Math.Min(IWidth, IHeight);
                int O = 0;                                 // Число октав
                while (minr > 32)
                {
                    minr /= 2;
                    O++;
                }

                Img TekImg = new Img(GrayImg.GrayMatrix, IWidth, IHeight);
                TekImg.Save(BasePath + "Lab 2/000.png");
                double sigmaD = sigma0;                        // Действительная сигма
                double sigmaTEK = sigma0;
                double qq = 1;

                GaussCore GaussMatrix = new GaussCore(sigma0);
                Img GaussImg = new Img(GaussMatrix.Matrix, GaussMatrix.Size, GaussMatrix.Size);
                TekImg.SvertkaWithNormalize(GaussMatrix.Matrix, GaussMatrix.Radius, 1);
                TekImg.Save(BasePath + "Lab 2/" + "00"
                    + " - S1=" + Convert.ToString(Math.Round(sigma0, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigma0, 2)) + ".png");
                GaussImg.Save(BasePath + "Lab 2/Core/" + "00" + ".png");

                for (int o = 0; o < O; o++)
                {
                    for (int s = 1; s <= S; s++)
                    {
                        sigmaTEK = sigma0 * Math.Pow(k, s);                       // Текущая сигма
                        sigmaD = sigma0 * Math.Pow(k, qq);
                        qq++;
                        GaussMatrix = new GaussCore(sigmaTEK);
                        GaussImg = new Img(GaussMatrix.Matrix, GaussMatrix.Size, GaussMatrix.Size);
                        TekImg.SvertkaWithNormalize(GaussMatrix.Matrix, GaussMatrix.Radius, 1);
                        TekImg.Save(BasePath + "Lab 2/" + Convert.ToString(o) + Convert.ToString(s)
                            + " - S1=" + Convert.ToString(Math.Round(sigmaTEK, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigmaD, 2)) + ".png");
                        GaussImg.Save(BasePath + "Lab 2/Core/" + Convert.ToString(o) + Convert.ToString(s) + ".png");

                    }
                    TekImg.Downsample();
                    TekImg.Save(BasePath + "Lab 2/" + Convert.ToString(o + 1) + "0"
                            + " - S1=" + Convert.ToString(Math.Round(sigma0, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigmaD, 2)) + ".png");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != "" && textBox4.Text != "")
            {
                try
                {
                    int radius = Convert.ToInt32(textBox3.Text);
                    double dolya = Convert.ToDouble(textBox4.Text);

                    GrayImg.Draw(pictureBox8);
                    GrayImg.Draw(pictureBox10);

                    Moravek = new Moravek(GrayImg, radius, dolya);

                    Img SI = new Img(Moravek.S, IWidth, IHeight);
                    SI.Draw(pictureBox9);

                    Moravek.ImageWithPoints.Draw(pictureBox7);
                    Moravek.ImageWithPoints.Save(BasePath + "Lab 3/Moravek.png");

                    label9.Text = "Оператор Моравека (Точек: " + Moravek.NPoints + ")";

                    Moravek.ImageWithPoints.Draw(pictureBox12);
                    label14.Text = "Точек: " + Moravek.NPoints;

                    button6.Enabled = true;
                    button8.Enabled = true;
                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox6.Text != "" && textBox5.Text != "")
            {
                try
                {
                    int radius = Convert.ToInt32(textBox6.Text);
                    double dolya = Convert.ToDouble(textBox5.Text);

                    GrayImg.Draw(pictureBox14);

                    Harris = new Harris(GrayImg, radius, dolya);

                    Harris.ImageWithPoints.Draw(pictureBox13);

                    Img DX = new Img(Harris.MinL, IWidth, IHeight);
                    Img DY = new Img(Harris.MaxL, IWidth, IHeight);

                    DX.Draw(pictureBox15);
                    DY.Draw(pictureBox16);

                    Harris.ImageWithPoints.Save(BasePath + "Lab 3/Harris.png");

                    label15.Text = "Оператор Харриса (Точек: " + Harris.NPoints + ")";

                    Harris.ImageWithPoints.Draw(pictureBox18);
                    label20.Text = "Точек: " + Harris.NPoints;

                    button7.Enabled = true;
                    button9.Enabled = true;
                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (textBox7.Text != "")
            {
                try
                {
                    int Npoints = Convert.ToInt32(textBox7.Text);
                    Moravek.ANMS(Npoints);
                    Moravek.ImageWithANMS.Draw(pictureBox11);
                    label13.Text = "Точек: " + Moravek.NewPoints;
                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox8.Text != "")
            {
                try
                {
                    int Npoints = Convert.ToInt32(textBox8.Text);
                    Harris.ANMS(Npoints);
                    Harris.ImageWithANMS.Draw(pictureBox17);
                    label19.Text = "Точек: " + Harris.NewPoints;
                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }
    }
}
