using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditManagementCore.Service.Security
{
    public interface IEncryption
    {
        string Encrypt(string clearText);
        string Decrypt(string cipherText);
    }
}
