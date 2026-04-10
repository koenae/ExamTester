using ExamTester.Components;
using ExamTester.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ExamService>();
builder.Services.AddScoped<TimerService>();
builder.Services.AddSingleton<PersistenceService>();
builder.Services.AddSingleton<ExamCatalogService>();
builder.Services.AddScoped<LlmService>();
builder.Services.AddScoped<ExamGeneratorService>();
builder.Services.AddScoped<AnalyticsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
