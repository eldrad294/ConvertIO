using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using WebRole1.Models;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net.Mail;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private QueueClient queueClient;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            queueClient = QueueClient.CreateFromConnectionString(connectionString, "ConvertIO");

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                UploadViewModel uvm = ReceiveMessage();
                if (uvm != null)
                {
                    Trace.TraceInformation("SPAWNED WORKER");
                    //Convert File with selected extension
                    uvm.File = Convert.ConvertFile(uvm.File, uvm.EndExtension);

                    if (uvm.File != null)
                    {
                        //Upload UploadViewModel to Blob Storage
                        string BlobUrl = UploadAsBlob(uvm);

                        //Send Email with 
                        sendMail(uvm.Email, uvm.Email, BlobUrl);
                    }
                }

                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }

        private UploadViewModel ReceiveMessage()
        {
            BrokeredMessage orderOutMsg = queueClient.Receive();
            //var messageBodyType = Type.GetType(receivedMessage.Properties["messageType"].ToString());

            if (orderOutMsg != null)
            {
                orderOutMsg.Complete();
                // Deserialize the message body to an object of type UploadViewModel
                return orderOutMsg.GetBody<UploadViewModel>();
            }
            return null;
        }

        private string UploadAsBlob(UploadViewModel uvm)
        {
            string ContainerName = "convertedimages";
            string BlobName = "IMAGE." + uvm.EndExtension;

            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            //Setting Container Permissions to public
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);

            // Create or overwrite the "myblob" blob with contents from a local file
            using (Stream ms = new MemoryStream(uvm.File))
            {
                blockBlob.UploadFromStream(ms);
            }
            return "https://cloudiostorage.blob.core.windows.net/" + ContainerName + "/" + BlobName;
        }

        private void sendMail(string emailTo, string emailFrom, string BlobUri)
        {
            try {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("convertiodemo@gmail.com", "blackwidow"),
                    EnableSsl = true
                };
                client.Send(emailFrom, emailTo, "You Converted Image - CONVERTIO", "Your converted image can be downloaded from this link: \n" + BlobUri);
            }catch(Exception e){
                Console.WriteLine(e);
            }
        }
    }
}
