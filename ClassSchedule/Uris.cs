using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ClassSchedule {
    public static class Uris {
        public static Uri MainPage = new Uri("/MainPage.xaml", UriKind.Relative);
        public static Uri SelectUniversity = new Uri("/SelectUniversity.xaml", UriKind.Relative);
        public static Uri ImportClassSchedule = new Uri("/ImportClassSchedule.xaml", UriKind.Relative);
    }
}
