using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video; 
using AForge.Video.DirectShow;
using System.Windows;
using Newtonsoft.Json;
using Twilio;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;
using System.Threading;
using AForge;
using AForge.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Analytics;
using System.IO;
using System.Configuration;


namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public static MobileServiceClient MobileService = new MobileServiceClient("https://facemobile.azure-mobile.net/", "xzMhfBEvqoGAJiUGUqhGhsigjgiwoS48");
        public IMobileServiceTable<facetable> faceTable = MobileService.GetTable<facetable>();
        public Form1()
        {
            InitializeComponent();
        }
        private FilterInfoCollection webcam;
        private VideoCaptureDevice cam;
        private Bitmap bit;
        private int i;

        private void Form1_Load(object sender, EventArgs e)
        {
            //
            webcam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in webcam)
            {
                comboBox1.Items.Add(VideoCaptureDevice.Name);
            }
            comboBox1.SelectedIndex = 0;

            //cam start
            cam = new VideoCaptureDevice(webcam[comboBox1.SelectedIndex].MonikerString);
            cam.NewFrame += new NewFrameEventHandler(cam_NewFrame);
            cam.Start();
        }
  
        void cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bit = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bit;

        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string AccountSid = "ACd489d0930dc658a3384b1b52a28cbced";
            string AuthToken = "b4f632beb8bbf85f696693d0df69dba3";
            FaceServiceClient faceClient = new FaceServiceClient("0e58dbc56e5445ac8fcdfa9ffbf5ef60");
            if (!cam.IsRunning)
            {
                System.Threading.Thread.Sleep(1000);
            }
            bit.Save(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg");
            Thread.Sleep(1000);
            StorageCredentials storageCredentials = new StorageCredentials("faceimage", "DYrgou0cTTp6J7KDdMVVxR3BDtM31zh393oyf0CfWdTuihRUgDwyryQuIqj203SnPHMJVK7VvLGm/KtfIpUncw==");

            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("facecontainer");
            container.CreateIfNotExistsAsync();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("pic.jpg");

            using (var fileStream = System.IO.File.OpenRead(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg"))
            {
                blockBlob.UploadFromStream(fileStream);
            }
            using (var fileStream = System.IO.File.OpenRead(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg"))
            {
                blockBlob.UploadFromStream(fileStream);
            }
            double[] ages = await UploadAndDetectFaceAges(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg", faceClient);
            string[] genders = await UploadAndDetectFaceGender(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg", faceClient);
            Guid[] ids = await UploadAndDetectFaceId(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg", faceClient);

            InsertData(ids[0].ToString(), genders[0], ages[0].ToString(), textBox1.Text);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        public async Task<double[]> UploadAndDetectFaceAges(string imageFilePath, FaceServiceClient faceServiceClient)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream, false, true, true, false);

                    var faceAge = faces.Select(face => face.Attributes.Age);

                    return faceAge.ToArray();
                }
            }
            catch (Exception e)
            {
                return new double[1];
            }
        }

        public async Task<string[]> UploadAndDetectFaceGender(string imageFilePath, FaceServiceClient faceServiceClient)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream, false, true, true, false);

                    var faceGender = faces.Select(face => face.Attributes.Gender);

                    return faceGender.ToArray();
                }
            }
            catch (Exception)
            {
                return new string[1];
            }
        }

        public async void InsertData(string idface, string gender, string age, string phonenumber)
        {
            var newFace = new facetable
            {
                IdFace = idface,
                Gender = gender,
                Age = age,
                PhoneNumber = phonenumber,
                Active = "Active"
            };

            await faceTable.InsertAsync(newFace);
        }

        public async Task<string> GetGenderFace(string idface)
        {
            var face = await faceTable.LookupAsync(idface);
            return face.Gender;
        }
        public async Task<string> GetActiveFace(string idface)
        {
            var face = await faceTable.LookupAsync(idface);
            return face.Active;
        }

        public async Task<string> GetAgeFace(string idface)
        {
            var face = await faceTable.LookupAsync(idface);
            return face.Age;
        }
        public async Task<string> GetPhoneNumberFace(string idface)
        {
            var face = await faceTable.LookupAsync(idface);
            return face.PhoneNumber;
        }

        public async Task<string> GetIdFace(string idface)
        {
            var face = await faceTable.LookupAsync(idface);
            return face.IdFace;
        }
        public async Task<List<facetable>> FetchAllFaces()
        {
            var allCustomers = new List<facetable>();

            var list = await faceTable.Take(50).Where(facetable => facetable.Active == "Active").ToListAsync();
            foreach (var customer in list)
            {
                allCustomers.Add(customer);
            }
           

            return allCustomers;
        }

        public async Task<Guid[]> UploadAndDetectFaceId(string imageFilePath, FaceServiceClient faceServiceClient)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream, false, true, true, false);

                    var faceId = faces.Select(face => face.FaceId);

                    return faceId.ToArray();
                }
            }
            catch (Exception)
            {
                return new Guid[1];
            }
        }
        
        async private void button2_Click(object sender, EventArgs e)
        {
            string AccountSid = "ACd489d0930dc658a3384b1b52a28cbced";
            string AuthToken = "b4f632beb8bbf85f696693d0df69dba3";
            FaceServiceClient faceClient = new FaceServiceClient("0e58dbc56e5445ac8fcdfa9ffbf5ef60");
            double[] ages = await UploadAndDetectFaceAges(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg", faceClient);
            string[] genders = await UploadAndDetectFaceGender(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg", faceClient);

            while(true)
            {
                VerifyResult verification = new VerifyResult();
                verification.Confidence = 0;
                int numBoys = 0;
                int numgirls = 0;
                int ppl = 0;
                int avgAge = 0;
                int totAge = 0;
                if (!cam.IsRunning)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                bit.Save(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg");
                Thread.Sleep(1000);

                double[] ages1 = await UploadAndDetectFaceAges(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg", faceClient);
                string[] genders2 = await UploadAndDetectFaceGender(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg", faceClient);

                StorageCredentials storageCredentials = new StorageCredentials("faceimage", "DYrgou0cTTp6J7KDdMVVxR3BDtM31zh393oyf0CfWdTuihRUgDwyryQuIqj203SnPHMJVK7VvLGm/KtfIpUncw==");

                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference("facecontainer");
                container.CreateIfNotExistsAsync();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference("pic.jpg");

                using (var fileStream = System.IO.File.OpenRead(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg"))
                {
                    blockBlob.UploadFromStream(fileStream);
                }
                Guid[] ids = await UploadAndDetectFaceId(@"C:\Users\ma_eg_000\Desktop\PrincetonHack\pic.jpg", faceClient);

                InsertData(ids[0].ToString(), genders2[0], ages1[0].ToString(), "5149941737");
                
                List<facetable> ftable = await FetchAllFaces();
                string toCall = "null";
                foreach(facetable fTable in ftable)
                {
                    ppl++;
                    if(fTable.Gender == "male")
                    {
                        numBoys++;
                    }
                    else
                    {
                        numgirls++;
                    }
                    totAge = totAge + Int32.Parse(fTable.Age);
                    Guid id2 = new Guid(fTable.IdFace);
                    VerifyResult temp = await faceClient.VerifyAsync(ids[0],id2 );
                    if (temp.Confidence >= verification.Confidence)
                    {
                        verification = temp;
                        toCall = fTable.PhoneNumber;
                    }
                }
                avgAge = totAge / ppl;
                if(verification.Confidence>= 0.40)
                {
                    richTextBox1.Text = "Number of Males Customers : "+numBoys+ " Number of Female Customers :"+numgirls+ " Average age of Customers :"+avgAge;
                    var twilio = new TwilioRestClient(AccountSid, AuthToken);
                    var message = twilio.SendMessage("16263449948", toCall, "WE HAVE THE BEST DEALS FOR YOU TODAY!!! Free Selfie Sticks and T-Shirts Gallore", "");
                }
                Thread.Sleep(1000);
                
            }
        }
    }

    public class facetable
    {
        public string id { get; set; }
        public string IdFace { get; set; }

        public string Gender { get; set; }

        public string Age { get; set; }

        public string PhoneNumber { get; set; }

        public string Active { get; set; }
    }
}
