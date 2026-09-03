

using Newtonsoft.Json;
using System.Timers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
 
using System.Collections.Generic;
using System.Net.Http;

using System.Net.Http.Headers;
using System.Text;
using RestSharp;
//using AccountModelsLib.ModelTax;
namespace AccountModelsLib.Models
{
    public class API
    {
        public static bool IsAllMyRestResponse = true;
        private static System.Timers.Timer aTimer = (System.Timers.Timer)null;

        public HttpResponseMessage result = new HttpResponseMessage();
        public RestResponse restResponse;
        public static void GetToken()
        {
            


            //RestClient restClient = new RestClient(StaticData.TokenUrl ?? "");
            
            //restClient.Timeout = -1;
            //client.Timeout = -1;
            var options = new RestClientOptions(StaticData.TokenUrl ?? "")
            {

                Timeout = Timeout.InfiniteTimeSpan
            };
            var client = new RestClient(options);


            var request = new RestRequest();
            request.Method = Method.Post;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", (object)("grant_type=client_credentials&client_id=" + StaticData.ClientId + "&client_secret=" + StaticData.ClientSecret + "&scope=InvoicingAPI"), ParameterType.RequestBody);
            RestResponse restResponse = client.Execute((RestRequest)request);
            if (restResponse.StatusCode != HttpStatusCode.OK)
                return;
            API.aTimer = new System.Timers.Timer(3600000.0);
            API.aTimer.Elapsed += new ElapsedEventHandler(API.UpdateTokenEachOneHour);
            API.aTimer.Start();
            StaticData.Token = JsonConvert.DeserializeObject<AccessTokenResposnse>(restResponse.Content).access_token;
        }
        private static void UpdateTokenEachOneHour(object sender, ElapsedEventArgs e)
        {
            API.aTimer.Dispose();
            API.GetToken();
        }

        public async Task<RestResponse> SearchmyEGSCodeUsageRequests(string Count)
        {
            string url = StaticData.Host + "/codetypes/requests/my?Active=true&Status=Approved&PageSize=100&PageNumber=1&OrderDirections=Descending";
            //RestClient client = new RestClient(url);
            //client.Timeout = -1;
            var options = new RestClientOptions(url)
            {
                Timeout = Timeout.InfiniteTimeSpan // البديل الصحيح لـ 1- القديمة
            };
            var client = new RestClient(options);


            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddHeader("Authorization", "Bearer " + StaticData.Token);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept-Language", "ar");
            RestResponse restResponse = await client.ExecuteAsync((RestRequest)request, new CancellationToken());
            return restResponse;
        }

        public async Task<RestResponse> CreateEGSCodeUsage(List<ItemCodeToSendPortal> ItemCodeTaxToSend)
        {
            string url = StaticData.Host + "/api/v1.0/codetypes/requests/codes";
            RestClient client = new RestClient(url);
            //client.Timeout = -1;
          
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Authorization", "Bearer " + StaticData.Token);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept-Language", "ar");
            request.AddJsonBody((object)ItemCodeTaxToSend);
            RestResponse restResponse2 = await client.ExecuteAsync((RestRequest)request, new CancellationToken());

            restResponse = restResponse2;
            return restResponse;
        }

        

        public async Task CreateEGSCodeUsage_HttpClient(List<ItemCodeToSendPortal> ItemCodeTaxToSend)
        {
            var client2 = new HttpClient();

            client2.BaseAddress = new Uri(StaticData.Host);
            client2.DefaultRequestHeaders.Accept.Clear();
            client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
             

            var content = new StringContent(JsonConvert.SerializeObject(ItemCodeTaxToSend), System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage result2 = await client2.PostAsync("/api/v1.0/codetypes/requests/codes", content);
            result = result2;
            //if (result.StatusCode == System.Net.HttpStatusCode.Created)
            //    return true;

            
            string returnValue = result2.Content.ReadAsStringAsync().Result;
            string xx = result2.StatusCode.ToString();
            //  return returnValue;
            // throw new Exception($"Failed to POST data: ({result.StatusCode}): {returnValue}");



            //if (responsePost.IsSuccessStatusCode)
            //{
            //    // Get the URI of the created resource.
            //    Uri returnUrl = responsePost.Headers.Location;
            //    Console.WriteLine(returnUrl);
            //}
        }

       
         
    }
   
     
}
internal class TokenCls
    {
        public string access_token { get; set; }

        public string expires_in { get; set; }

        public string token_type { get; set; }

        public string scope { get; set; }
    }
 
 
