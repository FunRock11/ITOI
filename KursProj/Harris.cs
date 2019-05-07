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
        public Img ImageWithANMS;
        public Img ImageWithMS;
        public int WindowRadius;
        public double T;
        public double[,] DerivativeX;
        public double[,] DerivativeY;
        public double[,] MinL;
        public double[,] MaxL;
        List<InterestingPoint> InterestingPoints;
        List<InterestingPoint> InterestingPointsANMS;
        List<InterestingPoint> InterestingPointsMS;
        public GaussCore GaussMatrix;
        private double[,] MTX;
        public int NPoints;
        public int NewPoints;
        public int NeedPoints;

        public string Path;

        private double MAXmin = -999999999;
        private double MINmin = 999999999;

        public double[,] Theta;
        public double[,] Sobel;

        public int P;

        public Harris(Img image, int windowradius, double r, int size)
        {
            Image = new Img(image.Bitmap);
            Path = Image.Path;
            WindowRadius = windowradius;
            T = r;

            NormalizeSize(size);

            MTX = new double[Image.Height, Image.Width];
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    MTX[y, x] = Image.GrayMatrixDouble[y, x];
                }
            }
            Rotate();

            GaussMatrix = new GaussCore(windowradius, 1);
            MTX = F.Svertka(Image.GrayMatrixDouble, Image.Width, Image.Height, GaussMatrix.Matrix, GaussMatrix.Radius, 1);

            
            Derivative();
            Minl();
            IntPoints();
            PColor(1);
        }

        private void NormalizeSize(int size)
        {
            int SizeMin = Math.Min(Image.Height, Image.Width);
            Bitmap bitmap = new Bitmap(Image.Bitmap, new Size(size, size));
            Image = new Img(bitmap);
        }

        private void SobelAndTheta()
        {
            Derivative();
            double dx = 0.0;
            double dy = 0.0;
            Theta = new double[Image.Height, Image.Width];
            Sobel = new double[Image.Height, Image.Width];

            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    Theta[y, x] = Math.Atan2(DerivativeX[y, x], DerivativeY[y, x]) + Math.PI;

                    dx = Math.Pow(DerivativeX[y, x], 2);
                    dy = Math.Pow(DerivativeY[y, x], 2);
                    Sobel[y, x] = Math.Sqrt(dx + dy);
                }
            }
        }

        private void Rotate()
        {
            SobelAndTheta();

            double[,] korzinaO = new double[36, 2];
            for (int i = 0; i < 36; i++)
            {
                korzinaO[i, 0] = (Math.PI / 18) * i;
                korzinaO[i, 1] = (Math.PI / 18) * (i + 1);
            }

            double[] D = new double[36];
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    double L = Sobel[y, x];
                    double Fi = Theta[y, x];
                    if (Fi == 0)
                    {
                        Fi = Math.PI * 2;
                    }
                    int korzina1 = -10, korzina2 = -10; // смежные корзины
                    double c1 = 0, c2 = 0; // коэф-ты для корзин
                    for (int i = 0; i < 36; i++)
                    {
                        if (Fi == korzinaO[i, 1])
                        {
                            korzina1 = i;
                            korzina2 = i + 1;
                            if (korzina2 == 36)
                            {
                                korzina2 = 0;
                            }
                            c1 = 0.5;
                            c2 = 0.5;
                        }
                    }
                    if (korzina1 == -10 || korzina2 == -10)
                    {
                        for (int i = 0; i < 36; i++)
                        {
                            if (Fi > korzinaO[i, 0] && Fi < korzinaO[i, 1])
                            {
                                korzina1 = i;
                                double a1 = korzinaO[i, 1] - Fi;
                                double a0 = Fi - korzinaO[i, 0];
                                if (a0 > a1)
                                {
                                    korzina2 = i + 1;
                                    if (korzina2 == 36)
                                    {
                                        korzina2 = 0;
                                    }

                                    double d = Math.PI / 18;
                                    double b = a1 + (Math.PI / 36);
                                    c1 = b / d;
                                    c2 = 1 - c1;
                                }
                                else if (a1 > a0)
                                {
                                    korzina2 = i - 1;
                                    if (korzina2 == -1)
                                    {
                                        korzina2 = 35;
                                    }

                                    double d = Math.PI / 18;
                                    double b = a0 + (Math.PI / 36);
                                    c1 = b / d;
                                    c2 = 1 - c1;
                                }
                                else
                                {
                                    korzina2 = 0;
                                    c1 = 1;
                                    c2 = 0;
                                }
                            }
                        }
                    }
                    D[korzina1] += L * c1;
                    D[korzina2] += L * c2;
                }
            }

            double GMaxVal = -999999999;
            int GMax = 0;

            for (int i = 0; i < 36; i++)
            {
                if (D[i] > GMaxVal)
                {
                    GMaxVal = D[i];
                    GMax = i;
                }
            }

            double alpha = (korzinaO[GMax, 1] - korzinaO[GMax, 0]) / 2 + korzinaO[GMax, 0];
            float ugol = (float)(alpha * 180 / Math.PI);
            Bitmap bitmap = F.RotateImage(Image.Bitmap, ugol);
            Image = new Img(bitmap);

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




        public void ANMS(int needpoints)
        {
            NeedPoints = needpoints;
            RANMS();
            IWANMS(1);
        }

        private void RANMS()
        {
            NewPoints = NPoints;
            InterestingPointsANMS = new List<InterestingPoint>();
            foreach (InterestingPoint ip in InterestingPoints)
            {
                InterestingPointsANMS.Add(ip);
            }

            if (NPoints > NeedPoints)
            {
                int r = WindowRadius + 1;
                while (NeedPoints < NewPoints)
                {
                    bool[] ToDelete = new bool[InterestingPointsANMS.Count];
                    for (int i = 0; i < InterestingPointsANMS.Count; i++)
                    {
                        ToDelete[i] = false;
                    }
                    int i1 = 0;
                    foreach (InterestingPoint ip in InterestingPointsANMS)
                    {
                        if (!ToDelete[i1])
                        {
                            int i2 = 0;
                            foreach (InterestingPoint ip1 in InterestingPointsANMS)
                            {
                                if (!ToDelete[i2])
                                {
                                    if (Math.Abs(ip.X - ip1.X) <= r && Math.Abs(ip.Y - ip1.Y) <= r)
                                    {
                                        if (Math.Abs(ip.X - ip1.X) == 0 && Math.Abs(ip.Y - ip1.Y) == 0)
                                        {
                                            continue;
                                        }
                                        else if (ip1.Value <= ip.Value)
                                        {
                                            ToDelete[i2] = true;
                                            NewPoints--;
                                            if (NeedPoints == NewPoints)
                                            {
                                                for (int i = InterestingPointsANMS.Count - 1; i >= 0; i--)
                                                {
                                                    if (ToDelete[i])
                                                    {
                                                        InterestingPointsANMS.RemoveAt(i);
                                                    }
                                                }
                                                return;
                                            }
                                        }
                                    }
                                }
                                i2++;
                            }
                        }
                        i1++;
                    }

                    for (int i = InterestingPointsANMS.Count - 1; i >= 0; i--)
                    {
                        if (ToDelete[i])
                        {
                            InterestingPointsANMS.RemoveAt(i);
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

            foreach (InterestingPoint ip in InterestingPointsANMS)
            {
                for (int hWinX = -r; hWinX <= r; hWinX++)
                {
                    for (int hWinY = -r; hWinY <= r; hWinY++)
                    {
                        if (ip.X + hWinX < ImageWithANMS.Width && ip.X + hWinX >= 0
                            && ip.Y + hWinY < ImageWithANMS.Height && ip.Y + hWinY >= 0)
                        {
                            color = Color.FromArgb(255, 255, 0, 0);
                            ImageWithANMS.Bitmap.SetPixel(ip.X + hWinX, ip.Y + hWinY, color);
                        }
                    }
                }
            }

        }

        public void PointComparisonANMS(Harris harris, int E)
        {
            int P = 0;

            foreach (InterestingPoint ip in InterestingPointsANMS)
            {
                foreach (InterestingPoint ip1 in harris.InterestingPointsANMS)
                {
                    if (Math.Abs(ip.X - ip1.X) <= E && Math.Abs(ip.Y - ip1.Y) <= E)
                    {
                        P++;
                        break;
                    }
                }
            }

            this.P = P;
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
                    foreach (InterestingPoint ip in InterestingPointsMS)
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

        public void PointComparisonMS(Harris harris, int E)
        {
            int P = 0;

            foreach (InterestingPoint ip in InterestingPointsMS)
            {
                foreach (InterestingPoint ip1 in harris.InterestingPointsMS)
                {
                    if (Math.Abs(ip.X - ip1.X) <= E && Math.Abs(ip.Y - ip1.Y) <= E)
                    {
                        P++;
                        break;
                    }
                }
            }

            this.P = P;
        }

    }
}
