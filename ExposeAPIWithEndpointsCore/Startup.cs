using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using ExposeAPIWithEndpointsCore.Models;
using System.IO;
using System.Data.Common;
using System.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography.X509Certificates;
using ExposeAPIWithEndpointsCore.eslabs;
using Microsoft.EntityFrameworkCore;

namespace ExposeAPIWithEndpointsCore
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
            services.AddMvc();

          //  services.Add(new ServiceDescriptor(typeof(ContainerContext), new ContainerContext(Configuration.GetConnectionString("IsgecDb"))));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new Info { Title = "My API", Version = "v1" });
            });

            // services.AddSingleton(typeof(DbConnection), (IServiceProvider) =>
            // InitializeDatabase());

            // services.AddDbContext<IronManContext>(options =>
            // options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));
            var connectionString = new MySqlConnectionStringBuilder(
                Configuration["CloudSql:ConnectionString"])
            {
                // Connecting to a local proxy that does not support ssl.
                SslMode = MySqlSslMode.None,
            };

            services.AddDbContext<eslabsContext>(options =>
        options.UseMySql(connectionString.ConnectionString));


        }

        DbConnection InitializeDatabase()
        {
            DbConnection connection;
            connection = NewMysqlConnection();
            connection.Open();
            using (var createTableCommand = connection.CreateCommand())
            {
                createTableCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS
                    visits (
                        time_stamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        user_ip CHAR(64)
                    )";
                createTableCommand.ExecuteNonQuery();
            }
            return connection;
        }

        DbConnection NewMysqlConnection()
        {
            // [START gae_flex_mysql_env]
            var connectionString = new MySqlConnectionStringBuilder(
                Configuration["CloudSql:ConnectionString"])
            {
                // Connecting to a local proxy that does not support ssl.
                SslMode = MySqlSslMode.None,
            };
            DbConnection connection =
                new MySqlConnection(connectionString.ConnectionString);
            // [END gae_flex_mysql_env]
            return connection;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            //Redirect non api calls to angular app that will handle routing of the app.    
            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value) && !context.Request.Path.Value.StartsWith("/api/"))
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });
            // configure the app to serve index.html from /wwwroot folder    
            app.UseDefaultFiles();
            app.UseStaticFiles();
            // configure the app for usage as api    
            app.UseMvcWithDefaultRoute();
        }
    }
}
