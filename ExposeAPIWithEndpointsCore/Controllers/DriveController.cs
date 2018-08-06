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
    public class DriveController : Controller
    {
        // GET api/values
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        const string ApplicationName = "Current Legislators";

        const string SpreadsheetId = "1fsCH3MnMsdRXAlXrGmAciGETruNv4Af8TX9U2dAoDpU";
        const string sheet = "containers";
        static SheetsService service;
        private readonly IHostingEnvironment _hostingEnvironment;
        public DriveController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;




        [HttpGet]
        [Route("api/Sheet")]

        public string Get()
        {
            var containers = getcontainerList();

            return JsonConvert.SerializeObject(containers);
        }

        // GET api/values/5
        [HttpGet]
        [Route("api/Sheet/{id}")]
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

        [HttpGet]
        [Route("api/Sheet/Create")]
        public string Create()
        {
            string path = _hostingEnvironment.ContentRootPath;
            var containers = getcontainerListExcel();
            string jsondata = JsonConvert.SerializeObject(containers);
           

            // for (int i = 0; i < 4; i++)
            // {      
                // Write that JSON to txt file,  
                System.IO.File.Delete("output.json");
                System.IO.File.WriteAllText("output.json", jsondata);

            //}

            return "Data Saved Successfully";

        }

        // POST api/values




        private List<containerdetails> getcontainerListExcel()
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

            var range = $"{sheet}!A:J";
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
                        containerno = row[7].ToString(),
                        shippingline = row[2].ToString(),
                        sbno = row[0].ToString(),
                        movementdate = row[4].ToString(),
                        color = "#99FF0000",
                        party = row[8].ToString()
                    });
                }
            }

            return containers;
        }

        private List<containerdetails> getcontainerList()
        {
            var JsonString = System.IO.File.ReadAllText("output.json");
            return JsonConvert.DeserializeObject<List<containerdetails>>(JsonString);
        }


        private string getColorCode(containerdetails data)
        {
            if (String.IsNullOrEmpty(data.movementdate) || data.movementdate == "00-00-0000")
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
