﻿using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using P2FixAnAppDotNetCode.Models;
using P2FixAnAppDotNetCode.Models.Repositories;
using P2FixAnAppDotNetCode.Models.Services;

namespace P2FixAnAppDotNetCode
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
            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
            services.AddScoped<ICart, Cart>();
            services.AddSingleton<ILanguageService, LanguageService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddMemoryCache();
            services.AddSession();
            //services.AddScoped<ICart>(sp => Cart.GetCart(sp.GetService<IHttpContextAccessor>().HttpContext.Session));
            services.AddScoped<ICart>(sp =>
            {
                var session = sp.GetService<IHttpContextAccessor>().HttpContext.Session;
                var productService = sp.GetService<IProductService>();
                return Cart.GetCart(session, productService); // Initialiser Cart avec la session
            });

            services.AddMvc()
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(opts =>
            { 
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en-GB"),
                    new CultureInfo("en-US"),
                    new CultureInfo("en"),
                    new CultureInfo("fr-FR"),
                    new CultureInfo("fr"),
                };

                opts.DefaultRequestCulture = new RequestCulture("en");
                // Formatting numbers, dates, etc.
                opts.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                opts.SupportedUICultures = supportedCultures;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Product}/{action=Index}/{id?}");
            });
        }
    }
}
