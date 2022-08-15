using DLL.DTOs;
using DLL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private IConfiguration _configuration;

        public UsersService(IConfiguration configuration,
       IOptions<UserStoreDatabaseSettings> userStoreDatabaseSettings)
        {
            _configuration = configuration;

            var mongoClient = new MongoClient(
                userStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                userStoreDatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                userStoreDatabaseSettings.Value.BooksCollectionName);
        }


        public string Authenticate(CredentialsDTO credentials)
        {
            var LoginEmail = _configuration.GetSection("LoginUser:Email").Value;
            var LoginPassword = _configuration.GetSection("LoginUser:Password").Value;

            if (LoginEmail != credentials.Email || LoginPassword != credentials.Password)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _configuration.GetSection("LoginSecret").Value;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                       new Claim(ClaimTypes.NameIdentifier, "1"),

                    new Claim(ClaimTypes.Email, LoginEmail)
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenValue = tokenHandler.WriteToken(token);

            return tokenValue;
        }

        public async Task<List<User>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

    }
}
