using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace DiceApi.Middlewares
{
    public class SqlInjectionDetectionMiddleware
    {
        private readonly RequestDelegate _next;

        // Подозрительные выражения, которые могут указывать на SQL-инъекцию
        private readonly List<string> _suspiciousPatterns = new List<string>
        {
            @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|TABLE)\b)",
            @"(--|\#|;)",
            @"(\bUNION\b.+\bSELECT\b)",
            @"(\bEXEC(\s|\+)+(s|x)p\w+)"
        };

        public SqlInjectionDetectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            string requestBody;

            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();

                context.Request.Body.Position = 0;
            }

            if (HasSuspiciousSql(requestBody))
            {
                // Здесь вы можете вести журнал, блокировать запрос или предпринимать другие действия
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("SQL Injection detected.");
                return;
            }

            // Продолжить обработку запроса
            await _next(context);
        }

        private bool HasSuspiciousSql(string input)
        {
            return _suspiciousPatterns.Any(pattern => Regex.IsMatch(input ?? string.Empty, pattern, RegexOptions.IgnoreCase));
        }
    }
}
