using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditManagementCore.Web.Services
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "scope" ))
                return Task.CompletedTask;
            //check if user have superAdmin Role 
            if(context.User.IsInRole("superadmin"))
                context.Succeed(requirement);

            // Split the scopes string into an array
            var scopes = context.User.FindFirst(c => c.Type == "scope").Value.Split(';');

            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s == requirement.Scope))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
