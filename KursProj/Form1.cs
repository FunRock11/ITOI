﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KursProj
{
    public partial class Form1 : Form
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        string BasePath = "../../../files/";

        Img BeginImg;
        List<Img> Images;

        public Form1()
        {
            InitializeComponent();
            BeginImg = new Img(BasePath + "Begin/BeginImage9.png");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
    }
}