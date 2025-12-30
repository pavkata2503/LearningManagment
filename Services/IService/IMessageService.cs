using AppData.Models;
using AppData.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IMessageService
    {
        Task<PaginatedResult<Message>> GetMessagesAsync(
            string userEmail,
            string? sender,
            bool? isRead,
            int pageSize,
            int pageNumber);

        Task AddMessageAsync(Message message, ApplicationUser sender);

        Task DeleteAsync(int id);
        Task<Message> GetByIdAsync(int id);

        Task MarkAsReadAsync(int id);
    }

}
