using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SQLKonstantin.Data;
using SQLKonstantin.Models;
using System;
using System.Linq;

namespace SQLKonstantin.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new SQLKonstantinContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<SQLKonstantinContext>>()))
            {
                // Look for any movies.
                if (context.Person.Any())
                {
                    return;   // DB has been seeded
                }

                context.Person.AddRange(
                    new Person
                    {
                        Id=1,
                        Age=21,
                        Name="Kostya"
                    },
                    new Person
                    {
                        Id = 2,
                        Age = 21,
                        Name = "Dima"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}

