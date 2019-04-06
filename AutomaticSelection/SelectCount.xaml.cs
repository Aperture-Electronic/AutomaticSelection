using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutomaticSelection
{
    /// <summary>
    /// SelectCount.xaml 的交互逻辑
    /// </summary>
    public partial class SelectCount : Window
    {
        public int number;

        public SelectCount(int defaultCount = 1)
        {
            InitializeComponent();
            txtNumber.Text = defaultCount.ToString();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            bool s = int.TryParse(txtNumber.Text, out int result);

            if(!s)
            {
                MessageBox.Show("无效的数量", "无法抽选", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                return;
            }

            if (result <= 0)
            {
                MessageBox.Show("无效的数量", "无法抽选", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                return;
            }

            number = result;
            DialogResult = true;
            return;
        }
    }
}
