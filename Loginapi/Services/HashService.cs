using System.Security.Cryptography;
using System.Text;

namespace Loginapi.Services
{
    public class HashService
    {
        public static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            string hashPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashPassword;
        }
    }
}
