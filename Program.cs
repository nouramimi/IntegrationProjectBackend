using Microsoft.EntityFrameworkCore;
using NOTIFICATIONSAPP.Data;
using NOTIFICATIONSAPP.Services;
using NOTIFICATIONSAPP.Services.Implementations;
using NOTIFICATIONSAPP.Services.Interfaces;
using NOTIFICATIONSAPP.Services.Background; 
using NOTIFICATIONSAPP.Repositories.Interfaces;
using NOTIFICATIONSAPP.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IIntegrationRepository, IntegrationRepository>();
builder.Services.AddScoped<IIntegrationCredentialRepository, IntegrationCredentialRepository>();
builder.Services.AddScoped<IIntegrationChannelRepository, IntegrationChannelRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
builder.Services.AddScoped<IDeliveryAttemptRepository, DeliveryAttemptRepository>();
builder.Services.AddScoped<INotificationRouterService, NotificationRouterService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IIntegrationCredentialService, IntegrationCredentialService>();
builder.Services.AddScoped<IIntegrationChannelService, IntegrationChannelService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();
builder.Services.AddScoped<IDeliveryAttemptService, DeliveryAttemptService>();

builder.Services.AddHostedService<WebhookProcessorService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        app.Logger.LogInformation("✅ Database migrated successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Error migrating database");
    }
}
app.Run();
