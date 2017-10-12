using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using WebApplicationIdentityServer2.Entities;
using WebApplicationIdentityServer2.Services;
using System.Reflection;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;


namespace WebApplicationIdentityServer2
{
    public class Startup
    {
        public static IConfigurationRoot configurationRoot;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            configurationRoot = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var conecctionString = configurationRoot["ConnectionStrings:MarvinUserDBConnectionString"];
            services.AddDbContext<MarvinUserContext>(o => o.UseSqlServer(conecctionString));

            var identityServerDataDBConnectionString =
               configurationRoot["connectionStrings:identityServerDataDBConnectionString"];

            var migrationsAssembly = typeof(Startup)
              .GetTypeInfo().Assembly.GetName().Name;

            services.AddScoped<IMarvinUserRepository, MarvinUserRepository>();

            services.AddMvc();

            services.AddIdentityServer()
            .AddMarvinUserStore()
            .AddTemporarySigningCredential()
            .AddConfigurationStore(builder =>
                builder.UseSqlServer(identityServerDataDBConnectionString, options => options.MigrationsAssembly(migrationsAssembly)))
            .AddOperationalStore(builder => 
                builder.UseSqlServer(identityServerDataDBConnectionString, options => options.MigrationsAssembly(migrationsAssembly)));
              
              
              //.AddTestUsers(Config.GetUsers())
              //.AddInMemoryIdentityResources(Config.GetIdentityResources())
              //.AddInMemoryApiResources(Config.GetApiResource())
              //.AddInMemoryClients(Config.GetClients());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, MarvinUserContext marvinUserContext, 
            ConfigurationDbContext configurationDbContext, PersistedGrantDbContext persistedGrantDbContext)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            configurationDbContext.Database.Migrate();
            configurationDbContext.EnsureSeedDataForContext();

            persistedGrantDbContext.Database.Migrate();

            marvinUserContext.Database.Migrate();
            marvinUserContext.EnsureSeedDataForContext();


            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
