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
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.Threading.Tasks;

namespace ExposeAPIWithEndpointsCore.MSC
{
    public class MscFirebaseController : Controller
    {

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        private readonly IHostingEnvironment _hostingEnvironment;
        public MscFirebaseController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;

        /*Firebase */
        public static readonly IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "ZIhwwj7XtVUrc3WNu4GpSXgGADj1cJmT6NcrIvJJ",
            BasePath = @"https://rgbfirestore.firebaseio.com/"
        };


        [HttpGet]
        [Route("api/msc/GetContainerDetails/{containerno}")]
        public string GetContainerDetails(string containerno)
        {
            return getContainerInfo(containerno).Result;
        }


        [HttpGet]
        [Route("api/msc/Create")]
        public string Create()
        {
            return InsertContainersList().Result;
        }



        private async Task<string> getContainerInfo(string containerno)
        {
            var client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = await client.GetTaskAsync("MSC/MNR/" + containerno.ToUpper());
            return response.Body;
        }


        private async Task<string> InsertContainersList()
        {
            IList<IList<object>> values = readContainerListExcel();
            var client = new FireSharp.FirebaseClient(config);

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    EMnr container = new EMnr();
                    container.containerno = row[0].ToString().ToUpper();
                    container.party = row[1].ToString();
                    container.status = row[2].ToString();
                    container.color = Common.getColorCode(row[2].ToString());
                    container.footer = "Status : " + row[2].ToString() + "\n" + (row[2].ToString() == "Allocated" ? "   Party : " + row[1].ToString() : "");

                    await client.SetTaskAsync("MSC/MNR/" + container.containerno, container);
                }
            }
            return "Data Saved Successfully";
        }

        private IList<IList<object>> readContainerListExcel()
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
                ApplicationName = Constants.ApplicationName,
            });

            var range = $"{Constants.sheet}!A:C";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(Constants.SpreadsheetId, range);

            ValueRange response = request.Execute();

            return response.Values;
        }



    }
}
