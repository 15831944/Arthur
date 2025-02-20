﻿using Arthur.App;
using Arthur.App.Data;
using Arthur.App.View.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Arthur.App.View.Account.Role
{
    /// <summary>
    /// Details.xaml 的交互逻辑
    /// </summary>
    public partial class Details : UserControl
    {
        private readonly App.AppContext _AppContext = new App.AppContext();
        public Details(int id)
        {
            InitializeComponent();
            this.DataContext = _AppContext.Roles.FirstOrDefault(o => o.Id == id);
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Helper.ExecuteParentUserControlMethod(this, "RoleManage", "SwitchWindow", "Index", 0);
        }

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            if (Current.User.Role.Level < _AppContext.Roles.Single(r => r.Name == "系统管理员").Level)
            {
                MessageBox.Show("当前用户权限不足！", "异常提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var id = Convert.ToInt32((sender as Button).Tag);
            Helper.ExecuteParentUserControlMethod(this, "RoleManage", "SwitchWindow", "Edit", id);
        }
    }
}
