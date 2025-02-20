﻿using Arthur.Core;
using Arthur.App.View.Utils;
using GMCC.Sorter.Data;
using GMCC.Sorter.Model;
using GMCC.Sorter.Other;
using GMCC.Sorter.Run;
using GMCC.Sorter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GMCC.Sorter.Dispatcher.UserControls.Setting.Option
{
    /// <summary>
    /// Edit.xaml 的交互逻辑
    /// </summary>
    public partial class Edit : UserControl
    {
        public Edit(int id)
        {
            InitializeComponent();
            this.DataContext = Current.Option;
        }

        private void textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            tip.Visibility = Visibility.Collapsed;
        }

        private void level_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Helper.ExecuteParentUserControlMethod(this, "MainMachine", "SwitchWindow", "Details", 0);
        }

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            var tray11_code = this.tray11_code.Text.Trim();
            var tray12_code = this.tray12_code.Text.Trim();
            var tray13_code = this.tray13_code.Text.Trim();
            var tray21_code = this.tray21_code.Text.Trim();
            var tray22_code = this.tray22_code.Text.Trim();
            var tray23_code = this.tray23_code.Text.Trim();

            var jaw_traycode = this.jaw_traycode.Text.Trim();
            var still_timespan = this.still_timespan.Text.Trim();

            var product_model = this.product_model.Text.Trim();

            tip.Background = new SolidColorBrush(Colors.Red);

            try
            {

                var tray11_id = GetObject.GetByCode<ProcTray>(tray11_code).Id;
                if (tray11_id != Current.Option.Tray11_Id && tray11_id > 0)
                {
                    var result = Current.MainMachine.Commor.Write("D434", (ushort)1);
                    if (!result.IsSucceed)
                    {
                        Running.ShowErrorMsg("手动输入绑盘托盘条码后发送给PLC结果失败，msg：" + result.Msg);
                    }
                }

                Current.Option.Tray11_Id = tray11_id;
                Current.Option.Tray12_Id = GetObject.GetByCode<ProcTray>(tray12_code).Id;
                Current.Option.Tray13_Id = GetObject.GetByCode<ProcTray>(tray13_code).Id;
                Current.Option.Tray21_Id = GetObject.GetByCode<ProcTray>(tray21_code).Id;
                Current.Option.Tray22_Id = GetObject.GetByCode<ProcTray>(tray22_code).Id;
                Current.Option.Tray23_Id = GetObject.GetByCode<ProcTray>(tray23_code).Id;

                Current.Option.JawProcTrayId = GetObject.GetByCode<ProcTray>(jaw_traycode).Id;
                Current.Option.StillTimeSpan = Convert.ToInt32(still_timespan);
                Current.Option.TaskPriorityType = GetTaskPriorityType();

                Current.Option.ProductModel = product_model;

                tip.Background = new SolidColorBrush(Colors.Green);
                tip.Text = "修改信息成功！";
            }
            catch (Exception ex)
            {

                tip.Text = "修改信息失败：" + ex.Message;
            }

            tip.Visibility = Visibility.Visible;
        }

        private TaskPriorityType GetTaskPriorityType()
        {
            for (int i = 0; i < 2; i++)
            {
                var radioButton = ControlsSearchHelper.GetChildObject<RadioButton>(taskPriorityType, string.Format("taskPriorityType{0}", i + 1));
                if (radioButton.IsChecked.Value)
                {
                    return (TaskPriorityType)Enum.Parse(typeof(TaskPriorityType), radioButton.Content.ToString());
                }
            }
            return TaskPriorityType.未知;
        }
    }
}
