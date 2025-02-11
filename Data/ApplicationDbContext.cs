using SODERIA_I.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SODERIA_I.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
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
