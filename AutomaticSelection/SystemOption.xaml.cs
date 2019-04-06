using AutomaticSelection.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using static AutomaticSelection.MainWindow;

namespace AutomaticSelection
{
    public partial class SystemOption : Window
    {
        public MainWindow context;

        public SystemOption()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void BtnSaveInterval_Click(object sender, RoutedEventArgs e)
        {
            bool ok = int.TryParse(txtInterval.Text, out int interval);
            if (!ok)
            {
                MessageBox.Show("错误的数值", "不能保存设置", MessageBoxButton.OK, MessageBoxImage.Error);
                txtInterval.Text = Settings.Default.freq.ToString();
                return;
            }

            context.dynamicSettings.Interval = interval;
            Settings.Default.freq = interval;
            Settings.Default.Save();
        }

        private void BtnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认删除历史记录？这项操作将不可恢复。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                if (File.Exists(HistoryFilePath))
                {
                    File.Delete(HistoryFilePath);
                    context.ReloadHistory();
                }
            }

            context.RefreshList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddModify addModify = new AddModify(context);
            if(addModify.ShowDialog() == true)
            {
                // Get the result
                NamePair pair = addModify.pair;

                // Add a new line to the dictionary
                context.nameDictionary.Add(pair);

                // Save the new dictionary
                context.SaveNewDictionary();

                // Refresh the list
                context.RefreshList();
            }

            btnAdd.Focus();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            // Get the selection
            if (lstNames.SelectedIndex >= 0)
            {
                NamePair pair = (NamePair)lstNames.Items[lstNames.SelectedIndex];

                // Search and delete
                int index = context.nameDictionary.FindIndex((NamePair p) => (p.id == pair.id) && (p.name == pair.name));

                if(index >= 0)
                    context.nameDictionary.RemoveAt(index);

                // Save the new dictionary
                context.SaveNewDictionary();

                // Refresh the list
                context.RefreshList();
            }
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            // Get the selection
            if (lstNames.SelectedIndex >= 0)
            {
                NamePair pair = (NamePair)lstNames.Items[lstNames.SelectedIndex];

                // Search and modify
                int index = context.nameDictionary.FindIndex((NamePair p) => (p.id == pair.id) && (p.name == pair.name));

                if (index >= 0)
                {
                    AddModify addModify = new AddModify(context, pair.id, pair.name);
                    if (addModify.ShowDialog() == true)
                    {
                        // Get the result
                        NamePair pair_mod = addModify.pair;

                        // Add a new line to the dictionary
                        context.nameDictionary[index] = pair_mod; 

                        // Save the new dictionary
                        context.SaveNewDictionary();

                        // Refresh the list
                        context.RefreshList();
                    }
                    else
                    {
                        return;
                    }
                }

                // Save the new dictionary
                context.SaveNewDictionary();

                // Refresh the list
                context.RefreshList();
            }
        }
    }
}
