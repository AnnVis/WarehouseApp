using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using StorageManager;
using StorageManager.App_Start;
using System;
using System.Configuration;
using System.Text;
using System.Web.Http;
using System.Web.Http.Owin;

[assembly: OwinStartup(typeof(StorageManager.StorageManager.Startup))]

namespace StorageManager.StorageManager
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Read JWT settings from Web.config appSettings
            var issuer = ConfigurationManager.AppSettings["Jwt:Issuer"];
            var audience = ConfigurationManager.AppSettings["Jwt:Audience"];
            var secret = ConfigurationManager.AppSettings["Jwt:Key"] ?? string.Empty;
            var key = Encoding.UTF8.GetBytes(secret);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = System.TimeSpan.FromMinutes(5)
            };

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = tokenValidationParameters
            });

            // Configure Web API to run on OWIN
            var config = new HttpConfiguration();

            // If you already have WebApiConfig.Register, call it so your routes/controllers are registered:
            WebApiConfig.Register(config);

            //Autofac DI container setup
            AutofacConfig.Register(config);

            // JSON defaults
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            app.UseWebApi(config);
        }
    }
}