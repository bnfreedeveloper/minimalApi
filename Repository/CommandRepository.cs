using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimalApi.Data;
using minimalApi.Models;

namespace minimalApi.Repository
{
    public class CommandRepository : IRepository
    {
        private readonly CommandDbContext _dbContext;

        public CommandRepository(CommandDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public async Task CreateAsync(Command cmd)
        {
            await _dbContext.AddAsync(cmd);
        }

        public void Delete(Command cmd)
        {
            _dbContext.Commands.Remove(cmd);
        }

        public async Task<IEnumerable<Command>> GetAllAsync()
        {
            return await _dbContext.Commands.ToListAsync();
        }

        public async Task<Command?> GetByIdAsync(int id)
        {
            return await _dbContext.Commands.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}