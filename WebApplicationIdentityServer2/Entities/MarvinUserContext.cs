using Microsoft.EntityFrameworkCore;

namespace WebApplicationIdentityServer2.Entities
{
    public class MarvinUserContext : DbContext
    {
        public MarvinUserContext(DbContextOptions<MarvinUserContext> options)
           : base(options)
        {
           
        }

        public DbSet<User> Users { get; set; }
    }
}
