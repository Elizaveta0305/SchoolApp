using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolApplication.Tests
{
    public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly string _databaseName;
        private static readonly Dictionary<string, bool> _databaseInitializedFlags = new Dictionary<string, bool>();

        public TestDbContextFactory()
        {
            _databaseName = Guid.NewGuid().ToString();
        }

        public ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

            var context = new ApplicationDbContext(options);

            lock (_databaseInitializedFlags)
            {
                if (!_databaseInitializedFlags.ContainsKey(_databaseName) || !_databaseInitializedFlags[_databaseName])
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    _databaseInitializedFlags[_databaseName] = true;
                }
            }
            context.ChangeTracker.Clear();
            return context;
        }

        public void SeedData(ApplicationDbContext context, params object[] entities)
        {
            context.ChangeTracker.Clear();

            foreach (var entity in entities)
            {
                context.Add(entity);
            }
            context.SaveChanges();
            context.ChangeTracker.Clear();
        }
    }
}