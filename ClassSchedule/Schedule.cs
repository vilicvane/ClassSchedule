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
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Linq;

namespace ClassSchedule {

    public static class Schedule {
        private static string universityInfoPath = "UniversityInfo";
        private static string classInfosPath = "ClassInfos";
        private static IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

        private static UniversityInfo universityInfo;
        public static UniversityInfo UniversityInfo {
            get { return universityInfo; }
            set {
                universityInfo = value;
                var writer = new StreamWriter(isf.OpenFile(universityInfoPath, FileMode.Create));
                writer.Write(Json.Stringify(value));
                writer.Close();
            }
        }

        private static ClassInfo[] classInfos;
        public static ClassInfo[] ClassInfos {
            get { return classInfos; }
            set {
                classInfos = value;
                var writer = new StreamWriter(isf.OpenFile(classInfosPath, FileMode.Create));
                writer.Write(Json.Stringify(value));
                writer.Close();
            }
        }

        static Schedule() {
            if (isf.FileExists(universityInfoPath)) {
                var uReader = new StreamReader(isf.OpenFile(universityInfoPath, FileMode.Open));
                universityInfo = Json.Parse<UniversityInfo>(uReader.ReadToEnd());
                uReader.Close();
            }
            
            if (isf.FileExists(classInfosPath)) {
            var cReader = new StreamReader(isf.OpenFile(classInfosPath, FileMode.Open));
                classInfos = Json.Parse<ClassInfo[]>(cReader.ReadToEnd());
                cReader.Close();
            }
        }

        public static Dictionary<int, SpecificClassInfo>[] GetClassesForWeek(int week) {
            var infos = new Dictionary<int, SpecificClassInfo>[7];
            for (var i = 0; i < 7; i++)
                infos[i] = new Dictionary<int, SpecificClassInfo>();

            if (Schedule.ClassInfos != null)
                foreach (var classInfo in Schedule.ClassInfos)
                    foreach (var subClassInfo in classInfo.Classes)
                        if (subClassInfo.Weeks.Contains(week))
                            foreach (var session in subClassInfo.Sessions)
                                infos[subClassInfo.DayOfWeek][session] = new SpecificClassInfo() {
                                    Name = classInfo.Name,
                                    Teacher = subClassInfo.Teacher,
                                    Location = subClassInfo.Location
                                };

            return infos;
        }

        public static Session GetSession(int n) {
            n--;

            var periods = UniversityInfo.SessionPeriods;
            for (var i = 0; i < periods.Length; i++) {
                var sessions = periods[i].Sessions;
                var count = sessions.Length;
                if (n >= count)
                    n -= count;
                else
                    return sessions[n];
            }

            return null;
        }

        internal static string GetSessionPeriodName(int n) {
            n--;

            var periods = UniversityInfo.SessionPeriods;
            for (var i = 0; i < periods.Length; i++) {
                var period = periods[i];
                var count = period.Sessions.Length;
                if (n >= count)
                    n -= count;
                else
                    return period.Name;
            }

            return null;
        }
    }
}
