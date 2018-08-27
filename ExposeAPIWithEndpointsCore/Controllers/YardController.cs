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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;
using Google.Cloud.Firestore;
using System.Threading.Tasks;

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
        const string ORANGE = "#99FFFF00";
        const string GREEN = "#99008000";

        const string sheet = "capacity";
        const string yard_sheet = "yard";


        // GET api/values
        //private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string[] StorageScope = { StorageService.Scope.DevstorageReadWrite };
        const string ApplicationName = "Current Legislators";

        const string SpreadsheetId = "10sVqu4KBFiwTFVQr3H2FItSU17tAShuOYOe99LQrD-A";

        // static SheetsService service;
        static IHostingEnvironment _hostingEnvironment;

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

            UpdateCapacityToSheet(service, count, cell);
            InsertContainerToSheet(service, containerno, yardid, "", "");
            // service.Dispose();

            return "{'response':'Location Saved'}";
        }

        private static void UpdateCapacityToSheet(SheetsService service, int count, string cell)
        {
            var range = $"{sheet}!" + cell;
            var valueRange = new ValueRange();

            var oblist = new List<object>() { count };
            valueRange.Values = new List<IList<object>> { oblist };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();
        }

        private static async void InsertContainerToSheet(SheetsService service, string containerno, string yardid, string snapshot, string color)
        {
            var range = $"{yard_sheet}!A:D";
            var valueRange = new ValueRange();
           // DateTime indianTime =  TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

            string captureDate = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
            var oblist = new List<object>() { containerno, yardid, captureDate, snapshot };
            valueRange.Values = new List<IList<object>> { oblist };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = appendRequest.Execute();

            var gcpCredentaialPath = "firestore_client_secret.json";
                        System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gcpCredentaialPath);
           
            var gcpCredential = GoogleCredential.GetApplicationDefault();


            FirestoreDb db = FirestoreDb.Create("rgbfirestore");

            CollectionReference collection = db.Collection("yard-containers");

            DocumentReference docRef = collection.Document(containerno + "_" + Guid.NewGuid());

            Dictionary<string, object> container = new Dictionary<string, object>
            {
                { "containerno", containerno },
                { "snapshot", snapshot },
                { "yardcolor", color },
                { "yardid", yardid },
                { "captureDate",  captureDate} //SentinelValue.ServerTimestamp
            };

            WriteResult writeResult = await docRef.SetAsync(container);

        }

        private SheetsService getService()
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


        // [HttpPost]
        // [Route("upload")]
        // public string PostUserImage()
        // {

        // try
        // {
        //     var file = Request.Form.Files[0];
        //     string folderName = "Upload";
        //     string webRootPath = _hostingEnvironment.WebRootPath;
        //     string newPath = Path.Combine(webRootPath, folderName);
        //     if (!Directory.Exists(newPath))
        //     {
        //         Directory.CreateDirectory(newPath);
        //     }
        //     if (file.Length > 0)
        //     {
        //         string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
        //         string fullPath = Path.Combine(newPath, fileName);
        //         using (var stream = new FileStream(fullPath, FileMode.Create))
        //         {
        //             file.CopyTo(stream);
        //         }
        //     }
        //     return "Upload Successful.";
        // }
        // catch (System.Exception ex)
        // {
        //     return "Upload Failed: " + ex.Message;
        // }

        // }

        [HttpPost]
        [Route("api/Yard/PostToYard")]
        public string PostToYard(string containerno, string yardid)
        {

            SheetsService service = getService();

            var yardCapacity = getYardCapacity(service);

            string yardcolor = yardid == "1A" ? getYardColor(yardCapacity.yardA) : getYardColor(yardCapacity.yardB);


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

            var file = Request.Form.Files[0];
            UpdateCapacityToSheet(service, count, cell);

            string snapshot = UploadAndGetSnapshotUrl(file, containerno);
            InsertContainerToSheet(service, containerno, yardid, snapshot, yardcolor);


            return "{'response':'Location Saved'}";
        }

        private string UploadAndGetSnapshotUrl(IFormFile file, string containerno)
        {
            if (file == null || file.Length == 0)
                return "error";

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyToAsync(stream);
                GoogleCredential credential;
                using (var gstream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(gstream)
                        .CreateScoped(StorageScope);
                }

                var client = StorageClient.Create(credential);

                // Create a bucket
                string bucketName = "rgb";
                // var bucket = client.CreateBucket("pin-code-recognizer", bucketName);

                // Upload some files
                string name = containerno + ".jpg";
                var obj2 = client.UploadObject(bucketName, "yard/" + name, "image/jpeg", stream);

                return "https://storage.cloud.google.com/rgb/yard/" + name;
            }
        }


        [HttpPost]
        [Route("api/Yard/PostToYardBase")]
        public async Task<string> PostToYardBase(string containerno, string yardid, string base64image)
        {

            SheetsService service = getService();

            var yardCapacity = getYardCapacity(service);

            string yardcolor = yardid == "1A" ? getYardColor(yardCapacity.yardA) : getYardColor(yardCapacity.yardB);


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


            UpdateCapacityToSheet(service, count, cell);
            var snapshot = "";
            var bytes = Convert.FromBase64String(base64image);

            if (bytes.Length <= 0)
                return "error";

            Random random = new Random();  
            string name = random.Next(0, 1000).ToString()+"_"+containerno + ".jpg";
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        name);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();

                GoogleCredential credential;
                using (var gstream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(gstream)
                        .CreateScoped(StorageScope);
                }

                var client = StorageClient.Create(credential);

                // Create a bucket
                string bucketName = "rgb";
                // var bucket = client.CreateBucket("pin-code-recognizer", bucketName);

                // Upload some files

                var obj2 = client.UploadObject(bucketName, "yard/" + name, "image/jpeg", stream);

                snapshot = "https://storage.cloud.google.com/rgb/yard/" + name;
            }

       
            InsertContainerToSheet(service, containerno, yardid, snapshot, yardcolor);


            return "{'response':'Location Saved'}";
        }

    }
}
