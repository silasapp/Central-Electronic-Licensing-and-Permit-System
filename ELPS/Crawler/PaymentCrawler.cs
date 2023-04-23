using ELPS.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;

namespace ELPS.Crawler
{
    public class PaymentCrawler
    {
        // Runs every 15 mins of the day
        public static void StartPaymentCrawler(string em, string ip)
        {
            //ROMSContext storeDB = new ROMSContext();
            //RunTime cw = new RunTime();
            //cw.ResponseMessage = "Leave Crawler Started";
            //cw.LastRunTime = DateTime.Now;

            //storeDB.RunTimes.Add(cw);
            //storeDB.SaveChanges(em, ip);

            UtilityHelper.LogPaymentTrack($"Payment Track Started: {DateTime.Now}");

            var waitHandle = new AutoResetEvent(false);

            ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                // Method to execute
                (state, timeout) =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        UtilityHelper.LogPaymentTrack($"Payment Track STILL RUNNING: {DateTime.Now}");
                        var path = ConfigurationManager.AppSettings["paymentTrackerUrl"].ToString();
                        var parameters = new System.Collections.Specialized.NameValueCollection();
                        parameters.Add("em", em);
                        parameters.Add("ip", ip);
                        string response = string.Empty;

                        wc.Headers[HttpRequestHeader.Accept] = "application/json";
                        wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                        var jn = "{'em':'" + em + "','ip':'" + ip + "'}";

                        try
                        {
                            var x = wc.UploadString(path, "Post", jn);
                            UtilityHelper.LogPaymentTrack($"Payment Track Response: {x}");
                            //response = Encoding.UTF8.GetString(x);
                        }
                        catch (Exception ex)
                        {
                            //Log error
                            UtilityHelper.LogPaymentTrack($"Error while trying to treat Pending Payment. ({ex.Message})");
                        }
                    }
                },
                null,       //Optional state object to pass to the method
                            //TimeSpan.FromMinutes(3),       //Execute the method after X time set 
                TimeSpan.FromMinutes(15),
                false        //True: Do once; FALSE: Repeat at every X time set above.
            );
        }

        // Runs every hour of the day
        public static void StartLicenseExpiryCrawler(string em, string ip)
        {
            UtilityHelper.LogExpiryTrack($"License Expiry Started: {DateTime.Now}");
            var tx = Convert.ToInt32(ConfigurationManager.AppSettings["LicenseExpiryTimer"]);
            var waitHandle = new AutoResetEvent(false);
            ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                // Method to execute
                (state, timeout) =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        UtilityHelper.LogExpiryTrack($"License Expiry STILL RUNNING: {DateTime.Now}");
                        var path = ConfigurationManager.AppSettings["LicenseExpiryUrl"].ToString();
                        wc.Headers[HttpRequestHeader.Accept] = "application/json";
                        wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                        var jn = "{'e':'" + em + "','i':'" + ip + "'}";

                        try
                        {
                            var x = wc.UploadString(path, "Post", jn);
                            UtilityHelper.LogMessage($"License Expiry Response: {x}");
                        }
                        catch (Exception ex)
                        {
                            //Log error
                            //UtilityHelper.LogMessage($"License Expiry Error:: ({ex.Message})");
                        }
                    }
                },
                null,       //Optional state object to pass to the method
                            //TimeSpan.FromMinutes(3),       //Execute the method after X time set 
                TimeSpan.FromMinutes(tx),
                false        //True: Do once; FALSE: Repeat at every X time set above.
            );
        }

        // Runs every 30 mins (but actually works on the last day of the month)
        public static void StartLicenseExpiryReportCrawler(string em, string ip)
        {
            UtilityHelper.LogExpiryTrack($"License Expiry Report Started: {DateTime.Now}");
            var waitHandle = new AutoResetEvent(false);
            var tx = Convert.ToInt32(ConfigurationManager.AppSettings["LicenseExpiryReporTimer"]);
            ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                // Method to execute
                (state, timeout) =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        UtilityHelper.LogExpiryTrack($"License Expiry Report STILL RUNNING: {DateTime.Now}");
                        var path = ConfigurationManager.AppSettings["LicenseExpiryReportUrl"].ToString();
                        wc.Headers[HttpRequestHeader.Accept] = "application/json";
                        wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                        var jn = $"{{'e':'{em}','i':'{ip}', 'doNow': true}}";
                        //var jn = $"{{'e':'{em}','i':'{ip}'}}";

                        try
                        {
                            var x = wc.UploadString(path, "Post", jn);
                            UtilityHelper.LogMessage($"License Expiry Report Response: {x}");
                        }
                        catch (Exception ex)
                        {
                            //Log error
                            //UtilityHelper.LogMessage($"License Expiry Report Error:: ({ex.Message})");
                        }
                    }
                },
                null,       //Optional state object to pass to the method
                            //TimeSpan.FromMinutes(3),       //Execute the method after X time set 
                TimeSpan.FromMinutes(tx),
                false        //True: Do once; FALSE: Repeat at every X time set above.
            );
        }
    }

}