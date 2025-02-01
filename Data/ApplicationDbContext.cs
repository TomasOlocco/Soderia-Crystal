using SODERIA_I.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace SODERIA_I.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para las entidades
        public DbSet<Cliente> clientes { get; set; }
        public DbSet<Compra> compras { get; set; }
    }
}
