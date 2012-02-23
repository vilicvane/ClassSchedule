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
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClassSchedule {
    public static class Config {
        static string configPath = "config";
        static IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

        public static DateTime FirstWeek { get; private set; }
        public static int WeekCount { get; private set; }
        public static Dictionary<int, string> SessionStartTimes { get; private set; }
        public static Dictionary<int, string> SessionEndTimes { get; private set; }
        public static bool Terminated = false;

        private static Dictionary<string, string> schedules = new Dictionary<string, string>();
        private static bool loaded = false;
        public static bool Loaded { get { return loaded; } }

        public static bool Load() {
            if (!isf.FileExists(configPath)) return false;
            try {
                var stream = isf.OpenFile(configPath, FileMode.Open);
                var text = new StreamReader(stream).ReadToEnd();
                stream.Close();

                if (text == "") return false;

                var items = text.Split(',');
                FirstWeek = DateTime.Parse(items[0]);

                var periods = items[1].Split('#');

                WeekCount = int.Parse(items[2]);

                //morning afternoon evening
                var pChrs = new char[] { 'm', 'a', 'e' };

                SessionStartTimes = new Dictionary<int, string>();
                SessionEndTimes = new Dictionary<int, string>();

                var session = 1;

                for (var i = 0; i < pChrs.Length; i++) {
                    var times = periods[i].Split('/');

                    for (var j = 0; j < times.Length; j++) {
                        var time = times[j];
                        SessionStartTimes[session] = pChrs[i] + time.Substring(0, 2) + ":" + time.Substring(2, 2);
                        SessionEndTimes[session] = time.Substring(4, 2) + ":" + time.Substring(6, 2);
                        session++;
                    }
                }
                loaded = true;
                return true;
            }
            catch {
                return false;
            }
        }

        public static void SetUniversity(string university) {
            if (!schedules.ContainsKey(university)) return;
            var stream = isf.CreateFile(configPath);
            var writer = new StreamWriter(stream);
            writer.Write(schedules[university]);
            writer.Flush();
            writer.Close();
            Load();
        }

        public delegate void FetchCallback(string[] result);

        public static void Fetch(FetchCallback callback) {
            var client = new WebClient();
            client.DownloadStringAsync(new Uri("https://raw.github.com/vilic/ClassSchedule/master/schedules.txt"));
            client.DownloadStringCompleted += (sender, args) => {
                try {
                    schedules.Clear();
                    var lines = args.Result.Split('\n');

                    foreach (var l in lines) {
                        var line = l.Trim();
                        var index = line.IndexOf(':');
                        schedules.Add(line.Substring(0, index), line.Substring(index + 1));
                    }

                    callback(schedules.Keys.ToArray());
                }
                catch {
                    callback(new string[0]);
                    return;
                }
            };
        }
    }
}
