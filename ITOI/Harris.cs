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
        public Img ImageWithANMS;
        public int WindowRadius;
        public double R; // Отбираются точки, которые больше R * max
        public double[,] DerivativeX;
        public double[,] DerivativeY;
        public double[,] MinL;
        public double[,] MaxL;
        public bool[,] InterestingPoints;
        public bool[,] InterestingPointsANMS;
        public GaussCore GaussMatrix;
        private double[,] MTX;
        public int NPoints;
        public int NewPoints;
        public int NeedPoints;

        private double MAXmin = -999999999;
        private double MINmin = 999999999;
        
        public Harris(Img image, int windowradius, double r)
        {
            Image = image;
            WindowRadius = windowradius;
            R = r;

            GaussMatrix = new GaussCore(windowradius, 1);
            MTX = F.Svertka(Image.GrayMatrix, Image.Width, Image.Height, GaussMatrix.Matrix, GaussMatrix.Radius, 1);

            Derivative();
            Minl();
            IntPoints1();
            PColor(1);
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

        private void Minl()
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

        /*private void IntPoints()
        {
            InterestingPoints = new bool[Image.Height, Image.Width];

            double T = (MAXmin - MINmin) * R;
            for (int y = WindowRadius; y < Image.Height - WindowRadius; y++)
            {
                for (int x = WindowRadius; x < Image.Width - WindowRadius; x++)
                {
                    if (MinL[y, x] > T)
                    {
                        double vMax = -999999999;
                        for (int hWinX = -WindowRadius * 2; hWinX <= WindowRadius * 2; hWinX++)
                        {
                            for (int hWinY = -WindowRadius * 2; hWinY <= WindowRadius * 2; hWinY++)
                            {
                                if (x + hWinX < Image.Width && x + hWinX >= 0
                                    && y + hWinY < Image.Height && y + hWinY >= 0)
                                {
                                    if (hWinX == 0 && hWinY == 0)
                                    {
                                        continue;
                                    }
                                    else if (MinL[y + hWinY, x + hWinX] > vMax)
                                    {
                                        vMax = MinL[y + hWinY, x + hWinX];
                                    }
                                }
                            }
                        }
                        if (MinL[y, x] > vMax)
                        {
                            InterestingPoints[y, x] = true;
                        }
                        else
                        {
                            InterestingPoints[y, x] = false;
                        }
                    }
                    else
                    {
                        InterestingPoints[y, x] = false;
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
                    if (MinL[y, x] >= T)
                    {
                        InterestingPoints[y, x] = true;
                    }
                    else
                    {
                        InterestingPoints[y, x] = false;
                    }
                }
            }
            
            for (int y = 0; y < Image.Height - 0; y++)
            {
                for (int x = 0; x < Image.Width - 0; x++)
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
                                    else if (MinL[y + hWinY, x + hWinX] <= MinL[y,x])
                                    {
                                        InterestingPoints[y + hWinY, x + hWinX] = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            NPoints = 0;
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    if (InterestingPoints[y, x])
                    {
                        NPoints++;
                    }
                }
            }

        }

        private void PColor(int r)
        {
            ImageWithPoints = new Img(Image.GrayMatrix, Image.Width, Image.Height);
            Color color;
            for (int y = WindowRadius; y < ImageWithPoints.Height - WindowRadius; y++)
            {
                for (int x = WindowRadius; x < ImageWithPoints.Width - WindowRadius; x++)
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

        public void ANMS(int needpoints)
        {
            NeedPoints = needpoints;
            RANMS();
            IWANMS(1);
        }

        private void RANMS()
        {
            NewPoints = NPoints;
            InterestingPointsANMS = new bool[Image.Height, Image.Width];
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    InterestingPointsANMS[y, x] = InterestingPoints[y, x];
                }
            }
            if (NPoints > NeedPoints)
            {
                int r = WindowRadius + 1;
                while (NeedPoints < NewPoints)
                {
                    for (int y = 0; y < Image.Height; y++)
                    {
                        for (int x = 0; x < Image.Width; x++)
                        {
                            if (InterestingPointsANMS[y, x])
                            {
                                for (int hWinX = -r; hWinX <= r; hWinX++)
                                {
                                    for (int hWinY = -r; hWinY <= r; hWinY++)
                                    {
                                        if (x + hWinX < Image.Width && x + hWinX >= 0
                                            && y + hWinY < Image.Height && y + hWinY >= 0)
                                        {
                                            if (InterestingPointsANMS[y + hWinY, x + hWinX])
                                            {
                                                if (hWinX == 0 && hWinY == 0)
                                                {
                                                    continue;
                                                }
                                                else if (MinL[y + hWinY, x + hWinX] <= MinL[y, x])
                                                {
                                                    InterestingPointsANMS[y + hWinY, x + hWinX] = false;
                                                    NewPoints--;
                                                    if (NeedPoints == NewPoints)
                                                    {
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    r++;
                }

            }
        }

        private void IWANMS(int r)
        {
            ImageWithANMS = new Img(Image.GrayMatrix, Image.Width, Image.Height);
            Color color;
            for (int y = 0; y < ImageWithANMS.Height; y++)
            {
                for (int x = 0; x < ImageWithANMS.Width; x++)
                {
                    if (InterestingPointsANMS[y, x])
                    {
                        for (int hWinX = -r; hWinX <= r; hWinX++)
                        {
                            for (int hWinY = -r; hWinY <= r; hWinY++)
                            {
                                if (x + hWinX < ImageWithANMS.Width && x + hWinX >= 0
                                    && y + hWinY < ImageWithANMS.Height && y + hWinY >= 0)
                                {
                                    color = Color.FromArgb(255, 255, 0, 0);
                                    ImageWithANMS.Bitmap.SetPixel(x + hWinX, y + hWinY, color);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
