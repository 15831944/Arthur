﻿using Arthur.Core;
using Arthur.App;
using Arthur.App.Comm;
using Arthur.App.Model;
using GMCC.Sorter.Business;
using GMCC.Sorter.Data;
using GMCC.Sorter.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GMCC.Sorter.Model;

namespace GMCC.Sorter.ViewModel
{
    /// <summary>
    /// 电池扫码枪
    /// </summary>
    public sealed class BatteryScanerViewModel : EthernetCommorViewModel, IScanerViewModel
    {

        private string scanCommand = null;
        /// <summary>
        /// 扫码指令
        /// </summary>
        public string ScanCommand
        {
            get
            {
                if (scanCommand == null)
                {
                    scanCommand = ((BatteryScaner)this.Commor.Communicator).ScanCommand;
                }
                return scanCommand;
            }
            set
            {
                if (scanCommand != value)
                {
                    using (var db = new Data.AppContext())
                    {
                        db.BatteryScaners.FirstOrDefault(o => o.Id == this.Id).ScanCommand = value;
                        db.SaveChanges();
                    }

                    Arthur.App.Business.Logging.AddOplog(string.Format("设备管理. {0}扫码指令: [{1}] 修改为 [{2}]", Name, scanCommand, value), Arthur.App.Model.OpType.编辑);
                    SetProperty(ref scanCommand, value);
                }
            }
        }

        public BatteryScanerViewModel(Commor commor) : base(commor)
        {

        }

        public void Comm()
        {
            if (Arthur.App.Current.Option.RemainingMinutes <= 0) return;

            if (Current.MainMachine.IsAlive && Current.Option.IsBatteryScanReady && !Current.Option.IsAlreadyBatteryScan && Current.Option.Tray11_Id > 0)
            {
                //绑盘位电池已满，不扫码，直到出现新托盘再扫
                if (ProcTrayManage.GetBatteryCount(Current.Option.Tray11_Id) >= Common.TRAY_BATTERY_COUNT)
                {
                    Running.ShowErrorMsg("绑盘位扫码电池数超过最大值：" + Common.TRAY_BATTERY_COUNT);
                    return;
                }

                Current.Option.IsAlreadyBatteryScan = true;
                var ret = this.Commor.Comm(this.ScanCommand, this.ReadTimeout);
                if (!ret.IsSucceed || ret.Data.ToString().StartsWith("NG"))
                {
                    ret = this.Commor.Comm(this.ScanCommand, this.ReadTimeout);
                    if (!ret.IsSucceed || ret.Data.ToString().StartsWith("NG"))
                    {
                        ret = this.Commor.Comm(this.ScanCommand, this.ReadTimeout);
                        if (!ret.IsSucceed || ret.Data.ToString().StartsWith("NG"))
                        {
                            var msg = ret.Data.ToString().StartsWith("NG") ? "扫码NG" : " 扫码失败！" + ret.Msg;
                            this.RealtimeStatus = msg;
                            Current.MainMachine.Commor.Write("D433", (ushort)2);
                            Running.ShowErrorMsg(this.Name + msg);
                            this.IsAlive = false;
                            return;
                        }
                    }
                }

                var code = ret.Data.ToString();
                this.RealtimeStatus = "+" + code;
                Current.MainMachine.Commor.Write("D433", (ushort)1);
                this.IsAlive = true;

                //把电池条码保存进数据库
                var saveRet = new Business.BatteryManage().Create(new Model.Battery() { Code = code }, true);
                if (saveRet.IsSucceed)
                {
                    var t = new Thread(() =>
                    {
                        //界面交替显示扫码状态
                        Thread.Sleep(2000);
                        this.RealtimeStatus = "等待扫码...";
                    });
                    t.Start();
                }
                else
                {
                    Running.StopRunAndShowMsg(saveRet.Msg);
                }
            }
        }
    }
}
