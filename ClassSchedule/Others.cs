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
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Navigation;
using System.Linq;

namespace ClassSchedule {

    public class Session {
        public string StartTime;
        public string EndTime;
    }

    public class SessionPeriod {
        public string Name;
        public Session[] Sessions;
    }

    public class UniversityInfo {
        public int Id;
        public string Name;
        public bool HasVerifier;
        public DateTime FirstWeek;
        public int WeekCount;
        public SessionPeriod[] SessionPeriods;
        public SessionPeriod[] SummerSessionPeriods;
    }

    public class UniversityListInfo {
        public int Id;
        public string Name;
    }

    public class ClassInfo {
        public string Name;
        public SubClassInfo[] Classes;
    }

    public class SubClassInfo {
        public string Teacher;
        public int[] Weeks;
        public int DayOfWeek;
        public int[] Sessions;
        public string Location;
    }

    public class SpecificClassInfo {
        public string Name;
        public string Teacher;
        public string Location;
    }

    public class ClassPeriodInfo {
        public string Name;
        public string Teacher;
        public string Location;
        public string PeriodName;
        public string StartTime;
        public string EndTime;
    }

    public static class Json {
        public static string Stringify(object @object) {
            if (@object == null)
                return "null";
            var serializer = new DataContractJsonSerializer(@object.GetType());
            var stream = new MemoryStream();
            serializer.WriteObject(stream, @object);
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        public static T Parse<T>(string json) {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            stream.Position = 0;
            var @object = serializer.ReadObject(stream);
            return (T)@object;
        }
    }

    public static class Time {
        public static int ThisWeek {
            get {
                if (Schedule.UniversityInfo == null)
                    return 1;
                
                return (int)(((DateTime.Today - Schedule.UniversityInfo.FirstWeek).TotalDays) / 7) + 1;
            }
        }

        public static bool IsSummer(int week, int dayOfWeek) {
            var days = (week - 1) * 7 + (dayOfWeek == 0 ? 7 : dayOfWeek);
            var info = Schedule.UniversityInfo;
            if (info == null) return false;
            var date = info.FirstWeek.AddDays(days);
            var summerStart = new DateTime(date.Year, 5, 1);
            var summerEnd = new DateTime(date.Year, 9, 30);

            return date >= summerStart && date <= summerEnd;
        }
    }

    public class Navigation {
        private NavigationService Service { get; set; }

        public Navigation(NavigationService service) {
            Service = service;
        }

        public void ClearHistory() {
            while (Service.CanGoBack)
                Service.RemoveBackEntry();
        }

        public void BackToOrGoTo(Uri uri) {
            var stack = Service.BackStack;
            var count = 0;
            foreach (var entry in stack) {
                if (entry.Source == uri) break;
                count++;
            }

            if (count < stack.Count()) {
                while (count-- > 0)
                    Service.RemoveBackEntry();
                Service.GoBack();
            }
            else Service.Source = uri;
        }
    }
}
