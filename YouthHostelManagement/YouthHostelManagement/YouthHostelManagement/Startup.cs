using DataAccess.DemoDB;
using ElmahCore.Mvc;
using ElmahCore.Sql;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;
using System;
using System.Globalization;

namespace YouthHostelManagement
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //----------------------------------------------------------------------------------------
            //Code required for session
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromDays(3);
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });
            //Code required for session END
            //----------------------------------------------------------------------------------------

            services.Configure<CookiePolicyOptions>(options =>
            {
                //Auto Generated
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //Code to provide connection string to entity framework
            services.AddDbContext<DemoDBContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("DemoDBConnection"),
                opt => opt.CommandTimeout(900).MigrationsHistoryTable("__MyMigrationsHistory", "dbo")));
            //Code to provide connection string to entity framework END


            // configure basic authentication - only for cookie authentication, not for Identity.
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(option =>
            {
                option.LoginPath = "/Account/Login";
                option.AccessDeniedPath = "/Account/AccessDenied";
            });

            services.AddHttpContextAccessor();

            //This allow to refresh view while running project
            services.AddControllersWithViews().AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = null);


            //ElmahCOre for errorLogging
            services.AddElmah<SqlErrorLog>(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("DemoDBConnection");
            });


            //-------------------------------------------------DI-----------------------------------------------
            //Services
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IUserService, UserService>();
            //-------------------------------------------------DI END-------------------------------------------

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //Code for set culture globally
            //var supportedCultures = new[] { new CultureInfo("es-GB") };
            //app.UseRequestLocalization(new RequestLocalizationOptions
            //{
            //    DefaultRequestCulture = new RequestCulture("es-GB"),
            //    SupportedCultures = supportedCultures,
            //    SupportedUICultures = supportedCultures
            //});

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseElmah();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}
