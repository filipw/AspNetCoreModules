﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Modules;
using System.Diagnostics;
using Microsoft.AspNetCore.ViewTemplates;
using Microsoft.AspNetCore.Modules.Mvc;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddViewTemplates();

            services.AddModules(options => 
            {
                options.PathBase["Module1"] = "/Module1";
            });

            services.AddMvc().ConfigureApplicationPartManager(partManager =>
            {
                var modules = new string[] { "Module1", "Microsoft.AspNetCore.Identity.Module" };
                foreach (var module in modules)
                {
                    var modulePart = partManager.ApplicationParts.FirstOrDefault(part => part.Name == module);
                    if (modulePart != null) partManager.ApplicationParts.Remove(modulePart);
                }
            });

            services.AddMiddlewareAnalysis();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DiagnosticListener diagnosticListener)
        {
            diagnosticListener.SubscribeWithAdapter(new TestDiagnosticListener());

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Information);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseModules();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}