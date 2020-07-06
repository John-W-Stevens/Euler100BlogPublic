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
    public class CommentController : Controller
    {
        private DBContext dbContext;

        public CommentController(DBContext context) { dbContext = context; }

        ///////////// BEGINNING OF CRUD METHODS FOR COMMENT MODEL /////////////

        //    REQUEST:      ROUTE:                     METHOD:
        //    --------------------------------------------------------------
        //    GET           /comments                     Comments()
        //    GET/POST      /comment/create               GET-> CreateCommentForm() / POST -> CreateComment(Comment comment)
        //    GET           /comment/{CommentId}             Comment(int commentId)
        //    GET/POST      /comment/{commentId}/update      GET-> EditComment(int commentId) / POST-> UpdateComment(int commentId, Comment comment)
        //    POST          /comment/{commentId}/delete      DeleteComment(int commentId)

        // Helper Functions
        public Comment[] GetAllComments() { return dbContext.Comments.ToArray(); }
        public Comment GetOneSingleCommentById (int commentId) { return dbContext.Comments.FirstOrDefault(c => c.CommentId == commentId); }
        public IActionResult GetCreateCommentForm () { ViewBag.Message = "Add"; ViewBag.CommentId = ""; return View("CreateOrUpdateComment"); }
        public IActionResult GetEditCommentForm(int commentId) { ViewBag.Message = "Edit"; ViewBag.CommentId = commentId; return View("CreateOrUpdateComment", GetOneSingleCommentById(commentId)); }

        public User GetLoggedUser() { return dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))); }
        public Boolean IsAdmin() { return GetLoggedUser().UserId == 1; }

        public Boolean IsAuthor(int commentId) { return GetLoggedUser().UserId == GetOneSingleCommentById(commentId).UserId; }

        [HttpGet("comments")]
        public IActionResult Comments() {
            if (!IsAdmin()) { return RedirectToAction("Problems", "EulerPost"); }
            return View("Comments", GetAllComments());
        }

        [HttpGet("comment/create")]
        public IActionResult CreateCommentForm() {
            if (!IsAdmin()) { return RedirectToAction("Problems", "EulerPost"); }
            return GetCreateCommentForm();
        }

        [HttpPost("comment/create")]
        public IActionResult CreateComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                dbContext.Add(comment);
                dbContext.SaveChanges();
                return RedirectToAction("Comments");
            }
            return GetCreateCommentForm();
        }

        [HttpGet("comment/{commentId}")]
        public IActionResult Comment(int commentId) {
            if (!IsAdmin()) { return RedirectToAction("Problems", "EulerPost"); }
            return View("Comment", GetOneSingleCommentById(commentId));
        }

        [HttpGet("comment/{commentId}/update")]
        public IActionResult EditComment(int commentId) {

            if (IsAdmin() || IsAuthor(commentId)) { return GetEditCommentForm(commentId); }
            return RedirectToAction("Problems", "EulerPost");
  
        }

        [HttpPost("comment/{commentId}/update")]
        public IActionResult UpdateComment(int commentId, Comment comment)
        {
            if (IsAdmin() || IsAuthor(commentId))
            {
                if (ModelState.IsValid)
                {
                    Comment prevComment = GetOneSingleCommentById(commentId);
                    prevComment.Content = comment.Content;
                    prevComment.UpdatedAt = DateTime.Now;
                    dbContext.SaveChanges();
                    return RedirectToAction("Problem", "EulerPost", new { eulerpostId = prevComment.EulerPostId });
                }
                else { return GetEditCommentForm(commentId); }
            }
            else { return RedirectToAction("Problems", "EulerPost"); }
        }

        [HttpPost("comment/{commentId}/delete")]
        public IActionResult DeleteComment(int commentId)
        {
            if (IsAdmin() || IsAuthor(commentId))
            {
                int eulerpostId = GetOneSingleCommentById(commentId).EulerPostId;

                dbContext.Comments.Remove(GetOneSingleCommentById(commentId));
                dbContext.SaveChanges();
                return RedirectToAction("Problem", "EulerPost", new { eulerpostId = eulerpostId });
            }
            return RedirectToAction("Problems", "EulerPost");
        }

        ///////////// END OF CRUD METHODS FOR COMMENT MODEL /////////////

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


