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
        bool Authenticate(string username, string password);
        string GenerateToken(string username);
    }

    public class SmartTicketAuthenticationManager : IAuthenticationManager
    {
        private readonly string _connectionString;
        private readonly string _key;
        public IDictionary<string, string> tokens = new Dictionary<string, string>();

        public SmartTicketAuthenticationManager(string key, string sqlConnectionString)
        {
            _connectionString = sqlConnectionString;
            _key = key;
        }

        public bool Authenticate(string username, string password)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                string usernameParamater = "@username";
                string passwordParameter = "@password";
                string commandString = $"SELECT TOP 1 user_id FROM SmartTicketUser WHERE username = {usernameParamater} AND password = {passwordParameter}";
                SqlCommand command = new SqlCommand(commandString, connection);
                command.Parameters.AddWithValue(usernameParamater, username);
                command.Parameters.AddWithValue(passwordParameter, password);
                SqlDataReader reader = command.ExecuteReader();
                if(!reader.HasRows)
                {
                    throw new Exception("User not found.");
                }
                connection.Close();
            }
            catch (Exception) 
            {
                return false;
            }
            return true;
        }

        public string GenerateToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, username) }), 
                Expires = DateTime.UtcNow.AddHours(1), 
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(new RSACryptoServiceProvider(2048)), SecurityAlgorithms.RsaSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
