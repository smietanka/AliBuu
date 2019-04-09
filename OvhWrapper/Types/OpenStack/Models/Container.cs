using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Based on: https://developer.openstack.org/api-ref/object-store/index.html
/// Container section
/// </summary>
namespace OvhWrapper.Types.OpenStack.Models
{
    public class Container
    {
        private AccessDetail Access { get; set; }
        private EndpointDetail Endpoint { get; set; }
        private ContainerObject ObjectManage { get; set; }


        //Includes
        private RestClient Client { get; set; }
        public Container(AccessDetail access, EndpointDetail endpoint)
        {
            Access = access;
            Endpoint = endpoint;
            Client = new RestClient(Endpoint.PublicUrl);
            ObjectManage = new ContainerObject(access, endpoint);
        }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns></returns>
        public bool Create(string containerName)
        {
            var request = new RestRequest("/{containerName}", Method.PUT);
            request.AddUrlSegment("containerName", containerName);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("X-Auth-Token", Access.Token.Id);
            request.AddHeader("X-Container-Read", ".r:*"); // publicznie mozna odczytywac obiekty, ale nie mozna wyswietlac listy obiektow
            var response = Client.Execute(request);
            return OvhStorage.IsResponseOk(response);
        }

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public bool Delete(string containerName)
        {
            var request = new RestRequest("/{containerName}", Method.DELETE);
            request.AddUrlSegment("containerName", containerName);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("X-Auth-Token", Access.Token.Id);

            var response = Client.Execute(request);
            return OvhStorage.IsResponseOk(response);
        }

        /// <summary>
        /// Get all items in container
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public List<ContainerItem> List(string containerName)
        {
            var request = new RestRequest("/{containerName}", Method.GET);
            request.AddUrlSegment("containerName", containerName);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("X-Auth-Token", Access.Token.Id);

            var response = Client.Execute<List<ContainerItem>>(request);

            OvhStorage.IsResponseOk(response, true);
            foreach (var eachObject in response.Data)
            {
                eachObject.Url = string.Format("{0}/{1}/{2}", Endpoint.PublicUrl, containerName, eachObject.Name);
            }
            return response.Data;
        }

        /// <summary>
        /// Delete all objects from container
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public bool DeleteAllObjects(string containerName)
        {
            var objectsInContainer = List(containerName);

            if(objectsInContainer.Any())
            {
                foreach(var eachObject in objectsInContainer)
                {
                    if(!ObjectManage.Delete(containerName, eachObject.Name))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Delete all objects in container and container
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public bool DeleteForce(string containerName)
        {
            if(DeleteAllObjects(containerName))
            {
                return Delete(containerName);
            }
            return true;
        }
    }
}
