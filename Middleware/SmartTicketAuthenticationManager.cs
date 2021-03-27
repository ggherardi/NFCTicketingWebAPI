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
        string GetRole(DbContext context, string username, string password);
        string GenerateToken(string username, string role);
    }

    public class SmartTicketAuthenticationManager : IAuthenticationManager
    {
        private readonly RsaSecurityKey _key;

        public SmartTicketAuthenticationManager(RsaSecurityKey key)
        {
            _key = key;
        }

        public string GetRole(DbContext context, string username, string password)
        {
            /*remember to add the encryption code here, after making the registration method*/
            return (context as NFCValidationStorageContext)?.SmartTicketUsers
                .Include(r => r.RoleNavigation)
                .FirstOrDefault(u => u.Username == username && u.Password == password).RoleNavigation.Name;
        }

        public string GenerateToken(string username, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                { 
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }), 
                Expires = DateTime.UtcNow.AddHours(1), 
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
