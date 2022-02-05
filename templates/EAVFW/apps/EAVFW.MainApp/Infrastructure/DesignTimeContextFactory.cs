using System;
using System.IO;
using DotNetDevOps.Extensions.EAVFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace EAVFW.Framework.Infrastructure
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<DynamicContext>
    {
        public DynamicContext CreateDbContext(string[] args)
        {
            Console.WriteLine(string.Join(",", args));
            var op = new DbContextOptionsBuilder<DynamicContext>().UseSqlServer("empty",
                x => x.MigrationsHistoryTable("__MigrationsHistory", "$(schema)")).Options;
            return new DynamicContext(op, Options.Create(new DynamicContextOptions
            {
                Manifests = new[] { JToken.Parse(File.ReadAllText($"obj/manifest.g.json")) },
                PublisherPrefix = "$(schema)",
                EnableDynamicMigrations = true,
                Namespace = "EAVFW.Models",
                DTOAssembly = typeof(EAVFW.Models.Constants).Assembly,
                //DTOBaseClasses = new[] { typeof(BaseOwnerEntity), typeof(BaseIdEntity) }
            }), new MigrationManager(NullLogger<MigrationManager>.Instance), NullLogger< DynamicContext>.Instance);
        }          
    }
}
