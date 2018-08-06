﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExposeAPIWithEndpointsCore.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public string Get()
        {

          
            var containers = getcontainerList();

            return JsonConvert.SerializeObject(containers);


        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(string id)
        {      
            var containers = getcontainerList();
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


        public class containerdetails
        {
            public string containerno;
            public string shippingline;
            public string sbno;
            public string movementdate;
            public string color;
        }

        private List<containerdetails> getcontainerList()
        {

            List<containerdetails> containers = new List<containerdetails>();

            containers.Add(new containerdetails
            {
                containerno = "HLBU1151814",
                shippingline = "HAPAG-LLYOD",
                sbno = "4438743",
                movementdate = "00-00-0000",
                color = "#99FF0000"
            });

            containers.Add(new containerdetails
            {
                containerno = "HLXU8035999",
                shippingline = "HAPAG-LLYOD",
                sbno = "",
                movementdate = "12-10-2017",
                color = "#99FFA500"
            });

            containers.Add(new containerdetails
            {
                containerno = "HLXU 855254 3",
                shippingline = "HAPAG-LLYOD",
                sbno = "4442715",
                movementdate = "12-10-2017",
                color = "#99008000"
            });



            containers.Add(new containerdetails
            {
                containerno = "SUDU6565934",
                shippingline = "EMPEZAR",
                sbno = "3784324",
                movementdate = "1-1-2017",
                color = "#99008000"
            });


            containers.Add(new containerdetails
            {
                containerno = "CLHU8825463",
                shippingline = "HAPAG-LLYOD",
                sbno = "4673356",
                movementdate = "6-12-2017",
                color = "#99FFA500"
            });


            containers.Add(new containerdetails
            {
                containerno = "MAEU4670844",
                shippingline = "MSC",
                sbno = "745784",
                movementdate = "16-11-2018",
                color = "#99FF0000"
            });
            return containers;
        }

        private string getColorCode(containerdetails data)
        {
            if (String.IsNullOrEmpty(data.movementdate))
            {
                return "#99FF0000";//Red
            }
            else if (data.sbno.Length == 0)
            {
                return "#99FFA500";//Orange
            }
            else
            {
                return "#99008000"; //Green
            }
        }
    }
}
