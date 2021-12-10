using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAVFW.Models;
using DotNetDevOps.Extensions.EAVFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace EAVFW.HelperScripts
{
    [TestClass]
    public class DbDevSetup
    {
        [TestMethod]
        public async Task InitializeDevDb()
        {
            var schema = "$(DBSchema)";
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(this.GetType().Assembly)
                .Build();

            Console.WriteLine("Test");

            var model = JToken.Parse(await File.ReadAllTextAsync(
                Path.Combine("../../../../../apps/EAVFW.MainApp/obj/", "manifest.g.json")));

            var models = Directory.EnumerateFiles("../../../../../apps/EAVFW.MainApp/manifests/")
                .Select(file => JToken.Parse(File.ReadAllText(file)))
                .OrderByDescending(k => Semver.SemVersion.Parse(k.SelectToken("$.version").ToString()))
                .ToArray();

            var optionsBuilder = new DbContextOptionsBuilder<DynamicContext>();

            optionsBuilder.UseSqlServer(configuration.GetValue<string>("ConnectionStrings:ApplicationDb") ?? "dummy",
                x => x.MigrationsHistoryTable("__MigrationsHistory", schema));
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>();

            var ctx = new DynamicContext(optionsBuilder.Options, Options.Create(
                new DynamicContextOptions
                {
                    Manifests = new[] { model }.Concat(models).ToArray(),
                    PublisherPrefix = schema,
                    EnableDynamicMigrations = true,
                    Namespace = "EAVFW.Models",
                    DTOAssembly = typeof(BaseOwnerEntity).Assembly,
                    DTOBaseClasses = new[] { typeof(BaseOwnerEntity), typeof(BaseIdEntity) }
                }), new MigrationManager(NullLogger<MigrationManager>.Instance)
            {
                SkipValidateSchemaNameForRemoteTypes = true
            }, NullLogger<DynamicContext>.Instance);

            var migrator = ctx.Database.GetInfrastructure().GetRequiredService<IMigrator>();
            var sql = migrator.GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);

            Directory.CreateDirectory("dbinit");
            await File.WriteAllTextAsync("dbinit/init.sql", sql);
        }

        [TestMethod]
        public async Task InitializeSystemAdministrator()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(this.GetType().Assembly)
                .Build();

            Console.WriteLine(configuration.GetValue<string>("AddPasswordLess"));
         
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                Console.WriteLine(item.Key + " " + item.Value);
            }


            var model = JToken.Parse(File.ReadAllText(
                Path.Combine("../../../../../apps/EAVFW.MainApp/obj/", "manifest.g.json")));

            var sb = new StringBuilder();

            var adminSGId = "$(SystemAdminSecurityGroupId)";

            sb.AppendLine(
                $"IF NOT EXISTS(SELECT * FROM [$(DBName)].[$(DBSchema)].[Identities] WHERE [Id] = '{adminSGId}')");
            sb.AppendLine("BEGIN");


            sb.AppendLine(
                $"INSERT INTO [$(DBName)].[$(DBSchema)].[Identities] (Id, Name, ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('{adminSGId}', 'System Administrator Group', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");
            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityGroups] (Id) VALUES('{adminSGId}')");

            sb.AppendLine(
                $"INSERT INTO [$(DBName)].[$(DBSchema)].[Identities] (Id, Name,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES ('$(UserGuid)', '$(UserName)', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");
            sb.AppendLine(
                "INSERT INTO [$(DBName)].[$(DBSchema)].[SystemUsers] (Id,Email,PrincipalName) VALUES ('$(UserGuid)', '$(UserEmail)', '$(UserPrincipalName)');");

            var adminSRId = Guid.NewGuid();
            sb.AppendLine(
                $"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityRoles] (Name, Description, Id,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('System Administrator', 'Access to all permissions', '{adminSRId}', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");

            sb.AppendLine(
                $"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityRoleAssignments] (IdentityId, SecurityRoleId, Id,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('{adminSGId}', '{adminSRId}', '{Guid.NewGuid()}',CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");
            sb.AppendLine(
                $"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityGroupMembers] (IdentityId, SecurityGroupId, Id,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('$(UserGuid)', '{adminSGId}', '{Guid.NewGuid()}',CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");

            foreach (var entity in model.SelectToken("$.entities").OfType<JProperty>())
            {
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "ReadGlobal", "Global Read", adminSGId,
                    adminSRId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "Read", "Read", adminSGId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "UpdateGlobal", "Global Update", adminSGId,
                    adminSRId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "Update", "Update", adminSGId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "CreateGlobal", "Global Create", adminSGId,
                    adminSRId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "Create", "Create", adminSGId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "DeleteGlobal", "Global Delete", adminSGId,
                    adminSRId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "Delete", "Delete", adminSGId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "ShareGlobal", "Global Share", adminSGId,
                    adminSRId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "Share", "Share", adminSGId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "AssignGlobal", "Global Assign", adminSGId,
                    adminSRId);
                WritePermissionStatement(sb, entity, Guid.NewGuid(), "Assign", "Assign", adminSGId);
            }

            sb.AppendLine("END;");
            Directory.CreateDirectory("dbinit");
            await File.WriteAllTextAsync("dbinit/init-systemadmin.sql", sb.ToString());
        }

        private static void WritePermissionStatement(StringBuilder sb, JProperty entity, Guid readPermissionGuid,
            string permission, string permissionName, string adminSgId, Guid? adminSrId = null)
        {
            sb.AppendLine(
                $"INSERT INTO [$(DBName)].[$(DBSchema)].[Permissions] (Name, Description, Id, ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('{entity.Value.SelectToken("$.collectionSchemaName")}{permission}', '{permissionName} access to {entity.Value.SelectToken("$.pluralName")}', '{readPermissionGuid}', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSgId}','{adminSgId}','{adminSgId}')");
            if (adminSrId.HasValue)
                sb.AppendLine(
                    $"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityRolePermissions] (Name, PermissionId, SecurityRoleId, Id,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('System Administrator - {entity.Value.SelectToken("$.collectionSchemaName")} - {permission}', '{readPermissionGuid}', '{adminSrId}', '{Guid.NewGuid()}', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSgId}','{adminSgId}','{adminSgId}')");
        }
    }
}