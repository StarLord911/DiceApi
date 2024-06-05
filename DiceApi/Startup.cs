using DiceApi.Common;
using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Mappings;
using DiceApi.MiddleWares;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using DiceApi.Services.Implements;
using DiceApi.Services.Jobs;
using DiceApi.Services.SignalRHubs;
using FluentScheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using static DiceApi.Services.Jobs.RouletteJob;

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
            services.AddAutoMapper(typeof(Startup));
            services.AddSingleton<MappingProfile>();
            services.AddSignalR();

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
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Токен авторизации пользователя",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                     }
                });
            });


            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "loyal-dragon-52724.upstash.io:6379,password=Ac30AAIncDE2Yjk4NTRmY2FlODg0YjE1OGFiZWMwYTYyZjM1MjBmMHAxNTI3MjQ,ssl=true"; // Укажите адрес и порт вашего Redis-сервера
                options.InstanceName = "gameCache"; // Опционально. Укажите имя вашего экземпляра Redis
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
            services.AddTransient<IWithdrawalsRepository, WithdrawalsRepository>();
            services.AddTransient<IPaymentRequisitesRepository, PaymentRequisitesRepository>();
            services.AddTransient<ICooperationRequestRepository, CooperationRequestRepository>();
            services.AddTransient<IMinesRepository, MinesRepository>();

            //регаем сервисы
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddSingleton<IPaymentAdapterService, PaymentAdapterService>();
            services.AddTransient<IWithdrawalsService, WithdrawalsService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddTransient<IMinesService, MinesService>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IRouletteService, RouletteService>();
            services.AddTransient<IHorseRaceService, HorseRaceService>();



            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("6829158443:AAFx85c81t7tTZFRtZtU-R0-xpWd-2hlMkg"));

            ConfigHelper.LoadConfig(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DiceApi v1"));
            
            app.UseRouting();

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseMiddleware<JwtMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NewGameHub>(ConfigHelper.GetConfigValue(ConfigerationNames.SignalRHubAddres));
                endpoints.MapHub<OnlineUsersHub>("/onlineusershub");
                endpoints.MapHub<ChatMessagesHub>("/userChatHub");
                endpoints.MapHub<RouletteEndGameHub>("/rouletteGameEndHub");
                endpoints.MapHub<RouletteBetsHub>("/rouletteBetsHub");
                endpoints.MapHub<HorseGameEndGameHub>("/horseGameEndHub");

                endpoints.MapControllers();
            });

            var cache = app.ApplicationServices.GetRequiredService<ICacheService>();
            var userService = app.ApplicationServices.GetRequiredService<IUserService>();
            var hub = app.ApplicationServices.GetRequiredService<IHubContext<RouletteEndGameHub>>();
            var newGameHub = app.ApplicationServices.GetRequiredService<IHubContext<NewGameHub>>();
            var horseGame = app.ApplicationServices.GetRequiredService<IHubContext<HorseGameEndGameHub>>();

            var log = app.ApplicationServices.GetRequiredService<ILogRepository>();

            
            var settingsCache = cache.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY).Result;

            if (settingsCache == null)
            {
                cache.WriteCache(CacheConstraints.SETTINGS_KEY, new Settings
                {
                    DiceAntiminus = 10000,
                    MinesAntiminus = 10000,
                    PaymentActive = true,
                    TechnicalWorks = false,
                    WithdrawalActive = true
                }).RunSynchronously();
            }

            JobManager.Initialize(new RouletteJob(cache, userService, hub, newGameHub));
            JobManager.Initialize(new HorseRaceJob(cache, userService, horseGame, newGameHub));
        }
    }
}