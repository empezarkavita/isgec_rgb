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

    

    public class EnblocCount
    {
        public int container_count;
        public int container_enbloced;
    }

    public class EnblocController : Controller
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

        public EnblocController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;


        // GET api/values/5
      
       
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

      

        [HttpPost]
        [Route("api/enbloc/PostToYardBase")]
        public async Task<string> PostToYardBase(string containerno, string vesselno, string yardid, string base64image)
        {

            var gcpCredentaialPatha = "firestore_client_secret.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gcpCredentaialPatha);
            var gcpCredential = GoogleCredential.GetApplicationDefault();
            FirestoreDb db = FirestoreDb.Create("rgbfirestore");

            //Get Yard Color & Count 
            Yard yard = await GetYardData(db,yardid);           
 
            if (yard.present_count == yard.total_capacity)
            {
                 return "{'response':'YARD CAPACITY IS FULL'}";               
            }

         
            //Change Yard Color 
            string yardcolor  = await UpdateYard(db,yard);

            //Change Enbloc Count on Vessal 
            EnblocCount enbloc_count =  await GetEnblocCount(db,vesselno);
           
            await UpdateEnblocedCount(db,vesselno,enbloc_count.container_enbloced);


            var snapshot = "";
            var bytes = Convert.FromBase64String(base64image);

            if (bytes.Length <= 0)
                return "error";

            Random random = new Random();
            string name = random.Next(0, 1000).ToString() + "_" + containerno + ".jpg";
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
                string bucketName = "elabs";
                // var bucket = client.CreateBucket("pin-code-recognizer", bucketName);

                // Upload some files

                var obj2 = client.UploadObject(bucketName, "yard/" + name, "image/jpeg", stream);

                snapshot = "https://storage.cloud.google.com/elabs/yard/" + name;
            }


            PostToFireStore(db,containerno,vesselno, yardid, snapshot, yardcolor);


            return "{'response':'Location Saved'}";
        }


        [HttpGet]
        [Route("api/enbloc/GetYardDetails/{containerno}/{userName}")]
        public async Task<string> GetYardDetails(string containerno, string userName)
        {

            var gcpCredentaialPatha = "firestore_client_secret.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gcpCredentaialPatha);
            var gcpCredential = GoogleCredential.GetApplicationDefault();
            FirestoreDb db = FirestoreDb.Create("rgbfirestore");


            string vessel_no =await GetVesselNumber(db,containerno);
            EnblocCount enbloc_count =  await GetEnblocCount(db,vessel_no);
           // string containers_enbloced = await GetContainersEnbloced(db,vessel_no);
            
            List<Yard> yards =  await GetYardDetails(db,userName);
            string count = enbloc_count.container_enbloced.ToString() + "/" + enbloc_count.container_count.ToString();
            string location = "Vessel : " + vessel_no;
            string yarddetails = "{\"location\":\"" + location + "\",\"vessel\":\"" + vessel_no + "\",\"count\":\"" + count +  "\",\"yards\":" + JsonConvert.SerializeObject(yards) + "}";

            return yarddetails;
        }

        private async Task<string> GetVesselNumber(FirestoreDb db,string containerno)
        {
            var collectionReference = db.Collection("enbloc-details");
            var query = collectionReference.Where("container_no", QueryOperator.Equal, containerno);

            string vessel_no = "";

            QuerySnapshot querySnapshot = await query.SnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {

                DocumentReference docRefCon = collectionReference.Document(documentSnapshot.Id);
                DocumentSnapshot snapshotCon = await docRefCon.SnapshotAsync();
                if (snapshotCon.Exists)
                {
                    Dictionary<string, object> enblock = snapshotCon.ToDictionary();
                    foreach (KeyValuePair<string, object> data in enblock)
                    {
                        if (data.Key == "vessel_no")
                        {
                            vessel_no = data.Value.ToString();
                        }

                    }
                }
            }
            return vessel_no;

        }

        private async Task<EnblocCount> GetEnblocCount(FirestoreDb db,string vessel_no)
        {
            var collectionReference = db.Collection("enbloc");
            var query = collectionReference.Where("vessel_no", QueryOperator.Equal, vessel_no);

            string enbloc_count = "0";
            EnblocCount eData = new EnblocCount();
            QuerySnapshot querySnapshot = await query.SnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {

                DocumentReference docRefCon = collectionReference.Document(documentSnapshot.Id);
                DocumentSnapshot snapshotCon = await docRefCon.SnapshotAsync();
                if (snapshotCon.Exists)
                {
                    Dictionary<string, object> enblock = snapshotCon.ToDictionary();
                    foreach (KeyValuePair<string, object> data in enblock)
                    {
                        if (data.Key == "container_count")
                        {
                            eData.container_count =Convert.ToInt32(data.Value);
                        }
                        if (data.Key == "container_enbloced")
                        {
                            eData.container_enbloced =Convert.ToInt32(data.Value);
                        }

                    }
                }
            }
            return eData;
        }  


        private async Task<string> GetContainersEnbloced(FirestoreDb db,string vessel_no)
        {
            var collectionReference = db.Collection("yard-containers");
            var query = collectionReference.Where("vessel_no", QueryOperator.Equal, vessel_no);


           

            QuerySnapshot querySnapshot = await query.SnapshotAsync();
            string enbloc_count = Convert.ToString(querySnapshot.Documents.Count);
           
            return enbloc_count;
        }  

        private async Task<List<Yard>> GetYardDetails(FirestoreDb db,string user)
        {
            Dictionary<string, object> yards = new Dictionary<string, object>();

            var collectionReference = db.Collection("yard-capacity");
            var query = collectionReference.Where("user", QueryOperator.Equal, "user_1");
            QuerySnapshot querySnapshot = await query.SnapshotAsync();

            List<Yard> lstYard = new List<Yard>();
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {

                DocumentReference docRefCon = collectionReference.Document(documentSnapshot.Id);

                DocumentSnapshot snapshotCon = await docRefCon.SnapshotAsync();
                
                if (snapshotCon.Exists)
                {
                    Yard yard = snapshotCon.Deserialize<Yard>();
                    lstYard.Add(yard);
                   
                }              
            }
            return lstYard;

        }
   
   
        private async Task<Yard> GetYardData(FirestoreDb db,string yardname)
        { 
            Dictionary<string, object> yards = new Dictionary<string, object>();

            var collectionReference = db.Collection("yard-capacity");
            var query = collectionReference.Where("yard", QueryOperator.Equal, yardname);
            QuerySnapshot querySnapshot = await query.SnapshotAsync();

           Yard yard = new Yard();
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {

                DocumentReference docRefCon = collectionReference.Document(documentSnapshot.Id);

                DocumentSnapshot snapshotCon = await docRefCon.SnapshotAsync();
                
                if (snapshotCon.Exists)
                {
                    yard = snapshotCon.Deserialize<Yard>();
                   
                   
                }              
            }
            return yard;

        }
   
   
        private async Task<string>UpdateYard(FirestoreDb db,Yard yard){

               
                CollectionReference collection = db.Collection("yard-capacity");
                DocumentReference docRef = collection.Document(yard.yard);

                string yardColor = getYardColor(yard.present_count +1) ;
                Dictionary<string, object> yardInfo = new Dictionary<string, object>
                {
                    { "present_color", yardColor},
                    { "present_count", yard.present_count +1 },
                    { "total_capacity", yard.total_capacity},
                    { "user", yard.user },
                    { "yard", yard.yard },
                  
                };
            WriteResult writeResult = await docRef.SetAsync(yardInfo);
            return yardColor;
        }


        private async Task<string> UpdateEnblocedCount(FirestoreDb db,string vesselno,int container_enbloced)
        {
            CollectionReference collection = db.Collection("enbloc");
                DocumentReference docRef = collection.Document(vesselno);

               
            Dictionary<FieldPath, object> eInfo = new Dictionary<FieldPath, object>
            {
               {  new FieldPath("container_enbloced"), container_enbloced+1 }   
            };
            WriteResult writeResult = await docRef.UpdateAsync(eInfo);

            return "";
        }

        private static async void PostToFireStore( FirestoreDb db, string containerno, string vesselno,string yardid, string snapshot, string color, string timestamp = null)
        {

           

            CollectionReference collection = db.Collection("yard-containers");

            DocumentReference docRef = collection.Document(containerno + "_" + Guid.NewGuid());


            Dictionary<string, object> container = new Dictionary<string, object>
            {
                { "containerno", containerno },
                { "snapshot", snapshot },
                { "yardcolor", color },
                { "yardid", yardid },
                { "vesselno", vesselno },
                { "captureDate", Timestamp.GetCurrentTimestamp()} //SentinelValue.ServerTimestamp
            };

            WriteResult writeResult = await docRef.SetAsync(container);

        }
    }
}
