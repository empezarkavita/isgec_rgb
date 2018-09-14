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

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("mail/[controller]/[action]")]
    public class GmailController : Controller
    {
        //public static UserCredential credential;
        public static string[] Scopes = {
            GmailService.Scope.GmailReadonly,
            GmailService.Scope.GmailSend,
            GmailService.Scope.GmailCompose
            };

        [HttpGet]
        public async Task<string> Send()
        {
            /*Sample Json Data contains Container no and Status */
            // var jsonstatusdata = File.ReadAllText("StatusData.json");
            //List<ContainmentStatusData> LstatusData = JsonConvert.DeserializeObject<List<ContainmentStatusData>>(jsonstatusdata);


            /* Read the credentials.json that is stored locally and refresh the token */

            //  UserCredential credential;

            // using (var stream =
            //     new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            // {
            //     string credPath = "token.json";
            //     credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //         GoogleClientSecrets.Load(stream).Secrets,
            //         Scopes,
            //         "user",
            //         CancellationToken.None,
            //         new FileDataStore(credPath, true)).Result;
            //     Console.WriteLine("Credential file saved to: " + credPath);
            // }

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets { ClientId = "598123410805-o68vcod1d67qnsajd1labodvlnbufgbd.apps.googleusercontent.com", ClientSecret = "35ko1a8z0rYhUK7-9r9dhiRb" },
  new[] { "https://mail.google.com/", "https://www.googleapis.com/auth/userinfo.email" }, "user", CancellationToken.None, new FileDataStore("Analytics.Auth.Store")).Result;



            // using (var stream =
            //   new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            // {

            //     string credPath = Directory.GetCurrentDirectory();
            //     credPath = Path.Combine(credPath, @"credential\token.json");

            //     credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //       GoogleClientSecrets.Load(stream).Secrets,
            //       Scopes,
            //       "user",
            //       CancellationToken.None,
            //       new FileDataStore(credPath, true)).Result;

            // }

            // Create Gmail API service. 
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail API",
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
                    if (emailInfoResponse != null && PayLoadHeader.Any(mParts => mParts.Name == "Subject" && mParts.Value.Contains("Require Container Details")))
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

                        GetAttachments(service, "me", emailInfoResponse.Id);

                        for (int i = 0; i < matches.Count; i++)
                        {
                            string party = "";
                            string ContainerNo = matches[i].ToString();
                            var queryMsc = mscReference.Where("containerno", QueryOperator.Equal, ContainerNo);
                            var queryIsgec = isgecReference.Where("containerno", QueryOperator.Equal, ContainerNo);

                            QuerySnapshot querySnapshotMsc = await queryMsc.SnapshotAsync();
                            if (querySnapshotMsc.Documents.Count > 0)
                            {
                                party = await GetParty(querySnapshotMsc, mscReference);
                            }
                            else
                            {
                                QuerySnapshot querySnapshotIsgec = await queryIsgec.SnapshotAsync();
                                party = await GetParty(querySnapshotIsgec, isgecReference);

                            }


                            strtextbody.Append("<tr><td>" + ContainerNo + "</td><td> " + party + "</td> </tr>");
                        }

                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        var enc1252 = Encoding.GetEncoding(1252);
                        var msg = new AE.Net.Mail.MailMessage
                        {
                            Subject = "Re: " + subject.ToString(),
                            Body = strtextbody.ToString(),
                            From = new MailAddress("testmail21082018@gmail.com")
                        };
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

        private void GetAttachments(GmailService service, String userId, String messageId)
        {
            try
            {
                Message message = service.Users.Messages.Get(userId, messageId).Execute();
                IList<MessagePart> parts = message.Payload.Parts;
                foreach (MessagePart part in parts)
                {
                    if (!String.IsNullOrEmpty(part.Filename))
                    {
                        String attId = part.Body.AttachmentId;
                        MessagePartBody attachPart = service.Users.Messages.Attachments.Get(userId, messageId, attId).Execute();

                        // Converting from RFC 4648 base64 to base64url encoding
                        // see http://en.wikipedia.org/wiki/Base64#Implementations_and_history
                        String attachData = attachPart.Data.Replace('-', '+');
                        attachData = attachData.Replace('_', '/');

                        byte[] data = Convert.FromBase64String(attachData);
                        System.IO.File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), part.Filename), data);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }

    }
}