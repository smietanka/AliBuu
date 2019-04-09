using Newtonsoft.Json;
using OvhWrapper.Types.OpenStack;
using OvhWrapper.Types.OpenStack.Models;
using Polly;
using RestSharp;
using System;
using System.Linq;

namespace OvhWrapper
{
    public class OvhStorage
    {
        private static string UserName { get; set; }
        private static string Password { get; set; }
        private static string TenantId { get; set; }
        private static string Region { get; set; }
        public static bool IsConnected { get; set; } = false;
        private static readonly object _lock = new object();

        public static Policy apiRetryPolicy = Policy.Handle<Exception>().WaitAndRetry(new[]
        {
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(7),
            TimeSpan.FromSeconds(31),
            TimeSpan.FromSeconds(240),
            TimeSpan.FromSeconds(360)
          });

        //manage container
        public static Container Container { get; set; }
        public static ContainerObject ContainerObject { get; set; }

        //Connection details
        private static AccessDetail Access { get; set; }
        private static ServiceDetail ObjectStorageService { get; set; }
        private static EndpointDetail CurrentRegionEndpoint { get; set; }

        /// <summary>
        /// Initialize object storage openstack api connection.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="tenantId"></param>
        /// <param name="region">For example: GRA1</param>
        public static void SetupConnection(string userName, string password, string tenantId, string region)
        {
            UserName = userName;
            Password = password;
            TenantId = tenantId;
            Region = region;
            apiRetryPolicy.Execute(() => { Connection(); });
        }

        /// <summary>
        /// Make connection to OpenStack
        /// </summary>
        private static void Connection()
        {
            RestClient Client = new RestClient("https://auth.cloud.ovh.net/v2.0");

            // jesli już jest nadany connection to nie ma co tu robić więcej
            // CloudStorage ma w parametrze ShopId, większosc metod w API (np. do pobrania specyfikacji, odwoluje się do GetSpecification z konrketnym ShopId) i tam jest:
            // var cs = new CloudStorage(shopId); 
            // Kazde takie stworzenie instacji obiektu jest rownoznaczne z wywolaniem konstruktora OvhStorage 
            // Który z kolei wywołuje Connection(). ze to juz jest static to sprawdzmy czy cos innego nie nadalo tym polom wartosci
            if (Container != null && ContainerObject != null) return;
            lock (_lock)
            {

                var requestBody = new
                {
                    auth = new
                    {
                        passwordCredentials = new
                        {
                            username = UserName,
                            password = Password
                        },
                        tenantId = TenantId
                    }
                };

                var request = new RestRequest("/tokens", Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddJsonBody(requestBody);
                var response = Client.Execute(request);

                if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                {
                    AccessRoot accesTo = JsonConvert.DeserializeObject<AccessRoot>(response.Content);
                    Access = accesTo.Access;
                    var tempService = Access.ServiceCatalog.Where(z => z.Type.Equals("object-store")).FirstOrDefault();
                    if (tempService != null)
                    {
                        ObjectStorageService = tempService;
                    }
                    else
                    {
                        throw new System.Exception("Cannot find object storage service in access");
                    }
                    var tempRegion = ObjectStorageService.Endpoints.Where(z => z.Region.Equals(Region)).FirstOrDefault();
                    if (tempRegion != null)
                    {
                        CurrentRegionEndpoint = tempRegion;
                    }
                    else
                    {
                        throw new Exception("Cannot find endpoint in storage service");
                    }
                    //Initialize manages
                    Container = new Container(Access, CurrentRegionEndpoint);
                    ContainerObject = new ContainerObject(Access, CurrentRegionEndpoint);
                    IsConnected = true;
                }
                else
                {
                    Container = null;
                    ContainerObject = null;
                }

            }
        }

        public static bool IsResponseOk(IRestResponse response, bool throwException = false, bool reconnect = true)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (reconnect)
                {
                    Container = null;
                    ContainerObject = null;
                    Connection();
                }
                if (Access != null && Access.Token.Expires.ToUniversalTime() < DateTime.Now.ToUniversalTime())
                {
                    throw new Exception("Ovh storage token expired.");
                }
                else
                {
                    throw new Exception("Ovh storage unauthorized.");
                }
            }

            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
            {
                if (throwException)
                {
                    throw new Exception(string.Format("Ovh Storage Error. {0} {1}", (int)response.StatusCode, response.Content));
                }
                return false;
            }
            return true;
        }
    }
}