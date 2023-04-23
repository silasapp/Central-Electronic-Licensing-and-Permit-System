using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ELPS.Helpers
{
    public class UtilityHelper
    {
        public static DateTime CurrentTime
        {
            get
            {
                DateTime timeToReturn = DateTime.UtcNow.AddHours(1);
                return timeToReturn;
            }
        }

        public static string TimePass(DateTime date)
        {
            var diff = CurrentTime - date;
            string response = string.Empty;

            if (diff.Days >= 7)
            {
                var weeks = (int)(diff.Days / 7);

                if (weeks > 1)
                {
                    response = weeks + " weeks ago";
                }
                else if (weeks == 1)
                {
                    response = weeks + " week ago";
                }
            }
            else if (diff.Days >= 1)
            {
                if (diff.Days > 1)
                {
                    response = diff.Days + " days ago";
                }
                else if (diff.Days == 1)
                {
                    response = "Yesterday";
                }
            }
            else if (diff.Hours >= 1)
            {
                if (diff.Hours > 1)
                {
                    response = diff.Hours + " Hours ago";
                }
                else if (diff.Hours == 1)
                {
                    response = diff.Hours + " Hour ago";
                }
            }
            else
            {
                response = diff.Minutes + " Minutes ago";
            }

            return response;
        }

        public static string GetMonthName(int month, bool shortMonth = false)
        {
            switch (month)
            {
                case 1:
                    return shortMonth ? "Jan" : "January";
                case 2:
                    return shortMonth ? "Feb" : "February";
                case 3:
                    return shortMonth ? "Mar" : "March";
                case 4:
                    return shortMonth ? "Apr" : "April";
                case 5:
                    return shortMonth ? "May" : "May";
                case 6:
                    return shortMonth ? "Jun" : "June";
                case 7:
                    return shortMonth ? "Jul" : "July";
                case 8:
                    return shortMonth ? "Aug" : "August";
                case 9:
                    return shortMonth ? "Sep" : "September";
                case 10:
                    return shortMonth ? "Oct" : "October";
                case 11:
                    return shortMonth ? "Nov" : "November";
                default:
                    return shortMonth ? "Dec" : "December";
            }
        }

        public static string GenerateReceiptNo(double Amount, long Id,string LicenseReceiptCode, DateTime datepaid)
        {
            //string S = LicenseReceiptCode;
            //string BK = "01";
            //if (Amount < 250000)
            //{
            //    BK = "02";
            //}
            //string MM = CurrentTime.Month.ToString("00");
            //string YY = CurrentTime.Year.ToString().Substring(2, 2);
            //string XXXX = Id.ToString("000000");
            //return string.Format("{0}{1}{2}{3}{4}", S, BK, MM, YY, "0" + XXXX);

            string S = LicenseReceiptCode;
            string BK = "01";
            //if (Amount < 250000)
            //{
            //    BK = "02";
            //}
            string MM = datepaid.Month.ToString("00");
            string YY = datepaid.Year.ToString().Substring(2, 2);
            string XXXX = Id.ToString("000000");
            return string.Format("{0}{1}{2}{3}{4}", S, BK, MM, YY, "0" + XXXX);
        }

        public static void LogMessage(string message)
        {
            try
            {
                var mth = DateTime.Now.Day.ToString("00") + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Year;

                string location = HttpContext.Current.Server.MapPath("\\App_Data\\Log\\" + mth);
                string filepath = Path.Combine(location, "errorLog.txt");
                if (!Directory.Exists(location))
                    Directory.CreateDirectory(location);

                FileStream fs;
                if (!System.IO.File.Exists(filepath))
                {
                    fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
                }
                else
                {
                    fs = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                }
                StreamWriter writer = new StreamWriter(fs);

                writer.WriteLine("Date: " + DateTime.Now);
                writer.WriteLine("ErrMessage: " + message);
                writer.WriteLine("---------------------------------------");
                writer.Close();
                fs.Close();
                fs.Dispose();
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        /// <summary>
        /// Logs Payment Crawlers report
        /// </summary>
        /// <param name="message">Build up message</param>
        public static void LogPaymentTrack(string message)
        {
            try
            {
                var mth = DateTime.Now.Day.ToString("00") + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Year;

                string location = System.Web.HttpContext.Current.Server.MapPath("\\App_Data\\Log\\" + mth);
                string filepath = Path.Combine(location, "PaymentTrack.txt");
                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }
                FileStream fs;
                if (!System.IO.File.Exists(filepath))
                {
                    fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
                }
                else
                {
                    fs = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                }
                StreamWriter writer = new StreamWriter(fs);

                writer.WriteLine("Date: " + DateTime.Now);
                writer.WriteLine("Message: " + message);
                writer.WriteLine("---------------------------------------");
                writer.Close();
                fs.Close();
                fs.Dispose();
            }
            catch (Exception)
            {
                // do nothing
            }
            
        }

        /// <summary>
        /// Logs Expiry Report
        /// </summary>
        /// <param name="message">Build up message</param>
        public static void LogExpiryTrack(string message)
        {
            try
            {
                var mth = DateTime.Now.Day.ToString("00") + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Year;

                string location = HttpContext.Current.Server.MapPath("\\App_Data\\Log\\" + mth);
                string filepath = Path.Combine(location, "ExpiryReport.txt");
                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }
                FileStream fs;
                if (!System.IO.File.Exists(filepath))
                {
                    fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
                }
                else
                {
                    fs = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                }
                StreamWriter writer = new StreamWriter(fs);

                writer.WriteLine("Date: " + DateTime.Now);
                writer.WriteLine("Message: " + message);
                writer.WriteLine("---------------------------------------");
                writer.Close();
                fs.Close();
                fs.Dispose();
            }
            catch (Exception)
            {
                // do nothing
            }

        }

        public static bool IsDPRStaff(string email)
        {
            var staff = false;
            if(email.EndsWith("@dpr.gov.ng") || email.EndsWith("@nuprc.gov.ng")){
                staff = true;
            }
            return staff;
        }

        public static double GetAmount(string what, string appItems, double ServiceCharge)
        {
            //UtilityHelper.LogMessage("Logging Payment for :::> " + appItems);
            if (string.IsNullOrEmpty(appItems))
            {
                if (what.ToLower() == "sc")
                {
                    return ServiceCharge;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                double sc = 0, oth = 0;
                List<AppItemModel> aItems = JsonConvert.DeserializeObject<List<AppItemModel>>(appItems);
                if(aItems != null && aItems.Any())
                {
                    var payment = aItems.Where(a => a.Group.ToLower() == "payment").FirstOrDefault();
                    if(payment != null)
                    {
                        if (!string.IsNullOrEmpty(payment.Description))
                        {
                            var descriptions = payment.Description.Split(';');
                            foreach (var _desc in descriptions)
                            {
                                if (!string.IsNullOrEmpty(_desc))
                                {
                                    var _line = _desc.Split('=');
                                    if (_line.Count() > 1)
                                    {
                                        if (!string.IsNullOrEmpty(_line[0]) && _line[0].Trim().ToLower().IndexOf("service charge") >= 0)
                                        {
                                            // Service charge
                                            if (!string.IsNullOrEmpty(_line[1]))
                                            {
                                                var _sc = Convert.ToDouble(_line[1].Trim());
                                                if (_sc >= 100)
                                                {
                                                    sc += _sc;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Other Fee
                                            if (!string.IsNullOrEmpty(_line[0]) && (_line[0].Trim().ToLower().IndexOf("no. of times not renewed") >= 0 ||
                                                _line[0].Trim().ToLower().IndexOf("unpaid arrears") >= 0))
                                            {
                                                // Do nothing
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(_line[1]))
                                                {
                                                    var _oth = Convert.ToDouble(_line[1].Trim());
                                                    if (_oth >= 100)
                                                    {
                                                        oth += _oth;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Likely an explainer line, do nothing
                                    }
                                }
                            }
                        }
                    }
                }
                //foreach (var item in aItems)
                //{
                //    if (item != null)
                //    {
                //        if (!string.IsNullOrEmpty(item.Group) && item.Group.ToLower() == "payment")
                //        {
                //            if (!string.IsNullOrEmpty(item.Description))
                //            {
                //                var descriptions = item.Description.Split(';');
                //                foreach (var _desc in descriptions)
                //                {
                //                    if (!string.IsNullOrEmpty(_desc))
                //                    {
                //                        var _line = _desc.Split('=');
                //                        if (!string.IsNullOrEmpty(_line[0]) && _line[0].Trim().ToLower().IndexOf("service charge") >= 0)
                //                        {
                //                            // Service charge
                //                            if (!string.IsNullOrEmpty(_line[1]))
                //                            {
                //                                var _sc = Convert.ToDouble(_line[1].Trim());
                //                                if (_sc >= 100)
                //                                {
                //                                    sc += _sc;
                //                                }
                //                            }
                //                        }
                //                        else
                //                        {
                //                            // Other Fee
                //                            if (!string.IsNullOrEmpty(_line[0]) && _line[0].Trim().ToLower().IndexOf("no. of times not renewed") >= 0)
                //                            {
                //                                // Do nothing
                //                            }
                //                            else
                //                            {
                //                                if (!string.IsNullOrEmpty(_line[1]))
                //                                {
                //                                    var _oth = Convert.ToDouble(_line[1].Trim());
                //                                    if (_oth >= 100)
                //                                    {
                //                                        oth += _oth;
                //                                    }
                //                                }
                //                            }
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                if (sc == 0)
                {
                    sc = ServiceCharge;
                }

                if (what.ToLower() == "sc")
                {
                    return sc;
                }
                else
                {
                    return oth;
                }
            }

        }
    }


    public class AppItemModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
    }
}