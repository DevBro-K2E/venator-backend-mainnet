using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Data.Models.Configuration;
using IsometricShooterWebApp.Managers;
using IsometricShooterWebApp.Managers.Abstraction;
using IsometricShooterWebApp.Utils;
using IsometricShooterWebApp.Utils.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace IsometricShooterWebApp
{
    public class Program
    {

        private const string CorsDefPolicyName = "AllowAllPolicy";
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.AddMemoryLogger();

            builder.Services.Configure<JWTConfigurationOptions>(builder.Configuration.GetSection(JWTConfigurationOptions.ConfigurationPath));

            builder.Services.AddSingleton<AzureContainerInstancesManager>();
            builder.Services.AddSingleton<IEmailSender, EmailSenderProvider>();
            builder.Services.AddSingleton<IBlockchainManager, BlockchainManager>();
            builder.Services.AddSingleton<RefereeManager>();

            builder.Services.AddSingleton<UserManager>();
            builder.Services.AddSingleton<GameManager>();
            builder.Services.AddSingleton<SeasonManager>();

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, c => c.UseNodaTime()));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<UserModel>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthentication()
                .AddJwtBearer(GameApiAuthorizeAttribute.Scheme, options =>
                {
                    var configuration = builder.Configuration.GetSection(JWTConfigurationOptions.ConfigurationPath).Get<JWTConfigurationOptions>();

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration.Issuer,
                        ValidAudience = configuration.Audience,
                        IssuerSigningKey = configuration.GetSecutiryKey()
                    };

                })
            .AddCookie("ApiCookie", options =>
            {
                options.LoginPath = string.Empty;
                options.AccessDeniedPath = string.Empty;

                options.SlidingExpiration = true;
                options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
                
                options.Cookie.SameSite = SameSiteMode.None;
                //options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                options.Events.OnSigningOut = context =>
                {
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
            });


            builder.Services.AddMvc((options) =>
            {
                options.ModelBinderProviders.Insert(0, new CustomBinderProvider());
            });

            builder.Services.AddControllers();

            builder.Services.AddRazorPages();

            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddCors(s =>
            {
                s.AddPolicy(CorsDefPolicyName, b => b.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed(origin=> true));
            });

            var app = builder.Build();

            app.UseHttpLogging();

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            using (var scope = app.Services.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

                var containerManager = scope.ServiceProvider.GetRequiredService<AzureContainerInstancesManager>();

                containerManager.Initialize();

#if DEBUG
                //await scope.ServiceProvider.GetRequiredService<SeasonManager>().GetStatistics(
                //    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(), 
                //    scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>(), 
                //    new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, "05291f69-b2f6-4a18-bff6-93de50f7cd4c") })), Data.Models.Enums.StatisticsTypeEnum.Day);

                //string se = "abc@dev.dev";
                //string up = "000000^a";

                //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

                //var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<UserModel>>();
                //var emailStore = userStore as IUserEmailStore<UserModel>;

                //for (int i = 0; i < 50; i++)
                //{
                //    var user = new UserModel();

                //    var uname = $"{i}_{se}";
                //    await userStore.SetUserNameAsync(user, uname, CancellationToken.None);
                //    await emailStore.SetEmailAsync(user, uname, CancellationToken.None);
                //    var result = await userManager.CreateAsync(user, up);
                //    if (result.Succeeded)
                //    {
                //        user.EmailConfirmed = true;

                //        await userManager.UpdateAsync(user);
                //    }
                //    else
                //    {
                //        throw new Exception(string.Join("\r\n", result.Errors.Select(x => x.Description)));
                //    }
                //}






#endif
            }

#if RELEASE

            app.Services.GetRequiredService<IBlockchainManager>().InitializeBinding();


#endif
            app.Services.GetRequiredService<SeasonManager>().Initialize();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(CorsDefPolicyName);

            app.UseSwagger();
            app.UseSwaggerUI();

#if RELEASE
            app.UseHttpsRedirection();
#endif
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.MapControllers();

            app.MapGet($"/dev_log/{app.Configuration.GetValue("logging:memory:key", "qsb53fhre")}", c =>
            {
                var lp = c.RequestServices.GetRequiredService<ILoggerProvider>();

                var logs = (lp as MemoryLoggerProvider).GetLogs();

                c.Response.ContentType = "text/html";
                c.Response.WriteAsync(string.Join("<br>", logs));

                return Task.CompletedTask;
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger-ui";
            });

            app.Run();
        }
    }
}