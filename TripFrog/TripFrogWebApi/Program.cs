using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using TripFrogModels;
using TripFrogWebApi;
using TripFrogWebApi.KeyVaultClasses;
using TripFrogWebApi.TokensCreator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<TripFrogContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        b => b.MigrationsAssembly("TripFrogWebApi")));
builder.Services.AddAutoMapper(typeof(Program).Assembly);

AddAzureKeyVault();

string tokenKeySectionName = "JwtTokenSettings:TokenKey";
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration.GetSection(tokenKeySectionName).Value!)),
    ValidateIssuer = false,
    ValidateAudience = false,
};
string tokenKey = builder.Configuration.GetSection(tokenKeySectionName).Value!;
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>(_ => new JwtTokenService(tokenKey, tokenValidationParameters));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "JWT Bearer authorization",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();
    }
);

builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies[CookieKeys.KeyForSavingJwtInCookie];
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Add("Authorization", $"Bearer {token}");
    }

    await next(context);
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

void AddAzureKeyVault()
{
    var keyVaultEndpoint = new Uri(builder.Configuration.GetSection("KeyVault:Url").Value!);
    var tenantId = builder.Configuration.GetSection("KeyVault:TenantId").Value!;
    var clientId = builder.Configuration.GetSection("KeyVault:ClientId").Value!;
    var thumbprint = builder.Configuration.GetSection("KeyVault:Thumbprint").Value!;
    var credential = new ClientCertificateCredential(tenantId, clientId, CertificateReceiver.GetCertificate(thumbprint));
    var client = new SecretClient(keyVaultEndpoint, credential);
    builder.Configuration.AddAzureKeyVault(client, new PrefixKeyVaultSecretManager("TripFrog"));
}
