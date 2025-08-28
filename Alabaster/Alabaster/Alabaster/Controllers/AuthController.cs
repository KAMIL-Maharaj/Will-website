using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Alabaster.Services;

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

        /// <summary>
        /// Register a new user (role "User" is automatically assigned)
        /// </summary>
        [HttpPost("Register")]
        public async Task<IActionResult> Register(string email, string password)
        {
            try
            {
                var result = await _authService.Register(email, password);

                // Store Firebase token in session (optional, can also skip until login)
                HttpContext.Session.SetString("FirebaseToken", result.FirebaseToken);

                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (msg.Contains("EMAIL_EXISTS")) ViewBag.Error = "Email already exists.";
                else if (msg.Contains("WEAK_PASSWORD")) ViewBag.Error = "Password too weak.";
                else ViewBag.Error = "Registration failed.";
                return View();
            }
        }

        /// <summary>
        /// Login a user or admin with role-based redirect
        /// </summary>
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var result = await _authService.Login(email, password);

                // Decode Firebase token to get custom claims (roles)
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(result.FirebaseToken);
                string role = decodedToken.Claims.ContainsKey("role") ? decodedToken.Claims["role"].ToString() : "User";

                // Store session info
                HttpContext.Session.SetString("FirebaseToken", result.FirebaseToken);
                HttpContext.Session.SetString("UserId", decodedToken.Uid);
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("UserRole", role);

                // Redirect based on role
                if (role == "Admin") return RedirectToAction("Index", "Admin");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Login failed: " + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                await _authService.SendPasswordResetEmail(email);
                ViewBag.Message = "Password reset link sent!";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("EMAIL_NOT_FOUND"))
                    ViewBag.Error = "No account with that email.";
                else
                    ViewBag.Error = "Something went wrong.";
            }
            return View();
        }

        /// <summary>
        /// Google Sign-In
        /// </summary>
        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
                return Json(new { success = false, error = "No token provided" });

            try
            {
                // Verify Firebase token
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                string uid = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;
                string role = decodedToken.Claims.ContainsKey("role") ? decodedToken.Claims["role"].ToString() : "User";

                // Store session info
                HttpContext.Session.SetString("FirebaseToken", idToken);
                HttpContext.Session.SetString("UserId", uid);
                HttpContext.Session.SetString("UserEmail", email ?? "");
                HttpContext.Session.SetString("UserRole", role);

                return Json(new { success = true });
            }
            catch (FirebaseAuthException ex)
            {
                return Json(new { success = false, error = $"Token verification failed: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Unexpected error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Logout the current user
        /// </summary>
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
