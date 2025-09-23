using Firebase.Auth;
using FirebaseAdmin.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Alabaster.Services;
using System;
using System.Threading.Tasks;

namespace Alabaster.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly FirebaseAuthService _authService;
        private readonly FirebaseClient _dbClient;

        public AuthController(FirebaseAuthService authService)
        {
            _authService = authService;
            _dbClient = new FirebaseClient("https://alabaster-8cfcd-default-rtdb.firebaseio.com/");
        }

        [HttpGet("Register")]
        public IActionResult Register() => View();

        [HttpGet("Login")]
        public IActionResult Login() => View();

        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword() => View();

        [HttpPost("Register")]
        public async Task<IActionResult> Register(string email, string password)
        {
            try
            {
                var result = await _authService.Register(email, password);

                string uid = result.User.LocalId; // FIX: Use LocalId instead of UserUid

                // Store token, email, UID in session
                HttpContext.Session.SetString("FirebaseToken", result.FirebaseToken);
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("UserId", uid);

                // Check if user is admin
                var isAdmin = await _dbClient
                    .Child("admins")
                    .Child(uid)
                    .OnceSingleAsync<bool?>() ?? false;

                HttpContext.Session.SetString("IsAdmin", isAdmin ? "true" : "false");

                TempData["Success"] = "Registration successful! You are now logged in.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (msg.Contains("EMAIL_EXISTS")) ViewBag.Error = "Email already exists.";
                else if (msg.Contains("WEAK_PASSWORD")) ViewBag.Error = "Password too weak (min 6 chars).";
                else ViewBag.Error = "Registration failed. " + msg;
                return View();
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var result = await _authService.Login(email, password);

                string uid = result.User.LocalId; // FIX: Use LocalId instead of UserUid

                // Store token, email, UID in session
                HttpContext.Session.SetString("FirebaseToken", result.FirebaseToken);
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("UserId", uid);

                // Check if user is admin
                var isAdmin = await _dbClient
                    .Child("admins")
                    .Child(uid)
                    .OnceSingleAsync<bool?>() ?? false;

                HttpContext.Session.SetString("IsAdmin", isAdmin ? "true" : "false");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (msg.Contains("EMAIL_NOT_FOUND") || msg.Contains("INVALID_LOGIN_CREDENTIALS"))
                    ViewBag.Error = "Account not found.";
                else if (msg.Contains("INVALID_PASSWORD"))
                    ViewBag.Error = "Incorrect password.";
                else
                    ViewBag.Error = "Login failed. " + msg;

                return View();
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                await _authService.SendPasswordResetEmail(email);
                ViewBag.Message = "Password reset link sent! Check your email.";
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (msg.Contains("EMAIL_NOT_FOUND"))
                    ViewBag.Error = "No account found with that email.";
                else
                    ViewBag.Error = "Failed to send reset link. " + msg;
            }
            return View();
        }

[HttpPost("GoogleLogin")]
public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
{
    if (string.IsNullOrEmpty(idToken))
        return Json(new { success = false, error = "No token provided." });

    try
    {
        // Fully qualified FirebaseAdmin namespace
        FirebaseAdmin.Auth.FirebaseToken decodedToken = 
            await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

        string uid = decodedToken.Uid;
        string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;

        HttpContext.Session.SetString("FirebaseToken", idToken);
        HttpContext.Session.SetString("UserEmail", email ?? "");
        HttpContext.Session.SetString("UserId", uid);

        // Admin check
        var isAdmin = await _dbClient
            .Child("admins")
            .Child(uid)
            .OnceSingleAsync<bool?>() ?? false;

        HttpContext.Session.SetString("IsAdmin", isAdmin ? "true" : "false");

        return Json(new { success = true });
    }
    catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
    {
        return Json(new { success = false, error = "Token verification failed: " + ex.Message });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, error = "Unexpected error: " + ex.Message });
    }
}


        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
