using BlazorProject.Components;
using BlazorProject.Data;
using BlazorProject.Data.Models;
using Microsoft.EntityFrameworkCore;
using BlazorProject.Services;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("No connection string found in config!!");
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<ConsultationStore>();

builder.Services.AddDbContextFactory<EiEngsofContext>((DbContextOptionsBuilder options) => options.UseNpgsql(connectionString));
builder.Services.AddScoped<UtilizadorService>();
builder.Services.AddScoped<ExameMedico>();
var app = builder.Build();

// ── DB Connection Test ──────────────────────────────────────────────────────
try
{
    var dbFactory = app.Services.GetRequiredService<IDbContextFactory<EiEngsofContext>>();
    await using var dbCtx = await dbFactory.CreateDbContextAsync();

    bool canConnect = await dbCtx.Database.CanConnectAsync();
    if (canConnect)
    {
        int utilizadores = await dbCtx.Utilizadors.CountAsync();
        int pacientes    = await dbCtx.Pacientes.CountAsync();
        int consultas    = await dbCtx.Consulta.CountAsync();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✔  Base de dados ligada com sucesso!");
        Console.WriteLine($"   → Utilizadores: {utilizadores}  |  Pacientes: {pacientes}  |  Consultas: {consultas}");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("✘  Não foi possível ligar à base de dados.");
        Console.WriteLine($"   → Connection string: {connectionString}");
        Console.ResetColor();
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"✘  Erro ao ligar à base de dados: {ex.Message}");
    Console.WriteLine($"   → Tipo: {ex.GetType().FullName}");
    if (ex.InnerException is not null)
        Console.WriteLine($"   → Inner: {ex.InnerException.Message}");
    Console.WriteLine($"   → StackTrace:\n{ex.StackTrace}");
    Console.ResetColor();
}
// ───────────────────────────────────────────────────────────────────────────

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
