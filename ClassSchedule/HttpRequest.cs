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
using System.IO;
using System.Text;

namespace ClassSchedule {
    public class HttpRequest {
        private HttpWebRequest request;
        private HttpWebResponse response;
        private static CookieContainer cookieContainer = new CookieContainer();

        public WebHeaderCollection RequestHeaders { get { return request.Headers; } }
        public WebHeaderCollection ResponseHeaders { get { return response.Headers; } }
        public string ContentType { get { return request.ContentType; } set { request.ContentType = value; } }

        public HttpStatusCode StatusCode { get { return response.StatusCode; } }

        private string responseText;
        public string ResponseText {
            get {
                if (responseText == null)
                    responseText = new StreamReader(ResponseStream).ReadToEnd();
                return responseText;
            }
        }
        public Stream ResponseStream { get { return response.GetResponseStream(); } }

        public event Action Complete = delegate { };

        public void Open(string method, string url) {
            request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = method;
            request.CookieContainer = cookieContainer;

            response = null;
        }

        public void Send(string data) {
            var bytes = new byte[data.Length];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = BitConverter.GetBytes(data[i])[0];
            Send(bytes);
        }

        public void Send() {
            Send(new byte[0]);
        }

        public void Send(byte[] bytes) {
            var GetResponse = new Action(() => {
                request.BeginGetResponse((result) => {
                    if (result.IsCompleted) {
                        try {
                            response = request.EndGetResponse(result) as HttpWebResponse;
                        }
                        catch (WebException e) {
                            response = e.Response as HttpWebResponse;
                        }
                        Complete();
                    }
                }, null);
            });

            if (bytes.Length > 0) {
                request.BeginGetRequestStream((result) => {
                    using (var stream = request.EndGetRequestStream(result)) {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    GetResponse();
                }, null);
            }
            else GetResponse();
        }
    }
}
