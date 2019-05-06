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

namespace ITOI
{
    public partial class Form1 : Form
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        Func F = new Func();
        string BasePath = "../../../files/";

        Img BeginImg;
        Img GrayImg;
        
        int IWidth;
        int IHeight;

        double[,] DerivativeX;
        double[,] DerivativeY;
        double[,] Sobel;

        Img DerivativeXImg;
        Img DerivativeYImg;
        Img SobelImg;

        Moravek Moravek;
        int MoraverRadius;
        double MoravekDolya;

        Harris Harris;
        int HarrisRadius;
        double HarrisDolya;

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

        public Form1()
        {
            InitializeComponent();
            /* Очищаем предыдущие результаты */
            F.ClearDir(BasePath + "Result");
            F.ClearDir(BasePath + "Lab 1");
            F.ClearDir(BasePath + "Lab 2");
            F.ClearDir(BasePath + "Lab 2/Core");
            F.ClearDir(BasePath + "Lab 3");
            F.ClearDir(BasePath + "Lab 4");
            F.ClearDir(BasePath + "Lab 5");
            F.ClearDir(BasePath + "Lab 6");
            /*-------------------------------*/

            BeginImg = new Img(BasePath + "Begin/BeginImage1.png");
            GrayImg = new Img(BeginImg.GrayMatrix, BeginImg.Width, BeginImg.Height);
            GrayImg.Save(BasePath + "Result/GrayImage.png");

            IWidth = BeginImg.Width;
            IHeight = BeginImg.Height;

            BeginImg.Draw(pictureBox1);
            GrayImg.Draw(pictureBox2);
        }

        /* Частные производные */
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

            DerivativeX = F.Svertka(GrayImg.GrayMatrix, IWidth, IHeight, maskX, k, 1);
            DerivativeXImg = new Img(DerivativeX, IWidth, IHeight);
            DerivativeXImg.Draw(pictureBox3);
            DerivativeXImg.Save(BasePath + "Lab 1/DerivativeX.png");

            DerivativeY = F.Svertka(GrayImg.GrayMatrix, IWidth, IHeight, maskY, k, 1);
            DerivativeYImg = new Img(DerivativeY, IWidth, IHeight);
            DerivativeYImg.Draw(pictureBox4);
            DerivativeYImg.Save(BasePath + "Lab 1/DerivativeY.png");

            button2.Enabled = true;
        }

        /* Собель */
        private void button2_Click(object sender, EventArgs e)
        {
            GrayImg.Draw(pictureBox6);

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

            SobelImg = new Img(Sobel, IWidth, IHeight);
            SobelImg.Draw(pictureBox5);
            SobelImg.Save(BasePath + "Lab 1/Sobel.png");
        }

        /* Пирамида */
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
                F.ClearDir(BasePath + "Lab 2");
                F.ClearDir(BasePath + "Lab 2/Core");

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

                Img TekImg = new Img(GrayImg.GrayMatrix, IWidth, IHeight);
                TekImg.Save(BasePath + "Lab 2/000.png");
                double sigmaD = sigma0;                        // Действительная сигма
                double sigmaTEK = sigma0;
                double qq = 1;

                GaussCore GaussMatrix = new GaussCore(sigma0);
                Img GaussImg = new Img(GaussMatrix.Matrix, GaussMatrix.Size, GaussMatrix.Size);
                TekImg.SvertkaWithNormalize(GaussMatrix.Matrix, GaussMatrix.Radius, 1);
                TekImg.Save(BasePath + "Lab 2/" + "00"
                    + " - S1=" + Convert.ToString(Math.Round(sigma0, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigma0, 2)) + ".png");
                GaussImg.Save(BasePath + "Lab 2/Core/" + "00" + ".png");

                for (int o = 0; o < O; o++)
                {
                    for (int s = 1; s <= S; s++)
                    {
                        double sigma1 = sigma0 * Math.Pow(k, (s - 1));
                        double sigma2 = sigma0 * Math.Pow(k, s);
                        sigmaTEK = Math.Sqrt(sigma2 * sigma2 - sigma1 * sigma1);// Текущая сигма
                        sigmaD = sigma0 * Math.Pow(k, qq);
                        qq++;
                        GaussMatrix = new GaussCore(sigmaTEK);
                        GaussImg = new Img(GaussMatrix.Matrix, GaussMatrix.Size, GaussMatrix.Size);
                        TekImg.SvertkaWithNormalize(GaussMatrix.Matrix, GaussMatrix.Radius, 1);
                        TekImg.Save(BasePath + "Lab 2/" + Convert.ToString(o) + Convert.ToString(s)
                            + " - S1=" + Convert.ToString(Math.Round(sigma2, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigmaD, 2)) + ".png");
                        GaussImg.Save(BasePath + "Lab 2/Core/" + Convert.ToString(o) + Convert.ToString(s) + ".png");

                    }
                    TekImg.Downsample();
                    TekImg.Save(BasePath + "Lab 2/" + Convert.ToString(o + 1) + "0"
                            + " - S1=" + Convert.ToString(Math.Round(sigma0, 2)) + " - Sd=" + Convert.ToString(Math.Round(sigmaD, 2)) + ".png");
                }
            }
        }

        /* Моравек */
        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != "" && textBox4.Text != "")
            {
                try
                {
                    MoraverRadius = Convert.ToInt32(textBox3.Text);
                    MoravekDolya = Convert.ToDouble(textBox4.Text);

                    GrayImg.Draw(pictureBox8);
                    GrayImg.Draw(pictureBox10);

                    Moravek = new Moravek(GrayImg, MoraverRadius, MoravekDolya);

                    Img SI = new Img(Moravek.S, IWidth, IHeight);
                    SI.Draw(pictureBox9);

                    Moravek.ImageWithPoints.Draw(pictureBox7);
                    Moravek.ImageWithPoints.Save(BasePath + "Lab 3/Moravek.png");

                    label9.Text = "Оператор Моравека (Точек: " + Moravek.NPoints + ")";

                    Moravek.ImageWithPoints.Draw(pictureBox12);
                    label14.Text = "Точек: " + Moravek.NPoints;
                    label13.Text = "Точек: 0";

                    Moravek.ImageWithPoints.Draw(pictureBox20);
                    Moravek.ImageWithPoints.Draw(pictureBox22);
                    Moravek.ImageWithPoints.Draw(pictureBox24);
                    Moravek.ImageWithPoints.Draw(pictureBox26);
                    Moravek.ImageWithPoints.Draw(pictureBox28);

                    Bitmap bmp = new Bitmap(IWidth, IHeight, PixelFormat.Format32bppArgb);
                    pictureBox11.Image = bmp;
                    pictureBox19.Image = bmp;
                    pictureBox21.Image = bmp;
                    pictureBox23.Image = bmp;
                    pictureBox25.Image = bmp;
                    pictureBox27.Image = bmp;

                    button6.Enabled = true;
                    button8.Enabled = true;
                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /* Харрис */
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox6.Text != "" && textBox5.Text != "")
            {
                try
                {
                    HarrisRadius = Convert.ToInt32(textBox6.Text);
                    HarrisDolya = Convert.ToDouble(textBox5.Text);

                    GrayImg.Draw(pictureBox14);

                    Harris = new Harris(GrayImg, HarrisRadius, HarrisDolya);

                    Harris.ImageWithPoints.Draw(pictureBox13);

                    Img DX = new Img(Harris.MinL, IWidth, IHeight);
                    Img DY = new Img(Harris.MaxL, IWidth, IHeight);

                    DX.Draw(pictureBox15);
                    DY.Draw(pictureBox16);

                    Harris.ImageWithPoints.Save(BasePath + "Lab 3/Harris.png");

                    label15.Text = "Оператор Харриса (Точек: " + Harris.NPoints + ")";

                    Harris.ImageWithPoints.Draw(pictureBox18);
                    label20.Text = "Точек: " + Harris.NPoints;
                    label19.Text = "Точек: 0";
                    
                    Harris.ImageWithPoints.Draw(pictureBox32);
                    Harris.ImageWithPoints.Draw(pictureBox34);
                    Harris.ImageWithPoints.Draw(pictureBox36);
                    Harris.ImageWithPoints.Draw(pictureBox38);
                    Harris.ImageWithPoints.Draw(pictureBox40);
                    
                    Bitmap bmp = new Bitmap(IWidth, IHeight, PixelFormat.Format32bppArgb);
                    pictureBox17.Image = bmp;
                    pictureBox31.Image = bmp;
                    pictureBox33.Image = bmp;
                    pictureBox35.Image = bmp;
                    pictureBox37.Image = bmp;
                    pictureBox39.Image = bmp;

                    button7.Enabled = true;
                    button9.Enabled = true;
                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /* ANMS Моравека */
        private void button6_Click(object sender, EventArgs e)
        {
            if (textBox7.Text != "")
            {
                try
                {
                    int Npoints = Convert.ToInt32(textBox7.Text);
                    Moravek.ANMS(Npoints);
                    Moravek.ImageWithANMS.Draw(pictureBox11);
                    Moravek.ImageWithANMS.Save(BasePath + "Lab 3/MoravekANMS.png");
                    label13.Text = "Точек: " + Moravek.NewPoints;
                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /* ANMS Харриса */
        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox8.Text != "")
            {
                try
                {
                    int Npoints = Convert.ToInt32(textBox8.Text);
                    Harris.ANMS(Npoints);
                    Harris.ImageWithANMS.Draw(pictureBox17);
                    Harris.ImageWithANMS.Save(BasePath + "Lab 3/HarrisANMS.png");
                    label19.Text = "Точек: " + Harris.NewPoints;
                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /* Повторяемость Моравека */
        private void button8_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked || radioButton6.Checked)
            {
                /*Сдвиг*/
                byte[,] SdvigMtx = F.Sdvig(GrayImg.GrayMatrix, GrayImg.Width, GrayImg.Height, out int nWidth, out int nHeight, 50, 20);
                Img SdvigImg = new Img(SdvigMtx, nWidth, nHeight);
                Moravek SdvigMoravek = new Moravek(SdvigImg, MoraverRadius, MoravekDolya);
                SdvigMoravek.ImageWithPoints.Draw(pictureBox19);
            }
            if (radioButton2.Checked || radioButton6.Checked)
            {
                /*Поворот*/
                int size = Convert.ToInt32(Math.Ceiling(Math.Sqrt(IWidth * IWidth + IHeight * IHeight)));
                Bitmap RotateBMP = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        RotateBMP.SetPixel(x, y, Color.White);
                    }
                }
                int o = (size - IHeight) / 2;
                int oo = (size - IWidth) / 2;
                for (int y = o; y < IHeight + o; y++)
                {
                    for (int x = oo; x < IWidth + oo; x++)
                    {
                        RotateBMP.SetPixel(x, y, GrayImg.Bitmap.GetPixel(x - oo, y - o));
                    }
                }
                RotateBMP = F.RotateImage(RotateBMP, 30.0F);
                Img RotateImg = new Img(RotateBMP);
                Moravek RotateMoravek = new Moravek(RotateImg, MoraverRadius, MoravekDolya);
                RotateMoravek.ImageWithPoints.Draw(pictureBox21);
            }
            if (radioButton3.Checked || radioButton6.Checked)
            {
                /*Шум*/
                double[,] NoiseMtx = F.Noise(GrayImg.GrayMatrix, IWidth, IHeight, 30);
                Img NoiseImg = new Img(NoiseMtx, IWidth, IHeight);
                Moravek NoiseMoravek = new Moravek(NoiseImg, MoraverRadius, MoravekDolya);
                NoiseMoravek.ImageWithPoints.Draw(pictureBox23);
            }
            if (radioButton4.Checked || radioButton6.Checked)
            {
                /*Контрастность*/
                byte[,] ContrastMtx = F.Contrast(GrayImg.GrayMatrix, IWidth, IHeight, 35);
                Img ContrastImg = new Img(ContrastMtx, IWidth, IHeight);
                Moravek ContrastMoravek = new Moravek(ContrastImg, MoraverRadius, MoravekDolya);
                ContrastMoravek.ImageWithPoints.Draw(pictureBox25);
            }
            if (radioButton5.Checked || radioButton6.Checked)
            {
                /*Яркость*/
                byte[,] BrightMtx = F.Brightness(GrayImg.GrayMatrix, IWidth, IHeight, 50);
                Img BrightImg = new Img(BrightMtx, IWidth, IHeight);
                Moravek BrightMoravek = new Moravek(BrightImg, MoraverRadius, MoravekDolya);
                BrightMoravek.ImageWithPoints.Draw(pictureBox27);
            }
        }

        /* Повторяемость Харриса */
        private void button9_Click(object sender, EventArgs e)
        {
            if (radioButton7.Checked || radioButton12.Checked)
            {
                /*Сдвиг*/
                byte[,] SdvigMtx = F.Sdvig(GrayImg.GrayMatrix, GrayImg.Width, GrayImg.Height, out int nWidth, out int nHeight, 50, 20);
                Img SdvigImg = new Img(SdvigMtx, nWidth, nHeight);
                Harris SdvigHarris = new Harris(SdvigImg, HarrisRadius, HarrisDolya);
                SdvigHarris.ImageWithPoints.Draw(pictureBox31);
            }
            if (radioButton8.Checked || radioButton12.Checked)
            {
                /*Поворот*/
                int size = Convert.ToInt32(Math.Ceiling(Math.Sqrt(IWidth * IWidth + IHeight * IHeight)));
                Bitmap RotateBMP = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        RotateBMP.SetPixel(x, y, Color.White);
                    }
                }
                int o = (size - IHeight) / 2;
                int oo = (size - IWidth) / 2;
                for (int y = o; y < IHeight + o; y++)
                {
                    for (int x = oo; x < IWidth + oo; x++)
                    {
                        RotateBMP.SetPixel(x, y, GrayImg.Bitmap.GetPixel(x - oo, y - o));
                    }
                }
                RotateBMP = F.RotateImage(RotateBMP, 30.0F);
                Img RotateImg = new Img(RotateBMP);
                Harris RotateHarris = new Harris(RotateImg, HarrisRadius, HarrisDolya);
                RotateHarris.ImageWithPoints.Draw(pictureBox33);
            }
            if (radioButton9.Checked || radioButton12.Checked)
            {
                /*Шум*/
                double[,] NoiseMtx = F.Noise(GrayImg.GrayMatrix, IWidth, IHeight, 30);
                Img NoiseImg = new Img(NoiseMtx, IWidth, IHeight);
                Harris NoiseHarris = new Harris(NoiseImg, HarrisRadius, HarrisDolya);
                NoiseHarris.ImageWithPoints.Draw(pictureBox35);
            }
            if (radioButton10.Checked || radioButton12.Checked)
            {
                /*Контрастность*/
                byte[,] ContrastMtx = F.Contrast(GrayImg.GrayMatrix, IWidth, IHeight, 35);
                Img ContrastImg = new Img(ContrastMtx, IWidth, IHeight);
                Harris ContrastHarris = new Harris(ContrastImg, HarrisRadius, HarrisDolya);
                ContrastHarris.ImageWithPoints.Draw(pictureBox37);
            }
            if (radioButton11.Checked || radioButton12.Checked)
            {
                /*Яркость*/
                byte[,] BrightMtx = F.Brightness(GrayImg.GrayMatrix, IWidth, IHeight, 50);
                Img BrightImg = new Img(BrightMtx, IWidth, IHeight);
                Harris BrightHarris = new Harris(BrightImg, HarrisRadius, HarrisDolya);
                BrightHarris.ImageWithPoints.Draw(pictureBox39);
            }
        }

        /* Дескрипторы (Лаб 4)*/
        private void button10_Click(object sender, EventArgs e)
        {
            if (textBox11.Text != "" && textBox10.Text != "" && textBox9.Text != "")
            {
                try
                {
                    int HarrisRadius = Convert.ToInt32(textBox11.Text);
                    double HarrisDolya = Convert.ToDouble(textBox10.Text);
                    int Npoints = Convert.ToInt32(textBox9.Text);

                    Img BegImg1 = new Img(GrayImg.GrayMatrix, IWidth, IHeight);

                    /*-------------------------*/
                    int size = Convert.ToInt32(Math.Ceiling(Math.Sqrt(IWidth * IWidth + IHeight * IHeight)));
                    Bitmap RotateBMP = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            RotateBMP.SetPixel(x, y, Color.White);
                        }
                    }
                    int o = (size - IHeight) / 2;
                    int oo = (size - IWidth) / 2;
                    for (int y = o; y < IHeight + o; y++)
                    {
                        for (int x = oo; x < IWidth + oo; x++)
                        {
                            RotateBMP.SetPixel(x, y, GrayImg.Bitmap.GetPixel(x - oo, y - o));
                        }
                    }
                    RotateBMP = F.RotateImage(RotateBMP, 5.0F);
                    Img TempImg = new Img(RotateBMP);

                    byte[,] SdvigMtx = F.Sdvig(TempImg.GrayMatrix, TempImg.Width, TempImg.Height, out int nWidth, out int nHeight, 0, -100);
                    TempImg = new Img(SdvigMtx, nWidth, nHeight);

                    byte[,] ContrastMtx = F.Contrast(TempImg.GrayMatrix, TempImg.Width, TempImg.Height, -10);
                    Img BegImg2 = new Img(ContrastMtx, TempImg.Width, TempImg.Height);
                    /*--------------------------*/

                    BegImg1.Draw(pictureBox30);
                    BegImg2.Draw(pictureBox29);

                    Harris Harris1 = new Harris(BegImg1, HarrisRadius, HarrisDolya);
                    Harris1.ANMS(Npoints);
                    Harris Harris2 = new Harris(BegImg2, HarrisRadius, HarrisDolya);
                    Harris2.ANMS(Npoints);

                    Harris1.ImageWithANMS.Draw(pictureBox42);
                    Harris2.ImageWithANMS.Draw(pictureBox41);

                    Harris1.Descript4();
                    Harris2.Descript4();

                    int[] S = F.DescriptSootv4(Harris1, Harris2, 8 * 16, 0.8);
                    int rst = 20;
                    int h = Math.Max(Harris1.ImageWithANMS.Height, Harris2.ImageWithANMS.Height);
                    Bitmap SootvBmp = new Bitmap(Harris1.ImageWithANMS.Width + Harris2.ImageWithANMS.Width + rst, h, PixelFormat.Format32bppArgb);
                    
                    for (int y = 0; y < Harris1.ImageWithANMS.Height; y++)
                    {
                        for (int x = 0; x < Harris1.ImageWithANMS.Width; x++)
                        {
                            SootvBmp.SetPixel(x, y, Harris1.ImageWithANMS.Bitmap.GetPixel(x, y));
                        }
                    }
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = Harris1.ImageWithANMS.Width; x < Harris1.ImageWithANMS.Width + rst; x++)
                        {
                            SootvBmp.SetPixel(x, y, Color.White);
                        }
                    }
                    for (int y = 0; y < Harris2.ImageWithANMS.Height; y++)
                    {
                        for (int x = Harris1.ImageWithANMS.Width + rst; x < Harris1.ImageWithANMS.Width + rst + Harris2.ImageWithANMS.Width; x++)
                        {
                            SootvBmp.SetPixel(x, y, Harris2.ImageWithANMS.Bitmap.GetPixel(x - (Harris1.ImageWithANMS.Width + rst), y));
                        }
                    }

                    Graphics g = Graphics.FromImage(SootvBmp);
                    Pen pen = new Pen(Brushes.Blue, 2);
                    Point p1, p2;
                    for (int i = 0; i < Harris1.NewPoints; i++)
                    {
                        if (S[i] != -1)
                        {
                            p1 = new Point(Harris1.IntPointsCoord[i, 1], Harris1.IntPointsCoord[i, 0]);
                            p2 = new Point(Harris2.IntPointsCoord[S[i], 1] + (Harris1.ImageWithANMS.Width + rst), Harris2.IntPointsCoord[S[i], 0]);
                            g.DrawLine(pen, p1, p2);
                        }
                    }
                    pictureBox44.Image = SootvBmp;

                    Harris1.ImageWithANMS.Save(BasePath + "Lab 4/HarrisANMS1.png");
                    Harris2.ImageWithANMS.Save(BasePath + "Lab 4/HarrisANMS2.png");
                    SootvBmp.Save(BasePath + "Lab 4/Result.png");

                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /* Дескрипторы (Лаб 5)*/
        private void button11_Click(object sender, EventArgs e)
        {
            if (textBox14.Text != "" && textBox13.Text != "" && textBox12.Text != "")
            {
                try
                {
                    int HarrisRadius = Convert.ToInt32(textBox14.Text);
                    double HarrisDolya = Convert.ToDouble(textBox13.Text);
                    int Npoints = Convert.ToInt32(textBox12.Text);

                    Img BegImg1 = new Img(GrayImg.GrayMatrix, IWidth, IHeight);

                    /*-------------------------*/
                    int size = Convert.ToInt32(Math.Ceiling(Math.Sqrt(IWidth * IWidth + IHeight * IHeight)));
                    Bitmap RotateBMP = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            RotateBMP.SetPixel(x, y, Color.White);
                        }
                    }
                    int o = (size - IHeight) / 2;
                    int oo = (size - IWidth) / 2;
                    for (int y = o; y < IHeight + o; y++)
                    {
                        for (int x = oo; x < IWidth + oo; x++)
                        {
                            RotateBMP.SetPixel(x, y, GrayImg.Bitmap.GetPixel(x - oo, y - o));
                        }
                    }
                    RotateBMP = F.RotateImage(RotateBMP, 45.0F);
                    Img BegImg2 = new Img(RotateBMP);
                    /*
                    byte[,] SdvigMtx = F.Sdvig(TempImg.GrayMatrix, TempImg.Width, TempImg.Height, out int nWidth, out int nHeight, 0, -50);
                    TempImg = new Img(SdvigMtx, nWidth, nHeight);
                    
                    byte[,] ContrastMtx = F.Contrast(TempImg.GrayMatrix, TempImg.Width, TempImg.Height, -10);
                    Img BegImg2 = new Img(ContrastMtx, TempImg.Width, TempImg.Height);
                    */
                    /*--------------------------*/

                    //Img BegImg2 = new Img(BasePath + "Temp/GrayImage.png");

                    BegImg1.Draw(pictureBox45);
                    BegImg2.Draw(pictureBox43);

                    Harris Harris1 = new Harris(BegImg1, HarrisRadius, HarrisDolya);
                    Harris1.ANMS(Npoints);
                    Harris Harris2 = new Harris(BegImg2, HarrisRadius, HarrisDolya);
                    Harris2.ANMS(Npoints);

                    Harris1.ImageWithANMS.Draw(pictureBox47);
                    Harris2.ImageWithANMS.Draw(pictureBox46);

                    Harris1.Descript5();
                    Harris2.Descript5();

                    int[] S = F.DescriptSootv5(Harris1, Harris2, 16 * 8, 0.8);
                    int rst = 20;
                    int h = Math.Max(Harris1.ImageWithANMS.Height, Harris2.ImageWithANMS.Height);
                    Bitmap SootvBmp = new Bitmap(Harris1.ImageWithANMS.Width + Harris2.ImageWithANMS.Width + rst, h, PixelFormat.Format32bppArgb);

                    for (int y = 0; y < Harris1.ImageWithANMS.Height; y++)
                    {
                        for (int x = 0; x < Harris1.ImageWithANMS.Width; x++)
                        {
                            SootvBmp.SetPixel(x, y, Harris1.ImageWithANMS.Bitmap.GetPixel(x, y));
                        }
                    }
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = Harris1.ImageWithANMS.Width; x < Harris1.ImageWithANMS.Width + rst; x++)
                        {
                            SootvBmp.SetPixel(x, y, Color.White);
                        }
                    }
                    for (int y = 0; y < Harris2.ImageWithANMS.Height; y++)
                    {
                        for (int x = Harris1.ImageWithANMS.Width + rst; x < Harris1.ImageWithANMS.Width + rst + Harris2.ImageWithANMS.Width; x++)
                        {
                            SootvBmp.SetPixel(x, y, Harris2.ImageWithANMS.Bitmap.GetPixel(x - (Harris1.ImageWithANMS.Width + rst), y));
                        }
                    }

                    Random rand = new Random();
                    Graphics g = Graphics.FromImage(SootvBmp);
                    Pen pen;
                    Point p1, p2;
                    for (int i = 0; i < Harris1.NewPoints; i++)
                    {
                        if (S[i] != -1)
                        {
                            int ra = rand.Next(0, 101);
                            if (ra >= 0 && ra < 20)
                            {
                                pen = new Pen(Brushes.Blue, 2);
                            }
                            else if (ra >= 20 && ra < 40)
                            {
                                pen = new Pen(Brushes.Green, 2);
                            }
                            else if (ra >= 40 && ra < 60)
                            {
                                pen = new Pen(Brushes.Aqua, 2);
                            }
                            else if (ra >= 60 && ra < 80)
                            {
                                pen = new Pen(Brushes.Yellow, 2);
                            }
                            else
                            {
                                pen = new Pen(Brushes.Violet, 2);
                            }

                            p1 = new Point(Harris1.IntPointsCoord[i, 1], Harris1.IntPointsCoord[i, 0]);
                            p2 = new Point(Harris2.IntPointsCoord[S[i], 1] + (Harris1.ImageWithANMS.Width + rst), Harris2.IntPointsCoord[S[i], 0]);
                            g.DrawLine(pen, p1, p2);
                        }
                    }
                    pictureBox48.Image = SootvBmp;

                    Harris1.ImageWithANMS.Save(BasePath + "Lab 5/HarrisANMS1.png");
                    Harris2.ImageWithANMS.Save(BasePath + "Lab 5/HarrisANMS2.png");
                    SootvBmp.Save(BasePath + "Lab 5/Result.png");

                }
                catch
                {
                    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /* Дескрипторы (Лаб 6)*/
        private void button12_Click(object sender, EventArgs e)
        {
            if (textBox18.Text != "" && textBox17.Text != "" && textBox16.Text != "" && textBox15.Text != "")
            {
                //try
                //{
                    int HarrisRadius = Convert.ToInt32(textBox17.Text);
                    double HarrisDolya = Convert.ToDouble(textBox16.Text);
                    int Npoints = Convert.ToInt32(textBox15.Text);
                    int Mashtab = Convert.ToInt32(textBox18.Text);

                    Img BegImg1 = new Img(GrayImg.GrayMatrix, IWidth, IHeight);

                    /*-------------------------*/
                    /*
                    int size = Convert.ToInt32(Math.Ceiling(Math.Sqrt(IWidth * IWidth + IHeight * IHeight)));
                    Bitmap RotateBMP = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            RotateBMP.SetPixel(x, y, Color.White);
                        }
                    }
                    int o = (size - IHeight) / 2;
                    int oo = (size - IWidth) / 2;
                    for (int y = o; y < IHeight + o; y++)
                    {
                        for (int x = oo; x < IWidth + oo; x++)
                        {
                            RotateBMP.SetPixel(x, y, GrayImg.Bitmap.GetPixel(x - oo, y - o));
                        }
                    }
                    RotateBMP = F.RotateImage(RotateBMP, 45.0F);
                    Img BegImg2 = new Img(RotateBMP);
                    */
                    /*
                    byte[,] SdvigMtx = F.Sdvig(TempImg.GrayMatrix, TempImg.Width, TempImg.Height, out int nWidth, out int nHeight, 0, -50);
                    TempImg = new Img(SdvigMtx, nWidth, nHeight);
                    
                    byte[,] ContrastMtx = F.Contrast(TempImg.GrayMatrix, TempImg.Width, TempImg.Height, -10);
                    Img BegImg2 = new Img(ContrastMtx, TempImg.Width, TempImg.Height);
                    */
                    /*--------------------------*/

                    //Img BegImg2 = new Img(BasePath + "Temp/GrayImage.png");

                    BegImg1.Draw(pictureBox50);
                    //BegImg2.Draw(pictureBox49);

                    SIFT SIFT1 = new SIFT(BegImg1, HarrisRadius, HarrisDolya, Npoints, Mashtab);

                
                Graphics g = Graphics.FromImage(BegImg1.Bitmap);

                for (int i = 0; i < SIFT1.InterestingPoints.Count; i++)
                {
                    int x0 = SIFT1.InterestingPoints[i].X - SIFT1.InterestingPoints[i].Radius;
                    int y0 = SIFT1.InterestingPoints[i].Y - SIFT1.InterestingPoints[i].Radius;
                    int d = SIFT1.InterestingPoints[i].Radius * 2;
                    Pen pen = new Pen(Brushes.Blue, 1);
                    g.DrawEllipse(pen, x0, y0, d, d);
                }



                pictureBox49.Image = BegImg1.Bitmap;

                //}
                //catch
                //{
                //    MessageBox.Show("Введите данные корректно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            else
            {
                MessageBox.Show("Введите данные!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
