﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arthur.App.Model
{
    /// <summary>
    /// 系统事件日志
    /// </summary>
    public class EventLog : Logging
    {

        public EventLog() : this(-1)
        {

        }

        public EventLog(int id)
        {
            Id = id;
        }


        #region 属性

        /// <summary>
        /// 事件内容
        /// </summary>
        [MaxLength(255)]
        public string Content { get; set; }

        public EventType EventType { get; set; }

        #endregion
    }

    public enum EventType
    {
        信息 = 1,
        警告 = 2,
        错误 = 3
        //[Description("信息")]
        //Info = 1,
        //[Description("警告")]
        //Warn = 2,
        //[Description("错误")]
        //Error = 3
    }

}
