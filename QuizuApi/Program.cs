using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Validations;
using QuizuApi.Data;
using QuizuApi.Models;
using QuizuApi.Models.Database;
using QuizuApi.Repository;
using QuizuApi.Repository.IRepository;
using QuizuApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IAccessTokenCreatorService, AccessTokenCreatorService>();
builder.Services.AddScoped<IAccessTokenReaderService, AccessTokenReaderService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuestionAnswerRepository, QuestionAnswerRepository>();
builder.Services.AddScoped<IRepository<QuizLike>, Repository<QuizLike>>();
builder.Services.AddScoped<IRepository<QuizComment>, Repository<QuizComment>>();
builder.Services.AddScoped<IRepository<QuizSettings>, Repository<QuizSettings>>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        o.InvalidModelStateResponseFactory = actionContext => new QuizuBadRequestObjectResult(new ApiResponse(actionContext));
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<QuizuApiDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<QuizuApiDbContext>();

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwt =>
{
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetSection("Key").Value)),
        ValidateIssuer = false, // for dev
        ValidateAudience = false, // for dev
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(name: "CorsPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuizuApi");
    options.RoutePrefix = String.Empty;
});

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
