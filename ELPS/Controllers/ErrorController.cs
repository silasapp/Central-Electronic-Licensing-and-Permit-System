using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult CustomError(string ErrorCode,string ErrorMessage)
        {
            ViewBag.ErrorCode = ErrorCode;
            ViewBag.ErrorMessage = ErrorMessage;
            return View();
        }

        public ActionResult BadRequest()
        {
            return View();
        }
        public ActionResult Forbidden()
        {
            return View();
        }
        public ActionResult NotFound()
        {
            return View();
        }

    }
}