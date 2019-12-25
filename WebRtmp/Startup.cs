using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Harmonic.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebRtmp
{
    public class RtmpServerOptionsHolder
    {
        public RtmpServerOptions options { get; set; }
    }
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
            RtmpServerOptionsHolder holder = new RtmpServerOptionsHolder();
            services.AddSingleton(holder);
            services.AddControllersWithViews();
            services.AddSingleton(provider =>
            {
                return new RtmpServerBuilder()
                .UseStartup<RtmpStartup>()
                .UseHarmonic(config =>
                {
                    holder.options = config;
                })
                .UseWebSocket(c =>
                {
                    c.BindEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080);
                })
                .Build();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RtmpServer rtmpServer)
        {
            rtmpServer.StartAsync();

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
