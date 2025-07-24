using Microsoft.EntityFrameworkCore;
using MvcAuthApp.Models;

namespace MvcAuthApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }  // Добавь эту строку

        // Если есть другие модели, например Employee
        public DbSet<Employee> Employees { get; set; }
    }
}
