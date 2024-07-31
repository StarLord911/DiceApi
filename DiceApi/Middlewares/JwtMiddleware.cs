using DiceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiceApi.MiddleWares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        //private readonly ILogger _logger;

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await AttachUserToContext(context, userService, token);
            }

            await _next(context);
        }

        public async Task AttachUserToContext(HttpContext context, IUserService userService, string token)
        {
            
            var tokenHandler = new JwtSecurityTokenHandler();
            // min 16 characters
            var key = Encoding.ASCII.GetBytes("220fd41d670cecc84c27b91f8fac94bf0189d0299a219035a6a1ca9542cba9a3");
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

            var isCorrectUser = await IsUserCorrect(context, userId);

            if (!isCorrectUser)
            {
                throw new BadHttpRequestException("Incorrect JWT token information");
            }

            context.Items["User"] = userService.GetById(userId);
        }

        private async Task<bool> IsUserCorrect(HttpContext context, long userIdFromJwt)
        {
            context.Request.EnableBuffering(); // Включаем буферизацию

            string requestBody;

            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync(); // Считываем тело запроса как строку

                // Сбрасываем положение потока в начало для последующего использования
                context.Request.Body.Position = 0;
            }

            // Парсим строку JSON
            using (var jsonDocument = JsonDocument.Parse(requestBody))
            {
                // Проверяем наличие и значение userId
                if (jsonDocument.RootElement.TryGetProperty("userId", out JsonElement userIdElement))
                {
                    var userId = userIdElement.GetInt64();
                    return userId == userIdFromJwt;
                }
            }

            // Если ключ userId не найден или не равен userIdFromJwt, возвращаем false
            return true;
        }
    }
}
