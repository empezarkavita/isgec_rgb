using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExposeAPIWithEndpointsCore.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    // [Route("api/[controller]")]
    public class SheetController : Controller
    {
        // GET api/values
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        const string ApplicationName = "Current Legislators";

        const string SpreadsheetId = "1he3L42Ypxfi5nryz4G5a6-q1fezgkRcr3l3YhUY9JLg";
        const string sheet = "containers";
        static SheetsService service;
        private readonly IHostingEnvironment _hostingEnvironment;
        public SheetController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;




        [HttpGet]
        [Route("api/Drive")]
  
        public string Get()
        {
            var containers = getcontainerList();

            return JsonConvert.SerializeObject(containers);
        } 

        // GET api/values/5
        [HttpGet]
        [Route("api/Drive/{id}")]
        public string GetData(string id)
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

            GoogleCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var range = $"{sheet}!A:I";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();

            List<containerdetails> containers = new List<containerdetails>();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    containers.Add(new containerdetails
                    {
                        containerno = row[8].ToString(),
                        shippingline = row[2].ToString(),
                        sbno = row[0].ToString(),
                        movementdate = row[4].ToString(),
                        color = "#99FF0000"
                    });
                }
            }

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
                return "#99FFFF00";//Orange
            }
            else
            {
                return "#99008000"; //Green
            }
        }
    }
}
