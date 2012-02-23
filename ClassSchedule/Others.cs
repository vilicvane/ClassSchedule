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

namespace ClassSchedule {

    public static class Time {
        public static int ThisWeek {
            get {
                return (int)((DateTime.Today - Config.FirstWeek).TotalDays / 7) + 1;
            }
        }
    }

    public class ClassInfo {
        public string Name { get; private set; }
        public string Teacher { get; private set; }
        public int[] Weeks { get; private set; }
        public int Day { get; private set; }
        public int[] Sessions { get; private set; }
        public string Location { get; private set; }

        public ClassInfo(string infoLine) {
            var infos = infoLine.Split(';');
            Name = infos[0];
            Teacher = infos[1];
            Weeks = GetNumbers(infos[2]);
            Day = int.Parse(infos[3]);
            Sessions = GetNumbers(infos[4]);
            Location = infos[5];
        }

        public bool HasClassOn(int day, int week = 0) {
            if (week == 0)
                week = Time.ThisWeek;

            return Day == day && Array.IndexOf(Weeks, week) >= 0;
        }

        public bool HasClassIn(int week) {
            return Array.IndexOf(Weeks, week) >= 0;
        }

        private static int[] GetNumbers(string str) {
            var nums = new List<int>();
            var ns = str.Split(',');
            foreach (var n in ns)
                nums.Add(int.Parse(n));

            return nums.ToArray();
        }
    }

    public class SpecificClassInfo {
        public string Name;
        public string StartsAt;
        public string EndsAt;
        public string Teacher;
        public string Location;
    }

    public static class Classes {
        static string dataPath = "data";
        static IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

        private static List<ClassInfo> classes = new List<ClassInfo>();
        public static int Count { get { return classes.Count; } }

        public static void Load() {
            string text;

            if (!isf.FileExists(dataPath)) {
                isf.CreateFile(dataPath).Close();
                text = "";
            }
            else {
                var stream = isf.OpenFile(dataPath, FileMode.Open);
                text = new StreamReader(stream).ReadToEnd();
                stream.Close();
            }

            var lines = text.Length > 0 ? text.Split('\n') : new string[0];

            foreach (var line in lines)
                classes.Add(new ClassInfo(line));
        }

        public static void Update(string data) {
            var stream = isf.OpenFile(dataPath, FileMode.Create);
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            writer.Close();
            classes.Clear();
        }

        public static Dictionary<int, ClassInfo>[] GetClassesForWeek(int week) {
            var infos = new Dictionary<int, ClassInfo>[7];
            for (var i = 0; i < 7; i++)
                infos[i] = new Dictionary<int, ClassInfo>();

            foreach (var cl in classes)
                if (cl.HasClassIn(week))
                    foreach (var session in cl.Sessions)
                        infos[cl.Day][session] = cl;

            return infos;
        }


    }
}
