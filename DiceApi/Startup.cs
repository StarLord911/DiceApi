using DiceApi.DataAcces.Repositoryes;
using DiceApi.MiddleWares;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using DiceApi.Services.Implements;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiceApi
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
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DiceApi", Version = "v1" });
            });

            services.AddTransient<ILogRepository, LogRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IDiceService, DiceService>();
            services.AddTransient<IDiceGamesRepository, DiceGamesRepository>();
            services.AddTransient<IPromocodeService, PromocodeService>();
            services.AddTransient<IPromocodeRepository, PromocodeRepository>();
            services.AddTransient<IPromocodeActivationHistory, PromocodeActivationHistory>();
            services.AddTransient<IWageringRepository, WageringRepository>();

            //регаем репы.
            services.AddTransient<IPaymentRepository, PaymentRepository>();


            //регаем сервисы
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IPaymentAdapterService, PaymentAdapterService>();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DiceApi v1"));
            }
            app.UseRouting();

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseMiddleware<JwtMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
