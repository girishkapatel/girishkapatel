using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Security;
using AuditManagementCore.Service.UserService;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using VJLiabraries.GenericRepository;
using Wkhtmltopdf.NetCore;

namespace AuditManagementCore.Web
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        { // requires using Microsoft.Extensions.Options

            //  string domain = $"https://{Configuration["Auth0:Domain"]}/";
            string domain = $"https://abc/";

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
            builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
            });
            services.Configure<MongoDbSettings>(
                Configuration.GetSection(nameof(MongoDbSettings)));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Jwt";
                options.DefaultChallengeScheme = "Jwt";
            }).AddJwtBearer("Jwt", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    //ValidAudience = "the audience you want to validate",
                    ValidateIssuer = false,
                    //ValidIssuer = "the isser you want to validate",

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the secret that needs to be at least 16 characeters long for HmacSha256")),

                    ValidateLifetime = true, //validate the expiration and not before values in the token

                    ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
                };
            });

            services.AddAuthorization();


            services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
            services.AddTransient(typeof(IMongoGenericRepository<>), typeof(MongoGenericRepository<>));
            services.AddTransient(typeof(IGenericRepository<>), typeof(MongoGenericRepository<>));
            services.AddSingleton(typeof(IEncryption), typeof(Encryption));
            services.AddSingleton(typeof(IUserService), typeof(UserService));
            services.AddSingleton(typeof(IAuthorizationHandler), typeof(HasScopeHandler));
            services.AddSingleton(typeof(IAuthorizationPolicyProvider), typeof(AuthorizationPolicyProvider));

            services.AddTransient<CommonServices>();
            var uploadPath = Configuration.GetSection("FileLocation");
            services.AddSingleton<IDocumentUpload>(x => new DocumentUpload(uploadPath.Value));


            var mailsetting = Configuration.GetSection("MailSetting").Get<MailSetting>();
            services.AddSingleton<IEmailUtility>(x => new MailUtility(mailsetting));

            var globalConfiguration = Configuration.GetSection("GlobalConfigurationSetting").Get<GlobalConfigurationSetting>();
            services.AddSingleton<IGlobalConfiguration>(x => new GlobalConfiguration(globalConfiguration));
             services.AddWkhtmltopdf("wkhtmltopdf");
             services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("AllowAll");
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
