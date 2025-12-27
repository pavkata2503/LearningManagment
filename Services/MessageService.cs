using AppData.Models;
using AppData.Models.ViewModels;
using DataContext;
using Microsoft.EntityFrameworkCore;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Message>> GetMessagesAsync(
            string userEmail,
            string? sender,
            bool? isRead,
            int pageSize,
            int pageNumber)
        {
            var query = _context.Messages
                .Where(m => m.Receiver == userEmail);

            if (!string.IsNullOrEmpty(sender))
            {
                query = query.Where(m => m.Sender == sender);
            }

            if (isRead.HasValue)
            {
                query = query.Where(m => m.IsRead == isRead.Value);
            }

            query = query.OrderByDescending(m => m.SendDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var messages = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Message>
            {
                Items = messages,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task AddMessageAsync(Message message, ApplicationUser sender)
        {
            message.Sender = sender.Name;
            message.SenderEmail = sender.Email;
            message.SendDate = DateTime.Now;
            message.IsRead = false;
            message.ApplicationUserId = sender.Id;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return;

            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

}
