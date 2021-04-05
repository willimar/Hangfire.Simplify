using Hangfire;
using Hangfire.Simplify.Entities;
using Hangfire.Simplify.Services;
using Hangfire.States;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Worker
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
            services.AddControllers();
            services.ConfigureHangfireService(new ConfigureOptions()
            {
                ConnnectionString = "mongodb://localhost:27017/e-stock?readPreference=primary&appname=MongoDB%20Compass&ssl=false",
                HangfireServerName = new List<string>() { "E-Mail [Worker]" },
                MongoPrefix = "hangfire.worker"
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IBackgroundJobClient backgroundJobs, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.ConfigureHangfireApplication(true);
            app.SetupHangfireServers();
            app.RemoveUnusedHangfireServers();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var a = BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget jobs - Inicia imediatamente o processo que quer executar."));
            var b = BackgroundJob.Schedule(() => Console.WriteLine("Deleyed jobs - Faz agendamento do processo a ser executado."), TimeSpan.FromMinutes(3));
            RecurringJob.AddOrUpdate(() => Console.WriteLine("Recurring jobs - Faz agendamento recorrente do processo"), "*/1 * * * *"); // https://en.wikipedia.org/wiki/Cron
            BackgroundJob.ContinueJobWith(a, () => Console.WriteLine("Continuations jobs - Faz com que o job seja executado novamente pelo id da execução."));
        }
    }
}
