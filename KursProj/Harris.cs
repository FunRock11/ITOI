using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KursProj
{
    class Harris
    {
        Func F = new Func();
        public Img Image;
        public Img ImageWithPoints;
        public Img ImageWithMS;
        public int WindowRadius;
        public double T;
        public double[,] DerivativeX;
        public double[,] DerivativeY;
        public double[,] MinL;
        public double[,] MaxL;
        List<InterestingPoint> InterestingPoints;
        List<InterestingPoint> InterestingPointsMS;
        public GaussCore GaussMatrix;
        private double[,] MTX;
        public int NPoints;
        public int NewPoints;
        public int NeedPoints;

        public int[] Hash;

        private double MAXmin = -999999999;
        private double MINmin = 999999999;

        public Harris(Img image, int windowradius, double r)
        {
            Image = image;
            WindowRadius = windowradius;
            T = r;

            GaussMatrix = new GaussCore(windowradius, 1);
            MTX = F.Svertka(Image.GrayMatrixDouble, Image.Width, Image.Height, GaussMatrix.Matrix, GaussMatrix.Radius, 1);

            Derivative();
            Minl();
            IntPoints();
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

        private void IntPoints()
        {
            InterestingPoints = new List<InterestingPoint>();
            List<InterestingPoint> InterestingPointsAdd = new List<InterestingPoint>();

            for (int y = WindowRadius; y < Image.Height - WindowRadius; y++)
            {
                for (int x = WindowRadius; x < Image.Width - WindowRadius; x++)
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
                                else if (MinL[y + hWinY, x + hWinX] > MinL[y,x])
                                {
                                    goto label1;
                                }
                                else if (MinL[y + hWinY, x + hWinX] == MinL[y, x])
                                {
                                    foreach(InterestingPoint ip in InterestingPointsAdd)
                                    {
                                        if (ip.X == x + hWinX && ip.Y == y + hWinY)
                                        {
                                            goto label1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    InterestingPointsAdd.Add(new InterestingPoint(x, y, MinL[y, x]));

                    label1:;
                }
            }

            foreach (InterestingPoint ip in InterestingPointsAdd)
            {
                if (ip.Value >= T)
                {
                    InterestingPoints.Add(ip);
                }
            }

            NPoints = InterestingPoints.Count;

        }

        private void PColor(int r)
        {
            ImageWithPoints = new Img(Image.GrayMatrix, Image.Width, Image.Height);
            Color color;
            foreach (InterestingPoint ip in InterestingPoints)
            {
                for (int hWinX = -r; hWinX <= r; hWinX++)
                {
                    for (int hWinY = -r; hWinY <= r; hWinY++)
                    {
                        if (ip.X + hWinX < ImageWithPoints.Width && ip.X + hWinX >= 0
                            && ip.Y + hWinY < ImageWithPoints.Height && ip.Y + hWinY >= 0)
                        {
                            color = Color.FromArgb(255, 255, 0, 0);
                            ImageWithPoints.Bitmap.SetPixel(ip.X + hWinX, ip.Y + hWinY, color);
                        }
                    }
                }
            }
        }

        public void MS(int needpoints)
        {
            NeedPoints = needpoints;
            RMS();
            IWMS(1);
        }

        private void RMS()
        {
            NewPoints = NPoints;
            InterestingPointsMS = new List<InterestingPoint>();
            foreach (InterestingPoint ip in InterestingPoints)
            {
                InterestingPointsMS.Add(ip);
            }

            if (NPoints > NeedPoints)
            {
                while (NeedPoints < NewPoints)
                {
                    double vMin = 999999999;
                    int iiMin = 0;
                    int ii = 0;
                    foreach(InterestingPoint ip in InterestingPointsMS)
                    {
                        if (ip.Value < vMin)
                        {
                            vMin = ip.Value;
                            iiMin = ii;
                        }
                        ii++;
                    }
                    InterestingPointsMS.RemoveAt(iiMin);
                    NewPoints--;
                }

            }
        }

        private void IWMS(int r)
        {
            ImageWithMS = new Img(Image.GrayMatrix, Image.Width, Image.Height);
            Color color;

            foreach (InterestingPoint ip in InterestingPointsMS)
            {
                for (int hWinX = -r; hWinX <= r; hWinX++)
                {
                    for (int hWinY = -r; hWinY <= r; hWinY++)
                    {
                        if (ip.X + hWinX < ImageWithMS.Width && ip.X + hWinX >= 0
                            && ip.Y + hWinY < ImageWithMS.Height && ip.Y + hWinY >= 0)
                        {
                            color = Color.FromArgb(255, 255, 0, 0);
                            ImageWithMS.Bitmap.SetPixel(ip.X + hWinX, ip.Y + hWinY, color);
                        }
                    }
                }
            }

        }
        
        public void Hashed()
        {
            int NumLines = NewPoints * (NewPoints - 1) / 2;
            Hash = new int[NumLines];

            int k = 0;
            for (int i = 0; i < InterestingPointsMS.Count; i++)
            {
                for (int y = i + 1; y < InterestingPointsMS.Count; y++)
                {
                    int rast = Convert.ToInt32(Math.Round(Math.Sqrt(Math.Pow((InterestingPointsMS[i].X - InterestingPointsMS[y].X), 2) + Math.Pow((InterestingPointsMS[i].Y - InterestingPointsMS[y].Y), 2))));
                    Hash[k] = rast;
                    k++;
                }
            }

            Array.Sort(Hash);
            Array.Reverse(Hash);
        }

        public int HemmingRast(Harris harris, int E)
        {
            int P = 0;
            int MaxVal = Math.Max(harris.Hash[0], Hash[0]);
            int LengthMaxVal = MaxVal.ToString().Length;
            string TempString = "";
            for (int i = 0; i < LengthMaxVal; i++)
            {
                TempString += "9";
            }
            int M = Convert.ToInt32(TempString);
            double Ka = (double)M / Hash[0];
            double Kb = (double)M / harris.Hash[0];

            int NumLines = NewPoints * (NewPoints - 1) / 2;
            int[] HashTemp1 = new int[NumLines];
            int[] HashTemp2 = new int[NumLines];
            for (int i = 0; i < NumLines; i++)
            {
                HashTemp1[i] = Convert.ToInt32(Math.Round(Hash[i] * Ka));
                HashTemp2[i] = Convert.ToInt32(Math.Round(harris.Hash[i] * Kb));
            }

            for (int i = 0; i < NumLines; i++)
            {
                int r = Math.Abs(HashTemp1[i] - HashTemp2[i]);
                if (r > E)
                {
                    P++;
                }
            }

            return P;
        }
    }
}
