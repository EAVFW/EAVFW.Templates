using System;
using System.IO;
using EAVFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace __EAVFW__.__MainApp__.Infrastructure
{
    //public class DesignTimeContextFactory : IDesignTimeDbContextFactory<DynamicContext>
    //{
    //    public DynamicContext CreateDbContext(string[] args)
    //    {
    //        Console.WriteLine(string.Join(",", args));
    //        var op = new DbContextOptionsBuilder<DynamicContext>().UseSqlServer("empty",
    //            x => x.MigrationsHistoryTable("__MigrationsHistory", "$(schema)")).Options;
    //        return new DynamicContext(op, Options.Create(new DynamicContextOptions
    //        {
    //            Manifests = new[] { JToken.Parse(File.ReadAllText($"obj/manifest.g.json")) },
    //            PublisherPrefix = "$(schema)",
    //            EnableDynamicMigrations = true,
    //            Namespace = "__EAVFW__.Models",
    //            DTOAssembly = typeof(__EAVFW__.Models.Constants).Assembly,
    //            //DTOBaseClasses = new[] { typeof(BaseOwnerEntity), typeof(BaseIdEntity) }
    //        }), new MigrationManager(NullLogger<MigrationManager>.Instance,Options.Create(new MigrationManagerOptions {  })), NullLogger< DynamicContext>.Instance);
    //    }          
    //}
}
