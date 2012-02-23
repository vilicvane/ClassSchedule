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
        public SelectUniversity()
        {
            InitializeComponent();

            Config.FetchCallback fetchCallback = delegate { };

            fetchCallback = (universities) => {

                if (universities.Length == 0) {
                    if (Config.Loaded) {
                        MessageBox.Show("failed to fetch university list", "ERROR", MessageBoxButton.OK);
                        NavigationService.GoBack();
                    }
                    else {
                        var result = MessageBox.Show("failed to fetch university list, do you want to retry?", "ERROR", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                            Config.Fetch(fetchCallback);
                        else {
                            Config.Terminated = true;
                            NavigationService.GoBack();
                        }
                    }
                    return;
                }

                foreach (var u in universities)
                    universityList.Items.Add(new ListBoxItem() {
                        Content = new TextBlock() {
                            Text = u,
                            Style = App.Current.Resources["SelectListFont"] as Style
                        }
                    });

                universityLoadingProgressBar.Visibility = Visibility.Collapsed;

            };

            Config.Fetch(fetchCallback);
        }

        private void universityList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var university = ((e.AddedItems[0] as ListBoxItem).Content as TextBlock).Text;
            Config.SetUniversity(university);
            NavigationService.GoBack();
        }
    }
}
