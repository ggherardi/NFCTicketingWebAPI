using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketingWebAPI
{
    public interface IAuthenticationManager
    {
        bool Authenticate(DbContext context, string username, string password);
        string GenerateToken(string username);
    }

    public class SmartTicketAuthenticationManager : IAuthenticationManager
    {
        private readonly string _connectionString;
        private readonly RsaSecurityKey _key;

        public SmartTicketAuthenticationManager(RsaSecurityKey key, string sqlConnectionString)
        {
            _connectionString = sqlConnectionString;
            _key = key;
        }

        public bool Authenticate(DbContext context, string username, string password)
        {
            return (context as NFCValidationStorageContext)?.SmartTicketUsers.FirstOrDefault(u => u.Username == username && u.Password == /*remember to add the encryption code here, after making the registration method*/ password) != null;
        }

        public string GenerateToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, username) }), 
                Expires = DateTime.UtcNow.AddHours(1), 
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
