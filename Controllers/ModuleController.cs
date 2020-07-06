using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using EulerBlog.Models;

namespace EulerBlog.Controllers
{
    public class ModuleController : Controller
    {
        private DBContext dbContext;

        public ModuleController(DBContext context) { dbContext = context; }

        ///////////// BEGINNING OF CRUD METHODS FOR MODULE MODEL /////////////

        //    REQUEST:      ROUTE:                     METHOD:
        //    --------------------------------------------------------------
        //    GET           /modules                     Modules()
        //    GET/POST      /module/create               GET-> CreateModuleForm() / POST -> CreateModule(Module module)
        //    GET           /module/{ModuleId}             Module(int moduleId)
        //    GET/POST      /module/{moduleId}/update      GET-> EditModule(int moduleId) / POST-> UpdateModule(int moduleId, Module module)
        //    POST          /module/{moduleId}/delete      DeleteModule(int moduleId)

        // Helper Functions
        public Module[] GetAllModules() { return dbContext.Modules.ToArray(); }
        public Module GetOneSingleModuleById (int moduleId) { return dbContext.Modules.Include(m => m.EulerPosts).ThenInclude(p => p.EulerPost).FirstOrDefault(m => m.ModuleId == moduleId); }
        public IActionResult GetCreateModuleForm () { ViewBag.Message = "Add"; ViewBag.ModuleId = ""; return View("CreateOrUpdateModule"); }
        public IActionResult GetEditModuleForm(int moduleId) { ViewBag.Message = "Edit"; ViewBag.ModuleId = moduleId; return View("CreateOrUpdateModule", GetOneSingleModuleById(moduleId)); }

        public Boolean IsLogged() { return HttpContext.Session.GetInt32("UserId") != null; }
        public User GetLoggedUser() { return dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))); }
        public Boolean IsAdmin() { return GetLoggedUser().UserId == 1; }
        public EulerPost[] GetAllEulerPosts() { return dbContext.EulerPosts.ToArray(); }


        [HttpGet("modules")]
        public IActionResult Modules() {
            if (IsLogged()) { if (IsAdmin()) { ViewBag.IsAdmin = true; } }
            else { ViewBag.IsAdmin = false; }
            return View("Modules", GetAllModules());
        }

        [HttpGet("module/create")]
        public IActionResult CreateModuleForm() { return GetCreateModuleForm(); }

        [HttpPost("module/create")]
        public IActionResult CreateModule(Module module)
        {
            if (!IsLogged()) { return RedirectToAction("Modules"); }
            if (!IsAdmin()) { return RedirectToAction("Modules"); }
            if (ModelState.IsValid)
            {
                dbContext.Add(module);
                dbContext.SaveChanges();
                return RedirectToAction("Modules");
            }
            return GetCreateModuleForm();
        }

        [HttpGet("module/{moduleId}")]
        public IActionResult Module(int moduleId) {
            if (!dbContext.Modules.Any(m => m.ModuleId == moduleId)) { return RedirectToAction("Modules"); }
            if (IsLogged()) { if (IsAdmin()) { ViewBag.IsAdmin = true; } }
            else { ViewBag.IsAdmin = false; }
            return View("Module", GetOneSingleModuleById(moduleId));
        }

        [HttpGet("module/{moduleId}/update")]
        public IActionResult EditModule(int moduleId) {
            if (!IsLogged()) { return RedirectToAction("Modules"); }
            if (!IsAdmin()) { return RedirectToAction("Modules"); }
            return GetEditModuleForm(moduleId);
        }

        [HttpPost("module/{moduleId}/update")]
        public IActionResult UpdateModule(int moduleId, Module module)
        {
            if (!IsLogged()) { return RedirectToAction("Modules"); }
            if (!IsAdmin()) { return RedirectToAction("Modules"); }
            if (ModelState.IsValid)
            {
                dbContext.Update(module);
                dbContext.Entry(module).Property("CreatedAt").IsModified = false;
                dbContext.SaveChanges();
                return RedirectToAction("Module", new { moduleId = moduleId });
            }
            else { return GetEditModuleForm(moduleId); }
        }

        [HttpPost("module/{moduleId}/delete")]
        public IActionResult DeleteModule(int moduleId)
        {
            if (!IsLogged()) { return RedirectToAction("Modules"); }
            if (!IsAdmin()) { return RedirectToAction("Modules"); }
            dbContext.Modules.Remove(GetOneSingleModuleById(moduleId));
            dbContext.SaveChanges();
            return RedirectToAction("Modules");
        }

        [HttpGet("module/create/association")]
        public IActionResult CreateMAForm()
        {
            if (!IsLogged()) { return RedirectToAction("Modules"); }
            if (!IsAdmin()) { return RedirectToAction("Modules"); }
            ViewBag.Message = "Create";
            ViewBag.EulerPosts = GetAllEulerPosts();
            ViewBag.Modules = GetAllModules();


            return View("CreateOrUpdateModuleAssociation");
        }

        [HttpPost("module/create/association")]
        public IActionResult CreateMA(ModuleAssociation association)
        {
            if (!IsLogged()) { return RedirectToAction("Modules"); }
            if (!IsAdmin()) { return RedirectToAction("Modules"); }

            EulerPost eulerpost = dbContext.EulerPosts.FirstOrDefault(p => p.EulerPostId == association.EulerPostId);
            Module module = dbContext.Modules.FirstOrDefault(m => m.ModuleId == association.ModuleId);

            association.EulerPost = eulerpost;
            association.Module = module;
            if (ModelState.IsValid)
            {
                dbContext.Add(association);
                dbContext.SaveChanges();
                return RedirectToAction("Modules");
            }
            return View("CreateOrUpdateModuleAssociation");

        }

        [HttpGet("module/update/association")]
        public IActionResult UpdateMAForm()
        {
            if (!IsLogged()) { return RedirectToAction("Problems", "EulerPost"); }
            if (!IsAdmin()) { return RedirectToAction("Problems", "EulerPost"); }
            ViewBag.Message = "Edit";
            return View("CreateOrUpdateModuleAssociation");
        }

        [HttpPost("module/update/association")]
        public IActionResult UpdateMA(ModuleAssociation association)
        {
            EulerPost eulerpost = dbContext.EulerPosts.FirstOrDefault(p => p.EulerPostId == association.EulerPostId);
            Module module = dbContext.Modules.FirstOrDefault(m => m.ModuleId == association.ModuleId);

            association.EulerPost = eulerpost;
            association.Module = module;
            if (ModelState.IsValid)
            {
                dbContext.Update(association);
                dbContext.Entry(association).Property("CreatedAt").IsModified = false;
                dbContext.SaveChanges();
                return RedirectToAction("Modules");
            }
            return View("CreateOrUpdateModuleAssociation");
        }

        ///////////// END OF CRUD METHODS FOR MODULE MODEL /////////////

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


