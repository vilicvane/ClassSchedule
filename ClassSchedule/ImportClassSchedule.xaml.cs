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

namespace ClassSchedule
{
    public partial class ImportClassSchedule : PhoneApplicationPage
    {
        public ImportClassSchedule()
        {
            InitializeComponent();
        }

        private void Wait() {
            studentNumberTextBox.IsEnabled =
            passwordTextBox.IsEnabled =
            importButton.IsEnabled = false;
            importProgressBar.Visibility = Visibility.Visible;
        }

        private void CancelWait() {
            studentNumberTextBox.IsEnabled =
            passwordTextBox.IsEnabled =
            importButton.IsEnabled = true;
            importProgressBar.Visibility = Visibility.Collapsed;
        }

        private void importButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            var studentNumber = studentNumberTextBox.Text;
            var password = passwordTextBox.Password;
            
            if (studentNumber.Length == 0) {
                MessageBox.Show("please enter your student number", "NOTICE", MessageBoxButton.OK);
                return;
            }
            if (password.Length == 0) {
                MessageBox.Show("please enter your password", "NOTICE", MessageBoxButton.OK);
                return;
            }

            Wait();

            var GetTable = new Action(() => {
                var request = new HttpRequest("POST", "http://202.202.1.41/znpk/Pri_StuSel_rpt.aspx");
                request.ContentType = "application/x-www-form-urlencoded";
                request.Send("Sel_XNXQ=20111&rad=on&px=0&Submit01=%BC%EC%CB%F7");
                request.Complete += (success, text) => {
                    Dispatcher.BeginInvoke(() => {
                        if (!success) {
                            MessageBox.Show("unknown error", "ERROR", MessageBoxButton.OK);
                            CancelWait();
                            return;
                        }

                        ProcessTable(text);
                        NavigationService.GoBack();
                    });
                };
            });

            var Login = new Action(() => {
                var request = new HttpRequest("POST", "http://202.202.1.41/_data/index_login.aspx");
                request.ContentType = "application/x-www-form-urlencoded";
                request.Send("Sel_Type=STU&UserID=" + HttpUtility.UrlEncode(studentNumber) + "&PassWord=" + HttpUtility.UrlEncode(password) + "&cCode=&pcInfo=Mozilla%2F5.0+%28compatible%3B+MSIE+9.0%3B+Windows+NT+6.1%3B+Trident%2F5.0%3B+SLCC2%3B+.NET+CLR+2.0.50727%3B+.NET+CLR+3.5.30729%3B+.NET+CLR+3.0.30729%3B+Media+Center+PC+6.0%3B+InfoPath.3%3B+.NET4.0C%3B+.NET4.0E%3B+Zune+4.7%3B+Tablet+PC+2.0%29x860+SN%3ANULL&typeName=%D1%A7%C9%FA");
                request.Complete += (success, text) => {
                    Dispatcher.BeginInvoke(() => {
                        if (!success) {
                            MessageBox.Show("unknown error", "ERROR", MessageBoxButton.OK);
                            NavigationService.GoBack();
                            return;
                        }

                        if (text.Contains("该账号尚未分配角色")) {
                            MessageBox.Show("the student number you entered doesn't exists", "ERROR", MessageBoxButton.OK);
                            CancelWait();
                            return;
                        }

                        if (text.Contains("账号或密码不正确")) {
                            MessageBox.Show("the student number and password you entered do not match", "ERROR", MessageBoxButton.OK);
                            CancelWait();
                            return;
                        }

                        if (!text.Contains("正在加载权限数据")) {
                            MessageBox.Show("unknown error", "ERROR", MessageBoxButton.OK);
                            CancelWait();
                            return;
                        }

                        GetTable();
                    });
                };
            });

            Login();

        }

        private void ProcessTable(string html) {
            var tbodyHTML = new Regex(@"<TABLE [\s\S]+?<tbody>([\s\S]+?)</tbody>").Match(html).Groups[1].Value;
            var trs = new Regex(@"<tr\s*>([\s\S]+?)</tr>").Matches(tbodyHTML);

            var weekTable = new Dictionary<string, int>() {
                {"日", 0},
                {"一", 1},
                {"二", 2},
                {"三", 3},
                {"四", 4},
                {"五", 5},
                {"六", 6}
            };

            var lines = new List<string>();

            foreach (Match tr in trs) {
                var tds = GetTdValues(tr.Groups[1].Value);

                var items = new List<string>();
                //Class
                items.Add(new Regex(@"\[\d+\](.+)").Match(tds[1]).Groups[1].Value);
                //Teacher
                items.Add(tds[9]);
                //Weeks
                items.Add(String.Join(",", GetNumbers(tds[10])));
                
                //Day and Sessions
                var ds = new Regex(@"([日一二三四五六])\[(.+)节\]").Match(tds[11]);
                items.Add(weekTable[ds.Groups[1].Value].ToString());
                items.Add(String.Join(",", GetNumbers(ds.Groups[2].Value)));

                //Location
                items.Add(tds.Length > 12 ? tds[12] : "");

                lines.Add(String.Join(";", items));
            }

            Classes.Update(String.Join("\n", lines));
        }

        private string[] GetTdValues(string tr) {
            var tdRE = new Regex(@"(?:<td(?:\s[^>]*)?\shidevalue='([\s\S]*?)'(?:\s[^>]*)?>|<td(?:\s[^>]*)?>([\s\S]*?))<br></td>");
            var matches = tdRE.Matches(tr);

            var values = new List<string>();
            foreach (Match match in matches)
                values.Add(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);

            return values.ToArray();
        }

        private static string[] GetNumbers(string str) {
            var nums = new List<string>();
            var periods = str.Split(',');
            foreach (var period in periods) {
                var ns = period.Split('-');
                var start = int.Parse(ns[0]);
                var end = (ns.Length > 1 ? int.Parse(ns[1]) : start) + 1;
                for (var i = start; i < end; i++)
                    nums.Add(i.ToString());
            }
            return nums.ToArray();
        }

        private void passwordTextBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                importButton_Click(null, null);
        }
    }
}
