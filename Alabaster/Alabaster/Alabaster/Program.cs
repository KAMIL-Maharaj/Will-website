using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Alabaster.Services;
using FirebaseAdmin.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register custom Firebase authentication service
builder.Services.AddSingleton<FirebaseAuthService>();

// Enable session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Initialize Firebase Admin SDK once
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile("serviceAccountKey.json"),
    });
}

var app = builder.Build();

// Configure error handling middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();          // Must be before UseAuthorization
app.UseAuthorization();

// Optional: Pre-create Admin account (run once and comment out after)
using (var scope = app.Services.CreateScope())
{
    try
    {
        // Get the admin user by email
        var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync("admin@yourdomain.com");

        // Assign the custom claim "role" = "Admin"
        await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.Uid,
            new Dictionary<string, object> { { "role", "Admin" } });

        Console.WriteLine("Admin role assigned successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Admin role assignment skipped: " + ex.Message);
    }
}


// Default route: redirect to login if no controller/action specified
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
