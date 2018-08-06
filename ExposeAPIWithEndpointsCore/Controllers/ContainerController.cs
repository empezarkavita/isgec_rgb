using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExposeAPIWithEndpointsCore.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("api/[controller]")]
    public class ContainerController : Controller
    {
        // GET api/values
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

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

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
                return "#99FFA500";
            }
            else
            {
                return "99008000";
            }
        }
    }
}
