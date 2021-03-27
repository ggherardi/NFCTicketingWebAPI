using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NFCTicketingWebAPI.Middleware
{
    public static class ExceptionHandlerMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(application =>
            {
                application.Run(async ctx =>
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    ctx.Response.ContentType = "application/json";
                    IExceptionHandlerFeature exceptionHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                    if(exceptionHandlerFeature != null)
                    {
                        await ctx.Response.WriteAsync("Unhandled exception");
                    }
                });
            });
        }
    }
}
