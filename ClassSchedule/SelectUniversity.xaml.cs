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

namespace ClassSchedule
{
    public partial class SelectUniversity : PhoneApplicationPage
    {
        private bool loaded = false;

        public SelectUniversity()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SelectUniversity_Loaded);
        }

        void SelectUniversity_Loaded(object sender, RoutedEventArgs e) {
            //Navigation.ClearHistory(NavigationService);

            if (loaded)
                return;

            loaded = true;

            ProxyCallback fetchCallback = delegate { };

            fetchCallback = (listObject, ex) => {
                Dispatcher.BeginInvoke(() => {
                    if (ex != null) {
                        if (Schedule.UniversityInfo != null) {
                            MessageBox.Show("failed to fetch university list", "ERROR", MessageBoxButton.OK);
                            NavigationService.GoBack();
                        }
                        else {
                            var result = MessageBox.Show("failed to fetch university list, do you want to retry?", "ERROR", MessageBoxButton.OKCancel);
                            if (result == MessageBoxResult.OK)
                                Proxy.ListUniversitis(fetchCallback);
                            else
                                NavigationService.Source = Uris.MainPage;
                        }
                        return;
                    }

                    foreach (var info in listObject as UniversityListInfo[])
                        universityList.Items.Add(new ListBoxItem() {
                            Content = new TextBlock() {
                                Text = info.Name,
                                Style = App.Current.Resources["SelectListFont"] as Style
                            },
                            Tag = info
                        });

                    universityLoadingProgressBar.Visibility = Visibility.Collapsed;
                });
            };

            Proxy.ListUniversitis(fetchCallback);
        }

        private void universityList_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            Schedule.UniversityListInfo = (e.AddedItems[0] as ListBoxItem).Tag as UniversityListInfo;
            
            new Navigation(NavigationService).BackToOrGoTo(Uris.ImportClassSchedule);

            /*
            if (e.AddedItems.Count == 0) return;
            universityLoadingProgressBar.Visibility = Visibility.Visible;
            universityList.IsEnabled = false;
            var id = (int)(e.AddedItems[0] as ListBoxItem).Tag;
            Proxy.GetUniversityInfo(id, (value, ex) => {
                Dispatcher.BeginInvoke(() => {
                    universityLoadingProgressBar.Visibility = Visibility.Collapsed;
                    universityList.IsEnabled = true;
                    universityList.SelectedItem = null;
                    if (ex == null) {
                        Schedule.UniversityInfo = value as UniversityInfo;
                        Schedule.ClassInfos = null;
                        new Navigation(NavigationService).BackToOrGoTo(Uris.ImportClassSchedule);
                    }
                    else MessageBox.Show("failed to fetch the information of the university selected, please try again.", "ERROR", MessageBoxButton.OK);
                });
            });
            */
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e) {
            HttpRequest.AbortAll();
        }
    }
}
