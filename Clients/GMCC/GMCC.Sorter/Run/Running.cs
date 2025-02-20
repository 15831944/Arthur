﻿using Arthur.Core;
using GMCC.Sorter.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMCC.Sorter.Run
{
    public static class Running
    {
        public static Result Start()
        {
            var commors = Factory.CommorFactory.GetCommors();
            for (var i = 0; i < commors.Count; i++)
            {
                if (commors[i].IsEnabled)
                {
                    var result = commors[i].Commor.Connect();
                    if (!result.IsSucceed)
                    {
                        Current.App.RunStatus = RunStatus.异常;
                        return result;
                    }
                    commors[i].IsAlive = true;
                    commors[i].RealtimeStatus = "连接成功！";
                }
            }

            Current.App.RunStatus = RunStatus.运行;
            TimerExec.IsRunning = true;
            return Result.Success;
        }

        public static Result Stop()
        {
            var commors = Factory.CommorFactory.GetCommors();
            for (var i = 0; i < commors.Count; i++)
            {
                if (commors[i].IsEnabled)
                {
                    commors[i].RealtimeStatus = "运行停止！";
                }
            }
            TimerExec.IsRunning = false;
            Current.App.RunStatus = RunStatus.停止;
            return Result.Success;
        }

        public static Result Reset()
        {
            var commors = Factory.CommorFactory.GetCommors();
            for (var i = 0; i < commors.Count; i++)
            {
                if (commors[i].IsEnabled)
                {
                    commors[i].Commor.EndConnect();
                    commors[i].RealtimeStatus = "尚未连接！";
                    commors[i].IsAlive = false;
                }
            }
            TimerExec.IsRunning = false;
            Current.App.RunStatus = RunStatus.闲置;
            Current.App.ErrorMsg = "";

            return Result.Success;
        }

        public static void StopRunAndShowMsg(string msg)
        {
            Current.MainMachine.Commor.Write("D437", (ushort)1);
            Current.App.ErrorMsg = string.Format("[{0}] {1}", DateTime.Now.ToString("M/d HH:mm:ss"), msg);
            Arthur.App.Business.Logging.AddEvent(msg, Arthur.App.Model.EventType.错误);
            LogHelper.WriteError("【严重异常】" + msg);
            Current.App.RunStatus = RunStatus.异常;
            TimerExec.IsRunning = false;
        }

        public static void StopRunAndShowMsg(Exception ex)
        {
            Current.MainMachine.Commor.Write("D437", (ushort)1);
            Current.App.ErrorMsg = string.Format("[{0}] {1}", DateTime.Now.ToString("M/d HH:mm:ss"), ex.Message);
            Arthur.App.Business.Logging.AddEvent(ex.Message, Arthur.App.Model.EventType.错误);
            LogHelper.WriteError("【严重异常】" + ex.ToString());
            Current.App.RunStatus = RunStatus.异常;
            TimerExec.IsRunning = false;
        }

        public static void ShowErrorMsg(string msg)
        {
            Current.App.ErrorMsg = string.Format("[{0}] {1}", DateTime.Now.ToString("M/d HH:mm:ss"), msg);
            Arthur.App.Business.Logging.AddEvent(msg, Arthur.App.Model.EventType.警告);
            LogHelper.WriteError(msg);
        }
    }
}
