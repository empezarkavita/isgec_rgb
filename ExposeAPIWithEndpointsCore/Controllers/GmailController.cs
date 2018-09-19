using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System;
using Google.Apis.Http;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;
using OfficeOpenXml;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    public class GmailController : Controller
    {
        public static string[] Scopes = {
            GmailService.Scope.GmailReadonly,
            GmailService.Scope.GmailSend,
            GmailService.Scope.GmailCompose
            };

        static readonly string[] StorageScope = { StorageService.Scope.DevstorageReadWrite };

        [HttpGet]
        [Route("mail/gmail/send")]
        public async Task<string> Send()
        {

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "146633937037-7a4hfiqn3sp0lfbts5k0h3tb9h3ipv9c.apps.googleusercontent.com",
                    ClientSecret = "IGniQmnUKRADxoZQh0jqWQFa"
                },
                Scopes = Scopes,
            });

          
            var credential = new UserCredential(flow, Environment.UserName, new TokenResponse
            {
                AccessToken = "ya29.GlsdBk2fqEeCmMBkjceYNSJ0Ee8SMIkqa1trjC7cgkjdkpBi0xA6kKEqNI8_iTuX9wXXqt7LTTtJFW6S8EzNk0izPthAfL4JL3mTC1hjQDpUDFaMZRoX9Q9yZ1Af",
                RefreshToken = "1/dRAhSp6zDZHEuc3GASJHYUtRhOTvQ0RxpgoLBgTkifEe-i53zmoEOLRT7owe0rk5"
            });
            var service = new GmailService(new BaseClientService.Initializer()
            {
                ApplicationName = "Gmail API",
                HttpClientInitializer = credential
            });

            var inboxlistRequest = service.Users.Messages.List("me");
            inboxlistRequest.LabelIds = "INBOX";
            inboxlistRequest.IncludeSpamTrash = true;
            inboxlistRequest.Q = "is:unread";
            //get our emails 
            var emailListResponse = inboxlistRequest.Execute();

            var gcpCredentaialPath = "firestore_client_secret.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gcpCredentaialPath);


            FirestoreDb db = FirestoreDb.Create("rgbfirestore");
            var mscReference = db.Collection("msc-mnr");
            var isgecReference = db.Collection("isgec");



            if (emailListResponse != null && emailListResponse.Messages != null)
            {
                //loop through each email and get what fields you want... 
                foreach (var email in emailListResponse.Messages)
                {

                    var emailInfoRequest = service.Users.Messages.Get("me", email.Id);
                    var emailInfoResponse = emailInfoRequest.Execute();
                    var PayLoadHeader = emailInfoResponse.Payload.Headers;
                    if (emailInfoResponse != null && PayLoadHeader.Any(mParts => mParts.Name == "Subject" && mParts.Value.Contains("Enbloc")))
                    {

                        var ID = emailInfoResponse.Id;
                        string msgBody = "";
                        string subject = PayLoadHeader.Single(mPart => mPart.Name == "Subject").Value;
                        string from = PayLoadHeader.Single(mPart => mPart.Name == "From").Value;
                        foreach (MessagePart p in emailInfoResponse.Payload.Parts)
                        {
                            if (p.MimeType == "text/html")
                            {
                                byte[] data = FromBase64ForUrlString(p.Body.Data);
                                msgBody = Encoding.UTF8.GetString(data);
                            }
                        }
                        MatchCollection matches = Regex.Matches(msgBody, "[A-Z]{3}[JRUZ][0-9]{7}");

                        StringBuilder strtextbody = new StringBuilder();

                        strtextbody.Append("<table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + "><tr bgcolor='#4da6ff'><td><b>Container No</b></td> <td> <b> Party</b> </td></tr>");

                        ProcessAttachments(service, "me", emailInfoResponse.Id);

                        // for (int i = 0; i < matches.Count; i++)
                        // {
                        //     string party = "";
                        //     string ContainerNo = matches[i].ToString();
                        //     var queryMsc = mscReference.Where("containerno", QueryOperator.Equal, ContainerNo);
                        //     var queryIsgec = isgecReference.Where("containerno", QueryOperator.Equal, ContainerNo);

                        //     QuerySnapshot querySnapshotMsc = await queryMsc.SnapshotAsync();
                        //     if (querySnapshotMsc.Documents.Count > 0)
                        //     {
                        //         party = await GetParty(querySnapshotMsc, mscReference);
                        //     }
                        //     else
                        //     {
                        //         QuerySnapshot querySnapshotIsgec = await queryIsgec.SnapshotAsync();
                        //         party = await GetParty(querySnapshotIsgec, isgecReference);

                        //     }


                        //     strtextbody.Append("<tr><td>" + ContainerNo + "</td><td> " + party + "</td> </tr>");
                        // }

                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        var enc1252 = Encoding.GetEncoding(1252);
                        var msg = new AE.Net.Mail.MailMessage
                        {
                            Subject = "Re: " + subject.ToString(),
                            
                            Body ="Your Enbloc has been processed by Empezar's Bot Technology. <br>To check live status,please click on the below link.<br> http://elabs-215913.appspot.com/view/Firestore/Enbloc <br><br><br>Thank You.",
                            
                            From = new MailAddress("empezar.systems@gmail.com"),

                        };
                        msg.ContentType = "text/html";
                        msg.To.Add(new MailAddress(from));
                        msg.ReplyTo.Add(msg.From); // Bounces without this!!
                        var msgStr = new StringWriter();
                        msg.Save(msgStr);

                        var result = service.Users.Messages.Send(new Message
                        {
                            ThreadId = ID,
                            Id = ID,
                            Raw = Base64UrlEncode(msgStr.ToString())
                        }, "me").Execute();

                        var markAsReadRequest = new ModifyThreadRequest {RemoveLabelIds = new[] {"UNREAD"}};
                            await service.Users.Threads.Modify(markAsReadRequest, "me", ID)
                                    .ExecuteAsync();


                    }
                }

            }

            return "Sent Mail";
        }


        private async Task<string> GetParty(QuerySnapshot querySnapshot, CollectionReference collectionRef)
        {
            string party = "";
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {

                DocumentReference docRefCon = collectionRef.Document(documentSnapshot.Id);
                DocumentSnapshot snapshotCon = await docRefCon.SnapshotAsync();
                if (snapshotCon.Exists)
                {
                    Dictionary<string, object> container = snapshotCon.ToDictionary();
                    foreach (KeyValuePair<string, object> pair in container)
                    {
                        if (pair.Key == "party")
                        {
                            party = pair.Value.ToString();
                        }

                    }
                }
            }
            return party;
        }
        private byte[] FromBase64ForUrlString(string base64ForUrlInput)
        {
            int padChars = (base64ForUrlInput.Length % 4) == 0 ? 0 : (4 - (base64ForUrlInput.Length % 4));
            StringBuilder result = new StringBuilder(base64ForUrlInput, base64ForUrlInput.Length + padChars);
            result.Append(String.Empty.PadRight(padChars, '='));
            result.Replace('-', '+');
            result.Replace('_', '/');
            return Convert.FromBase64String(result.ToString());
        }

        private string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }

        private void ProcessAttachments(GmailService service, String userId, String messageId)
        {
            try
            {
                Message message = service.Users.Messages.Get(userId, messageId).Execute();
                IList<MessagePart> parts = message.Payload.Parts;
                foreach (MessagePart part in parts)
                {
                    if (!String.IsNullOrEmpty(part.Filename) && part.Filename.ToLower().Contains(".xlsx"))
                    {
                        String attId = part.Body.AttachmentId;
                        MessagePartBody attachPart = service.Users.Messages.Attachments.Get(userId, messageId, attId).Execute();

                        String attachData = attachPart.Data.Replace('-', '+');
                        attachData = attachData.Replace('_', '/');

                        byte[] data = Convert.FromBase64String(attachData);
                        string fileName = Guid.NewGuid() + "_" + part.Filename;
                        string cloudUrl = SaveAttachments(fileName, data);

                        PostToFirebase(fileName, cloudUrl);

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }

        private string SaveAttachments(String name, byte[] data)
        {

            var path = Path.Combine(
                         Directory.GetCurrentDirectory(), "wwwroot",
                         name);

            // System.IO.File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), name), data);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                stream.Write(data, 0, data.Length);
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

                var obj2 = client.UploadObject(bucketName, "enbloc/" + name, "application/vnd.ms-excel", stream);
                return "https://storage.cloud.google.com/elabs/enbloc/" + name;
            }

        }

        // private void PostToFirebase(string fileName)
        // {

        //     var path = Path.Combine(
        //                 Directory.GetCurrentDirectory(), "wwwroot",
        //                 fileName);

        //     var excel = new ExcelQueryFactory(path);

        //     var indianaCompanies = from c in excel.WorksheetNoHeader()//Selects data within the B3 to G10 cell range
        //                            select c;

        //     indianaCompanies = indianaCompanies;
        // }

        private static async void PostToFirebase(string fileName, string cloudUrl)
        {
            string pathToExcelFile = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        fileName);



            var gcpCredentaialPath = "firestore_client_secret.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gcpCredentaialPath);

            var gcpCredential = GoogleCredential.GetApplicationDefault();


            FirestoreDb db = FirestoreDb.Create("rgbfirestore");



            // string sheetName = "Sheet1";

            FileInfo file = new FileInfo(pathToExcelFile);
            /// overwrite old file

            List<Enbloc> enblocs = new List<Enbloc>();

            using (ExcelPackage package = new ExcelPackage(file))
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;

                string document_date = Convert.ToString(worksheet.Cells["C1"].Value);
                string vessel = Convert.ToString(worksheet.Cells["B4"].Value);
                string voyage = Convert.ToString(worksheet.Cells["D4"].Value);
                string agent_name = Convert.ToString(worksheet.Cells["B5"].Value);
                string via_no = Convert.ToString(worksheet.Cells["D5"].Value);

                string vesselno = vessel.Split(' ').ToList().Aggregate((x, y) => x.Trim() + y.Trim()) + voyage.ToString();

                int count = 0;
                CollectionReference collection = db.Collection("enbloc-details");
                for (int row = 8; row <= rowCount; row++)
                {
                    if (Convert.ToString(worksheet.Cells[row, 1].Value).Trim() != "")
                    {
                        Dictionary<string, object> enbloc = new Dictionary<string, object>
                        {
                            {"srl",row-7},
                            {"srlno",Convert.ToString(worksheet.Cells[row, 1].Value).Trim()},
                            {"container_no", Convert.ToString(worksheet.Cells[row, 2].Value).Trim()},
                            {"container_type", Convert.ToString(worksheet.Cells[row, 3].Value).Trim()},
                            {"wt", Convert.ToString(worksheet.Cells[row, 4].Value).Trim()},
                            {"cargo", Convert.ToString(worksheet.Cells[row, 5].Value).Trim()},
                            {"iso_code", Convert.ToString(worksheet.Cells[row, 6].Value).Trim()},
                            {"seal_no_1", Convert.ToString(worksheet.Cells[row, 7].Value).Trim()},
                            {"seal_no_2", Convert.ToString(worksheet.Cells[row, 8].Value).Trim()},
                            {"seal_no_3", Convert.ToString(worksheet.Cells[row, 9].Value).Trim()},
                            {"imdg_class", Convert.ToString(worksheet.Cells[row, 10].Value).Trim()},
                            {"refer_temrature", Convert.ToString(worksheet.Cells[row, 11].Value).Trim()},
                            {"oog_deatils", Convert.ToString(worksheet.Cells[row, 12].Value).Trim()},
                            {"container_gross_details", Convert.ToString(worksheet.Cells[row, 13].Value).Trim()},
                            {"cargo_description", Convert.ToString(worksheet.Cells[row, 14].Value).Trim()},
                            {"bl_number", Convert.ToString(worksheet.Cells[row, 15].Value).Trim()},
                            {"name", Convert.ToString(worksheet.Cells[row, 16].Value).Trim()},
                            {"item_no", Convert.ToString(worksheet.Cells[row, 17].Value).Trim()},
                            {"disposal_mode", Convert.ToString(worksheet.Cells[row, 18].Value).Trim()},
                            {"vessel_no", Convert.ToString(vesselno)}

                        };
                        count++;
                        DocumentReference docRef1 = collection.Document(vesselno + "||" + Convert.ToString(worksheet.Cells[row, 2].Value));
                        WriteResult writeResult1 = await docRef1.SetAsync(enbloc);
                    }
                }


                collection = db.Collection("enbloc");
                DocumentReference docRef = collection.Document(vesselno);
                Dictionary<string, object> vesselInfo = new Dictionary<string, object>
                {
                    { "guid", Guid.NewGuid().ToString() },
                    { "agent_name", agent_name },
                    { "container_count", count},
                    { "created_date", Timestamp.GetCurrentTimestamp() },
                    { "document_date", document_date },
                    { "enbloc_excel", cloudUrl },
                    { "vessel", vessel },
                    { "via_no", via_no },
                    { "voyage", voyage },
                    { "vessel_no", vesselno }
                };
                WriteResult writeResult = await docRef.SetAsync(vesselInfo);

            }
        }


      


    }
}