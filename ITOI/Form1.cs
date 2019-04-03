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
            BeginImg = new Img(BasePath + "Begin/BeginImage1.png");
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

            DerivativeX = F.Svertka(GrayImg.GrayMatrix, IWidth, IHeight, maskX, k, 0);
            DerivativeXImg = new Img(DerivativeX, IWidth, IHeight);
            DerivativeXImg.Draw(pictureBox3);
            DerivativeXImg.Save(BasePath + "Lab 1/DerivativeX.png");

            DerivativeY = F.Svertka(GrayImg.GrayMatrix, IWidth, IHeight, maskY, k, 0);
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
            GrayImg.Draw(pictureBox8);

            Moravek moravek = new Moravek(GrayImg, 1, 1, 0.5);

            moravek.DrawImageWithPoints(pictureBox7);
            moravek.SaveImageWithPoints(BasePath + "Lab 3/Moravek.png");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            GrayImg.Draw(pictureBox14);

            Harris harris = new Harris(GrayImg);

            int radiusokna = 2;
            int UVMAX = 2;
            double[,] MaxL = new double[IHeight, IWidth];
            double[,] MinL = new double[IHeight, IWidth];
            /*
            double[,] MinX = new double[IHeight, IWidth];
            double[,] MinY = new double[IHeight, IWidth];
            double[,] MaxX = new double[IHeight, IWidth];
            double[,] MaxY = new double[IHeight, IWidth];
            */
            double MAXmin = -999999999;
            double R = 0.7;
            for (int y = radiusokna; y < IHeight - radiusokna; y++)
            {
                for (int x = radiusokna; x < IWidth - radiusokna; x++)
                {
                    double A = 0.0;
                    double B = 0.0;
                    double C = 0.0;
                    for (int hWinX = -radiusokna; hWinX <= radiusokna; hWinX++)
                    {
                        for (int hWinY = -radiusokna; hWinY <= radiusokna; hWinY++)
                        {
                            A += Math.Pow(DerivativeX[y + hWinY, x + hWinX], 2);
                            B += DerivativeX[y + hWinY, x + hWinX] * DerivativeY[y + hWinY, x + hWinX];
                            C += Math.Pow(DerivativeY[y + hWinY, x + hWinX], 2);
                        }
                    }
                    double E;
                    double Lmax = -999999999;
                    double Lmin = 999999999;
                    /*
                    int Xmax = 0;
                    int Ymax = 0;
                    int Xmin = 0;
                    int Ymin = 0;
                    */
                    for (int v = -UVMAX; v < UVMAX; v++)
                    {
                        for (int u = -UVMAX; u < UVMAX; u++)
                        {
                            if (x + u < IWidth && x + u >= 0
                                && y + v < IHeight && y + v >= 0
                                && u != 0 && v != 0)
                            {
                                E = A * Math.Pow(u, 2) + 2 * B * u * v + C * Math.Pow(v, 2);
                                if (E > Lmax)
                                {
                                    Lmax = E;
                                }
                                if (E < Lmin)
                                {
                                    Lmin = E;
                                }
                            }
                        }
                    }
                    MaxL[y, x] = Lmax;
                    MinL[y, x] = Lmin;
                    if (MinL[y, x] > MAXmin)
                    {
                        MAXmin = MinL[y, x];
                    }
                }
            }

            double T = MAXmin * R;
            bool[,] InterestingPoints = new bool[IHeight, IWidth];
            for (int y = radiusokna; y < IHeight - radiusokna; y++)
            {
                for (int x = radiusokna; x < IWidth - radiusokna; x++)
                {
                    if (MinL[y, x] > T)
                    {
                        InterestingPoints[y, x] = true;
                    }
                    else
                    {
                        InterestingPoints[y, x] = false;
                    }
                }
            }

            int r = 1;
            Img ImageWithPoints = new Img(GrayImg.GrayMatrix, GrayImg.Width, GrayImg.Height);
            Color color;
            for (int y = radiusokna; y < ImageWithPoints.Height - radiusokna; y++)
            {
                for (int x = radiusokna; x < ImageWithPoints.Width - radiusokna; x++)
                {
                    if (InterestingPoints[y, x])
                    {
                        for (int hWinX = -r; hWinX <= r; hWinX++)
                        {
                            for (int hWinY = -r; hWinY <= r; hWinY++)
                            {
                                if (x + hWinX < ImageWithPoints.Width && x + hWinX >= 0
                                    && y + hWinY < ImageWithPoints.Height && y + hWinY >= 0)
                                {
                                    color = Color.FromArgb(255, 255, 0, 0);
                                    ImageWithPoints.Bitmap.SetPixel(x + hWinX, y + hWinY, color);
                                }
                            }
                        }
                    }
                }
            }
            ImageWithPoints.Draw(pictureBox13);

        }
    }
}
