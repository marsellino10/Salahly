using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Middlewares
{
    //public class ProfileCompletionMiddleware
    //{
    //    private readonly RequestDelegate _next;

    //    public ProfileCompletionMiddleware(RequestDelegate next)
    //    {
    //        _next = next;
    //    }

    //    public async Task InvokeAsync(HttpContext context)
    //    {
    //        var user = context.User;
    //        if (user.Identity?.IsAuthenticated == true)
    //        {
    //            var isCompleted = user.Claims.FirstOrDefault(c => c.Type == "IsProfileCompleted")?.Value;

    //            // if user not completed, block access to specific endpoints
    //            var path = context.Request.Path.Value?.ToLower();
    //            if (isCompleted == "False" && path.Contains("/services")) // example: block service actions
    //            {
    //                context.Response.StatusCode = 403;
    //                await context.Response.WriteAsync("Profile not completed. Please complete your profile first.");
    //                return;
    //            }
    //        }

    //        await _next(context);
    //    }
    //}
    //in program.cs
    //app.UseMiddleware<ProfileCompletionMiddleware>();
    //using authorize policy
    //    services.AddAuthorization(options =>
    //{
    //    options.AddPolicy("ProfileCompleted", policy =>
    //        policy.RequireClaim("IsProfileCompleted", "True"));
    //});
    //this on action
    //[Authorize(Policy = "ProfileCompleted")]
    //[HttpGet("GetAllServices")]
}
