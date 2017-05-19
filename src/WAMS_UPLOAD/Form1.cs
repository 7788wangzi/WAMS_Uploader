using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace WAMS_UPLOAD
{
    public partial class Form1 : Form
    {
        public delegate void delegateProcess(List<string> folders);
        public delegate void delegateUpdateMessage(string msg);
        public Form1()
        {
            InitializeComponent();
            textBox1.Text = @"E:\CodeLibrary\2018\WAMS_UPLOAD\Video\Test-demo.mp4";
        }

        private void StartProcessing(List<string> folders)
        {
            if (folders != null)
            {
                foreach (var item in folders)
                {
                    StringBuilder assetInfo = new StringBuilder();
                    string path = (string)item;
                    string resultFile = Guid.NewGuid().ToString() + ".csv";

                    FIO.IoHelper ioHelper = new FIO.IoHelper();
                    List<string> mp4Files = ioHelper.FindFiles(path, new List<string>() { ".mp4" });
                    foreach (var videoFile in mp4Files)
                    {
                        assetInfo.AppendLine();
                        assetInfo.Append(Path.GetFileNameWithoutExtension(videoFile));

                        var videoAsset = MediaHelper.CreateAssetAndUploadSingleFile(videoFile, Microsoft.WindowsAzure.MediaServices.Client.AssetCreationOptions.None);
                        var encodedAsset = MediaHelper.EncodeToAdaptiveBitrateMP4s(videoAsset, Microsoft.WindowsAzure.MediaServices.Client.AssetCreationOptions.None);

                        List<string> mp4URLs = null;
                        string playerUrl = MediaHelper.PublishAssetAndGetURLs(encodedAsset, out mp4URLs);

                        if (mp4URLs.Count > 0)
                        {
                            assetInfo.Append("," + encodedAsset.Name);
                            foreach (var mp4Url in mp4URLs)
                            {
                                assetInfo.Append("," + mp4Url);
                            }
                        }
                    }

                    string resultFilepath = Path.Combine(path, resultFile);
                    //File.Create(resultFilepath);
                    using (StreamWriter writer = new StreamWriter(resultFilepath))
                    {
                        writer.WriteLine(assetInfo.ToString());
                        writer.Flush();
                    }

                    string message = string.Format("Complete - {0}",path);
                    UpdateMessage(message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "";

            List<string> inputFolders = new List<string>();
            foreach (var item in listBox1.Items)
            {
                inputFolders.Add((string)item);
            }

            delegateProcess dprocess = new delegateProcess(StartProcessing);
            dprocess.BeginInvoke(inputFolders, new AsyncCallback(ProcessComplete), null);

            UpdateMessage("Start.");
            //List<string> urls = new List<string>();
            //var aseet = MediaHelper.GetAssetByName("Test-demo - H264 MBR 720p");
            //if (aseet != null)
            //{
            //    string url = MediaHelper.PublishAssetGetURLs(aseet, out urls);
            //    StringBuilder videoString = new StringBuilder();
            //    videoString.Append(aseet.Name);
            //    foreach (var videoUrl in urls)
            //    {
            //        videoString.Append("," + videoUrl);
            //    }
            //    MessageBox.Show(videoString.ToString());
            //}
            //else
            //{
            //    MessageBox.Show("No asset");
            //}



            //string singleFile = string.Empty;
            //singleFile = textBox1.Text.Trim();
            //var asset = MediaHelper.CreateAssetAndUploadSingleFile(singleFile, Microsoft.WindowsAzure.MediaServices.Client.AssetCreationOptions.None);

            //MessageBox.Show("upload Complete!");
            //var encodedAsset= MediaHelper.EncodeToAdaptiveBitrateMP4s(asset, Microsoft.WindowsAzure.MediaServices.Client.AssetCreationOptions.None);
            //MessageBox.Show("Encoding Complete!");

            //List<string> urls = new List<string>();
            //string url = MediaHelper.PublishAssetGetURLs(encodedAsset, out urls );
            //MessageBox.Show("Published!");

        }

        private void ProcessComplete(IAsyncResult ar)
        {
            string message = "Done";
            UpdateMessage(message);
            StringBuilder sbLog = new StringBuilder();
            foreach (var item in listBox2.Items)
            {
                sbLog.AppendLine((string)item);
            }

            string timestamp = string.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now);
            string Filename = "Log_"+ timestamp.Replace(@"/", "_").Replace(" ", "_").Replace(":", "_")+".txt";
            using (StreamWriter writer = new StreamWriter(Filename))
            {
                writer.Write(sbLog);
                writer.Flush();
            }
        }

        private void UpdateMessage(string message)
        {
            string formatMessage = string.Format("{0:d/M/yyyy HH:mm:ss} {1}",DateTime.Now, message);
            //delegateUpdateMessage updateMessage = new delegateUpdateMessage((msg) =>
            //{
            //    listBox2.Items.Add(msg);
            //});
            //updateMessage.Invoke(formatMessage);
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox2.Items.Add(formatMessage);
            }));
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            string folder = textBox1.Text.Trim();
            if (Directory.Exists(folder))
            {
                listBox1.Items.Add(folder);
                textBox1.Text = "";
                label1.Text = string.Format("Added: {0}", folder);
            }
            else
            {
                MessageBox.Show("Not a valid folder");
                textBox1.Text = "";
            }
        }
    }
}
