using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using POCForVivek.Common;
using POCForVivek.Common.Identity;
using POCForVivek.Models;
using Raven.Client.Documents;
using Raven.DependencyInjection;
using Raven.Identity;
using System;
using System.Collections.Generic;

namespace POCForVivek
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


            services.AddMvc(/*options => options.Filters.Add(new AuthorizeFilter())*/).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //services.AddScoped<CustomUserValidator<A2ZClientCredentials>>();

            // Grab our RavenSettings object from appsettings.json.
            //services.Configure<RavenSettings>(Configuration.GetSection("RavenSettings"));

            // Add RavenDB and identity.
            services
                .AddRavenDbDocStore() // Create an IDocumentStore singleton from the RavenSettings.
                .AddRavenDbAsyncSession() // Create a RavenDB IAsyncDocumentSession for each request. You're responsible for calling .SaveChanges after each request.
                .AddIdentity<A2ZClientCredentials, Raven.Identity.IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = false;
                    //options.Password.RequiredLength = 8;
                    //options.Password.RequireNonAlphanumeric = false;
                    //options.Password.RequireLowercase = true;
                    //options.Password.RequireUppercase = false;
                    //options.Password.RequireDigit = true;

                }) // Adds an identity system to ASP.NET Core
                .AddUserManager<CustomUserManager<A2ZClientCredentials, IDocumentStore>>()
                .AddUserStore<RavenDBUserStore<A2ZClientCredentials, IDocumentStore>>()
                //.AddClaimsPrincipalFactory<AppClaimsPrincipleFactory>()
                .AddSignInManager<AppSignInManager>()
                .AddRavenDbIdentityStores<A2ZClientCredentials>(options => options.UserIdType = UserIdType.ServerGenerated); // Use RavenDB as the store for identity users and roles.

            services.AddTransient<IUserValidator<A2ZClientCredentials>, CustomUserValidator<A2ZClientCredentials>>();
            services.AddTransient<ICustomUserValidator<A2ZClientCredentials>, CustomUserValidator<A2ZClientCredentials>>();
            services.AddTransient<IRavenDBUserStore<A2ZClientCredentials>, RavenDBUserStore<A2ZClientCredentials,IDocumentStore>>();
            services.AddTransient<ICustomPasswordValidator<A2ZClientCredentials>, CustomPasswordValidator<A2ZClientCredentials>>();
            //services.AddTransient<IUserClaimsPrincipalFactory<A2ZClientCredentials>, AppClaimsPrincipleFactory>();

            services.AddHttpContextAccessor();


            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddControllersWithViews();
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            //services.AddSingleton<IDocumentStoreHolder, DocumentStoreHolder>();
            //services.AddSingleton(DocumentStoreHolder.Store);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                                        options =>
                                        {
                                            options.LoginPath = "/Account/Login";
                                            options.LogoutPath = "/Account/Logout";
                                            options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                                            options.SlidingExpiration = true;
                                        });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ClientAccess", policy => {
                    policy.RequireRole("Client");
                });

                //options.FallbackPolicy = new AuthorizationPolicyBuilder()
                //    .RequireAuthenticatedUser()
                //    .Build();
            });


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
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            var docStore = app.ApplicationServices.GetRequiredService<IDocumentStore>();
            docStore.EnsureExists();
            app.UseCookiePolicy();

            // Instruct ASP.NET Core to use authentication and authorization.
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
