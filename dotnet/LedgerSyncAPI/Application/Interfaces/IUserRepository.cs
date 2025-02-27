using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<int> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<User> GetByIdAsync(int id);
        Task EnsureTableExistsAsync();
    }
}
