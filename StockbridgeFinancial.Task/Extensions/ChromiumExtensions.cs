using CefSharp.MinimalExample.OffScreen.Models;
using CefSharp.OffScreen;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.OffScreen.Extensions
{
    public static class ChromiumExtensions
    {
        public static JsonSerializerSettings JsonSerializeSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        public static async Task<JavascriptResponse> Login(this ChromiumWebBrowser browser, string username, string password)
        {
            var loginScript = $@"document.querySelector('[name=""user[email]""]').value = '{username}';
                                  document.querySelector('[name=""user[password]""]').value = '{password}';
                                  document.querySelector('button[type=submit]').click();";
            JavascriptResponse result = await browser.EvaluateScriptAsync(loginScript);

            return result;
        }

        public static async Task<JavascriptResponse> SearchVehicle(this ChromiumWebBrowser browser,
            string stocktype, string makes, string models, int price, string distance, int zip)
        {
            var setModelSSearch = $@"document.querySelector('#make-model-search-stocktype').value = '{stocktype}';
                                    document.querySelector('#makes').value = '{makes}';
                                    document.querySelector('#models').value = '{models}';
                                    document.querySelector('#make-model-max-price').value = '{price}';
                                    document.querySelector('#make-model-maximum-distance').value = '{distance}';
                                    document.querySelector('#make-model-zip').value = {zip};
                                    document.querySelector('.sds-home-search__submit button[type=submit]').click();";
            
            JavascriptResponse result = await browser.EvaluateScriptAsync(setModelSSearch);

            return result;
        }

        public static async Task<List<Vehicle>> GetVehicleList(this ChromiumWebBrowser browser, string script)
        {
            List<Vehicle> vehicleList = new List<Vehicle>();

            var JsonSerializeSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            JavascriptResponse response = await browser.EvaluateScriptAsync(script);

            dynamic arrayPage = response.Result;

            foreach (dynamic obj in arrayPage)
            {
                if (obj != null)
                {
                    vehicleList.Add(JsonConvert.DeserializeObject<Vehicle>(obj, JsonSerializeSettings));
                }
            }

            return vehicleList;
        }

        public static async Task GoNextPage(this ChromiumWebBrowser browser)
        {
            await browser.EvaluateScriptAsync(Constants.GoNextPage);
        }

        public static async Task FilterHomeDelivery(this ChromiumWebBrowser browser)
        {
            await browser.EvaluateScriptAsync(Constants.FilterHomeDeliveryCheck);
        }

        public static async Task<VehicleDetail> CollectSpecificCarData(this ChromiumWebBrowser browser)
        {
            JavascriptResponse carDetail = await browser.EvaluateScriptAsync(Constants.CarDetailScript);
            string firstCarDetailJson = JsonConvert.SerializeObject(carDetail.Result, JsonSerializeSettings);
            var vehicleDetail = JsonConvert.DeserializeObject<VehicleDetail>(firstCarDetailJson, JsonSerializeSettings);

            return vehicleDetail;
        }

        public static async Task<ModelResult> GetCarReport(this ChromiumWebBrowser browser, IConfiguration configuration, string modelName)
        {
            ModelResult modelResult = new ModelResult();
            var carsComDetailUrl = configuration["Urls:carsComDetail"];
            var carsComUrl = configuration["Urls:carsCom"];
            var DelayForPageLoad = int.Parse(configuration["DelayForPageLoad"]);
            var zipCode = int.Parse(configuration["Adress:ZipCode"]);

            //await browser.SearchVehicle("used", "tesla", "tesla-model_s", 100_000, "all", zipCode);
            await browser.SearchVehicle("used", "tesla", modelName, 100_000, "all", zipCode);
            await Task.Delay(DelayForPageLoad);

            var firstPageList = await browser.GetVehicleList(Constants.VehiclesByBadging);
            await Task.Delay(DelayForPageLoad);

            modelResult.Vehicles.AddRange(firstPageList);

            await browser.GoNextPage();
            await Task.Delay(DelayForPageLoad);

            List<Vehicle> secondPageList = await browser.GetVehicleList(Constants.VehiclesByBadging);
            await Task.Delay(DelayForPageLoad);

            modelResult.Vehicles.AddRange(secondPageList);

            browser.LoadUrl(carsComDetailUrl + modelResult.Vehicles.FirstOrDefault().listing_id);
            await Task.Delay(DelayForPageLoad * 2);

            modelResult.SpecificVehicle = await browser.CollectSpecificCarData();
            await Task.Delay(DelayForPageLoad);

            browser.LoadUrl(carsComUrl);
            await Task.Delay(DelayForPageLoad);

            await browser.SearchVehicle("used", "tesla", "tesla-model_s", 100_000, "all", zipCode);
            await Task.Delay(DelayForPageLoad);

            await browser.FilterHomeDelivery();

            modelResult.VehiclesWithHomeDelivery = await browser.GetVehicleList(Constants.VehiclesByBadging);
            await Task.Delay(DelayForPageLoad);

            return modelResult;
        }
    }
}
