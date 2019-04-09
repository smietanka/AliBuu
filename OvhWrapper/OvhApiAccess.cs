using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OvhWrapper
{
    public class OvhApiAccess
    {
        private string ApplicationKey { get; set; }
        private string ApplicationSecret { get; set; }
        private string ConsumerKey { get; set; }
        private string ServiceName { get; set; } = "";

        private Dictionary<string, Uri> EndPoints = new Dictionary<string, Uri>()
        {
            { "ovh-eu", new Uri("https://eu.api.ovh.com/1.0") },
            { "ovh-ca", new Uri("https://ca.api.ovh.com/1.0") },
            { "kimsufi-eu", new Uri("https://eu.api.kimsufi.com/1.0") },
            { "kimsufi-ca", new Uri("https://ca.api.kimsufi.com/1.0") },
            { "soyoustart-eu", new Uri("https://eu.api.soyoustart.com/1.0") },
            { "soyoustart-ca", new Uri("https://ca.api.soyoustart.com/1.0") },
            { "runabove-ca", new Uri("https://api.runabove.com/1.0") },
        };

        private long? OvhTime { get; set; } = null;
        private RestClient Client;

        private OvhApiAccess() { }
        public OvhApiAccess(Uri endPoint, string applicationKey, string applicationSecret, string consumerKey)
        {
            ApplicationKey = applicationKey;
            ApplicationSecret = applicationSecret;
            ConsumerKey = consumerKey;
            if (EndPoints.ContainsValue(endPoint))
            {
                Client = new RestClient(endPoint);
            }
            else throw new ArgumentException("Bad endPoint");
        }
        public OvhApiAccess(string endPoint, string applicationKey, string applicationSecret, string consumerKey)
        {
            ApplicationKey = applicationKey;
            ApplicationSecret = applicationSecret;
            ConsumerKey = consumerKey;
            if (EndPoints.TryGetValue(endPoint, out Uri result))
            {
                Client = new RestClient(result);
            }
            else throw new ArgumentException("Bad endPoint");
        }
        public OvhApiAccess(Uri endPoint, string applicationKey, string applicationSecret, string consumerKey, string serviceName)
        {
            ApplicationKey = applicationKey;
            ApplicationSecret = applicationSecret;
            ConsumerKey = consumerKey;
            ServiceName = serviceName;
            if (EndPoints.ContainsValue(endPoint))
            {
                Client = new RestClient(endPoint);
            }
            else throw new ArgumentException("Bad endPoint");
        }
        public OvhApiAccess(string endPoint, string applicationKey, string applicationSecret, string consumerKey, string serviceName)
        {
            ApplicationKey = applicationKey;
            ApplicationSecret = applicationSecret;
            ConsumerKey = consumerKey;
            ServiceName = serviceName;
            if (EndPoints.TryGetValue(endPoint, out Uri result))
            {
                Client = new RestClient(result);
            }
            else throw new ArgumentException("Bad endPoint");
        }
        
        public T Get<T>(string url)
        {
            var response = CallApi(Method.GET, url);
            var objectToReturn = JsonConvert.DeserializeObject<T>(response);
            return objectToReturn;
        }
        public T Post<T>(string url, string body = "")
        {
            var response = CallApi(Method.POST, url, body);
            var objectToReturn = JsonConvert.DeserializeObject<T>(response);
            return objectToReturn;
        }
        public T Put<T>(string url, string body = "")
        {
            var response = CallApi(Method.GET, url, body);
            var objectToReturn = JsonConvert.DeserializeObject<T>(response);
            return objectToReturn;
        }
        public T Delete<T>(string url, string body = "")
        {
            var response = CallApi(Method.GET, url, body);
            var objectToReturn = JsonConvert.DeserializeObject<T>(response);
            return objectToReturn;
        }
        
        #region INCLUDE
        private string CallApi(Method method, string url, string body = "")
        {
            var json = "";
            if (!string.IsNullOrEmpty(body))
            {
                json = JsonConvert.SerializeObject(body);
            }
            var unixEpoch = GetUnixEpochTime();
            var request = new RestRequest(url, method);
            if(url.Contains("{ serviceName}"))
            {
                if(!string.IsNullOrEmpty(ServiceName))
                {
                    request.AddUrlSegment("serviceName", ServiceName);
                }
                else
                {
                    throw new Exception("Cannot find serviceName. Initialize new OvhStorage using ctor with service name.");
                }
            }
            
            var fullUrl = Client.BuildUri(request);

            string signature = GetSignature(method, json, unixEpoch, fullUrl);
            request.AddBody(json);
            request.AddHeader("X-Ovh-Application", ApplicationKey);
            request.AddHeader("X-Ovh-Timestamp", unixEpoch.ToString());
            request.AddHeader("X-Ovh-Consumer", ConsumerKey);
            request.AddHeader("X-Ovh-Signature", signature);

            var response = Client.Execute(request);
            return response.Content;
        }

        private string GetSignature(Method method, string json, long unixEpoch, Uri fullUrl)
        {
            var signatureText = string.Join("+", ApplicationSecret, ConsumerKey, method.ToString(), fullUrl, json, unixEpoch);
            var computedHash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(signatureText));
            var signature = string.Join("", computedHash.Select(b => b.ToString("x2")).ToArray());
            return string.Format("$1${0}", signature);
        }

        private long GetUnixEpochTime()
        {
            if (OvhTime == null)
            {
                var request = new RestRequest("/auth/time", Method.GET);
                var ovhTime = Client.Execute<long>(request).Data;

                OvhTime = ovhTime;
            }
            return OvhTime.Value;
        }
        #endregion
    }
}