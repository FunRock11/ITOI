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
            BeginImg = new Img(BasePath + "Begin/BeginImage4.png");
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

            int radius = 3;
            int p = radius;
            double Imin = 0;
            double Imax = 1000;
            double T = (Imax - Imin) * 2 / 4;
            T = 100;
            double[,] S = F.MoravekS(GrayImg.GrayMatrix, IWidth, IHeight, radius, 1);
            S = F.NormalizeMatrix(S, IWidth, IHeight, Imin, Imax);

            bool[,] InterestingPoints = new bool[IHeight, IWidth];

            double[,] SAdd = new double[IHeight + 2 * p, IWidth + 2 * p];

            for (int y = 0; y < IHeight + 2 * p; y++)
            {
                for (int x = 0; x < IWidth + 2 * p; x++)
                {
                    if (x < p || y < p || y >= IHeight + p || x >= IWidth + p)
                    {
                        SAdd[y, x] = 0;
                    }
                    else
                    {
                        SAdd[y, x] = S[y - p, x - p];
                    }
                }
            }

            for (int y = p; y < IHeight + p; y++)
            {
                for (int x = p; x < IWidth + p; x++)
                {
                    double vMax = -999999999;
                    for (int hWinX = -p; hWinX <= p; hWinX++)
                    {
                        for (int hWinY = -p; hWinY <= p; hWinY++)
                        {
                            if (hWinX == 0 && hWinY == 0)
                            {
                                continue;
                            }
                            if (SAdd[y + hWinY, x + hWinX] > vMax)
                            {
                                vMax = SAdd[y + hWinY, x + hWinX];
                            }
                        }
                    }
                    if (SAdd[y, x] > vMax && SAdd[y, x] > T)
                    {
                        InterestingPoints[y - p, x - p] = true;
                    }
                    else
                    {
                        InterestingPoints[y - p, x - p] = false;
                    }
                }
            }






            Bitmap bitmap = new Bitmap(IWidth, IHeight, PixelFormat.Format32bppArgb);
            Color color;
            bitmap = F.MatrixToImage(GrayImg.GrayMatrix, IWidth, IHeight);
            for (int y = 0; y < IHeight; y++)
            {
                for (int x = 0; x < IWidth; x++)
                {
                    if (InterestingPoints[y, x])
                    {
                        for (int hWinX = -1; hWinX <= 1; hWinX++)
                        {
                            for (int hWinY = -1; hWinY <= 1; hWinY++)
                            {
                                color = Color.FromArgb(255, 255, 0, 0);
                                bitmap.SetPixel(x + hWinX, y + hWinY, color);
                            }
                        }
                    }
                }
            }

            F.DrawImage(bitmap, pictureBox7);
            bitmap.Save("../../../files/Lab 3/Moravek.png");


        }
    }
}
