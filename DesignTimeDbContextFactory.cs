using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pharmacy_API.Context;

namespace Pharmacy_API
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AccountContext>
    {
        public AccountContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AccountContext>();

            // ✅ Dùng EXTERNAL URL (có .singapore-postgres.render.com)
            var connectionString = "Host=dpg-d8pphpv7f7vs73d7h5s0-a.singapore-postgres.render.com;Port=5432;Database=pharmacy_db_9y2c;Username=pharmacy_user;Password=X7kt0sRdI94uGHnGLDfw9Ur2ZCFaVp98;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=10;Timeout=30;CommandTimeout=60;";

            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                npgsqlOptions.CommandTimeout(120);
            });

            return new AccountContext(optionsBuilder.Options);
        }
    }
}