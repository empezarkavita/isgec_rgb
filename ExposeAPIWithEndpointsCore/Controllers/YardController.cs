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

    public class YardCapacity
    {
        public int yardA;
        public int yardB;
    }

    public class YardController : Controller
    {
        const string RED = "#99FF0000";
        const string ORANGE = "#99FFA500";
        const string GREEN = "#99008000";

        const string sheet = "capacity";


        // GET api/values
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        const string ApplicationName = "Current Legislators";

        const string SpreadsheetId = "1fsCH3MnMsdRXAlXrGmAciGETruNv4Af8TX9U2dAoDpU";

        // static SheetsService service;
        private readonly IHostingEnvironment _hostingEnvironment;

        public YardController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;


        // GET api/values/5
        [HttpGet]
        [Route("api/Yard/GetYardDetails/{id}")]
        public string GetYardDetails(string id)
        {
            SheetsService service = getService();
            var yardCapacity = getYardCapacity(service);
            string yardAcolor = getYardColor(yardCapacity.yardA);
            string yardBcolor = getYardColor(yardCapacity.yardB);
            string location = "";

            if (yardAcolor == GREEN)
            {
                location = "YARD 1A";
            }
            else if (yardBcolor == GREEN)
            {
                location = "YARD 1B";
            }
            else if (yardAcolor == ORANGE)
            {
                location = "YARD 1A";
            }
            else if (yardBcolor == ORANGE)
            {
                location = "YARD 1B";
            }
            else if (yardAcolor == RED && yardCapacity.yardA < 100)
            {
                location = "YARD 1A";
            }
            else if (yardAcolor == RED && yardCapacity.yardB < 100)
            {
                location = "YARD 1B";
            }
            else
            {
                location = "YARD CAPACITY IS FULL";
            }
            location = "Location : " + location + getParty(id);
            string yarddetails = "{'location':'" + location + "','color1':'" + yardAcolor + "','color2':'" + yardBcolor + "'}";

            //service.Dispose();

            return yarddetails;
        }


        [HttpGet]
        [Route("api/Yard/SendToYard/{containerno}/{yardid}")]
        public string SendToYard(string containerno, string yardid)
        {

            SheetsService service = getService();

            var yardCapacity = getYardCapacity(service);
            var count = 0;
            var cell = "";

            if (yardid == "1A")
            {
                if (yardCapacity.yardA >= 100)
                {
                    return "{'response':'YARD 1A CAPACITY IS FULL'}";
                }
                count = yardCapacity.yardA + 1;
                cell = "B2";

            }
            else
            {
                if (yardCapacity.yardB >= 100)
                {
                    return "{'response':'YARD 1B CAPACITY IS FULL'}";
                }
                count = yardCapacity.yardB + 1;
                cell = "B3";

            }
           

            var range = $"{sheet}!" + cell;
            var valueRange = new ValueRange();

            var oblist = new List<object>() { count };
            valueRange.Values = new List<IList<object>> { oblist };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();

             // service.Dispose();

            return "{'response':'Location Saved'}";
        }

        private SheetsService getService()
        {
            SheetsService service;
            GoogleCredential credential;
            using (var stream = new FileStream("capacity_client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;

        }

        private YardCapacity getYardCapacity(SheetsService service)
        {

            var range = $"{sheet}!A:C";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();

            YardCapacity yardCapacity = new YardCapacity();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                yardCapacity.yardA = Convert.ToInt32(values[1][1].ToString());
                yardCapacity.yardB = Convert.ToInt32(values[2][1].ToString());
            }

            return yardCapacity;
        }

        private string getYardColor(int presentCapacity)
        {
            if (presentCapacity <= 33)
            {
                return GREEN;
            }
            else if (presentCapacity >= 67)
            {
                return RED;
            }
            else
            {
                return ORANGE;
            }
        }

        private string getParty(string containerno)
        {

            var JsonString = System.IO.File.ReadAllText("output.json");
            List<containerdetails> data = JsonConvert.DeserializeObject<List<containerdetails>>(JsonString);

            var obj = data.Where(x => x.containerno == containerno).FirstOrDefault();
            return obj != null && obj.party != "" ? "\n    Party : " + obj.party : "";
        }

    }
}
