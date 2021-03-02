using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace YandexAPI_Test_App
{
    class Program
    {

        protected static string connection = "https://cloud-api.yandex.net";
        protected static string pathIn = @"D:\TEST";
        protected static string pathOut = "";
        protected static string oAuth= "AgAAAAAnFAj0AADLW5a62-5E8US-l47IxIg4tKY";


        public class JsonUploadHref
        {
            public string operation_id { get; set; }
            public string href { get; set; }
            public string method { get; set; }
            public bool templated { get; set; }
        }


        class MyHttpClient : HttpClient
        {
            public MyHttpClient()
            {
                this.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue(oAuth);
            }

        }


        protected static string GetUrlUpload { get { return connection + "/v1/disk/resources/upload?path="; } }
        static async Task Main(string[] args)
        {
            try
            {
                var files = Directory.EnumerateFiles(pathIn, "*", SearchOption.AllDirectories);
                var uploadTasks = new List<Task>();
                foreach (string currentFile in files)
                {
                    uploadTasks.Add(uploadFileAsync(currentFile)); 
                }

                while (uploadTasks.Count > 0)
                {
                    Task finishedTask = await Task.WhenAny(uploadTasks);
                    uploadTasks.Remove(finishedTask);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static string uploadFile(string currentFile)
        {
            string fileName = Path.GetFileName(currentFile);
            var uploadHref = GetJsonFromURL(GetUrlUpload + "?" + pathOut + fileName);
            using (Stream stream = new StreamReader(currentFile).BaseStream)
            {
                using (MyHttpClient httpClient = new MyHttpClient())
                {
                    var response = httpClient.PutAsync(uploadHref.href, new StreamContent(stream)).Result;
                }
            }

            return pathOut + fileName;
        }

        static async Task uploadFileAsync(string currentFile)
        {
             Console.WriteLine($"Загружается файл {currentFile}");
             string pathOut = await Task.Run(() => uploadFile(currentFile));
             Console.WriteLine($"Загружен файл {currentFile}");
             return;
        }

        protected static JsonUploadHref GetJsonFromURL(string qery)
        {
            using (MyHttpClient httpClient = new MyHttpClient())
            {
                var response = httpClient.GetAsync(qery + @"&overwrite=?true").Result;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(response.Content.ReadAsStreamAsync().Result, Encoding.Default))
                {
                    return JsonConvert.DeserializeObject<JsonUploadHref>(sr.ReadToEnd());
                }

            }
        }
    }

}