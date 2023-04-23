using ELPS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtilityController : Controller
    {
        // GET: Utility
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadLogs()
        {
            try
            {
                var toReturn = new List<DateTime>();
                string path = Server.MapPath(string.Format("~/App_Data/Log/"));
                var list = Directory.GetDirectories(path).ToList();
                foreach (var file in list)
                {
                    var l = file.Substring(file.LastIndexOf('\\') + 1);
                    var m = l.Substring(l.IndexOf('_') + 1, 2);
                    var d = l.Substring(0, 2);
                    var y = l.Substring(l.LastIndexOf('_') + 1);
                    toReturn.Add(DateTime.Parse(m + "/" + d + "/" + y));
                }

                return View(toReturn);
            }
            catch (UnauthorizedAccessException)
            {
                ViewBag.ErrMessage = "Access Denied";
                return View(); // List<string>();
            }
        }

        public ActionResult Log(string folder)
        {
            var body = "";
            using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Log\\" + folder) + "\\errorLog.txt"))
            {
                body = sr.ReadToEnd();
            }
            var response = new LogModel() { LogBody = body };
            return View(response); //Json(body, JsonRequestBehavior.AllowGet);
        }
    }
}