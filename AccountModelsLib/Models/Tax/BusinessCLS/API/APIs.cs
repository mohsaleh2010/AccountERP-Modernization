using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Threading;


namespace AccountModelsLib.Models
{
    public static class APIs
    {
        public static string token;
        public static string tokenpos;
        public static DateTime TargetDT2;
        public static TimeSpan CountDownFrom2 = TimeSpan.FromMinutes(10d);
        public static DateTime TargetDT1;
        public static TimeSpan CountDownFrom1 = TimeSpan.FromMinutes(60d);

        public static string Caption = "متصل";
        public static int Interval = 0;
        public static int StatusCodeOk = 1;


        public static void GetToken()
        {
            //return;
            string ClientId = Common.CommonTaxConfig.ClientId;
            string ClientSecret = Common.CommonTaxConfig.ClientSecret;
            string url = Common.CommonTaxConfig.TokenUrl;



            //if (Common.CommonTaxConfig.testtype == "تجريبي")
            //if (Common.CommonTaxConfig.PortalType == PortalType.Test)
            //{
            //    url = "https://id.preprod.eta.gov.eg/connect/token";
            //}
            //if (Common.CommonTaxConfig.testtype == "مباشر")
            //if (Common.CommonTaxConfig.PortalType == PortalType.Live)
            //{
            //url = "https://id.eta.gov.eg/connect/token";
            //}
            //var client = new RestClient(url ?? "") { Timeout = -1 };

            var options = new RestClientOptions(url ?? "")
            {
                Timeout = Timeout.InfiniteTimeSpan // البديل الصحيح لـ 1- القديمة
            };
            var client = new RestClient(options);


            var request = new RestRequest();
            request.Method = Method.Post;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", string.Concat(new string[] { "grant_type=client_credentials&client_id=", ClientId, "&client_secret=", ClientSecret, "&scope=InvoicingAPI" }), ParameterType.RequestBody);
            var response1 = client.Execute(request);

            if (response1.StatusCode == HttpStatusCode.OK)
            {

                token = JsonConvert.DeserializeObject<token1>(response1.Content).access_token.ToString();
                Caption = "متصل";
                Interval = JsonConvert.DeserializeObject<token1>(response1.Content).expires_in;
                StatusCodeOk = 1;
                TargetDT1 = DateTime.Now.Add(CountDownFrom1);
              
                //My.MyProject.Forms.MainForm.status1.Caption = "متصل";
                //My.MyProject.Forms.MainForm.Timer1.Interval = JsonConvert.DeserializeObject<token1>(response1.Content).expires_in;
                //My.MyProject.Forms.MainForm.Timer1.Start();
                //My.MyProject.Forms.MainForm.t1.Caption = My.MyProject.Forms.MainForm.Timer1.Interval.ToString();
            }
            else
            {
                Caption = "غير متصل";
                //My.MyProject.Forms.MainForm.status1.Caption = "غير متصل";
                    }
        }






        public class token1
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
        }
        public static byte[] Pdf(string uuid, string Lang = "ar")
        {
            //var client = new RestClient($"{Common.CommonTaxConfig.TokenUrl}/api/v{Common.CommonTaxConfig.documentTypeVersion}/documents/{uuid}/pdf");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.GET);
            // 1. إنشاء خيارات العميل وتحديد الرابط وإلغاء المهلة الزمنية (Infinite)
            var options = new RestClientOptions($"{Common.CommonTaxConfig.TokenUrl}/api/v{Common.CommonTaxConfig.documentTypeVersion}/documents/{uuid}/pdf")
            {
                Timeout = Timeout.InfiniteTimeSpan // البديل الصحيح لـ 1- القديمة
            };
            var client = new RestClient(options);

            // 2. إنشاء الطلب وصياغة نوع الدالة بالطريقة الصحيحة PascalCase
            var request = new RestRequest( );
            request.Method = Method.Get;
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept-Language", Lang);
            var response = client.Execute(request);
            if (response.RawBytes is null)
                return null;
            return response.RawBytes;
        }

       
    }
}