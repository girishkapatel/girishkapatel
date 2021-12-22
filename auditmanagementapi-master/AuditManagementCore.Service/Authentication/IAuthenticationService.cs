using System;
using System.Threading.Tasks;
using AuditManagementCore.Models;

namespace AuditManagementCore.Service.Authentication
{
    public interface IAuthenticationService
    {
        string Token(User user);
        void SignOut();
    }
}
