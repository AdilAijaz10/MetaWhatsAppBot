using MetaWhatsAppBot.Repositories;
using MetaWhatsAppBot.Repositories.Interfaces;
using MetaWhatsAppBot.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Repositories
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();

// ✅ Services
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();