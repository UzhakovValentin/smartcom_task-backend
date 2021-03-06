using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Smartcom.WebApp.Database;
using Smartcom.WebApp.Models;
using Smartcom.WebApp.Services;
using Smartcom.WebApp.Services.Intefaces;
using Smartcom.WebApp.UnitOfWork;
using Smartcom.WebApp.UnitOfWork.Interface;

namespace Smartcom.WebApp
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
#if DEBUG
            services.AddDbContext<AppDataBaseContext>(config =>
                config.UseSqlServer(Configuration.GetConnectionString("SmartComLocalDB"),
                options => options.MigrationsAssembly("Smartcom.WebApp")));
#else
            services.AddDbContext<AppDataBaseContext>(config =>
                config.UseNpgsql(Configuration.GetConnectionString("PostgresDB"),
                options => options.MigrationsAssembly("Smartcom.WebApp")));
#endif
            services.AddIdentity<Customer, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 0;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<AppDataBaseContext>();

            services.AddMvc(config => config.EnableEndpointRouting = false);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, config =>
                {
                    config.LoginPath = new PathString("/authentication/login");
                    config.ExpireTimeSpan = TimeSpan.FromDays(2);
                });

            services.AddAuthorization();

            services.AddScoped<IRepositoriesManager, RepositoriesManager>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IPasswordGenerator, PasswordGenerator>();
            services.AddSingleton<ICustomerCodeGenerator, CustomerCodeGenerator>();
            services.AddSingleton<IShoppingCartService<Order>, ShoppingCartService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMvc();
        }
    }
}