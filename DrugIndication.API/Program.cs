using DrugIndication.Application.Services;
using DrugIndication.Domain.Config;
using DrugIndication.Domain.Interfaces;
using DrugIndication.Infrastructure;
using DrugIndication.Infrastructure.Data;
using DrugIndication.Infrastructure.Repositories;
using DrugIndication.Parsing.Interfaces;
using DrugIndication.Parsing.Services;
using DrugIndication.Parsing.Transformers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


namespace DrugIndication.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter a valid JWT token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Config OpenAI options
            builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));

            // Config Mongo
            var mongoConfig = builder.Configuration.GetSection("Mongo");
            var mongoContext = new MongoDbContext(
                mongoConfig["ConnectionString"],
                mongoConfig["Database"]
            );

            builder.Services.AddSingleton(mongoContext);
            builder.Services.AddSingleton<ProgramTransformer>();
            builder.Services.AddSingleton<ProgramRepository>();
            builder.Services.AddSingleton<UserRepository>();
            builder.Services.AddSingleton<AuthService>();

            // Local ICD-10 service (needs CSV path)
            var icdCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icd10_codes.csv");
            builder.Services.AddSingleton(new Icd10Service(icdCsvPath));

            // Mapping service that uses both
            builder.Services.AddSingleton<IIcd10MappingService, Icd10MappingService>();
            builder.Services.AddSingleton<IOpenAiService, OpenAiService>();

            //Authorization
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

            var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtOptions.Issuer,
                            ValidAudience = jwtOptions.Audience,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtOptions.Key))
                        };
                    });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
