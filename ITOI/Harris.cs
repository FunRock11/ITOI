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
        public int WindowRadius;
        public int LocalRadius;
        public double R; // Отбираются точки, которые больше R * max
        public double[,] DerivativeX;
        public double[,] DerivativeY;
        public double[,] MinL;
        public double[,] MaxL;
        public bool[,] InterestingPoints;
        public GaussCore GaussMatrix;
        double[,] MTX;


        double MAXmin = -999999999;
        double MINmin = 999999999;
        
        public Harris(Img image, int windowradius, int localradius, double r)
        {
            Image = image;
            GaussMatrix = new GaussCore(windowradius, 1);
            MTX = F.Svertka(Image.GrayMatrix, Image.Width, Image.Height, GaussMatrix.Matrix, GaussMatrix.Radius, 1);

            WindowRadius = windowradius;
            LocalRadius = localradius;
            R = r;

            Derivative();
            Minl(1);
            IntPoints();
            PColor();
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

            DerivativeX = F.Svertka(MTX, Image.Width, Image.Height, maskX, k, 1);
            DerivativeY = F.Svertka(MTX, Image.Width, Image.Height, maskY, k, 1);
        }

        private void Minl(int kraimode)
        {
            MinL = new double[Image.Height, Image.Width];
            MaxL = new double[Image.Height, Image.Width];

            for (int y = WindowRadius; y < Image.Height - WindowRadius; y++)
            {
                for (int x = WindowRadius; x < Image.Width - WindowRadius; x++)
                {
                    double[,] H = new double[2, 2];
                    for (int hWinX = -WindowRadius; hWinX <= WindowRadius; hWinX++)
                    {
                        for (int hWinY = -WindowRadius; hWinY <= WindowRadius; hWinY++)
                        {
                            double tekgauss = GaussMatrix.Matrix[WindowRadius + hWinY, WindowRadius + hWinX];

                            H[0, 0] += Math.Pow(DerivativeX[y + hWinY, x + hWinX], 2) * tekgauss;
                            H[0, 1] += DerivativeX[y + hWinY, x + hWinX] * DerivativeY[y + hWinY, x + hWinX] * tekgauss;
                            H[1, 0] += DerivativeX[y + hWinY, x + hWinX] * DerivativeY[y + hWinY, x + hWinX] * tekgauss;
                            H[1, 1] += Math.Pow(DerivativeY[y + hWinY, x + hWinX], 2) * tekgauss;
                        }
                    }

                    double b = -1 * H[0, 0] - 1 * H[1, 1];
                    double c = H[0, 0] * H[1, 1] - H[0, 1] * H[1, 0];
                    double D = Math.Pow(b, 2) - 4 * c;
                    if (D > 0)
                    {
                        double S1 = (-b + Math.Sqrt(D)) / 2;
                        double S2 = (-b - Math.Sqrt(D)) / 2;
                        if (S1 > S2)
                        {
                            MinL[y, x] = S2;
                            MaxL[y, x] = S1;
                        }
                        else
                        {
                            MinL[y, x] = S1;
                            MaxL[y, x] = S2;
                        }
                        if (MinL[y, x] > MAXmin)
                        {
                            MAXmin = MinL[y, x];
                        }
                        if (MinL[y, x] < MINmin)
                        {
                            MINmin = MinL[y, x];
                        }
                    }
                    else
                    {
                        MinL[y, x] = -1;
                        MaxL[y, x] = -1;
                    }

                }
            }
            for (int y = WindowRadius; y < Image.Height - WindowRadius; y++)
            {
                for (int x = WindowRadius; x < Image.Width - WindowRadius; x++)
                {
                    if (MinL[y, x] == -1 || MaxL[y, x] == -1)
                    {
                        MinL[y, x] = MINmin;
                        MaxL[y, x] = MINmin;
                    }
                }
            }
            for (int y = 0; y < WindowRadius; y++)
            {
                for (int x = 0; x < WindowRadius; x++)
                {
                    MinL[y, x] = MINmin;
                    MaxL[y, x] = MINmin;
                }
            }
            for (int y = Image.Height - WindowRadius; y < Image.Height; y++)
            {
                for (int x = Image.Width - WindowRadius; x < Image.Width; x++)
                {
                    MinL[y, x] = MINmin;
                    MaxL[y, x] = MINmin;
                }
            }
        }

        private void IntPoints()
        {
            InterestingPoints = new bool[Image.Height, Image.Width];

            double T = (MAXmin - MINmin) * R;
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    /*
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
                    */
                    if (/*MinL[y, x] > vMax &&*/ MinL[y, x] > T)
                    {
                        InterestingPoints[y, x] = true;
                    }
                    else
                    {
                        InterestingPoints[y, x] = false;
                    }
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
