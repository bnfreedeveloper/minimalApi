using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimalApi.Models;

namespace minimalApi.Repository
{
    public interface IRepository
    {
        Task SaveChangesAsync();
        Task<Command?> GetByIdAsync(int id);
        Task<IEnumerable<Command>> GetAllAsync();
        void Delete(Command cmd);
        Task CreateAsync(Command cmd);
        //not really needed with ef
        // Task update(Command cmd);
    }
}