using API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace API
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            services.AddHttpClient<TestClientService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:55000/WeatherForecast");
            });

            services.AddSingleton(CircuitBreaker.CreatePolicy());


            //.SetHandlerLifetime(TimeSpan.FromMinutes(5))
            //.AddPolicyHandler(GetRetryPolicy())
            //.AddPolicyHandler(GetCircuitBreakerPolicy());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            //return HttpPolicyExtensions
            //.HandleTransientHttpError()
            //.CircuitBreakerAsync(3, TimeSpan.FromMinutes(1));

            /*
             * the circuit will be cut if 25% of requests fail in a 60 second window, with a minimum of 7 requests in the 60 second window, then the circuit should be cut for 30 seconds:
             * */

            return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .AdvancedCircuitBreakerAsync(0.25, TimeSpan.FromSeconds(60), 7, TimeSpan.FromSeconds(30));
        }


        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10));
        }
    }
}
