﻿using Arthur.App.View.Utils;
using GMCC.Sorter.Data;
using GMCC.Sorter.ViewModel;
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

namespace GMCC.Sorter.Dispatcher.UserControls.Machine.Tray
{
    /// <summary>
    /// Edit.xaml 的交互逻辑
    /// </summary>
    public partial class Edit : UserControl
    {
        private readonly Data.AppContext _AppContext = new Data.AppContext();

        private GMCC.Sorter.Model.Tray Tray;

        public Edit(int id)
        {
            InitializeComponent();
            this.Tray = _AppContext.Trays.Single(t => t.Id == id);
            this.DataContext = this.Tray;
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
            Helper.ExecuteParentUserControlMethod(this, "Tray", "SwitchWindow", "Index", 0);
        }

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            var company = this.company.Text.Trim();

            try
            {
                this.Tray.Company = company;
                _AppContext.SaveChanges();

                tip.Background = new SolidColorBrush(Colors.Green);
                tip.Text = "修改信息成功！";

            }
            catch (Exception ex)
            {
                tip.Background = new SolidColorBrush(Colors.Red);
                tip.Text = "修改信息失败：" + ex.Message;
            }

            tip.Visibility = Visibility.Visible;
        }
    }
}
