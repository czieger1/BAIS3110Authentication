using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BAIS3110Authentication
{
    public class Startup
    {
        public static string ConnectionString { get; private set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = new ConfigurationBuilder().SetBasePath(System.IO.Directory.GetCurrentDirectory()).AddJsonFile("appSettings.json").Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(cookieOptions =>
            {
                cookieOptions.LoginPath = "/Login";
                cookieOptions.LogoutPath = "/Admin/Logout";
                cookieOptions.SlidingExpiration = true;
                cookieOptions.AccessDeniedPath = "/Admin/Forbidden";
                cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(2); // Make this smaller for production
                cookieOptions.Cookie.HttpOnly = true;
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdmin", policy =>
                policy.RequireRole("Admin"));
            });
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Common/Admin", "RequireAdmin");
            });        
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                ConnectionString = Configuration["ConnectionStrings:ZiegerDB"];
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
