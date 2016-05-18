using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using WebRole1.Models;

namespace WebRole1.Controllers
{
    //[RequireHttps]
    public class HomeController : Controller
    {
        private UploadViewModel uvm = new UploadViewModel();
        private static string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
        private QueueClient Client = QueueClient.CreateFromConnectionString(connectionString, "ConvertIO");

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.UploadMarker = "false";
            ViewBag.Title = "Home Page";
            ViewBag.Email = "";
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase uploadedImage)
        {
            String Email = Request.Form["email"];
            String Extension = Request.Form["extension"];

            HttpPostedFileBase assignmentFile = Request.Files[0];
            byte[] imgData = new byte[assignmentFile.ContentLength];
            if (assignmentFile.ContentLength > 0)
            {
                using (var reader = new BinaryReader(assignmentFile.InputStream))
                {
                    imgData = reader.ReadBytes(assignmentFile.ContentLength);
                }
            }

            SetupQueue();

            //byte[] bytes = System.IO.File.ReadAllBytes("C:\\Users\\eldrad294\\Desktop\\Tiger.jpg");
            uvm.File = imgData;
            uvm.EndExtension = Extension;
            //uvm.Email = "adrian.vella@outlook.com";
            uvm.Email = Email;
            SendMessage(uvm);

            ViewBag.Title = "Home Page";
            ViewBag.UploadMarker = "true";
            ViewBag.Email = uvm.Email;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            //SetupQueue();
            
            //byte[] bytes = System.IO.File.ReadAllBytes("C:\\Users\\eldrad294\\Desktop\\Tiger.jpg");
            //uvm.File = bytes;
            //uvm.EndExtension = "png";
            ////uvm.Email = "adrian.vella@outlook.com";
            //uvm.Email = "eldrad294@hotmail.com";
            //SendMessage(uvm);
            
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private void SetupQueue()
        {
            // Configure queue settings.
            QueueDescription qd = new QueueDescription("ConvertIO");
            qd.MaxSizeInMegabytes = 5120;
            qd.DefaultMessageTimeToLive = new TimeSpan(0, 1, 0);

            // Create the queue if it does not exist already.
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists("ConvertIO"))
            {
                namespaceManager.CreateQueue(qd);
            }
        }

        private void SendMessage(UploadViewModel message)
        {
            BrokeredMessage brokeredMessage = new BrokeredMessage(message);
            brokeredMessage.Properties["UploadViewModel"] = message.GetType().AssemblyQualifiedName;

            Client.Send(brokeredMessage);
        }


        public JsonResult GetProgress(string email)
        {
            int counter = 0;
            BrokeredMessage brokeredMessage = Client.Peek();
            if (brokeredMessage != null)
            {
                UploadViewModel uvm = brokeredMessage.GetBody<UploadViewModel>();
                if (uvm.Email == email)
                {
                    counter = 1;
                }
                else
                {
                    counter = 2;
                }
            }
            return Json(counter, JsonRequestBehavior.AllowGet);
        }
    }
}