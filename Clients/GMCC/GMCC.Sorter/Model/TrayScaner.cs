﻿using Arthur.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMCC.Sorter.Model
{
    /// <summary>
    /// 托盘扫码枪
    /// </summary>
    public sealed class TrayScaner : SerialCommor, IScaner
    {
        public string ScanCommand { get; set; }
    }
}
