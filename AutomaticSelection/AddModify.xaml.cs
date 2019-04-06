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
using static AutomaticSelection.MainWindow;

namespace AutomaticSelection
{
    /// <summary>
    /// AddModify.xaml 的交互逻辑
    /// </summary>
    public partial class AddModify : Window
    {
        public MainWindow context;

        public NamePair pair;

        public AddModify(MainWindow context, string DefaultId = "", string DefaultName = "")
        {
            InitializeComponent();

            this.context = context;
            pair.id = DefaultId;
            pair.name = DefaultName;

            txtId.Text = DefaultId;
            txtName.Text = DefaultName;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text) || string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("ID和名称不可为空！", "无法保存", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            pair.id = txtId.Text;
            pair.name = txtName.Text;

            DialogResult = true;
        }
    }
}
