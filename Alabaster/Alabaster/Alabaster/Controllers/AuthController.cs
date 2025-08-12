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

        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
            {
                return Json(new { success = false, error = "No token provided" });
            }

            try
            {
                // Verify the token with Firebase Admin SDK
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                string uid = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;

                // Optional: Create or update user in your DB here based on uid/email

                // Store token and user info in session
                HttpContext.Session.SetString("FirebaseToken", idToken);
                HttpContext.Session.SetString("UserId", uid);
                HttpContext.Session.SetString("UserEmail", email ?? "");

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

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
} 
