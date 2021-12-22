using AuditManagementCore.Models;
using System.Collections.Generic;

namespace AuditManagementCore.Service.UserService
{
    public interface IUserService
    {
        List<User> GetUsers();

        User ValidateUser(string userName, string password);

        User ValidateEmail(string userName);
        User UpdateValidateResetUser(string userName, string password);

    }
}
