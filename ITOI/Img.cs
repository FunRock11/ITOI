using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ITOI
{
    class Img
    {
        public int Height;
        public int Width;
        public Bitmap Bitmap;
        public byte[,] GrayMatrix;
        public double[,] GrayMatrixDouble;

        public Img() { }

        public Img(byte[,] matrix, int width, int height)
        {
            Height = height;
            Width = width;
            Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            GrayMatrix = new byte[Height, Width];
            Color color;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    GrayMatrix[y, x] = matrix[y, x];
                    color = Color.FromArgb(255, GrayMatrix[y, x], GrayMatrix[y, x], GrayMatrix[y, x]);
                    Bitmap.SetPixel(x, y, color);
                }
            }
            GrayMatrixDouble = InitDoubleGrayMatrix(GrayMatrix, Width, Height);
        }

        public Img(double[,] matrix, int width, int height)
        {
            Height = height;
            Width = width;
            Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            GrayMatrixDouble = new double[Height, Width];
            Color color;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    GrayMatrixDouble[y, x] = matrix[y, x];
                }
            }
            GrayMatrix = InitGrayMatrix(matrix, Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    color = Color.FromArgb(255, GrayMatrix[y, x], GrayMatrix[y, x], GrayMatrix[y, x]);
                    Bitmap.SetPixel(x, y, color);
                }
            }
        }

        public Img(string path)
        {
            Bitmap = new Bitmap(path);
            Height = Bitmap.Height;
            Width = Bitmap.Width;
            GrayMatrix = new byte[Height, Width];
            Color color;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    color = Bitmap.GetPixel(x, y);
                    GrayMatrix[y, x] = Convert.ToByte(Math.Round(0.213 * color.R + 0.715 * color.G + 0.072 * color.B));
                }
            }
            GrayMatrixDouble = InitDoubleGrayMatrix(GrayMatrix, Width, Height);
        }

        public Img(Bitmap bitmap)
        {
            Height = bitmap.Height;
            Width = bitmap.Width;
            Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            GrayMatrix = new byte[Height, Width];
            Color color;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Bitmap.SetPixel(x, y, bitmap.GetPixel(x, y));
                    color = Bitmap.GetPixel(x, y);
                    GrayMatrix[y, x] = Convert.ToByte(Math.Round(0.213 * color.R + 0.715 * color.G + 0.072 * color.B));
                }
            }
            GrayMatrixDouble = InitDoubleGrayMatrix(GrayMatrix, Width, Height);
        }

        public void Draw(PictureBox pictureBox)
        {
            pictureBox.Height = Bitmap.Height;
            pictureBox.Width = Bitmap.Width;
            pictureBox.Image = Bitmap;
        }

        public void Save(string path)
        {
            Bitmap.Save(path);
        }

        public void Downsample()
        {
            int heightNew = Height / 2;
            int widthNew = Width / 2;
            byte[,] Result = new byte[heightNew, widthNew];

            int x1 = 0;
            int y1 = 0;
            for (int y = 0; y < heightNew; y++)
            {
                x1 = 0;
                for (int x = 0; x < widthNew; x++)
                {
                    Result[y, x] = GrayMatrix[y1, x1];
                    x1 += 2;
                    if (x1 >= Width)
                    {
                        x1 = Width - 1;
                    }
                }
                y1 += 2;
                if (y1 >= Height)
                {
                    y1 = Height - 1;
                }
            }

            Height = heightNew;
            Width = widthNew;
            Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            GrayMatrix = Result;
            Color color;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    color = Color.FromArgb(255, GrayMatrix[y, x], GrayMatrix[y, x], GrayMatrix[y, x]);
                    Bitmap.SetPixel(x, y, color);
                }
            }
            GrayMatrixDouble = NormalizeMatrix(GrayMatrix, Width, Height, GrayMatrixDouble);
        }

        public void SvertkaWithNormalize(double[,] mask, int k, int kraimode)
        {
            double[,] GrayMatrixAdd = new double[Height + 2 * k, Width + 2 * k];
            double[,] Result = new double[Height, Width];

            /* Краевые эффекты */
            // Копируем значение с края изображения
            if (kraimode == 1)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        GrayMatrixAdd[y + k, x + k] = GrayMatrixDouble[y, x];
                    }
                }
                for (int y = k; y < Height + k; y++)
                {
                    for (int x = 0; x < k; x++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[y, k];
                        GrayMatrixAdd[y, Width + k + x] = GrayMatrixAdd[y, Width + k - 1];
                    }
                }
                for (int x = 0; x < Width + 2 * k; x++)
                {
                    for (int y = 0; y < k; y++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[k, x];
                        GrayMatrixAdd[Height + k + y, x] = GrayMatrixAdd[Height + k - 1, x];
                    }
                }
            }
            // Всё снаружи чёрное
            else
            {
                for (int y = 0; y < Height + 2 * k; y++)
                {
                    for (int x = 0; x < Width + 2 * k; x++)
                    {
                        if (x < k || y < k || y >= Height + k || x >= Width + k)
                        {
                            GrayMatrixAdd[y, x] = 0;
                        }
                        else
                        {
                            GrayMatrixAdd[y, x] = GrayMatrixDouble[y - k, x - k];
                        }
                    }
                }
            }
            /*-----------------*/

            for (int y = k; y < Height + k; y++)
            {
                for (int x = k; x < Width + k; x++)
                {
                    double S = 0;
                    for (int hWinX = -k; hWinX <= k; hWinX++)
                    {
                        for (int hWinY = -k; hWinY <= k; hWinY++)
                        {
                            S += GrayMatrixAdd[y - hWinY, x - hWinX] * mask[k + hWinY, k + hWinX];
                        }
                    }
                    Result[y - k, x - k] = S;
                }
            }
            Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            GrayMatrixDouble = NormalizeMatrix(Result, Width, Height, GrayMatrixDouble);
            GrayMatrix = NormalizeMatrix(GrayMatrixDouble, Width, Height, GrayMatrix);
            Color color;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    color = Color.FromArgb(255, GrayMatrix[y, x], GrayMatrix[y, x], GrayMatrix[y, x]);
                    Bitmap.SetPixel(x, y, color);
                }
            }
        }

        // Нормализовать матрицу
        public double[,] NormalizeMatrix(double[,] Matrix, int width, int height, double[,] TekMatrix)
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

            double newMax = -999999999;
            double newMin = 999999999;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (TekMatrix[y, x] >= newMax)
                    {
                        newMax = TekMatrix[y, x];
                    }
                    else if (TekMatrix[y, x] <= newMin)
                    {
                        newMin = TekMatrix[y, x];
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

        // Нормализовать матрицу
        public byte[,] NormalizeMatrix(double[,] Matrix, int width, int height, byte[,] TekMatrix)
        {
            double[,] Result = new double[height, width];
            byte[,] ResultByte = new byte[height, width];

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

            double newMax = -999999999;
            double newMin = 999999999;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (TekMatrix[y, x] >= newMax)
                    {
                        newMax = TekMatrix[y, x];
                    }
                    else if (TekMatrix[y, x] <= newMin)
                    {
                        newMin = TekMatrix[y, x];
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = (Result[y, x] - vMin) * (newMax - newMin) / (vMax - vMin) + newMin;
                    ResultByte[y, x] = Convert.ToByte(Result[y, x]);
                }
            }

            return ResultByte;
        }

        // Нормализовать матрицу
        public double[,] NormalizeMatrix(byte[,] Matrix, int width, int height, double[,] TekMatrix)
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

            double newMax = -999999999;
            double newMin = 999999999;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (TekMatrix[y, x] >= newMax)
                    {
                        newMax = TekMatrix[y, x];
                    }
                    else if (TekMatrix[y, x] <= newMin)
                    {
                        newMin = TekMatrix[y, x];
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

        private double[,] InitDoubleGrayMatrix(byte[,] Matrix, int width, int height)
        {
            double[,] Result = new double[height, width];

            double vMax = 255;
            double vMin = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = Matrix[y, x];
                }
            }

            double newMax = 1;
            double newMin = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = (Result[y, x] - vMin) * (newMax - newMin) / (vMax - vMin) + newMin;
                }
            }

            return Result;
        }

        private byte[,] InitGrayMatrix(double[,] Matrix, int width, int height)
        {
            double[,] Result = new double[height, width];
            byte[,] ResultByte = new byte[height, width];

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

            double newMax = 255;
            double newMin = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = (Result[y, x] - vMin) * (newMax - newMin) / (vMax - vMin) + newMin;
                    ResultByte[y, x] = Convert.ToByte(Result[y, x]);
                }
            }

            return ResultByte;
        }

    }
}
