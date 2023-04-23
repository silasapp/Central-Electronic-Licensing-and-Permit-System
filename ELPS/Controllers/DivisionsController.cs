using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ELPS.Domain.Entities;
using ELPS.Domain.Helper;
using ELPS.Domain.Abstract;

namespace ELPS.Controllers
{
    public class DivisionsController : Controller
    {
        private readonly IDivisionRepo _divisionsRep;

        public DivisionsController(IDivisionRepo divisionsRep)
        {
            _divisionsRep = divisionsRep;
        }

        // GET: PortalCategories
        public ActionResult Index()
        {
            return View(_divisionsRep.GetAll());
        }

        //[HttpPost]
        //public ActionResult CategoryList()
        //{
        //    var parser = new Parser<Division>(Request.Form, db.Divisions);
        //    return Json(parser.Parse());

        //}

        // GET: PortalCategories/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Division portalCategory = await db.Divisions.FindAsync(id);
        //    if (portalCategory == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(portalCategory);
        //}

        // GET: PortalCategories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PortalCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,SortOrder")] Division division)
        {

            if (ModelState.IsValid)
            {
                if (_divisionsRep.Exist(division.Name))
                {
                    ModelState.AddModelError("Name", "Division already Exist");
                    return View(division);
                }
                _divisionsRep.Add(division);
                _divisionsRep.Save();
                return RedirectToAction("Index");
            }

            return View(division);
        }

        // GET: PortalCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var portalCategory = _divisionsRep.FindBy(m => m.Id == id.Value).FirstOrDefault();
            if (portalCategory == null)
            {
                return HttpNotFound();
            }
            return View(portalCategory);
        }

        // POST: PortalCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,SortOrder")] Division division)
        {

            if (ModelState.IsValid)
            {
                if (_divisionsRep.Exist(division.Id,division.Name))
                {
                    ModelState.AddModelError("Name", "Division already Exist");
                    return View(division);
                }
                var _division=_divisionsRep.FindBy(m => m.Id == division.Id).FirstOrDefault();
                if (_division == null)
                {
                    return HttpNotFound();
                }
                _division.Name = division.Name;
                _division.SortOrder = division.SortOrder;
                _divisionsRep.Edit(_division);
                _divisionsRep.Save();
                return RedirectToAction("Index");
            }
            return View(division);
        }

        // GET: PortalCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var portalCategory = _divisionsRep.FindBy(m => m.Id == id.Value).FirstOrDefault();
            if (portalCategory == null)
            {
                return HttpNotFound();
            }
            return View(portalCategory);
        }

         // POST: PortalCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Division portalCategory = _divisionsRep.FindBy(m => m.Id == id).FirstOrDefault();
            if (portalCategory == null)
                return HttpNotFound();

            _divisionsRep.Delete(portalCategory);
            _divisionsRep.Save();
            
            return RedirectToAction("Index");
        }


    }
}
