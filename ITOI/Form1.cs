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

/*Обработка изображений*/

namespace ITOI
{
    public partial class Form1 : Form
    {
        Bitmap BeginImage;
        int IWidth;
        int IHeight;
        
        Bitmap GrayImage;
        byte[,] GrayMatrix;

        double[,] DerivativeX;
        Bitmap DerivativeXImage;
        double[,] DerivativeY;
        Bitmap DerivativeYImage;

        double[,] Sobel;
        Bitmap SobelImage;

        public Form1()
        {
            InitializeComponent();

            /* Очищаем предыдущие результаты */
            ClearDir("../../../files/Result");
            ClearDir("../../../files/Lab 1");
            ClearDir("../../../files/Lab 2");
            ClearDir("../../../files/Lab 2/Core");
            ClearDir("../../../files/Lab 3");
            /*-------------------------------*/

            BeginImage = new Bitmap("../../../files/Begin/BeginImage1.png");
            IWidth = BeginImage.Width;
            IHeight = BeginImage.Height;

            DrawImage(BeginImage, pictureBox1);

            GrayMatrix = ColorImageToGrayMatrix(BeginImage);
            GrayImage = MatrixToImage(GrayMatrix, IWidth, IHeight);
            GrayImage.Save("../../../files/Result/GrayImage.png");
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

        public void ClearDir(string dir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
        }

        public byte[,] ColorImageToGrayMatrix(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[,] GrayImg = new byte[height, width];
            Color color;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    color = bitmap.GetPixel(x, y);
                    GrayImg[y, x] = Convert.ToByte(Math.Round(0.213 * color.R + 0.715 * color.G + 0.072 * color.B));
                }
            }
            return GrayImg;
        }

        public Bitmap MatrixToImage(byte[,] GrayMatrix, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Color color;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    color = Color.FromArgb(255, GrayMatrix[y, x], GrayMatrix[y, x], GrayMatrix[y, x]);
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }

        public Bitmap MatrixToImage(double[,] GrayMatrix, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Color color;
            byte[,] NormMtx = NormalizeMatrix(GrayMatrix, width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    color = Color.FromArgb(255, NormMtx[y, x], NormMtx[y, x], NormMtx[y, x]);
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }

        public double[,] Svertka(byte[,] GrayMatrix, int width, int height, double[,] mask, int k, int kraimode)
        {
            byte[,] GrayMatrixAdd = new byte[height + 2 * k, width + 2 * k];
            double[,] Result = new double[height, width];

            /* Краевые эффекты */
            // Копируем значение с края изображения
            if (kraimode == 1) 
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        GrayMatrixAdd[y + k, x + k] = GrayMatrix[y, x];
                    }
                }
                for (int y = k; y < height + k; y++)
                {
                    for (int x = 0; x < k; x++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[y, k];
                        GrayMatrixAdd[y, width + k + x] = GrayMatrixAdd[y, width + k - 1];
                    }
                }
                for (int x = 0; x < width + 2 * k; x++)
                {
                    for (int y = 0; y < k; y++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[k, x];
                        GrayMatrixAdd[height + k + y, x] = GrayMatrixAdd[height + k - 1, x];
                    }
                }
            }
            // Всё снаружи чёрное
            else
            {
                for (int y = 0; y < height + 2 * k; y++)
                {
                    for (int x = 0; x < width + 2 * k; x++)
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
            /*-----------------*/

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
                    Result[y - k, x - k] = S;
                }
            }

            return Result;
        }

        public byte[,] NormalizeMatrix(double[,] Matrix, int width, int height)
        {
            byte[,] ResultByte = new byte[height, width];
            double[,] Result = new double[height, width];

            double vMax = -999999999;
            double vMin = 999999999;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = Matrix[y, x];
                    if (Result[y, x] >= vMax)
                    {
                        vMax = Result[y, x];
                    }
                    else if (Result[y, x] <= vMin)
                    {
                        vMin = Result[y, x];
                    }
                }
            }

            vMax -= vMin;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = (Result[y, x] - vMin) * 255.0 / vMax;
                    ResultByte[y, x] = Convert.ToByte(Result[y, x]);
                }
            }

            return ResultByte;
        }

        public double[,] NormalizeMatrix(double[,] Matrix, int width, int height, double newMin, double newMax)
        {
            double[,] Result = new double[height, width];

            double vMax = -999999999;
            double vMin = 999999999;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = Matrix[y, x];
                    if (Result[y, x] >= vMax)
                    {
                        vMax = Result[y, x];
                    }
                    else if (Result[y, x] <= vMin)
                    {
                        vMin = Result[y, x];
                    }
                }
            }
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = (Result[y, x] - vMin) * (newMax - newMin) / (vMax - vMin) + newMin;
                }
            }

            return Result;
        }

        public double[,] GausCore(double sigma, out int k)
        {
            k = Convert.ToInt32(Math.Round(3.0 * sigma));
            double[,] GausMatrix = new double[2 * k + 1, 2 * k + 1];
            double d, stepen, e, pi, a;

            for (int hWinX = -k; hWinX <= k; hWinX++)
            {
                for (int hWinY = -k; hWinY <= k; hWinY++)
                {
                    d = Math.Sqrt(hWinX * hWinX + hWinY * hWinY);
                    stepen = -1.0 * ((d * d) / (2.0 * sigma * sigma));
                    e = Math.Pow(Math.E, stepen);
                    pi = Math.Sqrt(2.0 * Math.PI);
                    a = 1.0 / (pi * sigma);
                    GausMatrix[k + hWinY, k + hWinX] = a * e;
                }
            }

            return GausMatrix;
        }

        public byte[,] Downsample(byte[,] Img, int width, int height, out int widthNew, out int heightNew)
        {
            heightNew = height / 2;
            widthNew = width / 2;
            byte[,] Result = new byte[heightNew, widthNew];

            int x1 = 0; 
            int y1 = 0; 
            for (int y = 0; y < heightNew; y++)
            {
                x1 = 0;
                for (int x = 0; x < widthNew; x++)
                {
                    Result[y, x] = Img[y1, x1];
                    x1 += 2;
                    if (x1 >= width)
                    {
                        x1 = width - 1;
                    }
                }
                y1 += 2;
                if (y1 >= height)
                {
                    y1 = height - 1;
                }
            }

            return Result;
        }

        public void SaveGausCore(double[,] GausMatrix, int k, string path)
        {
            Bitmap bitmap = MatrixToImage(GausMatrix, k * 2 + 1, k * 2 + 1);
            //DrawImage(bitmap, pictureBox);
            bitmap.Save(path);
        }

        public double[,] MoravekS(byte[,] GrayMatrix, int width, int height, int k/*Радиус окна*/, int kraimode)
        {
            int k1 = k + 1;
            byte[,] GrayMatrixAdd = new byte[height + 2 * k1, width + 2 * k1];
            double[,] Result = new double[height, width];

            /* Краевые эффекты */
            // Копируем значение с края изображения
            if (kraimode == 1)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        GrayMatrixAdd[y + k1, x + k1] = GrayMatrix[y, x];
                    }
                }
                for (int y = k1; y < height + k1; y++)
                {
                    for (int x = 0; x < k1; x++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[y, k1];
                        GrayMatrixAdd[y, width + k1 + x] = GrayMatrixAdd[y, width + k1 - 1];
                    }
                }
                for (int x = 0; x < width + 2 * k1; x++)
                {
                    for (int y = 0; y < k1; y++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[k1, x];
                        GrayMatrixAdd[height + k1 + y, x] = GrayMatrixAdd[height + k1 - 1, x];
                    }
                }
            }
            // Всё снаружи чёрное
            else
            {
                for (int y = 0; y < height + 2 * k; y++)
                {
                    for (int x = 0; x < width + 2 * k; x++)
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
            /*-----------------*/

            for (int y = k1; y < height + k1; y++)
            {
                for (int x = k1; x < width + k1; x++)
                {
                    double S = 999999999;
                    for (int s = 1; s <= 8; s++)
                    {
                        int dx = 0;
                        int dy = 0;
                        if (s == 1)
                        {
                            dx = -1;
                            dy = -1;
                        }
                        else if (s == 2)
                        {
                            dx = -1;
                            dy = 0;
                        }
                        else if (s == 3)
                        {
                            dx = -1;
                            dy = 1;
                        }
                        else if (s == 4)
                        {
                            dx = 0;
                            dy = -1;
                        }
                        else if (s == 5)
                        {
                            dx = 0;
                            dy = 1;
                        }
                        else if (s == 6)
                        {
                            dx = 1;
                            dy = -1;
                        }
                        else if (s == 7)
                        {
                            dx = 1;
                            dy = 0;
                        }
                        else
                        {
                            dx = 1;
                            dy = 1;
                        }

                        double C = 0.0;
                        for (int hWinX = -k; hWinX <= k; hWinX++)
                        {
                            for (int hWinY = -k; hWinY <= k; hWinY++)
                            {
                                C += Math.Pow((double)GrayMatrixAdd[y + hWinY, x + hWinX] - (double)GrayMatrixAdd[y + hWinY + dx, x + hWinX + dy], 2);
                            }
                        }
                        if (C < S)
                        {
                            S = C;
                        }
                    }
                    Result[y - k1, x - k1] = S;
                }
            }

            return Result;
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

            DerivativeX = Svertka(GrayMatrix, IWidth, IHeight, maskX, k, 0);
            DerivativeXImage = MatrixToImage(DerivativeX, IWidth, IHeight);
            DrawImage(DerivativeXImage, pictureBox3);
            DerivativeXImage.Save("../../../files/Lab 1/DerivativeX.png");

            DerivativeY = Svertka(GrayMatrix, IWidth, IHeight, maskY, k, 0);
            DerivativeYImage = MatrixToImage(DerivativeY, IWidth, IHeight);
            DrawImage(DerivativeYImage, pictureBox4);
            DerivativeYImage.Save("../../../files/Lab 1/DerivativeY.png");

            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DrawImage(GrayImage, pictureBox6);

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

            SobelImage = MatrixToImage(Sobel, IWidth, IHeight);
            DrawImage(SobelImage, pictureBox5);
            SobelImage.Save("../../../files/Lab 1/Sobel.png");
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
                ClearDir("../../../files/Lab 2");
                ClearDir("../../../files/Lab 2/Core");

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

                Bitmap bitmapTEK;
                double sigmaD = sigma0;                        // Действительная сигма
                double sigmaTEK = sigma0;
                double qq = 1;
                byte[,] img = new byte[IHeight, IWidth];
                double[,] Dimg = new double[IHeight, IWidth];
                for (int y = 0; y < IHeight; y++)
                {
                    for (int x = 0; x < IWidth; x++)
                    {
                        img[y, x] = GrayMatrix[y, x];
                    }
                }

                int widthTEK = IWidth;
                int heightTEK = IHeight;
                bitmapTEK = MatrixToImage(img, widthTEK, heightTEK);
                bitmapTEK.Save("../../../files/Lab 2/000.png");
                double[,] GausMatrix;
                int n;

                GausMatrix = GausCore(sigma0, out n);
                Dimg = Svertka(img, widthTEK, heightTEK, GausMatrix, n, 1);
                img = NormalizeMatrix(Dimg, widthTEK, heightTEK);
                bitmapTEK = MatrixToImage(img, widthTEK, heightTEK);
                bitmapTEK.Save("../../../files/Lab 2/" + "00"
                    + " - S1=" + Convert.ToString(Math.Round(sigma0, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigma0, 2)) + ".png");
                SaveGausCore(GausMatrix, n, "../../../files/Lab 2/Core/" + "00" + ".png");

                for (int o = 0; o < O; o++)
                {
                    for (int s = 1; s <= S; s++)
                    {
                        sigmaTEK = sigma0 * Math.Pow(k, s);                       // Текущая сигма
                        sigmaD = sigma0 * Math.Pow(k, qq);
                        qq++;
                        GausMatrix = GausCore(sigmaTEK, out n);
                        Dimg = Svertka(img, widthTEK, heightTEK, GausMatrix, n, 1);
                        img = NormalizeMatrix(Dimg, widthTEK, heightTEK);
                        bitmapTEK = MatrixToImage(img, widthTEK, heightTEK);
                        bitmapTEK.Save("../../../files/Lab 2/" + Convert.ToString(o) + Convert.ToString(s)
                            + " - S1=" + Convert.ToString(Math.Round(sigmaTEK, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigmaD, 2)) + ".png");
                        SaveGausCore(GausMatrix, n, "../../../files/Lab 2/Core/" + Convert.ToString(o) + Convert.ToString(s) + ".png");
                    }
                    img = Downsample(img, widthTEK, heightTEK, out widthTEK, out heightTEK);
                    bitmapTEK = MatrixToImage(img, widthTEK, heightTEK);
                    bitmapTEK.Save("../../../files/Lab 2/"  + Convert.ToString(o + 1) + "0"
                            + " - S1=" + Convert.ToString(Math.Round(sigma0, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigmaD, 2)) + ".png");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DrawImage(GrayImage, pictureBox8);

            int radius = 3;
            int p = radius;
            double Imin = 0;
            double Imax = 1000;
            double T = (Imax - Imin) * 2 / 4;
            T = 100;
            double[,] S = MoravekS(GrayMatrix, IWidth, IHeight, radius, 1);
            S = NormalizeMatrix(S, IWidth, IHeight, Imin, Imax);

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
            bitmap = MatrixToImage(GrayMatrix, IWidth, IHeight);
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

            DrawImage(bitmap, pictureBox7);
            bitmap.Save("../../../files/Lab 3/Moravek.png");




        }
    }
}
