// Copyright © 2010-2021 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp.MinimalExample.OffScreen.Extensions;
using CefSharp.MinimalExample.OffScreen.Models;
using CefSharp.OffScreen;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.OffScreen
{
    public static class Program
    {
        public static int Main(string[] args)
        {
#if ANYCPU
            //Only required for PlatformTarget of AnyCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
#endif

            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

            var carsComSigninUrl = configuration["Urls:carsComSignin"];          
            var DelayForPageLoad = int.Parse( configuration["DelayForPageLoad"]);
            var userName = configuration["Login:User"];
            var password = configuration["Login:Password"];

            ExportResult exportResult = new ExportResult();

            AsyncContext.Run(async delegate
            {
                await InitializeCef();

                using (var browser = new ChromiumWebBrowser(carsComSigninUrl))
                {
                    await WaitForInitialLoadAsync(browser);

                    await browser.Login(userName, password);
                    await Task.Delay(DelayForPageLoad);

                    exportResult.ModelS = await browser.GetCarReport(configuration, "tesla-model_s");
                    exportResult.ModelX = await browser.GetCarReport(configuration, "tesla-model_x");

                    ExportReportToFile(exportResult);

                    Console.WriteLine("Report File saved. Press key for exit !!!");
                }
                Console.ReadKey();
                Cef.Shutdown();
            });

            return 0;
        }

        private static void ExportReportToFile(ExportResult exportResult)
        {
            var JsonSerializeSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var resultJson = JsonConvert.SerializeObject(exportResult, JsonSerializeSettings);

            var resultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "modelReport.json");

            File.WriteAllText(resultPath, resultJson);
        }

        private static async Task WaitForInitialLoadAsync(ChromiumWebBrowser browser)
        {
            var initialLoadResponse = await browser.WaitForInitialLoadAsync();

            if (!initialLoadResponse.Success)
            {
                throw new Exception(string.Format("Page load failed with ErrorCode:{0}, HttpStatusCode:{1}", initialLoadResponse.ErrorCode, initialLoadResponse.HttpStatusCode));
            }
        }

        private static async Task InitializeCef()
        {
            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };

            //Perform dependency check to make sure all relevant resources are in our output directory.
            var success = await Cef.InitializeAsync(settings, performDependencyCheck: true, browserProcessHandler: null);

            if (!success)
            {
                throw new Exception("Unable to initialize CEF, check the log file.");
            }
        }
    }
}
