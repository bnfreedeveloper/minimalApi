using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimalApi.Models;

namespace minimalApi.Data
{
    public class CommandDbContext : DbContext
    {
        public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options)
        {

        }
        public DbSet<Command> Commands => Set<Command>();
    }
}