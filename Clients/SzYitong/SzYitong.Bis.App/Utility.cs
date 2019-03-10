﻿using Arthur.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SzYitong.Bis.App
{
    public class Utility
    {

        public string AppTitle
        {
            get
            {
                return string.Format("{0} {1}   {2}", Current.Option.AppName, Version, UpdateTime.ToString("yyyy/M/d"));
            }
        }

        public string Version
        {
            get
            {
                string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //assemblyVersion = Regex.Match(assemblyVersion, @"^[\d]+.[\d]+.[\d]+.[\d]+").Value;
                assemblyVersion = Regex.Match(assemblyVersion, @"^[\d]+.[\d]+.[\d]+").Value;
                //只获取主版本和次版本
                return assemblyVersion;
                // return "V" + assemblyVersion;
            }
        }

        public DateTime UpdateTime
        {
            get
            {
                return System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location);
            }
        }
    }
}
