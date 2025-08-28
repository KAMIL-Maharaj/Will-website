using Firebase.Auth;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alabaster.Services
{
    public class FirebaseAuthService
    {
        private readonly string apiKey = "AIzaSyBEjxaEf_S9x9NZhmJrWwHLDdZvoyDajDg";
        private readonly FirebaseAuthProvider authProvider;
        private readonly FirebaseAdmin.Auth.FirebaseAuth adminAuth;

        public FirebaseAuthService()
        {
            authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));

            // Initialize Firebase Admin SDK
            if (FirebaseAdmin.FirebaseApp.DefaultInstance == null)
            {
                FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions
                {
                    Credential = GoogleCredential.GetApplicationDefault() // or use a service account JSON file
                });
            }

            adminAuth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
        }

        /// <summary>
        /// Registers a new user with default role "User"
        /// </summary>
        public async Task<FirebaseAuthLink> Register(string email, string password)
        {
            // Create user via Firebase client SDK
            var authLink = await authProvider.CreateUserWithEmailAndPasswordAsync(email, password);

            // Set default role as "User" using Admin SDK
            var userRecord = await adminAuth.GetUserByEmailAsync(email);
            var claims = new Dictionary<string, object> { { "role", "User" } };
            await adminAuth.SetCustomUserClaimsAsync(userRecord.Uid, claims);

            return authLink;
        }

        /// <summary>
        /// Logs in an existing user
        /// </summary>
        public async Task<FirebaseAuthLink> Login(string email, string password)
        {
            var authLink = await authProvider.SignInWithEmailAndPasswordAsync(email, password);
            return authLink;
        }

        /// <summary>
        /// Sends a password reset email
        /// </summary>
        public async Task SendPasswordResetEmail(string email)
        {
            await authProvider.SendPasswordResetEmailAsync(email);
        }

        /// <summary>
        /// Google Sign-In
        /// </summary>
        public async Task<FirebaseAuthLink> SignInWithGoogle(string idToken)
        {
            var authLink = await authProvider.SignInWithOAuthAsync(FirebaseAuthType.Google, idToken);
            return authLink;
        }

        /// <summary>
        /// Create Admin account (use manually)
        /// </summary>
        public async Task CreateAdminAccount(string email, string password)
        {
            var userRecordArgs = new UserRecordArgs
            {
                Email = email,
                Password = password
            };

            var user = await adminAuth.CreateUserAsync(userRecordArgs);

            // Set role as Admin
            var claims = new Dictionary<string, object> { { "role", "Admin" } };
            await adminAuth.SetCustomUserClaimsAsync(user.Uid, claims);
        }

        /// <summary>
        /// Get the role of a user by UID
        /// </summary>
        public async Task<string> GetUserRole(string uid)
        {
            var user = await adminAuth.GetUserAsync(uid);
            if (user.CustomClaims != null && user.CustomClaims.ContainsKey("role"))
                return user.CustomClaims["role"].ToString();

            return "User"; // default role
        }
    }
}
