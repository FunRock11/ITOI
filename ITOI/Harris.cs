using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ITOI
{
    class Harris
    {
        Func F = new Func();
        public Img Image;
        public Img ImageWithPoints;
        public Img ColorImage;
        public int WindowRadius;
        public int LocalRadius;
        public double R; // Отбираются точки, которые больше R * max
        public double[,] DerivativeX;
        public double[,] DerivativeY;
        public int UV;
        public double[,] MinL;
        public bool[,] InterestingPoints;

        double MAXmin = -999999999;
        double MINmin = 999999999;

        public Harris(Img image, int windowradius, int localradius, int uv, double r)
        {
            Image = new Img(image.GrayMatrix, image.Width, image.Height);
            /*
            GaussCore GaussMatrix = new GaussCore(1);
            Image.SvertkaWithNormalize(GaussMatrix.Matrix, GaussMatrix.Radius, 1);
            */
            WindowRadius = windowradius;
            LocalRadius = localradius;
            UV = uv;
            R = r;

            Derivative();
            Minl(1);
            IntPoints();
            DColor();
            PColor();
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

        public void DrawColorImage(PictureBox pictureBox)
        {
            pictureBox.Height = ColorImage.Height;
            pictureBox.Width = ColorImage.Width;
            pictureBox.Image = ColorImage.Bitmap;
        }

        public void SaveColorImage(string path)
        {
            ColorImage.Save(path);
        }

        private void Derivative()
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

            DerivativeX = F.Svertka(Image.GrayMatrix, Image.Width, Image.Height, maskX, k, 0);
            DerivativeY = F.Svertka(Image.GrayMatrix, Image.Width, Image.Height, maskY, k, 0);
        }

        private void Minl(int kraimode)
        {
            MinL = new double[Image.Height, Image.Width];

            for (int y = WindowRadius; y < Image.Height - WindowRadius; y++)
            {
                for (int x = WindowRadius; x < Image.Width - WindowRadius; x++)
                {
                    double A = 0.0;
                    double B = 0.0;
                    double C = 0.0;
                    for (int hWinX = -WindowRadius; hWinX <= WindowRadius; hWinX++)
                    {
                        for (int hWinY = -WindowRadius; hWinY <= WindowRadius; hWinY++)
                        {
                            A += Math.Pow(DerivativeX[y + hWinY, x + hWinX], 2);
                            B += DerivativeX[y + hWinY, x + hWinX] * DerivativeY[y + hWinY, x + hWinX];
                            C += Math.Pow(DerivativeY[y + hWinY, x + hWinX], 2);
                        }
                    }
                    double E;
                    double Lmin = 999999999;
                    for (int v = -UV; v < UV; v++)
                    {
                        for (int u = -UV; u < UV; u++)
                        {
                            if (x + u < Image.Width && x + u >= 0
                                && y + v < Image.Height && y + v >= 0
                                && u != 0 && v != 0)
                            {
                                E = A * Math.Pow(u, 2) + 2 * B * u * v + C * Math.Pow(v, 2);
                                if (E < Lmin)
                                {
                                    Lmin = E;
                                }
                            }
                        }
                    }
                    MinL[y, x] = Lmin;
                    if (MinL[y, x] > MAXmin)
                    {
                        MAXmin = MinL[y, x];
                    }
                    if (MinL[y, x] < MINmin)
                    {
                        MINmin = MinL[y, x];
                    }
                }
            }
        }

        private void IntPoints()
        {
            InterestingPoints = new bool[Image.Height, Image.Width];

            double T = MAXmin * R;
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    double vMax = -999999999;
                    for (int hWinX = -LocalRadius; hWinX <= LocalRadius; hWinX++)
                    {
                        for (int hWinY = -LocalRadius; hWinY <= LocalRadius; hWinY++)
                        {
                            if (x + hWinX < Image.Width && x + hWinX >= 0
                                && y + hWinY < Image.Height && y + hWinY >= 0
                                && hWinX != 0 && hWinY != 0)
                            {
                                if (MinL[y + hWinY, x + hWinX] > vMax)
                                {
                                    vMax = MinL[y + hWinY, x + hWinX];
                                }
                            }
                        }
                    }
                    if (MinL[y, x] > vMax && MinL[y, x] > T)
                    {
                        InterestingPoints[y, x] = true;
                    }
                    else
                    {
                        InterestingPoints[y, x] = false;
                    }
                }
            }
            for (int y = 0; y < WindowRadius; y++)
            {
                for (int x = 0; x < WindowRadius; x++)
                {
                    InterestingPoints[y, x] = false;
                }
            }
            for (int y = Image.Height - WindowRadius; y < Image.Height; y++)
            {
                for (int x = Image.Width - WindowRadius; x < Image.Width; x++)
                {
                    InterestingPoints[y, x] = false;
                }
            }
        }

        private void DColor()
        {
            double T = MAXmin * R;
            ColorImage = new Img(Image.GrayMatrix, Image.Width, Image.Height);
            Color color;
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    if (MinL[y, x] >= MINmin && MinL[y,x] < T)
                    {
                        color = Color.FromArgb(255, 0, 0, 255);
                    }
                    else if (MinL[y, x] >= T && MinL[y, x] < T + (1 - R) * 1 / 3)
                    {
                        color = Color.FromArgb(255, 0, 255, 0);
                    }
                    else if (MinL[y, x] >= T + (1 - R) * 1 / 3 && MinL[y, x] < T + (1 - R) * 2 / 3)
                    {
                        color = Color.FromArgb(255, 255, 255, 0);
                    }
                    else
                    {
                        color = Color.FromArgb(255, 255, 0, 0);
                    }
                    ColorImage.Bitmap.SetPixel(x, y, color);
                }
            }
        }

        private void PColor()
        {
            int r = 1;
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
