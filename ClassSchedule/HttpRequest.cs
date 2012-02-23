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
using GBKEncoding;

namespace ClassSchedule {
    class HttpRequest {
        HttpWebRequest request;
        static Dictionary<string, CookieContainer> cookieContainers = new Dictionary<string, CookieContainer>();

        public HttpRequest(string method, string url) {
            request = WebRequest.Create(url) as HttpWebRequest;

            var domain = new Uri(url).Host.ToLower();
            if (!cookieContainers.ContainsKey(domain))
                cookieContainers[domain] = new CookieContainer();

            request.CookieContainer = cookieContainers[domain];
            request.Method = method;
        }

        public void Send(string content = null) {

            var GetResponse = new Action(() => {
                try {
                    request.BeginGetResponse((result) => {
                        if (result.IsCompleted) {
                            try {
                                var response = request.EndGetResponse(result);
                                var stream = response.GetResponseStream();
                                var text = GBKEncoder.Read(stream);
                                Complete(true, text);
                            }
                            catch {
                                Complete(false, null);
                                return;
                            }
                        }
                    }, null);
                }
                catch {
                    Complete(false, null);
                }
            });

            if (content != null) {
                request.BeginGetRequestStream((result) => {
                    using (var stream = request.EndGetRequestStream(result)) {
                        var bytes = new List<byte>();
                        foreach (var chr in content)
                            bytes.Add(BitConverter.GetBytes(chr)[0]);
                        stream.Write(bytes.ToArray(), 0, bytes.Count);
                        request.Headers["Content-Length"] = bytes.Count.ToString();
                    }
                    GetResponse();
                }, null);
            }
            else GetResponse();
        }

        public string ContentType {
            get { return request.ContentType; }
            set { request.ContentType = value; }
        }

        public WebHeaderCollection Headers { get { return request.Headers; } }

        public delegate void Callback(bool success, string text);
        public event Callback Complete = delegate { };
    }
}
