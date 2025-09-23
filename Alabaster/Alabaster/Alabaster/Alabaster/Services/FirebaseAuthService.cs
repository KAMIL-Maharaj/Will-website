using Firebase.Auth;
using System.Threading.Tasks;

namespace Alabaster.Services
{
    public class FirebaseAuthService
    {
        private readonly string apiKey = "AIzaSyBEjxaEf_S9x9NZhmJrWwHLDdZvoyDajDg";
        private readonly FirebaseAuthProvider authProvider;

        public FirebaseAuthService()
        {
            authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        }

        public async Task<FirebaseAuthLink> Register(string email, string password)
            => await authProvider.CreateUserWithEmailAndPasswordAsync(email, password);

        public async Task<FirebaseAuthLink> Login(string email, string password)
            => await authProvider.SignInWithEmailAndPasswordAsync(email, password);

        public async Task SendPasswordResetEmail(string email)
            => await authProvider.SendPasswordResetEmailAsync(email);

        public async Task<FirebaseAuthLink> SignInWithGoogle(string idToken)
            => await authProvider.SignInWithOAuthAsync(FirebaseAuthType.Google, idToken);
    }
}
