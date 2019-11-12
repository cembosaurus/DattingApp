using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            var userInDb = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.UserName == username);
                
            if(userInDb == null)
                return null;

            if (!VerifyPassword(password, userInDb.PasswordHash, userInDb.PasswordSalt))
                return null;

            return userInDb;
        }
    

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExist(string username)
        {
            if(await _context.Users.AnyAsync(u => u.UserName == username))
                return true;

            return false;
        }



        //..........................................................................

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            byte[] passwordByteArray = Encoding.UTF8.GetBytes(password);

            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(passwordByteArray);
            }
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            var passwordByteArray = Encoding.UTF8.GetBytes(password);

            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedPasswordHash = hmac.ComputeHash(passwordByteArray);

                for(int i = 0; i < passwordHash.Length; i++)
                    if(passwordHash[i] != computedPasswordHash[i])
                        return false;
            }

            return true;
        }
    }
}