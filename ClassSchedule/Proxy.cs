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
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace ClassSchedule {
    #region Data Structures

    public class ListUniversitiesResponse {
        public UniversityListInfo[] d = null;
    }

    public class GetUniversityInfoResponse {
        public UniversityInfo d = null;
    }

    public class FetchClassInfosResponse {
        public ClassInfo[] d = null;
    }

    public class ProxyException {
        public string Message;
    }

    [DataContract]
    public class UniversityIdData {
        [DataMember]
        public int universityId;
    }

    [DataContract]
    public class LoginData {
        [DataMember]
        public int universityId;
        [DataMember]
        public string username;
        [DataMember]
        public string password;
        [DataMember]
        public string verifier;
    }

    #endregion

    public delegate void ProxyCallback(object value, ProxyException ex);

    public static class Proxy {
#if DEBUG
        private static string proxyBaseUrl = "http://localhost:53028/ClassScheduleProxy/Proxy.asmx/";// "http://csp.groinup.com/Proxy.asmx/";
#else
        private static string proxyBaseUrl = "http://csp.groinup.com/Proxy.asmx/";
#endif
        public static void ListUniversitis(ProxyCallback callback) {
            var request = new HttpRequest();
            request.Open("POST", proxyBaseUrl + "ListUniversities");
            request.ContentType = "application/json";
            request.Send("{}");

            request.Complete += () => {
                if (request.StatusCode == HttpStatusCode.OK) {
                    var response = Json.Parse<ListUniversitiesResponse>(request.ResponseText);
                    callback(response.d, null);
                    return;
                }
                else {
                    var ex =
                        request.StatusCode == HttpStatusCode.InternalServerError ?
                        Json.Parse<ProxyException>(request.ResponseText) :
                        new ProxyException { Message = "UnknownException" };
                    callback(null, ex);
                    return;
                }
            };
        }

        public static void GetUniversityInfo(int universityId, ProxyCallback callback) {
            var request = new HttpRequest();
            request.Open("POST", proxyBaseUrl + "GetUniversityInfo");
            request.ContentType = "application/json";
            request.Send(Json.Stringify(new UniversityIdData() {
                universityId = universityId
            }));

            request.Complete += () => {
                if (request.StatusCode == HttpStatusCode.OK) {
                    var response = Json.Parse<GetUniversityInfoResponse>(request.ResponseText);
                    callback(response.d, null);
                    return;
                }
                else {
                    var ex =
                        request.StatusCode == HttpStatusCode.InternalServerError ?
                        Json.Parse<ProxyException>(request.ResponseText) :
                        new ProxyException { Message = "UnknownException" };
                    callback(null, ex);
                    return;
                }
            };
        }

        public static void FetchVerifier(int universityId, ProxyCallback callback) {
            var request = new HttpRequest();
            request.Open("GET", proxyBaseUrl + "FetchVerifier?universityId=" + universityId);
            request.Send();

            request.Complete += () => {
                if (request.StatusCode == HttpStatusCode.OK) {
                    callback(request.ResponseStream, null);
                    return;
                }
                else {
                    callback(null, new ProxyException { Message = "FetchVerifierFailed" });
                    return;
                }
            };
        }

        public static void FetchClassInfos(int universityId, string username, string password, string verifier, ProxyCallback callback) {
            var request = new HttpRequest();
            request.Open("POST", proxyBaseUrl + "FetchClassInfos");
            request.ContentType = "application/json";
            request.Send(Json.Stringify(new LoginData() {
                universityId = universityId,
                username = username,
                password = password,
                verifier = verifier
            }));

            request.Complete += () => {
                if (request.StatusCode == HttpStatusCode.OK) {
                    var response = Json.Parse<FetchClassInfosResponse>(request.ResponseText);
                    callback(response.d, null);
                    return;
                }
                else {
                    var ex =
                        request.StatusCode == HttpStatusCode.InternalServerError ?
                        Json.Parse<ProxyException>(request.ResponseText) :
                        new ProxyException { Message = "UnknownException" };
                    callback(null, ex);
                    return;
                }
            };
        }

    }
}
