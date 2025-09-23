using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Alabaster.Services;
using System.Threading.Tasks;

namespace Alabaster.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly FirebaseAuthService _authService;

        public AuthController(FirebaseAuthService authService)
        {
            _authService = authService;
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

                // Store token and email in session
                HttpContext.Session.SetString("FirebaseToken", result.FirebaseToken);
                HttpContext.Session.SetString("UserEmail", email);

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

                // Store token and email in session
                HttpContext.Session.SetString("FirebaseToken", result.FirebaseToken);
                HttpContext.Session.SetString("UserEmail", email);

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
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;

                // Store session info
                HttpContext.Session.SetString("FirebaseToken", idToken);
                HttpContext.Session.SetString("UserEmail", email ?? "");
                HttpContext.Session.SetString("UserId", uid);

                return Json(new { success = true });
            }
            catch (FirebaseAuthException ex)
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
