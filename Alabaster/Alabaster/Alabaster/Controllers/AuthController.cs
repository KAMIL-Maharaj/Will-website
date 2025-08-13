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

        [HttpPost("Register")]
        public async Task<IActionResult> Register(string email, string password)
        {
            try
            {
                var result = await _authService.Register(email, password);
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

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var result = await _authService.Login(email, password);
                HttpContext.Session.SetString("FirebaseToken", result.FirebaseToken);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (msg.Contains("EMAIL_NOT_FOUND") || msg.Contains("INVALID_LOGIN_CREDENTIALS"))
                    ViewBag.Error = "Account not found.";
                else if (msg.Contains("INVALID_PASSWORD"))
                    ViewBag.Error = "Incorrect password.";
                else ViewBag.Error = "Login failed.";
                return View();
            }
        }

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

        // Google Register
        [HttpPost("GoogleRegister")]
        public async Task<IActionResult> GoogleRegister([FromBody] string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
                return Json(new { success = false, error = "No token provided" });

            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;

                // Check if user exists
                try
                {
                    var existingUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                    if (existingUser != null)
                        return Json(new { success = false, error = "Google account already registered. Please log in." });
                }
                catch (FirebaseAuthException)
                {
                    // Not found → OK to create
                }

                await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
                {
                    Uid = uid,
                    Email = email
                });

                HttpContext.Session.SetString("FirebaseToken", idToken);
                HttpContext.Session.SetString("UserId", uid);
                HttpContext.Session.SetString("UserEmail", email ?? "");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Google registration failed: {ex.Message}" });
            }
        }

        // Google Login
        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
                return Json(new { success = false, error = "No token provided" });

            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;

                try
                {
                    var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                    if (userRecord == null)
                        return Json(new { success = false, error = "Google account not registered. Please register first." });

                    HttpContext.Session.SetString("FirebaseToken", idToken);
                    HttpContext.Session.SetString("UserId", uid);
                    HttpContext.Session.SetString("UserEmail", userRecord.Email ?? "");

                    return Json(new { success = true });
                }
                catch (FirebaseAuthException)
                {
                    return Json(new { success = false, error = "Google account not registered. Please register first." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Google login failed: {ex.Message}" });
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
