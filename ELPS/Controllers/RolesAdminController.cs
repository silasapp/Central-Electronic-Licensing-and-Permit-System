using ELPS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;

namespace ELPS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesAdminController : Controller
    {
        IWorkRoleRepository _workRoleRep;
        public RolesAdminController(IWorkRoleRepository _work)
        {
            _workRoleRep = _work;
        }

        public RolesAdminController(ApplicationUserManager userManager,
            ApplicationRoleManager roleManager, IWorkRoleRepository _work)
        {
            UserManager = userManager;
            RoleManager = roleManager;
            _workRoleRep = _work;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        //
        // GET: /Roles/
        public ActionResult Index()
        {
            var roles = RoleManager.Roles;
            var returnRoles = new List<IdentityRole>();

            returnRoles = roles.Where(r => r.Name.ToLower() != "company").ToList();
            return View(returnRoles);
        }

        //
        // GET: /Roles/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            // Get the list of Users in this Role
            //var users = new List<ApplicationUser>();

            var requiredStaff = UserManager.Users.Where(C => C.Roles.Select(y => y.RoleId).Contains(id)).ToList();
            // Get the list of Users in this Role
            //foreach (var user in requiredStaff)
            //{
            //    if (await UserManager.IsInRoleAsync(user.Id, role.Name))
            //    {
            //        users.Add(user);
            //    }
            //}

            ViewBag.Users = requiredStaff;
            ViewBag.UserCount = requiredStaff.Count();
            return View(role);
        }

        //
        // GET: /Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Roles/Create
        [HttpPost]
        public async Task<ActionResult> Create(RoleViewModel roleViewModel)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole(roleViewModel.Name);
                WorkRole workRole = new WorkRole { Name = roleViewModel.Name };
                var roleresult = await RoleManager.CreateAsync(role);
                if (!roleresult.Succeeded)
                {
                    ModelState.AddModelError("", roleresult.Errors.First());
                    return View();
                }
                else
                {
                    _workRoleRep.Add(workRole);
                    _workRoleRep.Save(User.Identity.Name, Request.UserHostAddress);
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // GET: /Roles/Edit/Admin
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            RoleViewModel roleModel = new RoleViewModel { Id = role.Id, Name = role.Name };
            return View(roleModel);
        }

        //
        // POST: /Roles/Edit/5
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Name,Id")] RoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                var role = await RoleManager.FindByIdAsync(roleModel.Id);
                string initName = role.Name;

                role.Name = roleModel.Name;
                var result = await RoleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    var wkrole = _workRoleRep.FindBy(w => w.Name.ToLower() == initName.ToLower()).FirstOrDefault();
                    if (wkrole != null)
                    {
                        _workRoleRep.Delete(wkrole);
                        _workRoleRep.Save(User.Identity.Name, Request.UserHostAddress);

                        TempData["message"] = "Role " + initName + " updated to " + role.Name + " successfully.";
                        TempData["msgType"] = "pass";
                    }
                }
                else
                {
                    TempData["message"] = "An error occured while performing the update operation, please try again.";
                    TempData["msgType"] = "fail";
                }
            }
            return RedirectToAction("Index");
        }

        //
        // GET: /Roles/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        //
        // POST: /Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id, string deleteUser)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var role = await RoleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                if (role.Name.ToLower() != "admin" && role.Name.ToLower() != "staff" && role.Name.ToLower() != "company")
                {
                    IdentityResult result;
                    if (deleteUser != null)
                    {
                        result = await RoleManager.DeleteAsync(role);
                    }
                    else
                    {
                        result = await RoleManager.DeleteAsync(role);
                    }
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        return View();
                    }
                    else
                    {
                        var todel = _workRoleRep.FindBy(w => w.Name.ToLower() == role.Name.ToLower()).FirstOrDefault();
                        if (todel != null)
                        {
                            _workRoleRep.Delete(todel);
                            _workRoleRep.Save(User.Identity.Name, Request.UserHostAddress);
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
