using AutomaticSelection.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AutomaticSelection
{

    public partial class MainWindow : Window
    {
        SystemOption systemOption;

        public const string DictionaryFilePath = "list.csv";
        public const string HistoryFilePath = "history.csv";
        public const int SelectInterval = 10; // The interval of two final selections 

        public struct NamePair
        {
            public string id;
            public string name;

            public override string ToString() => $"{id},{name}";
        }

        public List<NamePair> nameDictionary;
        public List<NamePair> historyDictionary;

        public class DynamicSettings : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private int interval = 50;

            public int Interval
            {
                get => interval;
                set
                {
                    interval = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interval)));
                }
            }


            private int count = 0;

            public int Count
            {
                get => count;
                set
                {
                    count = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                }
            }
        }

        public DynamicSettings dynamicSettings;

        public enum SystemOptionTab
        {
            Alternative = 0,
            History = 1,
            Frequency = 2
        };

        public MainWindow()
        {
            InitializeComponent();

            ReInitializeOptionDialog();

            // Set dynamic properties and bindings
            dynamicSettings = new DynamicSettings();
            lblInterval.SetBinding(ContentProperty, new Binding()
            {
                Source = dynamicSettings,
                Converter = new IntervalConvertor(),
                Path = new PropertyPath(nameof(dynamicSettings.Interval)),
            });

            lblCount.SetBinding(ContentProperty, new Binding()
            {
                Source = dynamicSettings,
                Path = new PropertyPath(nameof(dynamicSettings.Count)),
            });

            // Reload names
            ReloadNames();

            // Reload history
            ReloadHistory();

            // Refresh the list
            RefreshList();

            // Load interval
            dynamicSettings.Interval = Settings.Default.freq;
            systemOption.txtInterval.Text = dynamicSettings.Interval.ToString();
        }

        public void SaveNewDictionary()
        {
            if (nameDictionary != null)
            {
                using (FileStream dictionaryFile = File.Open(DictionaryFilePath, FileMode.OpenOrCreate))
                {
                    StreamWriter writer = new StreamWriter(dictionaryFile);

                    foreach (NamePair pair in nameDictionary) 
                    {
                        writer.WriteLine($"{pair.id},{pair.name}");
                    }
                }
            }
        }

        public void SaveNewHistory()
        {
            if (historyDictionary != null)
            {
                using (FileStream historyFile = File.Open(HistoryFilePath, FileMode.OpenOrCreate))
                {
                    StreamWriter writer = new StreamWriter(historyFile);

                    foreach (NamePair pair in historyDictionary)
                    {
                        writer.WriteLine($"{pair.id},{pair.name}");
                    }

                    writer.Close();
                }
            }
        }

        public void ReloadNames()
        {
            // Read the dictionary from file
            nameDictionary = new List<NamePair>();
            using (FileStream dictionaryFile = File.Open(DictionaryFilePath, FileMode.OpenOrCreate))
            {
                StreamReader reader = new StreamReader(dictionaryFile);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] pair_str = line.Split(',');
                    NamePair pair = new NamePair();

                    if (pair_str.Length == 2)
                    {
                        pair.id = pair_str[0];
                        pair.name = pair_str[1];
                    }
                    else
                    {
                        pair.name = pair_str[0];
                    }

                    nameDictionary.Add(pair);
                }
            }
        }

        public void ReloadHistory()
        {
            // Read the history
            historyDictionary = new List<NamePair>();
            using (FileStream historyFile = File.Open(HistoryFilePath, FileMode.OpenOrCreate))
            {
                StreamReader reader = new StreamReader(historyFile);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] pair_str = line.Split(',');
                    NamePair pair = new NamePair();

                    if (pair_str.Length == 2)
                    {
                        pair.id = pair_str[0];
                        pair.name = pair_str[1];
                    }
                    else
                    {
                        pair.name = pair_str[0];
                    }

                    historyDictionary.Add(pair);
                }
            }
        }

        public void ReInitializeOptionDialog()
        {
            systemOption = new SystemOption();
            systemOption.context = this;
        }

        public void RefreshList()
        {
            // Add the dictionary to the list
            systemOption.lstNames.Items.Clear();
            systemOption.lstHistory.Items.Clear();

            // Re-add the items
            foreach (NamePair pair in nameDictionary)
                systemOption.lstNames.Items.Add(pair);

            foreach (NamePair pair in historyDictionary)
                systemOption.lstHistory.Items.Add(pair);
        }

        private void AlternativeList_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            systemOption.tabOption.SelectedIndex = (int)SystemOptionTab.Alternative;
            systemOption.ShowDialog();
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            systemOption.tabOption.SelectedIndex = (int)SystemOptionTab.History;
            systemOption.ShowDialog();
        }

        private void Frequency_Click(object sender, RoutedEventArgs e)
        {
            systemOption.tabOption.SelectedIndex = (int)SystemOptionTab.Frequency;
            systemOption.ShowDialog();
        }

        volatile int timer_count = 0;
        int maximun_count = 0;

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // Clean lists
            lstSelected.Items.Clear();

            // Request the number
            int count = 0;
            SelectCount selectCount = new SelectCount();
            if (selectCount.ShowDialog() == true)
            {
                count = selectCount.number;
            }
            else
                return;

            // Build a new dictionary
            List<NamePair> dictionary = new List<NamePair>(nameDictionary);
            List<NamePair> selected = new List<NamePair>();
            if (chkNoHistory.IsChecked == true)
            {
                foreach(NamePair pair in historyDictionary)
                {
                    int index = dictionary.FindIndex((NamePair p) => (p.id == pair.id) && (p.name == pair.name));
                    if (index >= 0)
                        dictionary.RemoveAt(index);
                }
            }

            // Check the number
            if ((dictionary.Count == 1) || (dictionary.Count == 0))
            {
                MessageBox.Show("人数过少，无法进行抽选。请尝试添加新的人员，或清空历史记录。", "抽选", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if ((count <= 0) || (count >= dictionary.Count))
            {
                MessageBox.Show("超出最大抽选数量，改用总人数减一进行抽选", "抽选", MessageBoxButton.OK, MessageBoxImage.Information);
                count = dictionary.Count - 1;
            }

            // Disable the button
            btnStart.IsEnabled = false;

            // Reset timer
            timer_count = 0;
            maximun_count = (count - 1) * SelectInterval + 1;

            // Start timer
            Random random = new Random();
            Timer timer = new Timer(dynamicSettings.Interval);
            timer.Elapsed += delegate
            {
                int index = random.Next(dictionary.Count);
                NamePair pair = dictionary[index];
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    lblId.Content = pair.id;
                    lblName.Content = pair.name;
                }));

                if(timer_count % SelectInterval == 0)
                {
                    // Select it
                    selected.Add(pair);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lstSelected.Items.Add(pair);
                    }));

                    // Remove from the list
                    dictionary.RemoveAt(index);
                }

                timer_count++;
                dynamicSettings.Count = timer_count;

                if (timer_count == maximun_count)
                {
                    timer.Stop();

                    // Add results to history
                    foreach(NamePair sel_pair in selected)
                    {
                        historyDictionary.Add(sel_pair);
                    }

                    // Save the dictionary
                    SaveNewHistory();

                    // Refresh the history
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RefreshList();
                    }));

                    // Enable the button
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        btnStart.IsEnabled = true;
                    }));

                    MessageBox.Show("抽选完毕", "抽选", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            timer.Start();
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认删除历史记录？这项操作将不可恢复。", "警告", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                if (File.Exists(MainWindow.HistoryFilePath))
                {
                    File.Delete(MainWindow.HistoryFilePath);
                    ReloadHistory();
                }
            }

            RefreshList();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
