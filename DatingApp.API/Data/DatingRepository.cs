using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);

        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == id) ;
        }

        public async Task<User> GetUser(int id)
        {

            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);

            return user;

        }


        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {

            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);

                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);

                users = users.Where(u => userLikees.Contains(u.Id));
            }


            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            // ... OR create orderBy function and add to expression tree:
            //Func<User, DateTime> f = (u) => { return u.Created; };

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    case "dob":
                        users = users.OrderByDescending(u => u.DateOfBirth);
                        break;
                    case "username":
                        users = users.OrderByDescending(u => u.UserName);
                        break;
                    case "id":
                        users = users.OrderByDescending(u => u.Id);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            // ... PagedList.CreatyeAsync() - static method to load IQueryable source (users) and create instance of PagedList with List<T> base class
            var pagedUsers = await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);

            return pagedUsers;
        }


        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }



        public async Task<Photo> GetMainPhoto(int userId)
        {
            var photo = await _context.Photos.Where(p => p.UserId == userId).FirstOrDefaultAsync(w => w.IsMain);

            return photo;
        }


        public async Task<Like> GetLike(int userId, int recipientId)
        {
            // ... check whether user doesn't already like the other one
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }


        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likes)
        {
            //// ....................... MY code:
            //if (likes)
            //{
            //    return await _context.Likes.Where(l => l.LikerId == id).Select(l => l.LikeeId).ToListAsync();
            //}

            //return await _context.Likes.Where(l => l.LikeeId == id).Select(l => l.LikerId).ToListAsync();

            // ........................ Neil's code:
            var user = await _context.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (likes)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }

        }



        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }



        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            //... OR:                                                 .ThenInclude(u => u.Photos.Where(p => p.IsMain))
            //
            var messages = _context.Messages.Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId && !m.DeletedByRecipient);
                    break;
                case "Outbox":
                    messages = messages.Where(m => m.SenderId == messageParams.UserId && !m.DeletedBySender);
                    break;
                default:                                                                 //... OR:    !m.IsRead
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.IsRead == false && m.DeletedByRecipient);
                    break;
            }

            messages.OrderByDescending(m => m.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }



        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages.Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .Where( m =>
                (m.SenderId == userId && m.RecipientId == recipientId && !m.DeletedBySender)
                ||
                (m.SenderId == recipientId && m.RecipientId == userId && !m.DeletedByRecipient))
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

            return messages;
        }
    }
}