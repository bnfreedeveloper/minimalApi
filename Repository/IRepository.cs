using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimalApi.Models;

namespace minimalApi.Repository
{
    public interface IRepository
    {
        Task SaveChanges();
        Task<Command?> GetById(int id);
        Task<IEnumerable<Command>> GetAll();
        void Delete(Command cmd);
        Task Create(Command cmd);
        //not really needed with ef
        // Task update(Command cmd);
    }
}