using FacturasSRI.Web.Components;
using FacturasSRI.Web.Handlers;
using FacturasSRI.Web.Services;
using FacturasSRI.Web.States;
using System;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<CurrentUserState>(); 

builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHttpClient("SRI_API", client =>
{
    client.BaseAddress = new Uri("http://localhost:5092");
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SRI_API"));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();