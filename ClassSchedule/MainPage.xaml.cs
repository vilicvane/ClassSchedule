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
                var item = new PivotItem();

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

                mainPivot.Items.Add(item);
            }

            //mainPivot.SelectedIndex = GetDayOfWeek();
            SwitchToToday();
        }

        private bool loaded = false;

        private void MainPage_Loaded(object sender, RoutedEventArgs e) {
            new Navigation(NavigationService).ClearHistory();

            if (!loaded) {
                loaded = true;
                if (Schedule.ClassInfos == null || Schedule.UniversityInfo == null) {
                    if (Schedule.UniversityInfo != null)
                        NavigationService.Source = Uris.ImportClassSchedule;
                    else
                        NavigationService.Source = Uris.SelectUniversity;
                    return;
                }
            }

            SwitchToToday();
        }

        private bool mainPivotItemChanging = false;

        void mainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (mainPivotItemChanging) return;

            mainPivotItemChanging = true;
            var added = e.AddedItems[0];
            var removed = e.RemovedItems[0];
            var sunItem = mainPivot.Items[0];
            var monItem = mainPivot.Items[1];
            if (added == monItem && removed == sunItem) {
                if (!GoNextWeek()) {
                    mainPivot.SelectedItem = removed;
                }
            }
            else if (added == sunItem && removed == monItem) {
                if (!GoLastWeek()) {
                    mainPivot.SelectedItem = removed;
                }
            }
            mainPivotItemChanging = false;
        }

        private void LoadClassScheduleForWeek(int week) {

            mainPivot.Title = "CLASS SCHEDULE (the " + AddOrdinal(week) + " week)";

            var classes = Schedule.GetClassesForWeek(week);

            for (var i = 0; i < mainPivot.Items.Count; i++) {
                var pivotItem = mainPivot.Items[i] as PivotItem;
                var listBox = listBoxes[i];
                var emptyText = emptyTexts[i];
                listBox.Items.Clear();

                var cls = classes[i];
                var infos = new List<ClassPeriodInfo>();

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

                ClassPeriodInfo lastInfo = null;

                for (var j = 1; remain > 0; j++)
                    if (cls.ContainsKey(j)) {
                        var cl = cls[j];
                        var session = Schedule.GetSession(j);

                        remain--;
                        if (session == null) continue;

                        if (lastInfo == null || lastInfo.Name != cl.Name) {
                            lastInfo = new ClassPeriodInfo() {
                                Name = cl.Name,
                                PeriodName = Schedule.GetSessionPeriodName(j),
                                StartTime = session.StartTime,
                                EndTime = session.EndTime,
                                Teacher = cl.Teacher,
                                Location = cl.Location
                            };

                            infos.Add(lastInfo);
                        }
                        else
                            lastInfo.EndTime = session.EndTime;
                    }

                var pName = "";

                foreach (var info in infos) {
                    if (pName != info.PeriodName) {
                        pName = info.PeriodName;
                        listBox.Items.Add(new ListBoxItem() {
                            Content = new TextBlock() {
                                Text = info.PeriodName.ToLower(),
                                Margin = new Thickness() { Left = 12 }
                            }
                        });
                    }

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
                        Text = (info.Location.Length > 0 ? info.Location + " " : "") + info.StartTime + "-" + info.EndTime,
                        Style = Resources["PhoneTextSubtleStyle"] as Style
                    });

                    listBox.Items.Add(new ListBoxItem() {
                        Content = stackPanel
                    });
                }

            }


        }

        private bool GoLastWeek() {
            if (WeekDisplaying == 1) return false;
            LoadClassScheduleForWeek(--WeekDisplaying);
            return true;
        }

        private bool GoNextWeek() {
            if (Schedule.UniversityInfo == null || WeekDisplaying == Schedule.UniversityInfo.WeekCount) return false;
            LoadClassScheduleForWeek(++WeekDisplaying);
            return true;
        }

        private void importMenuItem_Click(object sender, System.EventArgs e) {
            if (Schedule.UniversityInfo != null)
                NavigationService.Source = Uris.ImportClassSchedule;
            else
                NavigationService.Source = Uris.SelectUniversity;
        }

        private void aboutMenuItem_Click(object sender, System.EventArgs e) {
            MessageBox.Show("Class Schedule\nBy VILIC VANE\nwww.vilic.info", "ABOUT", MessageBoxButton.OK);
        }

        private void lastWeekButton_Click(object sender, System.EventArgs e) {
            if (!GoLastWeek())
                MessageBox.Show("already the first week", "NOTICE", MessageBoxButton.OK);
        }

        private void nextWeekButton_Click(object sender, System.EventArgs e) {
            if (!GoNextWeek())
                MessageBox.Show("already the last week", "NOTICE", MessageBoxButton.OK);
        }

        private void todayButton_Click(object sender, System.EventArgs e) {
            SwitchToToday();
        }

        private void SwitchToToday(bool reload = false) {
            var week = Time.ThisWeek;
            var dayOfWeek = GetDayOfWeek();
            var universityInfo = Schedule.UniversityInfo;
            if (universityInfo != null) {
                if (week > universityInfo.WeekCount) {
                    week = universityInfo.WeekCount;
                    dayOfWeek = 0;
                }
                else if (week < 1) {
                    week = 1;
                    dayOfWeek = 1;
                }
            }

            if (reload || WeekDisplaying != Time.ThisWeek)
                LoadClassScheduleForWeek(WeekDisplaying = Time.ThisWeek);

            mainPivot.SelectedIndex = dayOfWeek;
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