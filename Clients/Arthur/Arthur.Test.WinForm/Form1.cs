﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Arthur.Test.WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            byte b = (byte)127;
            Console.WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            Console.WriteLine(byte.MaxValue);
        }
    }
}
