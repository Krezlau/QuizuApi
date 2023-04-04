using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models;
using QuizuApi.Repository;
using QuizuApi.Repository.IRepository;
using QuizuApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAccessTokenCreatorService, AccessTokenCreatorService>();
builder.Services.AddScoped<IAccessTokenReaderService, AccessTokenReaderService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<QuizuApiDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<QuizuApiDbContext>();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
