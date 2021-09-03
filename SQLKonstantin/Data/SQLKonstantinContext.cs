using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SQLKonstantin.Models;

namespace SQLKonstantin.Data
{
    public class SQLKonstantinContext : DbContext
    {
        public SQLKonstantinContext (DbContextOptions<SQLKonstantinContext> options)
            : base(options)
        {
        }

        public DbSet<SQLKonstantin.Models.Person> Person { get; set; }
    }
}
