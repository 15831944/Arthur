﻿using Arthur.Core;
using Arthur.App.Model;
using GMCC.Sorter.Business;
using GMCC.Sorter.Data;
using GMCC.Sorter.Extensions;
using GMCC.Sorter.Factory;
using GMCC.Sorter.Model;
using GMCC.Sorter.Utils;
using GMCC.Sorter.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using GMCC.Sorter.Other;

namespace GMCC.Sorter.Run
{
    public static class TimerExec
    {
        public static bool IsRunning { get; set; }

        public static void TaskExec(object obj)
        {
            if (!IsRunning) return;

            if (Arthur.App.Current.Option.RemainingMinutes <= 0) return;       

            try
            {
                if (Current.Task.Status == Model.TaskStatus.完成)
                {
                    if (Current.App.TaskMode == ViewModel.TaskMode.自动任务)
                    {
                        foreach (var type in Factory.TaskFactory.TaskTypes)
                        {
                            if (type == TaskType.上料 && Current.Option.Tray13_Id < 1)
                            {
                                continue;
                            }
                            else if (type == TaskType.下料 && Current.Option.Tray21_Id > 0)
                            {
                                continue;
                            }

                            if (Current.Option.IsTaskReady)
                            {
                                var storages = Factory.TaskFactory.CanGetOrPutStorages(type);
                                if (storages.Count > 0)
                                {
                                    StorageViewModel storage = storages.OrderBy(o => o.GetPriority(type, Current.Option.TaskPriorityType)).First();

                                    Current.Task.StorageId = storage.Id;
                                    Current.Task.Type = type;
                                    Current.Task.StartTime = DateTime.Now;
                                    Current.Task.ProcTrayId = type == Model.TaskType.上料 ? Current.Option.Tray13_Id : storage.ProcTrayId;
                                    Current.Task.Status = Model.TaskStatus.就绪;

                                    LogHelper.WriteInfo(string.Format("=== 生成自动任务 类型：{0}，料仓：{1}，流程托盘ID：{2}，托盘条码：{3} ===",
                                        Current.Task.Type, storage.Name, Current.Task.ProcTrayId, GetObject.GetById<ProcTray>(Current.Task.ProcTrayId).Code));

                                    break;
                                }
                            }
                        }
                    }
                }
                else if (Current.Task.Status == Model.TaskStatus.就绪)
                {
                    var storage = GetObject.GetById<StorageViewModel>(Current.Task.StorageId);
                    var toMoveInfo = JawMoveInfo.Create(Current.Task.Type, storage);

                    //若指令已经发给PLC
                    if (Current.Option.JawMoveInfo.Equals(toMoveInfo))
                    {
                        Current.Option.JawProcTrayId = Current.Task.ProcTrayId;
                        Current.Task.Status = Model.TaskStatus.准备搬;
                        return;
                    }

                    Current.MainMachine.SendCommand(toMoveInfo);

                }
                else if (Current.Task.Status == Model.TaskStatus.准备搬)
                {
                    //若指令已经发给PLC
                    if (Current.Option.IsJawHasTray)
                    {
                        Current.Task.Status = Model.TaskStatus.搬运中;
                        return;
                    }
                }
                else if (Current.Task.Status == Model.TaskStatus.搬运中)
                {
                    if (Current.Option.IsTaskFinished)
                    {
                        var storage = GetObject.GetById<StorageViewModel>(Current.Task.StorageId);
                        if (Current.Task.Type == Model.TaskType.上料)
                        {
                            storage.ProcTrayId = Current.Task.ProcTrayId;
                            storage.ProcTray.StartStillTime = DateTime.Now;
                        }
                        else
                        {
                            Current.Option.Tray21_Id = Current.Task.ProcTrayId;
                            if (storage.ProcTrayId > 0)
                            {
                                storage.ProcTray.StillTimeSpan = Convert.ToInt32((DateTime.Now - storage.ProcTray.StartStillTime).TotalMinutes);
                                storage.ProcTrayId = 0;
                            }
                        }
                        Current.Option.JawProcTrayId = 0;
                        Current.Task.Status = Model.TaskStatus.回位中;
                    }
                }
                else if (Current.Task.Status == Model.TaskStatus.回位中)
                {
                    if (Current.Option.IsTaskReady)
                    {
                        Current.Task.PreType = Current.Task.Type;
                        if (Current.Task.Type == Model.TaskType.上料)
                        {
                            Current.Option.LastFeedTaskStorageColumn = GetObject.GetById<StorageViewModel>(Current.Task.StorageId).Column;
                        }
                        Current.Task.Status = Model.TaskStatus.完成;
                        new TaskManage().AddTaskLog();
                    }
                }
            }
            catch (Exception ex)
            {
                Running.StopRunAndShowMsg("执行任务出现异常：" + ex.Message);
                LogHelper.WriteError(ex);
            }
        }

        public static void GetShareDataExec(object obj)
        {
            if (!IsRunning) return;

            if (Arthur.App.Current.Option.RemainingMinutes <= 0) return;

            if (!Current.Option.IsGetShareDataExecting)
            {
                Current.Option.IsGetShareDataExecting = true;

                if (Current.ShareDatas.Count > 0)
                {
                    if (Current.Option.Tray12_Id > 0)
                    {
                        var chargeData = Current.ShareDatas.First(o => o.Key == "chargeCodes");
                        var bindCode = JsonHelper.DeserializeJsonToObject<BindCode>(chargeData.Value);
                        var procTray = GetObject.GetById<ProcTray>(Current.Option.Tray12_Id);

                        if (procTray.Id > 0 && chargeData.Status == 2)
                        {
                            if (procTray.Id == chargeData.ProcTrayId)
                            {

                            }
                            else
                            {

                                //充电位条码绑定信息传给BTS客户端
                                var batteries = procTray.GetBatteries();
                                var codes = new List<string>();
                                for (var i = 1; i <= Common.TRAY_BATTERY_COUNT; i++)
                                {
                                    var code = "";
                                    var battery = batteries.FirstOrDefault(o => o.GetChargeOrder() == i);
                                    if (battery != null)
                                    {
                                        code = battery.Code;
                                    }
                                    codes.Add(code);
                                }

                                var value = new BindCode { TrayCode = procTray.Code, BatteryCodes = string.Join(",", codes.ToArray()) };
                                chargeData.Value = JsonHelper.SerializeObject(value);
                                chargeData.Status = 1;
                                chargeData.ProcTrayId = procTray.Id;
                                chargeData.UpdateTime = DateTime.Now;
                                LogHelper.WriteInfo(string.Format("--------成功发送充电位条码绑定信息给BTS【流程托盘ID：{0}，条码：{1}】---------", procTray.Id, procTray.Code));
                            }
                        }
                    }

                    if (Current.Option.Tray22_Id > 0)
                    {
                        var dischargeData = Current.ShareDatas.First(o => o.Key == "dischargeCodes");
                        var bindCode = JsonHelper.DeserializeJsonToObject<BindCode>(dischargeData.Value);
                        var procTray = GetObject.GetById<ProcTray>(Current.Option.Tray22_Id);

                        if (procTray.Id > 0 && dischargeData.Status == 2)
                        {
                            if (procTray.Id == dischargeData.ProcTrayId)
                            {

                            }
                            else
                            {
                                //放电位条码绑定信息传给BTS客户端
                                var batteries = procTray.GetBatteries();
                                var codes = new List<string>();
                                for (var i = 1; i <= Common.TRAY_BATTERY_COUNT; i++)
                                {
                                    var code = "";
                                    var battery = batteries.FirstOrDefault(o => o.GetChargeOrder() == i);
                                    if (battery != null)
                                    {
                                        code = battery.Code;
                                    }
                                    codes.Add(code);
                                }

                                var value = new BindCode { TrayCode = procTray.Code, BatteryCodes = string.Join(",", codes.ToArray()) };
                                dischargeData.Value = JsonHelper.SerializeObject(value);
                                dischargeData.Status = 1;
                                dischargeData.ProcTrayId = procTray.Id;
                                dischargeData.UpdateTime = DateTime.Now;
                                LogHelper.WriteInfo(string.Format("--------成功发送放电位条码绑定信息给BTS【流程托盘ID：{0}，条码：{1}】---------", procTray.Id, procTray.Code));
                            }
                        }
                    }

                    if (Current.Option.Tray23_Id > 0)
                    {
                        var procTray = GetObject.GetById<ProcTray>(Current.Option.Tray23_Id);

                        var sortingResults = Current.ShareDatas.First(o => o.Key == "sortingResults");
                        var capResults = Current.ShareDatas.First(o => o.Key == "capResults");
                        var esrResults = Current.ShareDatas.First(o => o.Key == "esrResults");

                        if (sortingResults.Status == 1 && capResults.Status == 1 && esrResults.Status == 1)
                        {
                            var sortingResult_sort = JsonHelper.DeserializeJsonToObject<SortingResult>(sortingResults.Value);
                            var sortingResult_cap = JsonHelper.DeserializeJsonToObject<SortingResult>(capResults.Value);
                            var sortingResult_esr = JsonHelper.DeserializeJsonToObject<SortingResult>(esrResults.Value);

                            if (sortingResult_sort.TrayCode == procTray.Code)
                            {
                                try
                                {
                                    var sortList = sortingResult_sort.Results.Split(',');
                                    var capList = sortingResult_cap.Results.Split(',');
                                    var esrList = sortingResult_esr.Results.Split(',');

                                    for (int i = 0; i < sortList.Length; i++)
                                    {
                                        //i:绑盘序号
                                        Current.MainMachine.Commor.Write(string.Format("D{0:D3}", 401 + i), ushort.Parse(sortList[OrderManage.GetChargeOrderBySortOrder(i + 1) - 1]));
                                    }
                                    LogHelper.WriteInfo(string.Format("--------成功发送分选结果数据给PLC【流程托盘ID：{0}，条码：{1}】---------", procTray.Id, procTray.Code));

                                    var batteries = procTray.GetBatteries();
                                    var batteryViewModels = ContextToViewModel.Convert(batteries);

                                    for (int i = 0; i < sortList.Length; i++)
                                    {
                                        //i:通道序号
                                        var sort = int.Parse(sortList[i]);
                                        var cap = decimal.Parse(capList[i]);
                                        var esr = decimal.Parse(esrList[i]);

                                        if (sort > 0)
                                        {
                                            var battery = batteryViewModels.FirstOrDefault(o => o.Pos == OrderManage.GetBindOrderByChargeOrder(i + 1));
                                            if (battery != null)
                                            {
                                                battery.SortResult = (SortResult)sort;
                                                battery.CAP = cap;
                                                battery.ESR = esr;
                                            }
                                        }
                                    }

                                    sortingResults.Status = 2;
                                    sortingResults.ProcTrayId = procTray.Id;
                                    sortingResults.UpdateTime = DateTime.Now;

                                    capResults.Status = 2;
                                    capResults.ProcTrayId = procTray.Id;
                                    capResults.UpdateTime = DateTime.Now;


                                    esrResults.Status = 2;
                                    esrResults.ProcTrayId = procTray.Id;
                                    esrResults.UpdateTime = DateTime.Now;
                                }
                                catch (Exception ex)
                                {
                                    Running.StopRunAndShowMsg(ex);
                                }
                            }
                        }

                    }
                }
                Current.Option.IsGetShareDataExecting = false;
            }
        }

        public static void ExpireTimeExec(object obj)
        {
            Arthur.App.Current.Option.RemainingMinutes--;
            if (Arthur.App.Current.Option.RemainingMinutes > 0)
            {
                if (Arthur.App.Current.Option.RemainingMinutes < 24 * 60)
                {
                    Current.App.ExpireTip = string.Format("软件即将过期，剩余时间：{0}min，双击此处输入激活码", Arthur.App.Current.Option.RemainingMinutes);
                }
                else
                {
                    Current.App.ExpireTip = "已激活";
                }
            }
            else
            {
                Current.App.ExpireTip = string.Format("软件已经过期！双击此处输入激活码");
            }
        }
    }
}
