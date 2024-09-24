using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Winning;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Mappings;
using DiceApi.MiddleWares;
using DiceApi.Services;
using DiceApi.Services.BackgroundServices;
using DiceApi.Services.Contracts;
using DiceApi.Services.Implements;
using DiceApi.Services.Jobs;
using DiceApi.Services.SignalRHubs;
using EasyMemoryCache.Configuration;
using FluentScheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using FakeActiveService = DiceApi.Services.BackgroundServices.FakeActiveService;

namespace DiceApi
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            _env = env;
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

            ConfigHelper.LoadConfig(Configuration);

            if (_env.IsDevelopment())
            {
                services.AddEasyCache(new CacheSettings()
                {
                    CacheProvider = CacheProvider.MemoryCache,
                });
            }
            else
            {
                services.AddEasyCache(new CacheSettings()
                {
                    CacheProvider = CacheProvider.Redis,
                    RedisSerialization = SerializationType.Newtonsoft,
                    RedisConnectionString = "localhost:6379"
                });
            }
            

            services.AddTransient<ILastGamesService, LastGamesService>();
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
            services.AddTransient<IRefferalService, RefferalService>();
            services.AddTransient<IAntiMinusService, AntiMinusService>();

            //services.AddHostedService<RouleteService>();
            //services.AddHostedService<HorsesService>();
            //services.AddHostedService<PaymentConfirmService>();

            //services.AddHostedService<FakeActiveService>();
            //services.AddHostedService<FakeOnlineService>();
            //services.AddHostedService<FakeMinesGameService>();

            services.AddHostedService<FakeChatService>();


            ConfigHelper.LoadConfig(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            if (_env.IsDevelopment())
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
                endpoints.MapHub<LastGamesHub>(ConfigHelper.GetConfigValue(ConfigerationNames.SignalRHubAddres));
                endpoints.MapHub<OnlineUsersHub>("/onlineusershub");
                endpoints.MapHub<ChatMessagesHub>("/userChatHub");

                endpoints.MapHub<RouletteEndGameHub>("/rouletteGameEndHub");
                endpoints.MapHub<RouletteBetsHub>("/rouletteBetsHub");

                endpoints.MapHub<HorseGameEndGameHub>("/horseGameEndHub");
                endpoints.MapHub<HorseGameBetsHub>("/horseGameBetsHub");

                endpoints.MapHub<RouletteGameStartTaimerHub>("/rouletteGamesStartTaimerHub");
                endpoints.MapHub<HorsesGameStartTaimerHub>("/horsesGamesStartTaimerHub");


                endpoints.MapControllers();
            });

            var cache = app.ApplicationServices.GetRequiredService<ICacheService>();

            var settingsCache = cache.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY).Result;

            if (settingsCache == null)
            {
                cache.WriteCache(CacheConstraints.SETTINGS_KEY, new Settings
                {
                    DiceGameWinningSettings = new DiceGameWinningSettings
                    {
                        DiceMinusPercent = 2,
                        DiceAntiminusBallance = 100000
                    },
                    MinesGameWinningSettings = new MinesGameWinningSettings
                    {
                        MinesAntiminusBallance = 100000,
                        MinesMaxMultyplayer = 5,
                        MinesMaxSuccesMineOpens = 10,
                        MinesMaxWinSum = 5000
                    },
                    DiceGameActive = true,
                    MinesGameActive = true,
                    HorseGameActive = true,
                    RouletteGameActive = true,
                    PaymentActive = true,
                    TechnicalWorks = false,
                    WithdrawalActive = true
                }).GetAwaiter().GetResult();
            }

            var stats = cache.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY).Result;

            if (stats == null)
            {
                stats = new WinningStats();

                cache.WriteCache(CacheConstraints.WINNINGS_TO_DAY, stats).GetAwaiter().GetResult();
            }

       
            JobManager.Initialize(new DropWinningsJob(cache));
        }
    }
}