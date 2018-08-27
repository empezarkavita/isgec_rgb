using System;
using System.Collections.Generic;
using System.Linq;
using ExposeAPIWithEndpointsCore.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Google.Apis.Sheets.v4.Data;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    public class MscController : Controller
    {
        const string RED = "#99FF0000";
        const string YELLOW = "#99FFFF00";
        const string GREEN = "#99008000";

        const string sheet = "msc";


        // GET api/values
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        const string ApplicationName = "MSC";

        const string SpreadsheetId = "1r00LKD8rOkgx3-ZQrBFsTsYCG6T9zfIM_lt2P-xLWm0";

        // static SheetsService service;
        private readonly IHostingEnvironment _hostingEnvironment;

        public MscController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;


        // GET api/values/5
        [HttpGet]
        [Route("api/msc/GetContainerDetails/{id}")]
        public string GetContainerDetails(string id)
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

        private List<msc> getcontainerListExcel()
        {
            SheetsService service;
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

            var range = $"{sheet}!A:C";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            List<msc> containers = new List<msc>();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    containers.Add(new msc
                    {
                        containerno = row[0].ToString(),
                        color = "#99FF0000",
                        party = row[1].ToString(),
                        status = row[2].ToString(),
                        footer = "Status : " + row[2].ToString() + "\n" + (row[2].ToString() == "Allocated" ?  "   Party : " + row[1].ToString()  : "") 
                    });
                }
            }

            return containers;
        }

        private string getColorCode(msc data)
        {
            if (data.status == "Estimation")
            {
                return RED;
            }
            else if (data.status == "Under Repair")
            {
                return YELLOW;
            }
            else
            {
                return GREEN;
            }
        }

        // private string getFooter(msc data)
        // {
        //     var footer = "";
        //     if(String.IsNullOrEmpty(data.party))
        //         {
        //             footer =  "Party : " + data.party + "\n";
        //         }
        // }

           [HttpGet]
        [Route("api/msc/Create")]
        public string Create()
        {
            string path = _hostingEnvironment.ContentRootPath;
            var containers = getcontainerListExcel();
            string jsondata = JsonConvert.SerializeObject(containers);
           
            System.IO.File.Delete("output_msc.json");
            System.IO.File.WriteAllText("output_msc.json", jsondata);


            return "Data Saved Successfully";

        }
     private List<msc> getcontainerList()
        {
            var JsonString = System.IO.File.ReadAllText("output_msc.json");
            return JsonConvert.DeserializeObject<List<msc>>(JsonString);
        }
    }
}