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
        Bitmap BeginImage;
        int width;
        int height;

        byte[] GrayArray;
        Bitmap GrayImage;
        byte[,] GrayMatrix;

        byte[] DerivativeX;
        Bitmap DerivativeXImage;
        byte[] DerivativeY;
        Bitmap DerivativeYImage;

        byte[] Sobel;
        Bitmap SobelImage;

        public Form1()
        {
            InitializeComponent();
            BeginImage = new Bitmap("../../../files/Begin/BeginImage1.png");
            width = BeginImage.Width;
            height = BeginImage.Height;

            DrawImage(BeginImage, pictureBox1);

            GrayArray = ColorImageToGrayArray(BeginImage);
            GrayImage = GrayArrayToImage(GrayArray, width, height);
            GrayImage.Save("../../../files/Result/GrayImage.png");
            GrayMatrix = ArrayToMatrix(GrayArray, width, height);
            DrawImage(GrayImage, pictureBox2);
        }

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

        public void DrawImage(Bitmap bitmap, PictureBox pictureBox)
        {
            pictureBox.Height = bitmap.Height;
            pictureBox.Width = bitmap.Width;
            pictureBox.Image = bitmap;
        }

        public byte[] ColorImageToGrayArray(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] GrayImg = new byte[width * height];
            Color color;
            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    color = bitmap.GetPixel(x, y);
                    GrayImg[i] = Convert.ToByte(Math.Round(0.213 * color.R + 0.715 * color.G + 0.072 * color.B));
                    i++;
                }
            }
            return GrayImg;
        }

        public Bitmap GrayArrayToImage(byte[] GrayArray, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            int i = 0;
            Color color;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    color = Color.FromArgb(255, GrayArray[i], GrayArray[i], GrayArray[i]);
                    bitmap.SetPixel(x, y, color);
                    i++;
                }
            }
            return bitmap;
        }

        public byte[] SetBrightnessToGrayArray(byte[] GrayArray, int width, int height, int r)
        {
            int N = width * height;
            byte[] b = new byte[N];
            int q = 0;
            for (int i = 0; i < N; i++)
            {
                q = GrayArray[i] + r;
                if (q > 255)
                {
                    b[i] = 255;
                }
                else if (q < 0)
                {
                    b[i] = 0;
                }
                else
                {
                    b[i] = Convert.ToByte(q);
                }
            }
            return b;
        }

        public byte[] Svertka(byte[,] GrayMatrix, int width, int height, double[,] mask, int k, int kraimode)
        {
            byte[,] GrayMatrixAdd = new byte[height + 2 * k, width + 2 * k];
            byte[,] Result = new byte[height, width];

            for (int y = 0; y < height + 2 * k; y++)
            {
                for (int x = 0; x < width + 2 * k; x++)
                {
                    if (kraimode == 0) // Снаружи 0
                    {
                        if (x < k || y < k || y >= height + k || x >= width + k)
                        {
                            GrayMatrixAdd[y, x] = 0;
                        }
                        else
                        {
                            GrayMatrixAdd[y, x] = GrayMatrix[y - k, x - k];
                        }
                    }
                }
            }

            for (int y = k; y < height + k; y++)
            {
                for (int x = k; x < width + k; x++)
                {
                    double S = 0;
                    for (int hWinX = -k; hWinX <= k; hWinX++)
                    {
                        for (int hWinY = -k; hWinY <= k; hWinY++)
                        {
                            S += (double)GrayMatrixAdd[y - hWinY, x - hWinX] * mask[k + hWinY, k + hWinX];
                        }
                    }
                    if (S > 255)
                    {
                        Result[y - k, x - k] = 255;
                    }
                    else if(S < 0)
                    {
                        Result[y - k, x - k] = 0;
                    }
                    else
                    {
                        Result[y - k, x - k] = Convert.ToByte(Math.Round(S));
                    }
                }
            }

            return MatrixToArray(Result, width, height);
        }

        public double[,] GausCore (double sigma, out int k)
        {
            k = Convert.ToInt32(Math.Round(3 * sigma));
            double[,] GausMatrix = new double[2 * k + 1, 2 * k + 1];

            for (int hWinX = -k; hWinX <= k; hWinX++)
            {
                for (int hWinY = -k; hWinY <= k; hWinY++)
                {
                    GausMatrix[k + hWinY, k + hWinX] = (1.0 / (2.0 * Math.PI * Math.Pow(sigma, 2))) * Math.Pow(Math.E, (-1.0 * (Math.Pow(hWinX, 2) + Math.Pow(hWinY, 2)) / (2.0 * Math.Pow(sigma, 2))));
                }
            }

            return GausMatrix;
        }

        public byte[,] ArrayToMatrix(byte[] array, int width, int height)
        {
            byte[,] matrix = new byte[height, width];

            for (int i = 0; i < width * height; i++)
            {
                int x = i % width;
                int y = i / width;
                matrix[y, x] = array[i];
            }

            return matrix;
        }

        public byte[] MatrixToArray(byte[,] matrix, int width, int height)
        {
            byte[] array = new byte[height * width];

            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    array[i] = matrix[y, x];
                    i++;
                }
            }

            return array;
        }

        public void DrawGausCore(byte[,] GausMatrix, int k, PictureBox pictureBox)
        {
            double max = 0.0;
            for (int i = 0; i < k * 2 + 1; i++)
            {
                for (int j = 0; j < k * 2 + 1; j++)
                {
                    if (GausMatrix[i, j] > max)
                    {
                        max = GausMatrix[i, j];
                    }
                }
            }

            double kk = 255.0 / max;

            byte[,] b1 = new byte[k * 2 + 1, k * 2 + 1];
            for (int i = 0; i < k * 2 + 1; i++)
            {
                for (int j = 0; j < k * 2 + 1; j++)
                {
                    b1[i, j] = Convert.ToByte(GausMatrix[i, j] * kk);
                }
            }
            byte[] b2 = MatrixToArray(b1, k * 2 + 1, k * 2 + 1);
            Bitmap bitmap = GrayArrayToImage(b2, k * 2 + 1, k * 2 + 1);
            DrawImage(bitmap, pictureBox);
            bitmap.Save("../../../files/Result/GaussCore.png");
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

            DerivativeX = Svertka(GrayMatrix, width, height, maskX, k, 0);
            DerivativeXImage = GrayArrayToImage(DerivativeX, width, height);
            DrawImage(DerivativeXImage, pictureBox3);
            DerivativeXImage.Save("../../../files/Result/DerivativeX.png");

            DerivativeY = Svertka(GrayMatrix, width, height, maskY, k, 0);
            DerivativeYImage = GrayArrayToImage(DerivativeY, width, height);
            DrawImage(DerivativeYImage, pictureBox4);
            DerivativeYImage.Save("../../../files/Result/DerivativeY.png");

            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DrawImage(GrayImage, pictureBox6);

            Sobel = new byte[height * width];
            double S = 0;
            for (int i = 0; i < height * width; i++)
            {
                S = Math.Round(Math.Sqrt(Math.Pow(DerivativeX[i], 2) + Math.Pow(DerivativeY[i], 2)));
                if (S > 255)
                {
                    Sobel[i] = 255;
                }
                else if (S < 0)
                {
                    Sobel[i] = 0;
                }
                else
                {
                    Sobel[i] = Convert.ToByte(S);
                }
            }
            SobelImage = GrayArrayToImage(Sobel, width, height);
            DrawImage(SobelImage, pictureBox5);
            SobelImage.Save("../../../files/Result/Sobel.png");
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
                int sigma0 = Convert.ToInt32(textBox1.Text); // Сигма 0
                int S = Convert.ToInt32(textBox2.Text);  // Число масштабов в октаве

            }




            /*
            int k;
            double sigma = 5.5;
            double[,] GausMatrix = GausCore(sigma, out k);
            DerivativeX = Svertka(GrayMatrix, width, height, GausMatrix, k, 0);
            DerivativeXImage = GrayArrayToImage(DerivativeX, width, height);
            //DrawImage(DerivativeXImage, pictureBox8);
            */
        }
    }
}
