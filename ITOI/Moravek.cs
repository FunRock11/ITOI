using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ITOI
{
    class Moravek
    {
        public Img Image;
        public Img ImageWithPoints;
        public int WindowRadius;
        public int[,] S;
        public bool[,] InterestingPoints;
        public double R; // Отбираются точки, которые больше R * max



        public Moravek(Img image, int windowradius, double r)
        {
            Image = image;
            WindowRadius = windowradius;
            R = r;
            MoravekS(WindowRadius, 1);
            IntPoints1();
            IWP(1);
        }

        public void DrawImage(PictureBox pictureBox)
        {
            pictureBox.Height = Image.Height;
            pictureBox.Width = Image.Width;
            pictureBox.Image = Image.Bitmap;
        }

        public void SaveImage(string path)
        {
            Image.Save(path);
        }

        public void DrawImageWithPoints(PictureBox pictureBox)
        {
            pictureBox.Height = ImageWithPoints.Height;
            pictureBox.Width = ImageWithPoints.Width;
            pictureBox.Image = ImageWithPoints.Bitmap;
        }

        public void SaveImageWithPoints(string path)
        {
            ImageWithPoints.Save(path);
        }

        private void MoravekS(int k/*Радиус окна*/, int kraimode)
        {
            int k1 = k + 1;
            byte[,] GrayMatrixAdd = new byte[Image.Height + 2 * k1, Image.Width + 2 * k1];
            int[,] Result = new int[Image.Height, Image.Width];

            /* Краевые эффекты */
            // Копируем значение с края изображения
            if (kraimode == 1)
            {
                for (int y = 0; y < Image.Height; y++)
                {
                    for (int x = 0; x < Image.Width; x++)
                    {
                        GrayMatrixAdd[y + k1, x + k1] = Image.GrayMatrix[y, x];
                    }
                }
                for (int y = k1; y < Image.Height + k1; y++)
                {
                    for (int x = 0; x < k1; x++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[y, k1];
                        GrayMatrixAdd[y, Image.Width + k1 + x] = GrayMatrixAdd[y, Image.Width + k1 - 1];
                    }
                }
                for (int x = 0; x < Image.Width + 2 * k1; x++)
                {
                    for (int y = 0; y < k1; y++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[k1, x];
                        GrayMatrixAdd[Image.Height + k1 + y, x] = GrayMatrixAdd[Image.Height + k1 - 1, x];
                    }
                }
            }
            // Всё снаружи чёрное
            else
            {
                for (int y = 0; y < Image.Height + 2 * k; y++)
                {
                    for (int x = 0; x < Image.Width + 2 * k; x++)
                    {
                        if (x < k || y < k || y >= Image.Height + k || x >= Image.Width + k)
                        {
                            GrayMatrixAdd[y, x] = 0;
                        }
                        else
                        {
                            GrayMatrixAdd[y, x] = Image.GrayMatrix[y - k, x - k];
                        }
                    }
                }
            }
            /*-----------------*/

            for (int y = k1; y < Image.Height + k1; y++)
            {
                for (int x = k1; x < Image.Width + k1; x++)
                {
                    int Q = 999999999;
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

                        int C = 0;
                        for (int hWinX = -k; hWinX <= k; hWinX++)
                        {
                            for (int hWinY = -k; hWinY <= k; hWinY++)
                            {
                                C += Convert.ToInt32(Math.Pow(GrayMatrixAdd[y + hWinY, x + hWinX] - GrayMatrixAdd[y + hWinY + dx, x + hWinX + dy], 2));
                            }
                        }
                        if (C < Q)
                        {
                            Q = C;
                        }
                    }
                    Result[y - k1, x - k1] = Q;
                }
            }
            S = Result;
            WindowRadius = k;
        }

        /*private void IntPoints()
        {
            InterestingPoints = new bool[Image.Height, Image.Width];
            int[,] SAdd = new int[Image.Height + 2 * LocalRadius, Image.Width + 2 * LocalRadius];

            for (int y = 0; y < Image.Height + 2 * LocalRadius; y++)
            {
                for (int x = 0; x < Image.Width + 2 * LocalRadius; x++)
                {
                    if (x < LocalRadius || y < LocalRadius || y >= Image.Height + LocalRadius || x >= Image.Width + LocalRadius)
                    {
                        SAdd[y, x] = 0;
                    }
                    else
                    {
                        SAdd[y, x] = S[y - LocalRadius, x - LocalRadius];
                    }
                }
            }

            int sMax = -999999999;
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    if (S[y, x] > sMax)
                    {
                        sMax = S[y, x];
                    }
                }
            }

            for (int y = LocalRadius; y < Image.Height + LocalRadius; y++)
            {
                for (int x = LocalRadius; x < Image.Width + LocalRadius; x++)
                {
                    double vMax = -999999999;
                    for (int hWinX = -LocalRadius; hWinX <= LocalRadius; hWinX++)
                    {
                        for (int hWinY = -LocalRadius; hWinY <= LocalRadius; hWinY++)
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
                    if (SAdd[y, x] > vMax && SAdd[y, x] > sMax * R)
                    {
                        InterestingPoints[y - LocalRadius, x - LocalRadius] = true;
                    }
                    else
                    {
                        InterestingPoints[y - LocalRadius, x - LocalRadius] = false;
                    }
                }
            }
        }*/

        private void IntPoints1()
        {
            InterestingPoints = new bool[Image.Height, Image.Width];
            double T = (MAXmin - MINmin) * R;

            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
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

            for (int y = WindowRadius; y < Image.Height - WindowRadius; y++)
            {
                for (int x = WindowRadius; x < Image.Width - WindowRadius; x++)
                {
                    if (InterestingPoints[y, x])
                    {
                        for (int hWinX = -WindowRadius; hWinX <= WindowRadius; hWinX++)
                        {
                            for (int hWinY = -WindowRadius; hWinY <= WindowRadius; hWinY++)
                            {
                                if (x + hWinX < Image.Width && x + hWinX >= 0
                                    && y + hWinY < Image.Height && y + hWinY >= 0)
                                {
                                    if (hWinX == 0 && hWinY == 0)
                                    {
                                        continue;
                                    }
                                    else if (MinL[y + hWinY, x + hWinX] < MinL[y, x])
                                    {
                                        InterestingPoints[y + hWinY, x + hWinX] = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        private void IWP(int r)
        {
            ImageWithPoints = new Img(Image.GrayMatrix, Image.Width, Image.Height);
            Color color;
            for (int y = 0; y < ImageWithPoints.Height; y++)
            {
                for (int x = 0; x < ImageWithPoints.Width; x++)
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
        }

    }
}
