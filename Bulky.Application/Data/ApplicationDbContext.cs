using Bulky.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData
                (
                new Category { Id = 1, Name = "Sci-Fi", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Action", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
                );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Title="Interstellar", Description="Famous Sci-Fi movie", Author="Christopher Nolan", ISBN="ABD013X", ListPrice = 70, Price=70, Price50= 50, Price100= 30, CategoryId = 1, ImageUrl=""},
				new Product { Id = 2, Title = "Terminator 2", Description = "Famous Action movie", Author = "Hames Cameron", ISBN = "CFG956H", ListPrice = 70, Price = 70, Price50 = 50, Price100 = 30, CategoryId = 2, ImageUrl = "" },
				new Product { Id = 3, Title = "All Quiet on the Western Front", Description = "Famous History movie", Author = "Edward Berger", ISBN = "GTP034A", ListPrice = 70, Price = 70, Price50 = 50, Price100 = 30, CategoryId = 3, ImageUrl = "" }
				);
        }
    }
}
