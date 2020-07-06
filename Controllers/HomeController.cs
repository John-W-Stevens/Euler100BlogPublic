using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // for session
using Microsoft.AspNetCore.Identity; // for password hashing
using EulerBlog.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MimeKit;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EulerBlog.Controllers
{
    public class HomeController : Controller
    {

        private DBContext dbContext;

        public HomeController(DBContext context)
        {
            dbContext = context;
        }

        public Boolean IsLogged() { return HttpContext.Session.GetInt32("UserId") != null; }
        public User GetLoggedUser() { return dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))); }
        public Boolean IsAdmin() { return GetLoggedUser().UserId == 1; }

        public Boolean EmailInDb(string email) { return dbContext.Users.FirstOrDefault(u => u.Email == email) != null; }

        // Generate a long random string:
        public string GenerateRandomString()
        {
            // Generates a pseudo-random string of 26 chars
            StringBuilder RandomStringBuild = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < 52; i++) { RandomStringBuild.Append(Convert.ToChar(Convert.ToInt32(Math.Floor(25 * random.NextDouble())) + 65)); }
            return RandomStringBuild.ToString();
        }

        public void NotifyUser(string message, string email, string action, string urlAction)
        {
            MailMessage msg = new MailMessage();
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
            try
            {
                msg.Subject = "Euler 100 Blog";
                msg.Body = $"Please {action} by following this link: http://euler100blog.net/account/{urlAction}/{message}";
                msg.From = new MailAddress("euler100blog@gmail.com");
                msg.To.Add(email);
                msg.IsBodyHtml = true;
                client.Host = "smtp.gmail.com";
                System.Net.NetworkCredential basicauthenticationinfo = new System.Net.NetworkCredential("euler100blog@gmail.com", "VIPERcli12#$56");
                client.Port = int.Parse("587");
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = basicauthenticationinfo;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        // ROUTE:               METHOD:                VIEW:
        // -----------------------------------------------------------------------------------
        // GET("")              Index()                Index.cshtml
        // POST("/register")    Create(User user)      ------ (Index.cshtml to display errors)
        // POST("/login")       Login(LoginUser user)  ------ (Index.cshtml to display errors)
        // GET("/logout")       Logout()               ------
        // GET("/success")      Success()              Success.cshtml

        [HttpGet("")]
        public IActionResult Index()
        {
            return RedirectToAction("Home");
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            if (IsLogged()) { return RedirectToAction("Home"); }
            return View();
        }

        [HttpGet("/register")]
        public IActionResult Register()
        {
            if (IsLogged()) { return RedirectToAction("Home"); }
            return View();
        }

        [HttpPost("/register")]
        public IActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (dbContext.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Register");
                }

                // hash password
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);

                // make the first user to a register an administrator
                if (dbContext.Users.ToArray().Length == 0) { user.IsAdmin = true; }

                string EmailConfirmationCode = GenerateRandomString();
                user.EmailConfirmation = EmailConfirmationCode;

                // create user
                dbContext.Add(user);
                dbContext.SaveChanges();

                NotifyUser(EmailConfirmationCode, user.Email, "confirm your email address", "confirm_email");

                // sign user into session
                var NewUser = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                int UserId = NewUser.UserId;
                HttpContext.Session.SetInt32("UserId", UserId);
                HttpContext.Session.SetString("UserFirstName", NewUser.FirstName);
                HttpContext.Session.SetString("UserLastName", NewUser.LastName);

                // go to success
                return RedirectToAction("Problems", "EulerPost");
            }
            // display errors
            else
            {
                return View("Register");
            }
        }

        [HttpPost("/login")]
        public IActionResult Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.LoginEmail);
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Login");
                }
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.LoginPassword);
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("LoginPassword", "Password is invalid.");
                    return View("Login");
                }

                // sign user into session
                int UserId = userInDb.UserId;
                HttpContext.Session.SetInt32("UserId", UserId);
                HttpContext.Session.SetString("UserFirstName", userInDb.FirstName);
                HttpContext.Session.SetString("UserLastName", userInDb.LastName);

                if (userInDb.IsConfirmed == true) { HttpContext.Session.SetInt32("IsConfirmed", 1); }

                if (userInDb.IsAdmin == true) { HttpContext.Session.SetString("ia", "true"); }
                else { HttpContext.Session.SetString("ia", "false"); }

                return RedirectToAction("Problems", "EulerPost");
            }
            // display errors
            else
            {
                return View("Login");
            }
        }

        // confirm user:
        [HttpGet("/account/confirm_email/{emailConfirmation}")]
        public IActionResult ConfirmUser(string emailConfirmation)
        {
            User user = dbContext.Users.FirstOrDefault(u => u.EmailConfirmation == emailConfirmation && u.IsConfirmed == false);
            if (user != null)
            {
                user.IsConfirmed = true;
                dbContext.SaveChanges();
                if (IsLogged()) { HttpContext.Session.SetInt32("IsConfirmed", 1); }
                return RedirectToAction("Problems", "EulerPost");
            }
            return RedirectToAction("Home");
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet("Home")]
        public IActionResult Home()
        {
            return View();
        }

        // Remove Account
        [HttpGet("account/delete/{userId}")]
        public IActionResult RemoveAccount(int userId)
        {
            if (GetLoggedUser().UserId != userId) { return RedirectToAction("Index", "Home"); }
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost("account/delete/{userId}")]
        public IActionResult RemoveAccount(int userId, LoginUser user)
        {
            if (ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.LoginEmail);
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("RemoveAccount");
                }
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.LoginPassword);
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("LoginPassword", "Password is invalid.");
                    return View("RemoveAccount");
                }

                // sign user into session
                dbContext.Remove(userInDb);
                dbContext.SaveChanges();
                HttpContext.Session.Clear();
                return RedirectToAction("Problems", "EulerPost");
            }
            // display errors
            else
            {
                return View("RemoveAccount");
            }

        }

        [HttpGet("account/update/{userId}")]
        public IActionResult UpdateAccount(int userId)
        {
            User user = GetLoggedUser();
            if (user.UserId != userId) { return RedirectToAction("Index", "Home"); }
            return View("UpdateAccount", user);
        }

        [HttpPost("account/update/{userId}")]
        public IActionResult EditAccount(int userId, User user)
        {

            User userInDb = GetLoggedUser();
            if (userInDb.UserId != userId) { return RedirectToAction("Index", "Home"); }            

            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<User>();
                var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.Password);
                if (result == 0)
                {
                    ModelState.AddModelError("Password", "Password is incorrect.");
                    return View("UpdateAccount");
                }

                if (user.FirstName != userInDb.FirstName)
                {
                    userInDb.FirstName = user.FirstName;
                    dbContext.SaveChanges();
                }
                if (user.LastName != userInDb.LastName)
                {
                    userInDb.LastName = user.LastName;
                    dbContext.SaveChanges();
                }
                if (user.Email != userInDb.Email)
                {
                    if (dbContext.Users.Any(u => u.Email == user.Email))
                    {
                        ModelState.AddModelError("Email", "Email already in use!");
                        return View("UpdateAccount");
                    }

                    string EmailConfirmationCode = GenerateRandomString();
                    userInDb.Email = user.Email;
                    userInDb.IsConfirmed = false;
                    userInDb.EmailConfirmation = EmailConfirmationCode;
                    dbContext.SaveChanges();
                    NotifyUser(EmailConfirmationCode, userInDb.Email, "confirm your email address", "confirm_email");
                }
                HttpContext.Session.Clear();
                int UserId = userInDb.UserId;
                HttpContext.Session.SetInt32("UserId", UserId);
                HttpContext.Session.SetString("UserFirstName", userInDb.FirstName);
                HttpContext.Session.SetString("UserLastName", userInDb.LastName);

                if (userInDb.IsConfirmed == true) { HttpContext.Session.SetInt32("IsConfirmed", 1); }

                if (userInDb.IsAdmin == true) { HttpContext.Session.SetString("ia", "true"); }
                else { HttpContext.Session.SetString("ia", "false"); }

                return RedirectToAction("Account");
            }
            //foreach (var error in ModelState.Values)
            //{
            //    foreach (var e in error.Errors) {
            //        Console.WriteLine(e.Exception);
            //        Console.WriteLine(e.ErrorMessage);
            //    }
            //}
            return View("UpdateAccount");
        }



        [HttpGet("account/resetpassword")]
        public IActionResult PasswordReset()
        {
            if (TempData["PWResetError"] != null) { ViewBag.PWResetError = TempData["PWResetError"]; }
            else { ViewBag.PWResetError = ""; }
            return View();
        }

        [HttpPost("account/resetpassword")]
        public IActionResult ForgotPassword()
        {
            string Email = Request.Form["Email"];

            if (!EmailInDb(Email)){
                TempData["PWResetError"] = "Invalid Email Address";
                return RedirectToAction("PasswordReset");
            }

            User user = dbContext.Users.FirstOrDefault(u => u.Email == Email);
            string pwReset = GenerateRandomString();
            user.PasswordReset = pwReset;
            dbContext.SaveChanges();

            NotifyUser(pwReset, user.Email, "reset your password", "resetpassword");

            return RedirectToAction("Home");
        }

        [HttpGet("/account/resetpassword/{reset_pw}")]
        public IActionResult ResetPasswordForm(string reset_pw)
        {
            User userToUpdate = dbContext.Users.FirstOrDefault(u => u.PasswordReset == reset_pw);
            if (userToUpdate != null)
            {
                return View();
            }
            return RedirectToAction("Login");
        }

        [HttpPost("/account/resetpassword/{reset_pw}")]
        public IActionResult ProcessPasswordReset(string reset_pw, ResetPW resetPW)
        {
            if (ModelState.IsValid)
            {
                User user = dbContext.Users.FirstOrDefault(u => u.PasswordReset == reset_pw);
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, resetPW.Password);
                dbContext.SaveChanges();
                return RedirectToAction("Login");
            }
            return View("ResetPasswordForm");
        }

        [HttpGet("/news")]
        public IActionResult News() { return View(); }

        [HttpGet("/resources")]
        public IActionResult Resources() { return View(); }

        [HttpGet("/about")]
        public IActionResult About() { return View(); }

        [HttpGet("/account")]
        public IActionResult Account()
        {
            User user = GetLoggedUser();
            if (user == null) { return RedirectToAction("Index"); }
            return View("Account", user);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

