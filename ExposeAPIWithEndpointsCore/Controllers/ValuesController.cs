using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExposeAPIWithEndpointsCore.Models;
using Google.Apis.Auth.OAuth2;


using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Google.Cloud.Firestore;
using System.IO;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public async Task<string> Get(string container)
        {
            var path = Directory.GetCurrentDirectory() +"\\firestore_client_secret.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            var credential = GoogleCredential.GetApplicationDefault();
           

            FirestoreDb db = FirestoreDb.Create("rgbfirestore");

            CollectionReference collection = db.Collection("msc-mnr");

            DocumentReference docRef = db.Collection("msc-mnr").Document("LA");
            Dictionary<string, object> city = new Dictionary<string, object>
            {
                { "name", "Los Angeles" },
                { "state", "CA" },
                { "country", "USA" }
            };
            WriteResult writeResult = await docRef.SetAsync(city);

            //DocumentReference document = await collection.SetAsync(new { color = "123", containerno = "1815", footer = "1815", party = "1815", status = "1815" });

            // A DocumentReference doesn't contain the data - it's just a path.
            // Let's fetch the current document.
            //DocumentSnapshot snapshot = await document.SnapshotAsync();


            //             DocumentReference docRef = db.Collection("cities").Document("LA");
            // Dictionary<string, object> city = new Dictionary<string, object>
            // {
            //     { "name", "Los Angeles" },
            //     { "state", "CA" },
            //     { "country", "USA" }
            // };
            //             Google.Cloud.Firestore.WriteResult writeResult = await docRef.SetAsync(city);

            // CollectionReference collection = db.Collection("containers");
            // DocumentReference document = await collection.AddAsync(new { ID = "ABC", Name = "SU" });

            // // A DocumentReference doesn't contain the data - it's just a path.
            // // Let's fetch the current document.
            // DocumentSnapshot snapshot = await document.SnapshotAsync();


            // DocumentReference document = db.Collection("containers").Document("ejJPYkQu3p6NFwcLFqBV");
            // DocumentSnapshot snapshot =  await document.SnapshotAsync();

            var containers = "";// getcontainerList();

            return JsonConvert.SerializeObject(containers);


        }

        // public async Task<string> InsertContainers()
        // {
        //     var client = new FireSharp.FirebaseClient(config);

        //     var containers = getcontainerList();

        //     string outval = "";

        //     foreach (var container in containers)
        //     {
        //         FirebaseResponse response = await client.SetTaskAsync(container.containerno, container);
        //         containerdetails result = response.ResultAs<containerdetails>();
        //         outval += result.containerno;
        //     }

        //     return "Data Inserted";

        // }

        // public async Task<string> GettContainers()
        // {

        //     var client = new FireSharp.FirebaseClient(config);

        //     FirebaseResponse response = await client.GetTaskAsync("CLHU8825463");
        //     var containers = JsonConvert.DeserializeObject<containerdetails>(response.Body);
        //     return JsonConvert.SerializeObject(containers);

        // }

        // public async Task<string> UpdateContainers()
        // {
        //     var client = new FireSharp.FirebaseClient(config);
        //     var container = getcontainerList().Where(x => x.containerno == "CLHU8825463").FirstOrDefault();
        //     container.sbno = "testno";
        //     FirebaseResponse response = await client.UpdateTaskAsync("CLHU8825463", container);
        //     return response.Body;
        // }

        // public async Task<string> DeleteContainers()
        // {    <!-- <PackageReference Include="Google.Cloud.Firestore" Version="*"/> -->

        //     var client = new FireSharp.FirebaseClient(config);
        //     FirebaseResponse response = await client.DeleteTaskAsync("CLHU8825463");
        //     return response.Body;
        // }





        // // GET api/values/5
        // [HttpGet("{id}")]
        // public string Get(string id)
        // {
        //     var containers = getcontainerList();
        //     var containerdata = containers.Where(x => x.containerno == id).FirstOrDefault();
        //     if (containerdata == null)
        //     {
        //         return "";
        //     }
        //     else
        //     {
        //         containerdata.color = getColorCode(containerdata);
        //         return JsonConvert.SerializeObject(containerdata);
        //     }


        // }

        // // POST api/values
        // [HttpPost]
        // public void Post([FromBody]string value)
        // {
        // }

        // // PUT api/values/5
        // [HttpPut("{id}")]
        // public void Put(int id, [FromBody]string value)
        // {
        // }

        // // DELETE api/values/5
        // [HttpDelete("{id}")]
        // public void Delete(int id)
        // {
        // }

        // public class containerdetails
        // {
        //     public string containerno;
        //     public string shippingline;
        //     public string sbno;
        //     public string movementdate;
        //     public string color;
        // }

        // private List<containerdetails> getcontainerList()
        // {

        //     List<containerdetails> containers = new List<containerdetails>();

        //     containers.Add(new containerdetails
        //     {
        //         containerno = "HLBU1151814",
        //         shippingline = "HAPAG-LLYOD",
        //         sbno = "4438743",
        //         movementdate = "00-00-0000",
        //         color = "#99FF0000"
        //     });

        //     containers.Add(new containerdetails
        //     {
        //         containerno = "HLXU8035999",
        //         shippingline = "HAPAG-LLYOD",
        //         sbno = "",
        //         movementdate = "12-10-2017",
        //         color = "#99FFA500"
        //     });

        //     containers.Add(new containerdetails
        //     {
        //         containerno = "HLXU 855254 3",
        //         shippingline = "HAPAG-LLYOD",
        //         sbno = "4442715",
        //         movementdate = "12-10-2017",
        //         color = "#99008000"
        //     });



        //     containers.Add(new containerdetails
        //     {
        //         containerno = "SUDU6565934",
        //         shippingline = "EMPEZAR",
        //         sbno = "3784324",
        //         movementdate = "1-1-2017",
        //         color = "#99008000"
        //     });


        //     containers.Add(new containerdetails
        //     {
        //         containerno = "CLHU8825463",
        //         shippingline = "HAPAG-LLYOD",
        //         sbno = "4673356",
        //         movementdate = "6-12-2017",
        //         color = "#99FFA500"
        //     });


        //     containers.Add(new containerdetails
        //     {
        //         containerno = "MAEU4670844",
        //         shippingline = "MSC",
        //         sbno = "745784",
        //         movementdate = "16-11-2018",
        //         color = "#99FF0000"
        //     });
        //     return containers;
        // }

        // private string getColorCode(containerdetails data)
        // {
        //     if (String.IsNullOrEmpty(data.movementdate))
        //     {
        //         return "#99FF0000";//Red
        //     }
        //     else if (data.sbno.Length == 0)
        //     {
        //         return "#99FFA500";//Orange
        //     }
        //     else
        //     {
        //         return "#99008000"; //Green
        //     }
        // }
    }
}
