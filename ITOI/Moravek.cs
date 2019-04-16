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
        Func F = new Func();
        public Img Image;
        public Img ImageWithPoints;
        public Img ImageWithANMS;
        public int WindowRadius;
        public double[,] S;
        public bool[,] InterestingPoints;
        public bool[,] InterestingPointsANMS;
        public double R; // Отбираются точки, которые больше R * max
        public int NPoints;
        public int NewPoints;
        public int NeedPoints;
        public double[,] Mtx;

        private double MAXmin = -999999999;
        private double MINmin = 999999999;

        public Moravek(Img image, int windowradius, double r)
        {
            Image = image;

            GaussCore GaussMatrix = new GaussCore(1);
            Mtx = F.Svertka(Image.GrayMatrix, Image.Width, Image.Height, GaussMatrix.Matrix, GaussMatrix.Radius, 1);
            WindowRadius = windowradius;
            R = r;

            MoravekS();
            IntPoints1();
            IWP(1);
        }

        private void MoravekS()
        {
            int k1 = WindowRadius + 1;
            S = new double[Image.Height, Image.Width];

            for (int y = k1; y < Image.Height - k1; y++)
            {
                for (int x = k1; x < Image.Width - k1; x++)
                {
                    double Q = 999999999;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if  (dx == 0 && dy == 0)
                            {
                                continue;
                            }
                            else
                            {
                                double C = 0;
                                for (int hWinX = -WindowRadius; hWinX <= WindowRadius; hWinX++)
                                {
                                    for (int hWinY = -WindowRadius; hWinY <= WindowRadius; hWinY++)
                                    {
                                        C += Math.Pow((double)Mtx[y + hWinY, x + hWinX] - (double)Mtx[y + hWinY + dx, x + hWinX + dy], 2);
                                    }
                                }
                                if (C < Q)
                                {
                                    Q = C;
                                }
                            }
                        }
                    }
                    S[y, x] = Q;

                    if (S[y, x] > MAXmin)
                    {
                        MAXmin = S[y, x];
                    }
                    if (S[y, x] < MINmin)
                    {
                        MINmin = S[y, x];
                    }
                }
            }
            for (int y = 0; y < k1; y++)
            {
                for (int x = 0; x < k1; x++)
                {
                    S[y, x] = MINmin;
                    S[y, x] = MINmin;
                }
            }
            for (int y = Image.Height - k1; y < Image.Height; y++)
            {
                for (int x = Image.Width - k1; x < Image.Width; x++)
                {
                    S[y, x] = MINmin;
                    S[y, x] = MINmin;
                }
            }
        }

        private void IntPoints1()
        {
            InterestingPoints = new bool[Image.Height, Image.Width];
            double T = (MAXmin - MINmin) * R;

            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    if (S[y, x] >= T)
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
                                    else if (S[y + hWinY, x + hWinX] <= S[y, x])
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
                                                else if (S[y + hWinY, x + hWinX] <= S[y, x])
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
