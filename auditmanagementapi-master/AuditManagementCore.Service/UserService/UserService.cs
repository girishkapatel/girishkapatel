using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.MongoDb.IUnitOfWork;
using AuditManagementCore.Service.Security;
using System.Collections.Generic;
using System.Linq;
using VJLiabraries.GenericRepository;

namespace AuditManagementCore.Service.UserService
{
    public class UserService : IUserService
    {
        IMongoDbSettings _dbsetting;
        public IEncryption _encryption;

        public UserService(IMongoDbSettings dbsetting, IEncryption encryption)
        {
            _dbsetting = dbsetting;
            _encryption = encryption;
        }

        public List<User> GetUsers()
        {
            var _userRepo = new MongoGenericRepository<User>(_dbsetting);
            return _userRepo.GetAllWithInclude<Role>().ToList();
        }


        public User ValidateUser(string userName, string password)
        {
            var _userRepo = new MongoGenericRepository<User>(_dbsetting);
            var user = _userRepo.GetWithInclude<Role>(ur => ur.EmailId.ToLower() == userName.ToLower() && ur.IsActive == true).FirstOrDefault();
            if (user != null)
            {
                var encyPassword = _encryption.Encrypt(password);
                if (encyPassword == user.Password)
                {
                    return user;
                }
            }
            return null;
        }
        public User ValidateEmail(string userName)
        {
            var _userRepo = new MongoGenericRepository<User>(_dbsetting);
            var user = _userRepo.GetWithInclude<Role>(ur => ur.EmailId.ToLower() == userName.ToLower() && ur.IsActive == true).FirstOrDefault();
            if (user != null)
            {
                var decPassword = _encryption.Decrypt(user.Password);
                user.Password = decPassword;
                return user;
            }
            return null;
        }
        public User UpdateValidateResetUser(string userName, string password)
        {
            var _userRepo = new MongoGenericRepository<User>(_dbsetting);
            var user = _userRepo.GetWithInclude<Role>(ur => ur.Id== userName && ur.IsActive == true).FirstOrDefault();
            if (user != null)
            {
                var encyPassword = _encryption.Encrypt(password);
                user.Password = encyPassword;
                _userRepo.Update(user);
                return user;
            }
            return null;
        }
    }
}
