using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;
using Google.Cloud.Firestore;
using System.Threading.Tasks;

namespace ExposeAPIWithEndpointsCore.Controllers
{

    public class ContainerFireController : Controller
    {
        static readonly string[] StorageScope = { StorageService.Scope.DevstorageReadWrite };

        [HttpPost]
        [Route("api/ContainerFire/PostToYardBase")]
        public async Task<string> PostToYardBase(string containerno, string source, string base64image)
        {


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
                string bucketName = "rgb";
                // var bucket = client.CreateBucket("pin-code-recognizer", bucketName);

                // Upload some files

                var obj2 = client.UploadObject(bucketName, "yard/" + name, "image/jpeg", stream);

                snapshot = "https://storage.cloud.google.com/rgb/yard/" + name;
            }


            var gcpCredentaialPatha = "firestore_client_secret.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gcpCredentaialPatha);
            var gcpCredential = GoogleCredential.GetApplicationDefault();


            FirestoreDb db = FirestoreDb.Create("rgbfirestore");



            CollectionReference collection = db.Collection("scanned-containers");

            DocumentReference docRef = collection.Document(containerno + "_" + Guid.NewGuid());

            Dictionary<string, object> container = new Dictionary<string, object>
            {
                { "containerno", containerno },
                { "source", source },
                { "snapshot", snapshot },
                { "captureDate",  DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}
            };
            WriteResult writeResult = await docRef.SetAsync(container);

            return "{'response':'Container Saved'}";
        }

    }
}
