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
using Google.Cloud.Firestore;

namespace ExposeAPIWithEndpointsCore.ISGEC
{
    public class IsgecFirebaseController : Controller
    {

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        [HttpGet]
        [Route("api/isgec/Create")]
        public string Create()
        {
            return InsertContainersList().Result;
        }

        private async Task<string> InsertContainersList()
        {
            IList<IList<object>> values = readContainerListExcel();



            var gcpCredentaialPath = "firestore_client_secret.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gcpCredentaialPath);

            var gcpCredential = GoogleCredential.GetApplicationDefault();


            FirestoreDb db = FirestoreDb.Create("rgbfirestore");

            CollectionReference collection = db.Collection("isgec");


            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    if(row[7].ToString().ToUpper().Length == 11){
                    DocumentReference docRef = collection.Document(row[7].ToString().ToUpper());

                    Dictionary<string, object> container = new Dictionary<string, object>
                    {

                        { "color", Common.getColorCode(row[0].ToString(),row[4].ToString()) },
                        { "containerno", row[7].ToString().ToUpper() },
                        { "footer", "Shipping Line : " + row[2].ToString() + "\n" + "SB No : " + row[0].ToString() + "\n" + "Movement Date : " + row[4].ToString() },
                        { "party", row[8].ToString() }
                    };

                    WriteResult writeResult = await docRef.SetAsync(container);
                    }

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

            var range = $"{Constants.sheet}!A:J";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(Constants.SpreadsheetId, range);

            ValueRange response = request.Execute();

            return response.Values;
        }



    }
}
