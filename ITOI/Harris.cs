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
            MTX = F.Svertka(Image.GrayMatrixDouble, Image.Width, Image.Height, GaussMatrix.Matrix, GaussMatrix.Radius, 1);

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

        private void IntPoints1()
        {
            InterestingPoints = new bool[Image.Height, Image.Width];
            double T = R;

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
            
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
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
                                    else if (MinL[y + hWinY, x + hWinX] >= MinL[y,x])
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



        /*Дескриптор*/
        private double[,] Theta;
        private double[,] Sobel;
        private bool[,] InterestingPointsAdd;
        private double[,] ThetaAdd;
        private double[,] SobelAdd;

        public int[,] IntPointsCoord;
        public double[,] Descriptors;
        public double[,] Descriptors2;

        private void SobelAndTheta()
        {
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

        private void MtxAdd(int r, out int NewHeight, out int NewWidth)
        {
            NewHeight = Image.Height + r * 2;
            NewWidth = Image.Width + r * 2;
            InterestingPointsAdd = new bool[NewHeight, NewWidth];
            ThetaAdd = new double[Image.Height + r * 2, Image.Width + r * 2];
            SobelAdd = new double[Image.Height + r * 2, Image.Width + r * 2];

            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    InterestingPointsAdd[y + r, x + r] = InterestingPointsANMS[y, x];
                    ThetaAdd[y + r, x + r] = Theta[y, x];
                    SobelAdd[y + r, x + r] = Sobel[y, x];
                }
            }
            for (int y = r; y < Image.Height + r; y++)
            {
                int d1 = r;
                int d2 = -1;
                for (int x = 0; x < r; x++)
                {
                    d1--;
                    d2++;
                    InterestingPointsAdd[y, x] = false;
                    InterestingPointsAdd[y, Image.Width + r + x] = false;

                    ThetaAdd[y, x] = ThetaAdd[y, r + d1];
                    ThetaAdd[y, Image.Width + r + x] = ThetaAdd[y, Image.Width + r - 1 - d2];

                    SobelAdd[y, x] = SobelAdd[y, r + d1];
                    SobelAdd[y, Image.Width + r + x] = SobelAdd[y, Image.Width + r - 1 - d2];
                }
            }
            for (int x = 0; x < Image.Width + 2 * r; x++)
            {
                int d1 = r;
                int d2 = -1;
                for (int y = 0; y < r; y++)
                {
                    d1--;
                    d2++;
                    InterestingPointsAdd[y, x] = false;
                    InterestingPointsAdd[Image.Height + r + y, x] = false;

                    ThetaAdd[y, x] = ThetaAdd[r + d1, x];
                    ThetaAdd[Image.Height + r + y, x] = ThetaAdd[Image.Height + r - 1 - d2, x];

                    SobelAdd[y, x] = SobelAdd[r + d1, x];
                    SobelAdd[Image.Height + r + y, x] = SobelAdd[Image.Height + r - 1 - d2, x];
                }
            }

        }

        private void IntPointsCoords()
        {
            IntPointsCoord = new int[NewPoints, 2]; // 0 - y, 1 - x
            int p = 0;
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    if (InterestingPointsANMS[y, x])
                    {
                        IntPointsCoord[p, 0] = y;
                        IntPointsCoord[p, 1] = x;
                        p++;
                    }
                }
            }
        }

        private void Descr4(int NewHeight, int NewWidth)
        {
            Descriptors = new double[NewPoints, 16 * 8];
            GaussCore gauss = new GaussCore(8, 1);
            double[,] korzina = new double[8, 2];
            for (int i = 0; i < 8; i++)
            {
                korzina[i, 0] = (Math.PI / 4) * i;
                korzina[i, 1] = (Math.PI / 4) * (i + 1);
            }

            int point = -1;
            for (int y = 0; y < NewHeight; y++)
            {
                for (int x = 0; x < NewWidth; x++)
                {
                    if (InterestingPointsAdd[y,x])
                    {
                        point++;
                        double[] D = new double[16 * 8];
                        int region = 0;
                        for (int RegionY = -2; RegionY < 2; RegionY++)
                        {
                            for (int RegionX = -2; RegionX < 2; RegionX++)
                            {
                                for (int dy = 0; dy < 4; dy++)
                                {
                                    for (int dx = 0; dx < 4; dx++)
                                    {
                                        double L = SobelAdd[y + RegionY * 4 + dy, x + RegionX * 4 + dx];
                                        double Fi = ThetaAdd[y + RegionY * 4 + dy, x + RegionX * 4 + dx];
                                        int korzina1 = -10, korzina2 = -10; // смежные корзины
                                        double c1 = 0, c2 = 0; // коэф-ты для корзин
                                        for (int i = 0; i < 8; i++)
                                        {
                                            if (Fi == korzina[i, 1])
                                            {
                                                korzina1 = i;
                                                korzina2 = i + 1;
                                                if (korzina2 == 8)
                                                {
                                                    korzina2 = 0;
                                                }
                                                c1 = 0.5;
                                                c2 = 0.5;
                                            }
                                        }
                                        if (korzina1 == -10 || korzina2 == -10)
                                        {
                                            for (int i = 0; i < 8; i++)
                                            {
                                                if (Fi > korzina[i, 0] && Fi < korzina[i, 1])
                                                {
                                                    korzina1 = i;
                                                    double a1 = korzina[i, 1] - Fi;
                                                    double a0 = Fi - korzina[i, 0];
                                                    if (a0 > a1)
                                                    {
                                                        korzina2 = i + 1;
                                                        if (korzina2 == 8)
                                                        {
                                                            korzina2 = 0;
                                                        }

                                                        double d = Math.PI / 4;
                                                        double b = a1 + (Math.PI / 8);
                                                        c1 = b / d;
                                                        c2 = 1 - c1;
                                                    }
                                                    else if (a1 > a0)
                                                    {
                                                        korzina2 = i - 1;
                                                        if (korzina2 == -1)
                                                        {
                                                            korzina2 = 7;
                                                        }

                                                        double d = Math.PI / 4;
                                                        double b = a0 + (Math.PI / 8);
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

                                        double L1 = L * gauss.Matrix[(RegionY + 2) * 4 + dy, (RegionX + 2) * 4 + dx];
                                        D[region * 8 + korzina1] += L1 * c1;
                                        D[region * 8 + korzina2] += L1 * c2;
                                    }
                                }
                                region++;
                            }
                        }

                        D = F.NormalizeVector(D, 8 * 16, 0, 1);
                        for (int i = 0; i < 8 * 16; i++)
                        {
                            if (D[i] > 0.2)
                            {
                                D[i] = 0.2;
                            }
                        }
                        D = F.NormalizeVector(D, 8 * 16, 0, 1);
                        for (int i = 0; i < 8 * 16; i++)
                        {
                            Descriptors[point, i] = D[i];
                        }
                    }
                }
            }

        }

        private void Descr5(int NewHeight, int NewWidth)
        {
            Descriptors = new double[NewPoints, 16 * 8];
            Descriptors2 = new double[NewPoints, 16 * 8];
            for (int i = 0; i < NewPoints; i++)
            {
                Descriptors2[i, 0] = -1;
            }
            GaussCore gauss = new GaussCore(8, 1);
            double[,] korzina = new double[8, 2];
            double[,] korzinaO = new double[36, 2];
            for (int i = 0; i < 8; i++)
            {
                korzina[i, 0] = (Math.PI / 4) * i;
                korzina[i, 1] = (Math.PI / 4) * (i + 1);
            }
            for (int i = 0; i < 36; i++)
            {
                korzinaO[i, 0] = (Math.PI / 18) * i;
                korzinaO[i, 1] = (Math.PI / 18) * (i + 1);
            }

            int point = -1;
            for (int y = 0; y < NewHeight; y++)
            {
                for (int x = 0; x < NewWidth; x++)
                {
                    if (InterestingPointsAdd[y, x])
                    {
                        point++;
                        double[] D = new double[36];
                        int region = 0;
                        for (int RegionY = -2; RegionY < 2; RegionY++)
                        {
                            for (int RegionX = -2; RegionX < 2; RegionX++)
                            {
                                for (int dy = 0; dy < 4; dy++)
                                {
                                    for (int dx = 0; dx < 4; dx++)
                                    {
                                        double L = SobelAdd[y + RegionY * 4 + dy, x + RegionX * 4 + dx];
                                        double Fi = ThetaAdd[y + RegionY * 4 + dy, x + RegionX * 4 + dx];
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

                                        double L1 = L * gauss.Matrix[(RegionY + 2) * 4 + dy, (RegionX + 2) * 4 + dx];
                                        D[korzina1] += L1 * c1;
                                        D[korzina2] += L1 * c2;
                                    }
                                }
                                region++;
                            }
                        }
                        //double[] DR = new double[36];
                        /*
                        for (int i = 0; i < 36; i++)
                        {
                            for (int j = 0; j < 16; j++)
                            {
                                DR[i] = D[j * 36 + i];
                            }
                        }*/
                        /* Ищем пики */
                        double GMaxVal1 = -999999999;
                        double GMaxVal2 = -999999999;
                        int GMax1 = 0;
                        int GMax2 = 0;

                        for (int i = 0; i < 36; i++)
                        {
                            if (D[i] > GMaxVal1)
                            {
                                GMaxVal1 = D[i];
                                GMax1 = i;
                            }
                        }
                        for (int i = 0; i < 36; i++)
                        {
                            if (i == GMax1)
                            {
                                continue;
                            }
                            else if (D[i] > GMaxVal2)
                            {
                                GMaxVal2 = D[i];
                                GMax2 = i;
                            }
                        }

                        double alpha1 = (korzinaO[GMax1, 1] - korzinaO[GMax1, 0]) / 2 + korzinaO[GMax1, 0];
                        /*------------------------*/

                        int[,] mX = new int[16, 16];
                        int[,] mY = new int[16, 16];

                        for (int cy = 0; cy < 16; cy++)
                        {
                            for (int cx = 0; cx < 16; cx++)
                            {
                                int dx = cx - 8;
                                int dy = cy - 8;
                                mX[cy, cx] = Convert.ToInt32(Math.Round(dx * Math.Cos(alpha1) + dy * Math.Sin(alpha1)));
                                mY[cy, cx] = Convert.ToInt32(Math.Round(dy * Math.Cos(alpha1) - dx * Math.Sin(alpha1)));
                            }
                        }

                        D = new double[16 * 8];
                        region = 0;
                        for (int RegionY = -2; RegionY < 2; RegionY++)
                        {
                            for (int RegionX = -2; RegionX < 2; RegionX++)
                            {
                                for (int dy = 0; dy < 4; dy++)
                                {
                                    for (int dx = 0; dx < 4; dx++)
                                    {
                                        //int Nx = x + RegionX * 4 + dx;
                                        //int Ny = y + RegionY * 4 + dy;

                                        int cx = mX[(RegionY + 2) * 4 + dy, (RegionX + 2) * 4 + dx];
                                        int cy = mY[(RegionY + 2) * 4 + dy, (RegionX + 2) * 4 + dx];

                                        double L = SobelAdd[y + cy, x + cx];
                                        double Fi = ThetaAdd[y + cy, x + cx] - alpha1;

                                        if (Fi <= 0)
                                        {
                                            Fi = Fi + Math.PI * 2;
                                        }
                                        int korzina1 = -10, korzina2 = -10; // смежные корзины
                                        double c1 = 0, c2 = 0; // коэф-ты для корзин
                                        for (int i = 0; i < 8; i++)
                                        {
                                            if (Fi == korzina[i, 1])
                                            {
                                                korzina1 = i;
                                                korzina2 = i + 1;
                                                if (korzina2 == 8)
                                                {
                                                    korzina2 = 0;
                                                }
                                                c1 = 0.5;
                                                c2 = 0.5;
                                            }
                                        }
                                        if (korzina1 == -10 || korzina2 == -10)
                                        {
                                            for (int i = 0; i < 8; i++)
                                            {
                                                if (Fi > korzina[i, 0] && Fi < korzina[i, 1])
                                                {
                                                    korzina1 = i;
                                                    double a1 = korzina[i, 1] - Fi;
                                                    double a0 = Fi - korzina[i, 0];
                                                    if (a0 > a1)
                                                    {
                                                        korzina2 = i + 1;
                                                        if (korzina2 == 8)
                                                        {
                                                            korzina2 = 0;
                                                        }

                                                        double d = Math.PI / 4;
                                                        double b = a1 + (Math.PI / 8);
                                                        c1 = b / d;
                                                        c2 = 1 - c1;
                                                    }
                                                    else if (a1 > a0)
                                                    {
                                                        korzina2 = i - 1;
                                                        if (korzina2 == -1)
                                                        {
                                                            korzina2 = 7;
                                                        }

                                                        double d = Math.PI / 4;
                                                        double b = a0 + (Math.PI / 8);
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

                                        double L1 = L * gauss.Matrix[(RegionY + 2) * 4 + dy, (RegionX + 2) * 4 + dx];
                                        D[region * 8 + korzina1] += L1 * c1;
                                        D[region * 8 + korzina2] += L1 * c2;
                                    }
                                }
                                region++;
                            }
                        }
                        D = F.NormalizeVector(D, 16 * 8, 0, 1);
                        for (int i = 0; i < 16 * 8; i++)
                        {
                            if (D[i] > 0.2)
                            {
                                D[i] = 0.2;
                            }
                        }
                        D = F.NormalizeVector(D, 16 * 8, 0, 1);
                        for (int i = 0; i < 16 * 8; i++)
                        {
                            Descriptors[point, i] = D[i];
                        }

                        /*------------------------*/

                        if (GMaxVal2 / GMaxVal1 >= 0.8)
                        {
                            double alpha2 = (korzinaO[GMax2, 1] - korzinaO[GMax2, 0]) / 2 + korzinaO[GMax2, 0];
                            /*------------------------*/

                            mX = new int[16, 16];
                            mY = new int[16, 16];

                            for (int cy = 0; cy < 16; cy++)
                            {
                                for (int cx = 0; cx < 16; cx++)
                                {
                                    mX[cy, cx] = Convert.ToInt32(Math.Round(cx * Math.Cos(alpha2) + cy * Math.Sin(alpha2)));
                                    mY[cy, cx] = Convert.ToInt32(Math.Round(cy * Math.Cos(alpha2) - cx * Math.Sin(alpha2)));
                                }
                            }

                            D = new double[16 * 8];
                            region = 0;
                            for (int RegionY = -2; RegionY < 2; RegionY++)
                            {
                                for (int RegionX = -2; RegionX < 2; RegionX++)
                                {
                                    for (int dy = 0; dy < 4; dy++)
                                    {
                                        for (int dx = 0; dx < 4; dx++)
                                        {
                                            //int Nx = x + RegionX * 4 + dx;
                                            //int Ny = y + RegionY * 4 + dy;

                                            int cx = mX[(RegionY + 2) * 4 + dy, (RegionX + 2) * 4 + dx];
                                            int cy = mY[(RegionY + 2) * 4 + dy, (RegionX + 2) * 4 + dx];

                                            double L = SobelAdd[y + cy - 8, x + cx - 8];
                                            double Fi = ThetaAdd[y + cy - 8, x + cx - 8] - alpha2;
                                            if (Fi <= 0)
                                            {
                                                Fi = Fi + Math.PI * 2;
                                            }
                                            int korzina1 = -10, korzina2 = -10; // смежные корзины
                                            double c1 = 0, c2 = 0; // коэф-ты для корзин
                                            for (int i = 0; i < 8; i++)
                                            {
                                                if (Fi == korzina[i, 1])
                                                {
                                                    korzina1 = i;
                                                    korzina2 = i + 1;
                                                    if (korzina2 == 8)
                                                    {
                                                        korzina2 = 0;
                                                    }
                                                    c1 = 0.5;
                                                    c2 = 0.5;
                                                }
                                            }
                                            if (korzina1 == -10 || korzina2 == -10)
                                            {
                                                for (int i = 0; i < 8; i++)
                                                {
                                                    if (Fi > korzina[i, 0] && Fi < korzina[i, 1])
                                                    {
                                                        korzina1 = i;
                                                        double a1 = korzina[i, 1] - Fi;
                                                        double a0 = Fi - korzina[i, 0];
                                                        if (a0 > a1)
                                                        {
                                                            korzina2 = i + 1;
                                                            if (korzina2 == 8)
                                                            {
                                                                korzina2 = 0;
                                                            }

                                                            double d = Math.PI / 4;
                                                            double b = a1 + (Math.PI / 8);
                                                            c1 = b / d;
                                                            c2 = 1 - c1;
                                                        }
                                                        else if (a1 > a0)
                                                        {
                                                            korzina2 = i - 1;
                                                            if (korzina2 == -1)
                                                            {
                                                                korzina2 = 7;
                                                            }

                                                            double d = Math.PI / 4;
                                                            double b = a0 + (Math.PI / 8);
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

                                            double L1 = L * gauss.Matrix[(RegionY + 2) * 4 + dy, (RegionX + 2) * 4 + dx];
                                            D[region * 8 + korzina1] += L1 * c1;
                                            D[region * 8 + korzina2] += L1 * c2;
                                        }
                                    }
                                    region++;
                                }
                            }
                            D = F.NormalizeVector(D, 16 * 8, 0, 1);
                            for (int i = 0; i < 16 * 8; i++)
                            {
                                if (D[i] > 0.2)
                                {
                                    D[i] = 0.2;
                                }
                            }
                            D = F.NormalizeVector(D, 16 * 8, 0, 1);
                            for (int i = 0; i < 16 * 8; i++)
                            {
                                Descriptors2[point, i] = D[i];
                            }

                            /*------------------------*/
                        }








                    }
                }
            }

        }




        public void Descript4()
        {
            SobelAndTheta();
            MtxAdd(8, out int NewHeight, out int NewWidth);
            IntPointsCoords();
            Descr4(NewHeight, NewWidth);
        }

        public void Descript5()
        {
            SobelAndTheta();
            MtxAdd(48, out int NewHeight, out int NewWidth);
            IntPointsCoords();
            Descr5(NewHeight, NewWidth);
        }

    }
}
