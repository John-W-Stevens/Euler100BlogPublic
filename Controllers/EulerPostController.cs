using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using EulerBlog.Models;

namespace EulerBlog.Controllers
{
    public class EulerPostController : Controller
    {
        private DBContext dbContext;

        public EulerPostController(DBContext context) { dbContext = context; }

        ///////////// BEGINNING OF CRUD METHODS FOR EULERPOST MODEL /////////////

        //    REQUEST:      ROUTE:                     METHOD:
        //    --------------------------------------------------------------
        //    GET           /problems                          Problems()
        //    GET/POST      /problem/create                    GET-> CreateEulerPostForm() / POST -> CreateEulerPost(EulerPost eulerpost)
        //    GET           /problem/{EulerPostId}             Problem(int eulerpostId)
        //    GET/POST      /problem/{eulerpostId}/update      GET-> EditEulerPost(int eulerpostId) / POST-> UpdateEulerPost(int eulerpostId, EulerPost eulerpost)
        //    POST          /problem/{eulerpostId}/delete      DeleteEulerPost(int eulerpostId)

        // Helper Functions
        public EulerPost[] GetAllEulerPosts() { return dbContext.EulerPosts.ToArray(); }
        public EulerPost GetOneSingleEulerPostById (int eulerpostId) { return dbContext.EulerPosts.Include(p => p.Modules).ThenInclude(m => m.Module).FirstOrDefault(e => e.EulerPostId == eulerpostId); }
        public IActionResult GetCreateEulerPostForm () { ViewBag.Message = "Add"; ViewBag.EulerPostId = ""; return View("CreateOrUpdateEulerPost"); }
        public IActionResult GetEditEulerPostForm(int eulerpostId) { ViewBag.Message = "Edit"; ViewBag.EulerPostId = eulerpostId; return View("CreateOrUpdateEulerPost", GetOneSingleEulerPostById(eulerpostId)); }

        public Boolean IsLogged() { return HttpContext.Session.GetInt32("UserId") != null; }
        public User GetLoggedUser() { return dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))); }

        public Boolean IsAdmin() { return GetLoggedUser().UserId == 1; }


        [HttpGet("problems")]
        public IActionResult Problems() {

            if (IsLogged())
            {
                if (IsAdmin()) { ViewBag.IsAdmin = true; }
            }
            else
            {
                ViewBag.IsAdmin = false;
            }
            return View("Problems", GetAllEulerPosts());
        }

        [HttpGet("problem/create")]
        public IActionResult CreateEulerPostForm() {
            if (!IsLogged()) { return RedirectToAction("Problems"); }
            if (!IsAdmin()) { return RedirectToAction("Problems"); }
            return GetCreateEulerPostForm();
        }

        [HttpPost("problem/create")]
        public IActionResult CreateEulerPost(EulerPost eulerpost)
        {
            User user = GetLoggedUser();
            if (user.UserId != 1) { return RedirectToAction("Problems"); }

            eulerpost.UserId = user.UserId;
            
            if (ModelState.IsValid)
            {
                dbContext.Add(eulerpost);
                dbContext.SaveChanges();
                return RedirectToAction("Problems");
            }
            return GetCreateEulerPostForm();
        }

        [HttpGet("problem/{eulerpostId}")]
        public IActionResult Problem(int eulerpostId) {

	    if (! dbContext.EulerPosts.Any(p => p.EulerPostId == eulerpostId)) { return RedirectToAction("Problems"); }
            EulerPost post = dbContext.EulerPosts
                .Include(e => e.Comments)
                .ThenInclude(c => c.Author)
                .Include(e => e.Modules)
                .ThenInclude(m => m.Module)
                .FirstOrDefault(e => e.EulerPostId == eulerpostId);

            ViewBag.FirstName = "";
            if (IsLogged())
            {
                ViewBag.FirstName = GetLoggedUser().FirstName;
                if (IsAdmin()) { ViewBag.IsAdmin = true; }
            }
            else
            {
                ViewBag.IsAdmin = false;
            }
            if (TempData["ErrorMessage"] != null) { ViewBag.ErrorMessage = TempData["ErrorMessage"]; }

	    if (eulerpostId > 1) { ViewBag.Previous = eulerpostId - 1; }
            if (dbContext.EulerPosts.Any(u => u.EulerPostId == eulerpostId + 1)) { ViewBag.Next = eulerpostId + 1; }
            
	    return View("Problem", post);
        }

        [HttpGet("problem/{eulerpostId}/update")]
        public IActionResult EditEulerPost(int eulerpostId) {
            if (!IsLogged()) { return RedirectToAction("Problems"); }
            if (!IsAdmin()) { return RedirectToAction("Problems"); }

            return GetEditEulerPostForm(eulerpostId);
        }

        [HttpPost("problem/{eulerpostId}/comment")]
        public IActionResult CreateEulerPostComment(int eulerpostId)
        {
            if (Request.Form["Comment"] == "")
            {
                TempData["ErrorMessage"] = "Cannot submit an empty comment.";
                return RedirectToAction("Problem", new { eulerpostId = eulerpostId });
            }

            EulerPost post = GetOneSingleEulerPostById(eulerpostId);
            User user = GetLoggedUser();

            Comment comment = new Comment() {
                Author = user,
                UserId = user.UserId,
                EulerPost = post,
                EulerPostId = post.EulerPostId,
                Content = Request.Form["Comment"],
            };
            dbContext.Comments.Add(comment);
            dbContext.SaveChanges();

            return RedirectToAction("Problem", new { eulerpostId = eulerpostId });
        }

        [HttpPost("problem/{eulerpostId}/update")]
        public IActionResult UpdateEulerPost(int eulerpostId, EulerPost eulerpost)
        {
            User user = GetLoggedUser();
            if (!IsAdmin()) { return RedirectToAction("Problems"); }

            eulerpost.UserId = user.UserId;


            if (ModelState.IsValid)
            {
                dbContext.Update(eulerpost);
                dbContext.Entry(eulerpost).Property("CreatedAt").IsModified = false;
                dbContext.SaveChanges();
                return RedirectToAction("Problem", new { eulerpostId = eulerpost.EulerPostId });
            }
            else { return GetEditEulerPostForm(eulerpostId); }
        }

        [HttpPost("problem/{eulerpostId}/delete")]
        public IActionResult DeleteEulerPost(int eulerpostId)
        {
            if (!IsAdmin()) { return RedirectToAction("Problems"); }
            dbContext.EulerPosts.Remove(GetOneSingleEulerPostById(eulerpostId));
            dbContext.SaveChanges();
            return RedirectToAction("Problems");
        }

        ///////////// END OF CRUD METHODS FOR EULERPOST MODEL /////////////

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


