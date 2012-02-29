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
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.IO;

namespace ClassSchedule
{
    public partial class ImportClassSchedule : PhoneApplicationPage
    {
        public ImportClassSchedule()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ImportClassSchedule_Loaded);
        }

        void ImportClassSchedule_Loaded(object sender, RoutedEventArgs e) {
            //Navigation.ClearHistory(NavigationService);

            var info = Schedule.UniversityInfo;

            if (info == null) {
                NavigationService.Source = Uris.MainPage;
                return;
            }

            universityNameTextBlock.Text = info.Name;

            verifierImage.Source = null;

            if (info.HasVerifier) {
                verifierTextBox.IsEnabled = true;
                importButton.IsEnabled = false;
                FetchVerifier();
            }
            else {
                verifierTextBox.IsEnabled = false;
                importButton.IsEnabled = true;
            }
        }

        private bool fetchingVerifier = false;

        private void FetchVerifier() {
            if (fetchingVerifier) return;
            fetchingVerifier = true;

            importProgressBar.Visibility = Visibility.Visible;
            importButton.IsEnabled = false;

            Proxy.FetchVerifier(Schedule.UniversityInfo.Id, (value, ex) => {
                Dispatcher.BeginInvoke(() => {
                    fetchingVerifier = false;

                    if (ex == null) {
                        var image = new BitmapImage();
                        image.SetSource(value as Stream);
                        verifierImage.Source = image as BitmapImage;
                        importButton.IsEnabled = true;
                        importProgressBar.Visibility = Visibility.Collapsed;
                        return;
                    }

                    MessageBox.Show("failed to fetch verifier", "ERROR", MessageBoxButton.OK);
                    NavigationService.GoBack();
                    //InputScopeNameValue.
                });
            });

        }

        private static class BeforeWaitStatus {
            public static bool SelectUniversity;
            public static bool UsernameTextBox;
            public static bool PasswordTextBox;
            public static bool VerifierTextBox;
            public static bool ImportButton;
            public static ImageSource VerifierImage;
            //public static Visibility ImportProgressBar;
        }

        private bool CanSelectUniveristy = true;

        private void Wait() {
            BeforeWaitStatus.SelectUniversity = CanSelectUniveristy;
            BeforeWaitStatus.UsernameTextBox = usernameTextBox.IsEnabled;
            BeforeWaitStatus.PasswordTextBox = passwordTextBox.IsEnabled;
            BeforeWaitStatus.VerifierTextBox = verifierTextBox.IsEnabled;
            BeforeWaitStatus.ImportButton = importButton.IsEnabled;
            BeforeWaitStatus.VerifierImage = verifierImage.Source;

            CanSelectUniveristy = false;
            usernameTextBox.IsEnabled =
            passwordTextBox.IsEnabled =
            verifierTextBox.IsEnabled =
            importButton.IsEnabled = false;
            verifierImage.Source = null;
            importProgressBar.Visibility = Visibility.Visible;
        }

        private void CancelWait() {
            CanSelectUniveristy = BeforeWaitStatus.SelectUniversity;
            usernameTextBox.IsEnabled = BeforeWaitStatus.UsernameTextBox;
            passwordTextBox.IsEnabled = BeforeWaitStatus.PasswordTextBox;
            verifierTextBox.IsEnabled = BeforeWaitStatus.VerifierTextBox;
            importButton.IsEnabled = BeforeWaitStatus.ImportButton;
            verifierImage.Source = BeforeWaitStatus.VerifierImage;
            importProgressBar.Visibility = Visibility.Collapsed;
        }

        private void importButton_Click(object sender, RoutedEventArgs e) {
            var username = usernameTextBox.Text;
            var password = passwordTextBox.Password;
            var verifier = verifierTextBox.Text;
            
            if (username == "") {
                usernameTextBox.Focus();
                MessageBox.Show("please enter your username", "NOTICE", MessageBoxButton.OK);
                return;
            }

            if (password == "") {
                passwordTextBox.Focus();
                MessageBox.Show("please enter your password", "NOTICE", MessageBoxButton.OK);
                return;
            }

            if (verifierTextBox.IsEnabled && verifier == "") {
                verifierTextBox.Focus();
                MessageBox.Show("please enter your verifier", "NOTICE", MessageBoxButton.OK);
                return;
            }

            Wait();

            Proxy.FetchClassInfos(Schedule.UniversityInfo.Id, username, password, verifier, (value, ex) => {
                Dispatcher.BeginInvoke(() => {
                    if (ex == null) {
                        Schedule.ClassInfos = value as ClassInfo[];
                        NavigationService.Source = Uris.MainPage;
                        return;
                    }

                    string msg;

                    switch (ex.Message) {
                        case "UsernamePasswordMismatch":
                            msg = "the username and password you entered do not match";
                            break;
                        case "VerifierMismatch":
                            msg = "the verifier you entered is incorrect";
                            break;
                        case "UnknownLoginError":
                            msg = "an unknown error occurred when login";
                            break;
                        default:
                            msg = "unkown error";
                            break;
                    }

                    MessageBox.Show(msg, "ERROR", MessageBoxButton.OK);

                    CancelWait();
                });
            });
        }

        private void universityNameTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (CanSelectUniveristy)
                NavigationService.Source = Uris.SelectUniversity;
        }

        private void usernameTextBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                passwordTextBox.Focus();
        }

        private void passwordTextBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (verifierTextBox.IsEnabled)
                    verifierTextBox.Focus();
                else if (importButton.IsEnabled)
                    importButton_Click(null, null);
            }
        }

        private void verifierTextBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && importButton.IsEnabled)
                importButton_Click(null, null);
        }

        private void verifierImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            FetchVerifier();
        }
    }
}
