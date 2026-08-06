using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using JO.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace JO.Migrations.Tests
{
    public class MigrationTests : IAsyncLifetime
    {
        private string _connectionString;
        private DbContextOptions<TestMigrationContext> _options;
        private string _databaseName;

        public async Task InitializeAsync()
        {
           
            _databaseName = $"TestDb_{Guid.NewGuid():N}";
            _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true";

            await using var masterConn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;");
            await masterConn.OpenAsync();
            await using (var createCmd = masterConn.CreateCommand())
            {
                createCmd.CommandText = $"CREATE DATABASE [{_databaseName}]";
                await createCmd.ExecuteNonQueryAsync();
            }

            
            var migrationsAssembly = typeof(cart).Assembly.GetName().Name;
            _options = new DbContextOptionsBuilder<TestMigrationContext>()
                .UseSqlServer(_connectionString, x => x.MigrationsAssembly(migrationsAssembly))
                .Options;
        }

        public async Task DisposeAsync()
        {
            
            await using var masterConn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;");
            await masterConn.OpenAsync();
            await using (var cmd = masterConn.CreateCommand())
            {
               
                cmd.CommandText = $@"
ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE IF EXISTS [{_databaseName}];";
                await cmd.ExecuteNonQueryAsync();
            }
        }


          

        [Fact]
        public async Task Down_ShouldDropCartsAndCartItemsTables()
        {
            // Arrange: apply all migrations then revert to initial state
            await using (var context = new TestMigrationContext(_options))
            {
                var migrator = context.GetService<IMigrator>();
                await migrator.MigrateAsync();      // migrate to latest
                await migrator.MigrateAsync("0");  // revert all
            }

            // Act: query tables after rollback
            var tableNames = new List<string>();
            await using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name FROM sys.Tables WHERE is_ms_shipped = 0";
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tableNames.Add(reader.GetString(0));
                }
            }

            // Assert: our tables no longer exist after rollback
            Assert.DoesNotContain("Carts", tableNames);
            Assert.DoesNotContain("CartItems", tableNames);
        }

        private class TestMigrationContext : DbContext
        {
            public TestMigrationContext(DbContextOptions<TestMigrationContext> options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
               
            }
        }

    }
}
