using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ImageGallery.Client2.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ImageGallery.Client2
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            //Add an authorization Policy
            services.AddAuthorization(authorization => {
                authorization.AddPolicy(
                    "CanOrderFrame",
                    policyBuilder => {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.RequireClaim("country", "be");
                        policyBuilder.RequireClaim("subscriptionlevel", "PayinUser");
                    });
            });

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            

            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationScheme = "Cookies",
                AccessDeniedPath = "/Authorization/AccessDenied"
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions {
                AuthenticationScheme = "oidc",
                Authority = "https://localhost:44386/",
                RequireHttpsMetadata = true,
                ClientId = "imagegalleryclient",
                Scope = { "openid", "profile","address","roles", "imagegalleryapi", "subscriptionlevel", "country" },
                ResponseType = "code id_token",
                //CallbackPath = new PathString("...")
                SignInScheme = "Cookies",
                SaveTokens = true,
                ClientSecret = "password",
                GetClaimsFromUserInfoEndpoint = true,
                Events = new OpenIdConnectEvents() {
                     
                    OnTokenValidated = tokenValidatedContext =>
                    {
                        var identity = tokenValidatedContext.Ticket.Principal.Identity as ClaimsIdentity;

                        var SujectClaim = identity.Claims.FirstOrDefault(z => z.Type == "sub");

                        var newClaimsIdentity = new ClaimsIdentity(
                            tokenValidatedContext.Ticket.AuthenticationScheme,
                            "give_name",
                            "role"
                            );


                        newClaimsIdentity.AddClaim(SujectClaim);

                        tokenValidatedContext.Ticket = new AuthenticationTicket(
                            new ClaimsPrincipal(newClaimsIdentity),
                            tokenValidatedContext.Ticket.Properties,
                            tokenValidatedContext.Ticket.AuthenticationScheme);

                        return Task.FromResult(0);
                    },

                    OnUserInformationReceived = userinformationReceivedContext =>
                    {
                        userinformationReceivedContext.User.Remove("address");
                        return Task.FromResult(0);
                    }
                }
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
