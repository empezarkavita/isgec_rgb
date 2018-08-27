using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExposeAPIWithEndpointsCore.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("api/[controller]")]
    public class ContainerController : Controller
    {
        // GET api/values

        
            static readonly string[] Scopes = { StorageService.Scope.DevstorageReadWrite};

        [HttpGet]
        public string Get()
        {

            ContainerContext context = HttpContext.RequestServices.GetService(typeof(ContainerContext)) as ContainerContext;
            var containers_info = context.GetAllContainers();
            return JsonConvert.SerializeObject(containers_info);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(string id)
        {
            ContainerContext context = HttpContext.RequestServices.GetService(typeof(ContainerContext)) as ContainerContext;

            var containers = context.GetAllContainers();
            var containerdata = containers.Where(x => x.containerno == id).FirstOrDefault();
            if (containerdata == null)
            {
                return "";
            }
            else
            {
                containerdata.color = getColorCode(containerdata);
                return JsonConvert.SerializeObject(containerdata);
            }


        }

        private static void PushToCloudStorage(FileStream stream,string name)
        {
           
            GoogleCredential credential;
            using (var gstream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(gstream)
                    .CreateScoped(Scopes);
            }

            var client = StorageClient.Create(credential);

            // Create a bucket
            string bucketName = "rgb";
           // var bucket = client.CreateBucket("pin-code-recognizer", bucketName);

            // Upload some files
          
            var obj2 = client.UploadObject(bucketName, "isgec/"+name, "text/plain", stream);
        }
        // POST api/values


        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        private string getColorCode(Container data)
        {
            if (String.IsNullOrEmpty(data.movementdate))
            {
                return "99FF0000";
            }
            else if (data.sbno.Length == 0)
            {
                return "#99FFFF00";
            }
            else
            {
                return "99008000";
            }
        }
    }
}
