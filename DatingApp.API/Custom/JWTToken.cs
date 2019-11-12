using DatingApp.API.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DatingApp.API.Custom
{
    public class JWTToken
    {
        private readonly User _user;
        private readonly string _secret;

        public JWTToken(User user, string secret)
        {
            _user = user;
            _secret = secret;
        }

        public string Build()
        {
            // ... create Claims ...
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()),
                new Claim(ClaimTypes.Name, _user.UserName)
            };

            //... getting my SECRET string from 'AppSettings.json'. _config is IConfiguration from 'Microsoft.Extensions.Configuration' ...
            //var secret = _config.GetSection("Cembo_Settings:Token").Value;
            //... OR string from constructor:
            // ... encoding my string to byte array ...
            var mySecret = Encoding.UTF8.GetBytes(_secret);

            // creating key for my JWT
            var key = new SymmetricSecurityKey(mySecret);

            // ... Server is signing credentials for user into token:
            // ... generate Signing Credentials, encrypting my key using hashing algorythm ...
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // ... Creating TOKEN:
            // ... create Security Token Descriptor containing my Claims, Expire Date and Signing Credentials !!! ...
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            // ... Generating TOKEN:
            // ... generate JWT Token and pass Token Descriptor into it ...
            var tokenHandler = new JwtSecurityTokenHandler();
            var JWT_Token = tokenHandler.CreateToken(tokenDescriptor);

            // ... return TOKEN as an Object to client ...
            // ... user can verify received token at http://jwt.io ...
            return tokenHandler.WriteToken(JWT_Token);

        }

    }
}
