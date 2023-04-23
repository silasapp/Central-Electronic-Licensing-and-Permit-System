using ELPS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ELPS.Domain.Entities;
using ELPS.Domain.Abstract;
using ELPS.Helpers;

namespace ELPS.Controllers
{
    [Authorize]
    public class UsersAdminController : Controller
    {
        IStaffRepository _staffRep;

        public UsersAdminController(IStaffRepository staffRep)
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
            _staffRep = staffRep;
        }

        public UsersAdminController(UserManager<ApplicationUser> userManager)
        {
            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("One");
            userManager.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<ApplicationUser>(provider.Create("EmailConfirmation"));
            UserManager = userManager;

            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };
            //this.UserManager = userManager;
        }


        private ApplicationRoleManager _roleManager;
        public UserManager<ApplicationUser> UserManager { get; private set; }
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
        // GET: /Users/
        [Authorize(Roles = "Admin, Assignment, ITAdmin")]
        public ActionResult Index()
        {
            var Usrs = UserManager.Users.Where(C => C.Roles.Select(y => y.RoleId).Contains("45ea949b-c11f-4de0-9042-fb2e8d12d89d")).ToList();


            return View(Usrs);

        }

        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        //
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.Where(a => a.Id != "45ea949b-c11f-4de0-9042-fb2e8d12d89d").ToListAsync(), "Name", "Name");
            return View();
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterStaffViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                if (!UtilityHelper.IsDPRStaff(userViewModel.Email))
                {

                    var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email, EmailConfirmed = true, PhoneNumber = userViewModel.PhoneNo, PhoneNumberConfirmed = true };
                    var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                    //Add User to the selected Roles 
                    if (adminresult.Succeeded)
                    {
                        if (selectedRoles != null)
                        {
                            var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("", result.Errors.First());
                                ViewBag.RoleId = new SelectList(await RoleManager.Roles.Where(a => a.Id != "45ea949b-c11f-4de0-9042-fb2e8d12d89d").ToListAsync(), "Name", "Name");
                                return View();
                            }
                            else
                            {
                                Staff staff = new Staff();
                                staff.Email = userViewModel.Email;
                                staff.FirstName = userViewModel.FirstName;
                                staff.LastName = userViewModel.LastName;
                                staff.UserId = user.Id;
                                _staffRep.Add(staff);
                                _staffRep.Save(User.Identity.Name, Request.UserHostAddress);
                            }
                        }
                        else
                        {
                            Staff staff = new Staff();
                            staff.Email = userViewModel.Email;
                            staff.FirstName = userViewModel.FirstName;
                            staff.LastName = userViewModel.LastName;
                            staff.UserId = user.Id;
                            _staffRep.Add(staff);
                            _staffRep.Save(User.Identity.Name, Request.UserHostAddress);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", adminresult.Errors.First());
                        ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                        return View();

                    }
                }
                else
                {
                    ModelState.AddModelError("", "Create NUPRC Staff using active directory");
                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            return View();
        }

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var staff = _staffRep.FindBy(a => a.UserId.ToLower() == user.Id.ToLower()).FirstOrDefault();
            var userRoles = await UserManager.GetRolesAsync(user.Id);

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = staff != null ? staff.FirstName : "",
                LastName = staff != null ? staff.LastName : "",
                PhoneNo = staff != null ? staff.PhoneNo : "",

                RolesList = RoleManager.Roles.Where(a => a.Id != "45ea949b-c11f-4de0-9042-fb2e8d12d89d").ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel editUser, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.UserName = editUser.Email;
                user.Email = editUser.Email;

                var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }

                var staff = _staffRep.FindBy(a => a.UserId.ToLower() == user.Id.ToLower()).FirstOrDefault();
                if (staff == null)
                {
                    staff = new Staff();
                    staff.Email = editUser.Email;
                    staff.FirstName = editUser.FirstName;
                    staff.LastName = editUser.LastName;
                    staff.UserId = user.Id;
                    staff.PhoneNo = editUser.PhoneNo;
                    _staffRep.Add(staff);
                }
                else
                {
                    staff.Email = editUser.Email;
                    staff.FirstName = editUser.FirstName;
                    staff.LastName = editUser.LastName;
                    staff.PhoneNo = editUser.PhoneNo;
                    _staffRep.Edit(staff);
                }
                //_staffRep.Save(User.Identity.Name, Request.UserHostAddress);
                _staffRep.Save();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }

        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        #region
        //public ActionResult Index()
        //{
        //    if (User.IsInRole("Admin") || User.Identity.Name.ToLower() == "itoro.g@brandonemaxfront.com".ToLower())
        //    {
        //        List<vUserBranch> returnUser = new List<vUserBranch>(); // _vUserBranch.FindBy(a => a.UserId == user.Id).ToList();

        //        //RoleName = Staff
        //        var usr = UserManager.Users.Where(a => a.Roles.Select(b => b.RoleId).Contains("")).ToList();
        //        var Users = UserManager.Users.Where(C => C.Roles.Select(y => y.RoleId).Contains("45ea949b-c11f-4de0-9042-fb2e8d12d89d")).ToList();

        //        var userBranches = _vUserBranch.GetAll().ToList();
        //        foreach (var user in Users)
        //        {
        //            var staffHolder = _staffRep.FindBy(stf => stf.UserId.ToLower() == user.Id.ToLower()).FirstOrDefault(); //In STaff

        //            var staffs = userBranches.Where(a => a.UserId == user.Id).ToList(); //Staff has Branch
        //            foreach (var staff in staffs)
        //            {
        //                if (staff != null && staffHolder != null)
        //                {
        //                    staff.FirstName = staffHolder.FirstName;
        //                    staff.LastName = staffHolder.LastName;
        //                    returnUser.Add(staff);
        //                }
        //            }


        //            //var roles = UserManager.GetRoles(user.Id);
        //            //if (roles.Contains("Staff")) { Staffs.Add(user); }
        //        }

        //        if (TempData["message"] != null)
        //        {
        //            ViewBag.Msg = TempData["message"].ToString();
        //            ViewBag.Type = TempData["msgType"].ToString();
        //        }

        //        return View(returnUser);
        //    }
        //    else
        //    {
        //        return View("Error");
        //    }
        //}

        //[Authorize(Roles = "Admin, Manager, Approver, ManagerPlus")]
        //public ActionResult StaffDesk()
        //{
        //    var me = User.Identity.Name;
        //    var userId = User.Identity.GetUserId();
        //    var myBranch = _vUserBranch.FindBy(u => u.UserId.ToLower() == userId.ToLower()).ToList();

        //    if (TempData["message"] != null)
        //    {
        //        ViewBag.Msg = TempData["message"].ToString();
        //        ViewBag.Type = TempData["msgType"].ToString();
        //    }
        //    var roles = UserManager.GetRoles(userId);

        //    if (myBranch.Any())
        //    {
        //        List<vUserBranch> returnUser = new List<vUserBranch>();
        //        var staffs = UserManager.Users.Where(C => C.Roles.Select(y => y.RoleId).Contains("45ea949b-c11f-4de0-9042-fb2e8d12d89d")).ToList();

        //        foreach (var branch in myBranch)
        //        {
        //            if (roles.Contains("ManagerPlus"))
        //            {
        //                List<vUserBranch> myDeptStaff = new List<vUserBranch>();
        //                myDeptStaff = _vUserBranch.FindBy(u => u.DepartmentId == branch.DepartmentId).ToList();

        //                foreach (var user in staffs)
        //                {
        //                    var staffHolder = _staffRep.FindBy(stf => stf.UserId.ToLower() == user.Id.ToLower()).FirstOrDefault(); //In STaff

        //                    var staff = myDeptStaff.Where(a => a.UserId == user.Id).FirstOrDefault(); //Staff has Branch
        //                    if (staff != null && staffHolder != null && staff.Email.ToLower() != me.ToLower())
        //                    {
        //                        staff.FirstName = staffHolder.FirstName;
        //                        staff.LastName = staffHolder.LastName;
        //                        returnUser.Add(staff);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //ViewBag.DepartmentName = myBranch.DepartmentName;
        //                List<vUserBranch> myDeptStaff = new List<vUserBranch>();
        //                if (roles.Contains("Approver"))
        //                {
        //                    var DepartmentId = myBranch.FirstOrDefault().DepartmentId;
        //                    myDeptStaff = _vUserBranch.FindBy(u => u.DepartmentId == DepartmentId).ToList();
        //                }
        //                else
        //                {
        //                    myDeptStaff = _vUserBranch.FindBy(u => u.DepartmentId == branch.DepartmentId && u.BranchId == branch.BranchId).ToList();
        //                }

        //                foreach (var user in staffs)
        //                {
        //                    var staffHolder = _staffRep.FindBy(stf => stf.UserId.ToLower() == user.Id.ToLower()).FirstOrDefault(); //In STaff

        //                    var staff = myDeptStaff.Where(a => a.UserId == user.Id).FirstOrDefault(); //Staff has Branch
        //                    if (staff != null && staffHolder != null && staff.Email.ToLower() != me.ToLower())
        //                    {
        //                        staff.FirstName = staffHolder.FirstName;
        //                        staff.LastName = staffHolder.LastName;
        //                        returnUser.Add(staff);
        //                    }
        //                }
        //            }
        //        }
        //        return View(returnUser.OrderBy(s => s.DepartmentName).ToList());
        //    }
        //    return View();
        //}

        ////
        //// GET: /Users/Details/5
        //public async Task<ActionResult> Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var user = await UserManager.FindByIdAsync(id);
        //    var staff = _staffRep.FindBy(stf => stf.UserId.ToLower() == id.ToLower()).FirstOrDefault();
        //    ViewBag.UserName = staff.FirstName + " " + staff.LastName + " (" + user.UserName + ")";

        //    List<vUserBranch> returnUser = _vUserBranch.FindBy(a => a.UserId == user.Id).ToList();
        //    //ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

        //    return View(returnUser);
        //}




        //////
        ////// GET: /Users/Create
        ////[Authorize(Roles = "Admin, Assignment")]
        ////public async Task<ActionResult> Create(string id)
        ////{
        ////    //Get the list of Roles
        ////    ViewBag.RoleId = new SelectList(await RoleManager.Roles.Where(a => a.Name != "Staff").ToListAsync(), "Name", "Name");
        ////    List<WorkRole> allRoles = _workRoleRep.GetAll().ToList();
        ////    //List<Department> departments = _departmentRep.GetAll().ToList();
        ////    List<Branch> branches = _branchRep.GetAll().ToList();
        ////    //UserBranchModel usbm = new UserBranchModel();

        ////    if (!string.IsNullOrEmpty(id))
        ////    {
        ////        Staff staff = _staffRep.FindBy(st => st.UserId.ToLower() == id.ToLower()).FirstOrDefault();
        ////        UserBranch ub = _userBranchRep.FindBy(u => u.UserId.ToLower() == id.ToLower()).FirstOrDefault();
        ////        //usbm.FirstName = staff.FirstName;
        ////        //usbm.LastName = staff.LastName;
        ////        //usbm.UserBranch = ub;
        ////        //usbm.User = new RegisterViewModel();
        ////        //usbm.User.Email = UserManager.FindById(id).Email;
        ////        ViewBag.Operation = "Edit";
        ////        ViewBag.Roles = new SelectList(allRoles, "Id", "Name", ub.RoleId);
        ////        ViewBag.Department = new SelectList(departments, "Id", "Name", ub.DepartmentId);
        ////        ViewBag.Branches = new SelectList(branches, "Id", "Name", ub.BranchId);
        ////    }
        ////    else
        ////    {
        ////        ViewBag.Roles = new SelectList(allRoles, "Id", "Name");
        ////        ViewBag.Department = new SelectList(departments, "Id", "Name");
        ////        ViewBag.Branches = new SelectList(branches, "Id", "Name");
        ////    }


        ////    return View(usbm);
        ////}

        //////
        ////// POST: /Users/Create
        ////[Authorize(Roles = "Admin, Assignment")]
        ////[HttpPost]
        ////public async Task<ActionResult> Create(UserBranchModel userbranchModel, params string[] selectedRoles)
        ////{
        ////    var user = new ApplicationUser { UserName = userbranchModel.User.Email, Email = userbranchModel.User.Email, EmailConfirmed = true };

        ////    List<WorkRole> allRoles = _workRoleRep.GetAll().ToList();
        ////    List<Department> departments = _departmentRep.GetAll().ToList();
        ////    List<Branch> branches = _branchRep.GetAll().ToList();

        ////    WorkRole selectedWorkRole = allRoles.Where(C => C.Id == userbranchModel.UserBranch.RoleId).FirstOrDefault();

        ////    ViewBag.Roles = new SelectList(allRoles, "Name", "Name");
        ////    ViewBag.Department = new SelectList(departments, "Id", "Name");
        ////    ViewBag.Branches = new SelectList(branches, "Id", "Name");

        ////    if (string.IsNullOrEmpty(userbranchModel.UserBranch.UserId))
        ////    {
        ////        var adminresult = await UserManager.CreateAsync(user, userbranchModel.User.Password);
        ////        //New User
        ////        if (adminresult.Succeeded)
        ////        {
        ////            var x = await UserManager.AddToRoleAsync(user.Id, "Staff");     // Adds user to staff
        ////            if (selectedWorkRole != null)
        ////                await UserManager.AddToRoleAsync(user.Id, selectedWorkRole.Name);

        ////            UserBranch usb = userbranchModel.UserBranch;
        ////            usb.UserId = user.Id.ToString();
        ////            _userBranchRep.Add(usb);
        ////            _userBranchRep.Save(User.Identity.Name, Request.UserHostAddress);

        ////            Staff staff = new Staff();
        ////            staff.FirstName = userbranchModel.FirstName;
        ////            staff.LastName = userbranchModel.LastName;
        ////            staff.UserId = user.Id;
        ////            _staffRep.Add(staff);
        ////            _staffRep.Save(User.Identity.Name, Request.UserHostAddress);
        ////            TempData["message"] = "New Staff added successfully!";
        ////            TempData["msgType"] = "pass";
        ////        }
        ////    }
        ////    else
        ////    {
        ////        // Editing existing user
        ////        var userid = userbranchModel.UserBranch.UserId;
        ////        Staff staff = _staffRep.FindBy(a => a.UserId.ToLower() == userid.ToLower()).FirstOrDefault();
        ////        var ub = _userBranchRep.FindBy(a => a.UserId.ToLower() == userid.ToLower()).FirstOrDefault();
        ////        var newrole = _workRoleRep.FindBy(w => w.Id == userbranchModel.UserBranch.RoleId).FirstOrDefault();
        ////        var currentRoles = await UserManager.GetRolesAsync(userid);

        ////        foreach (var role in currentRoles)
        ////        {
        ////            if (role.ToLower() != "staff" && role.ToLower() != "admin")
        ////                await UserManager.RemoveFromRoleAsync(userid, role);
        ////        }

        ////        //Actual update now
        ////        await UserManager.AddToRoleAsync(userid, newrole.Name);
        ////        ub.DepartmentId = userbranchModel.UserBranch.DepartmentId;
        ////        ub.BranchId = userbranchModel.UserBranch.BranchId;
        ////        ub.RoleId = newrole.Id;
        ////        _userBranchRep.Edit(ub);
        ////        _userBranchRep.Save(User.Identity.Name, Request.UserHostAddress);
        ////        staff.FirstName = userbranchModel.FirstName;
        ////        staff.LastName = userbranchModel.LastName;
        ////        _staffRep.Edit(staff);
        ////        _staffRep.Save(User.Identity.Name, Request.UserHostAddress);

        ////        TempData["message"] = "Staff information updated successfully!";
        ////        TempData["msgType"] = "pass";
        ////        //ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
        ////        //return View();
        ////    }

        ////    return RedirectToAction("Index");

        ////}

        ////
        //// GET: /Users/Edit/1
        //[Authorize(Roles = "Admin, Assignment")]
        //public async Task<ActionResult> Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var user = await UserManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var userRoles = await UserManager.GetRolesAsync(user.Id);

        //    return View(new EditUserViewModel()
        //    {
        //        Id = user.Id,
        //        Email = user.Email,
        //        RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
        //        {
        //            Selected = userRoles.Contains(x.Name),
        //            Text = x.Name,
        //            Value = x.Name
        //        })
        //    });
        //}

        ////
        //// POST: /Users/Edit/5
        //[Authorize(Roles = "Admin, Assignment")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] EditUserViewModel editUser, params string[] selectedRole)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await UserManager.FindByIdAsync(editUser.Id);
        //        if (user == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        user.UserName = editUser.Email;
        //        user.Email = editUser.Email;

        //        var userRoles = await UserManager.GetRolesAsync(user.Id);

        //        selectedRole = selectedRole ?? new string[] { };

        //        var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

        //        if (!result.Succeeded)
        //        {
        //            ModelState.AddModelError("", result.Errors.First());
        //            //return View();
        //        }
        //        result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

        //        if (!result.Succeeded)
        //        {
        //            ModelState.AddModelError("", result.Errors.First());
        //            //return View();
        //        }
        //        return RedirectToAction("Index");
        //    }
        //    ModelState.AddModelError("", "Something failed.");
        //    return RedirectToAction("Index");
        //}

        ////
        //// GET: /Users/Delete/5
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var user = await UserManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(user);
        //}

        ////
        //// POST: /Users/Delete/5
        //[Authorize(Roles = "Admin")]
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(string id)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (id == null)
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        }

        //        var user = await UserManager.FindByIdAsync(id);
        //        var staff = _staffRep.FindBy(stf => stf.UserId.ToLower() == id.ToLower()).FirstOrDefault();
        //        if (user == null || staff == null)
        //        {
        //            return HttpNotFound();
        //        }
        //        var result = await UserManager.DeleteAsync(user);
        //        if (!result.Succeeded)
        //        {
        //            ModelState.AddModelError("", result.Errors.First());
        //            return View();
        //        }
        //        //Delete Staff Info
        //        _staffRep.Delete(staff);
        //        _staffRep.Save(User.Identity.Name, Request.UserHostAddress);

        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        //[Authorize(Roles = "Admin")]
        //public ActionResult CreateWorkRole()
        //{
        //    WorkRole model = new WorkRole();
        //    return View(model);
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public ActionResult CreateWorkRole(WorkRole model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _workRoleRep.Add(model);
        //        _workRoleRep.Save(User.Identity.Name, Request.UserHostAddress);

        //        TempData["status"] = "pass";
        //        TempData["message"] = "New Role '" + model.Name + "' Added Successfully";
        //        return RedirectToAction("Index", "Rule");
        //    }
        //    else
        //    {
        //        TempData["status"] = "fail";
        //        TempData["message"] = "Sorry there was an error while creating the new role.";
        //        return RedirectToAction("Index", "Rule");
        //    }
        //}

        #endregion
        #region

        //public ActionResult WorkRoles()
        //{
        //    var wroles = _workRoleRep.GetAll().ToList();
        //    return View(wroles);
        //}

        //[Authorize(Roles = "Admin")]
        //public ActionResult AssignMoreDepartments(string Id, int ubId)
        //{
        //    ViewBag.userId = Id;
        //    UserBranch usrb = _userBranchRep.FindBy(C => C.Id == ubId && C.UserId.ToLower() == Id.ToLower()).FirstOrDefault();

        //    List<UserBranch> usbList = _userBranchRep.FindBy(C => C.UserId.ToLower() == Id.ToLower()).ToList();
        //    if (usrb != null)
        //    {
        //        List<Department> departments = _departmentRep.GetAll().ToList();
        //        foreach (var item in departments)
        //        {
        //            var exist = usbList.Find(C => C.DepartmentId == item.Id);
        //            if (exist != null)
        //                item.Selected = true;
        //        }
        //        return View(departments);
        //    }
        //    else
        //    {
        //        return View("Error");
        //    }
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public ActionResult AssignMoreDepartments(string[] departments, string userId)
        //{
        //    List<UserBranch> usbList = _userBranchRep.FindBy(C => C.UserId.ToLower() == userId.ToLower()).ToList();

        //    if (usbList.Count > 0)
        //    {
        //        var usrb = usbList.FirstOrDefault();

        //        foreach (var item in departments)
        //        {
        //            var di = Convert.ToInt32(item);
        //            var ub = usbList.FirstOrDefault(a => a.DepartmentId == di);
        //            if (ub == null)
        //            {
        //                var Nu = new UserBranch { Active = usrb.Active, BranchId = usrb.BranchId, DepartmentId = di, DeskCount = usrb.DeskCount, RoleId = usrb.RoleId, UserId = usrb.UserId };
        //                _userBranchRep.Add(Nu);
        //            }
        //        }
        //        _userBranchRep.Save(User.Identity.Name, Request.UserHostAddress);
        //        TempData["message"] = "Staff Departments Updated Successfully";
        //        TempData["msgType"] = "pass";
        //    }
        //    else
        //    {
        //        TempData["message"] = "Staff has not been created. Please create staff first and try again.";
        //        TempData["msgType"] = "pass";
        //    }

        //    return RedirectToAction("Index");
        //}

        //[Authorize(Roles = "Admin")]
        //public ActionResult FreeManagers()
        //{
        //    ViewBag.Managers = _vUserBranch.FindBy(a => a.Role.Trim().ToLower() == "manager").ToList();
        //    return View();
        //}

        //[Authorize(Roles = "Admin")]
        //public ActionResult FreeManager(string id, int deptid)
        //{
        //    var mans = _vUserBranch.FindBy(a => a.DepartmentId == deptid && a.Email.ToLower() == id.ToLower()).FirstOrDefault();
        //    // Get all pending jobs (waivers and schedules)
        //    var managerWaivers = _vMSA.FindBy(a => a.UserId == mans.UserId).ToList();
        //    ViewBag.Manager = mans.Email;
        //    List<int> ids = new List<int>();
        //    foreach (var item in managerWaivers)
        //    {
        //        if (!ids.Contains(item.ApplicationId))
        //            ids.Add(item.ApplicationId);
        //    }

        //    List<vMeetingScheduleApplication> toReturn = new List<vMeetingScheduleApplication>();
        //    foreach (var _id in ids)
        //    {
        //        var range = managerWaivers.FindAll(a => a.ApplicationId == _id);
        //        var pick = range[range.Count() - 1];
        //        if (pick.Approved == null)
        //            toReturn.Add(pick);
        //    }
        //    ViewBag.managerSchmeeting = toReturn;

        //    //Load other managers in the specified man dept

        //    ViewBag.OtherMans = _vUserBranch.FindBy(a => a.DepartmentId == mans.DepartmentId && a.RoleId == mans.RoleId && a.BranchId == mans.BranchId && a.Email.ToLower() != id.ToLower() && a.Active).ToList();
        //    return View();
        //}

        //[Authorize, HttpPost]
        //public ActionResult PushToManager(string id, int appid)
        //{
        //    var newmanid = Guid.Parse(id);
        //    var man = _vUserBranch.FindBy(u => u.UserId == id).FirstOrDefault();
        //    if (man != null)
        //    {
        //        //Get the schedule first
        //        var schedule = _schedule.FindBy(a => a.ApplicationId == appid).ToList();
        //        if (schedule.Any())
        //        {
        //            //Schedule/Double waiver
        //            var schpick = schedule[schedule.Count() - 1];
        //            // Find its waiver
        //            if (schpick.WaiverRequest == true)
        //            {
        //                var waiver = _waive.FindBy(w => w.EntityId == schpick.Id).FirstOrDefault();
        //                waiver.AssignedManager = man.Email;
        //                _waive.Edit(waiver);
        //                _waive.Save(User.Identity.Name, Request.UserHostAddress);
        //            }
        //            if (schedule.Count() > 1)
        //            {
        //                //Set others to Approved = false
        //                schedule.Remove(schpick);
        //                for (int i = 0; i < schedule.Count; i++)
        //                {
        //                    schedule[i].Approved = false;
        //                    _schedule.Edit(schedule[i]);
        //                }
        //                _schedule.Save(User.Identity.Name, Request.UserHostAddress);
        //            }


        //            var mansch = _manSche.FindBy(a => a.ScheduleId == schpick.Id).FirstOrDefault();
        //            mansch.UserId = man.UserId.Trim();
        //            _manSche.Edit(mansch);
        //            _manSche.Save(User.Identity.Name, Request.UserHostAddress);

        //            var ajsp = _ajspRep.FindBy(a => a.ApplicationId == appid).FirstOrDefault();
        //            ajsp.InspectionRequired = true;
        //            ajsp.PresentationRequired = true;
        //            _ajspRep.Edit(ajsp);
        //            _ajspRep.Save(User.Identity.Name, Request.UserHostAddress);

        //            return Json(1, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    return Json(0, JsonRequestBehavior.AllowGet);
        //}
        //public ActionResult GetProcesses(int id)
        //{
        //    var processes = _appProcRep.FindBy(a => a.ApplicationId == id).OrderBy(a => a.sortOrder).ToList();
        //    return Json(processes, JsonRequestBehavior.AllowGet);
        //}

        #endregion
    }
}
