using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Text;
using System.IO;
using GBKEncoding;
using System.Text.RegularExpressions;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;

namespace ClassSchedule {

    public partial class MainPage : PhoneApplicationPage {

        private ListBox[] listBoxes;
        private TextBlock[] emptyTexts;
        private int WeekDisplaying = 0;

        // Constructor
        public MainPage() {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            var days = new string[] { "sun", "mon", "tue", "wed", "thu", "fri", "sat" };

            listBoxes = new ListBox[days.Length];
            emptyTexts = new TextBlock[days.Length];

            for (var i = 0; i < days.Length; i++ ) {
                var day = days[i];
                var item = new PivotItem() {
                };

                item.Header = day;

                var listBox = new ListBox() {
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                var emptyText = new TextBlock() {
                    Text = "empty",
                    Style = Resources["PhoneTextSubtleStyle"] as Style,
                    Margin = new Thickness() { Top = -50 }
                };

                listBoxes[i] = listBox;
                emptyTexts[i] = emptyText;
                /*
                var stackPanel = new StackPanel() {

                };
                stackPanel.Children.Add(emptyText);
                stackPanel.Children.Add(listBox);

                item.Content = stackPanel;*/
                
                //item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                //item.VerticalContentAlignment = VerticalAlignment.Stretch;
                mainPivot.Items.Add(item);
            }

            mainPivot.SelectedIndex = GetDayOfWeek();
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e) {

            if (Config.Terminated) {
                Config.Terminated = false;
                return;
            }

            if (!Config.Load()) {
                NavigationService.Navigate(new Uri("/SelectUniversity.xaml", UriKind.Relative));
                return;
            }

            Classes.Load();

            SwitchToToday(true);

            if (Classes.Count == 0) {
                var result = MessageBox.Show("no class has been added, do you want to import?", "IMPORT", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    NavigationService.Navigate(new Uri("/ImportClassSchedule.xaml", UriKind.Relative));
                return;
            }
        }

        private void LoadClassScheduleForWeek(int week) {

            mainPivot.Title = "CLASS SCHEDULE (the " + AddOrdinal(week) + " week)";

            var classes = Classes.GetClassesForWeek(week);

            for (var i = 0; i < mainPivot.Items.Count; i++) {
                var pivotItem = mainPivot.Items[i] as PivotItem;
                var listBox = listBoxes[i];
                var emptyText = emptyTexts[i];
                listBox.Items.Clear();

                var cls = classes[i];
                var infos = new List<SpecificClassInfo>();

                var remain = cls.Count;

                if (remain == 0) {
                    pivotItem.VerticalContentAlignment = VerticalAlignment.Center;
                    pivotItem.HorizontalContentAlignment = HorizontalAlignment.Center;
                    pivotItem.Content = emptyText;
                    //listBox.Visibility = Visibility.Collapsed;
                    //emptyText.Visibility = Visibility.Visible;
                    continue;
                }
                else {
                    pivotItem.VerticalContentAlignment = VerticalAlignment.Stretch;
                    pivotItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    pivotItem.Content = listBox;
                    //listBox.Visibility = Visibility.Visible;
                    //emptyText.Visibility = Visibility.Collapsed;
                }

                for (var j = 1; remain > 0; j++)
                    if (cls.ContainsKey(j)) {
                        var cl = cls[j];
                        if (infos.Count == 0 || infos.Last().Name != cl.Name) {
                            infos.Add(new SpecificClassInfo() {
                                Name = cl.Name,
                                StartsAt = Config.SessionStartTimes[j],
                                EndsAt = Config.SessionEndTimes[j],
                                Teacher = cl.Teacher,
                                Location = cl.Location
                            });
                        }
                        else if (Config.SessionEndTimes.ContainsKey(j))
                            infos.Last().EndsAt = Config.SessionEndTimes[j];
                        remain--;
                    }

                var pChr = '-';
                var pNames = new Dictionary<char, string>() {
                    {'m', "morning"},
                    {'a', "afternoon"},
                    {'e', "evening"}
                };

                foreach (var info in infos) {
                    var startsAt = info.StartsAt;
                    if (pChr != startsAt[0]) {
                        pChr = startsAt[0];
                        listBox.Items.Add(new ListBoxItem() {
                            Content = new TextBlock() {
                                Text = pNames[pChr],
                                Margin = new Thickness() { Left = 12 }
                            }
                        });
                    }

                    

                    startsAt = startsAt.Substring(1);

                    var stackPanel = new StackPanel() {
                        Margin = new Thickness() { Bottom = 17 }
                    };

                    stackPanel.Children.Add(new TextBlock() {
                        Text = info.Name,
                        Style = Resources["PhoneTextLargeStyle"] as Style
                    });

                    stackPanel.Children.Add(new TextBlock() {
                        Text = info.Teacher,
                        Style = Resources["PhoneTextSubtleStyle"] as Style
                    });

                    stackPanel.Children.Add(new TextBlock() {
                        Text = (info.Location.Length > 0 ? info.Location + " " : "") + startsAt + "-" + info.EndsAt,
                        Style = Resources["PhoneTextSubtleStyle"] as Style
                    });

                    listBox.Items.Add(new ListBoxItem() {
                        Content = stackPanel
                    });
                }

            }


        }

        private void importMenuItem_Click(object sender, System.EventArgs e) {
            NavigationService.Navigate(new Uri("/ImportClassSchedule.xaml", UriKind.Relative));
        }

        private void changeUniversityMenuItem_Click(object sender, System.EventArgs e) {
            NavigationService.Navigate(new Uri("/SelectUniversity.xaml", UriKind.Relative));
        }

        private void aboutMenuItem_Click(object sender, System.EventArgs e) {
            MessageBox.Show("Class Schedule for CQU\nBy VILIC VANE\nwww.vilic.info", "ABOUT", MessageBoxButton.OK);
        }

        private void lastWeekButton_Click(object sender, System.EventArgs e) {
            if (WeekDisplaying == 1) {
                MessageBox.Show("already the first week", "NOTICE", MessageBoxButton.OK);
                return;
            }

            LoadClassScheduleForWeek(--WeekDisplaying);
        }

        private void nextWeekButton_Click(object sender, System.EventArgs e) {
            if (WeekDisplaying == Config.WeekCount) {
                MessageBox.Show("already the last week", "NOTICE", MessageBoxButton.OK);
                return;
            }

            LoadClassScheduleForWeek(++WeekDisplaying);
        }

        private void todayButton_Click(object sender, System.EventArgs e) {
            SwitchToToday();
        }

        private void SwitchToToday(bool reload = false) {
            var week = Time.ThisWeek;
            if (week > Config.WeekCount)
                week = Config.WeekCount;
            else if (week < 1) week = 1;

            if (reload || WeekDisplaying != Time.ThisWeek)
                LoadClassScheduleForWeek(WeekDisplaying = Time.ThisWeek);

            mainPivot.SelectedIndex = GetDayOfWeek();
        }

        private int GetDayOfWeek() {
            return (new Dictionary<DayOfWeek, int>() {
                {DayOfWeek.Sunday, 0},
                {DayOfWeek.Monday, 1},
                {DayOfWeek.Tuesday, 2},
                {DayOfWeek.Wednesday, 3},
                {DayOfWeek.Thursday, 4},
                {DayOfWeek.Friday, 5},
                {DayOfWeek.Saturday, 6}
            })[DateTime.Now.DayOfWeek];
        }

        private string AddOrdinal(int num) {
            switch (num % 100) {
                case 11:
                case 12:
                case 13:
                    return num.ToString() + "th";
            }

            switch (num % 10) {
                case 1:
                    return num.ToString() + "st";
                case 2:
                    return num.ToString() + "nd";
                case 3:
                    return num.ToString() + "rd";
                default:
                    return num.ToString() + "th";
            }

        }
    }
    
}