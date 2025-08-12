using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Alabaster.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register Firebase authentication service (your custom service)
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
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile("serviceAccountKey.json")
    });
}

var app = builder.Build();

// Use development error page or exception handler
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

app.UseSession();
app.UseAuthorization();

// Default route: when no controller/action is specified, go to Auth/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Optional: Remove the explicit redirect if default route works
// app.MapGet("/", () => Results.Redirect("/Auth/Login"));

app.Run();
