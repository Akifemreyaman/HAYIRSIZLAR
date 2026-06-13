using Microsoft.EntityFrameworkCore;
using System;

namespace HayirsizlarApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Apply migrations (will create db if not exists)
            context.Database.Migrate();
        }
    }
}
