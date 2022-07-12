using DotNetDevOps.Extensions.EAVFramework;
using EAVFW.Extensions.SecurityModel;
using __EAVFW__.__MainApp__;
using __EAVFW__.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace __EAVFW__.HelperScripts
{
    [TestClass]
    public class DBDevSetup
    {

        //[ClassInitialize]
        //public static void TestClassinitialize(TestContext context)
        //{
        //    // var webAppUrl = context.Properties["AddPasswordLess"].ToString();
        //    foreach (DictionaryEntry item in context.Properties)
        //    {
        //        Console.WriteLine(item.Key + " " + item.Value);
        //    }

        //    //other settings etc..then use your test settings parameters here...
        //}

        [TestMethod]
        public async Task InitializeDevDB()
        {
            var schema = "$(DBSchema)";
            var configuration = new ConfigurationBuilder()
           .AddEnvironmentVariables()
           .AddUserSecrets(this.GetType().Assembly)
           .Build();
            Console.WriteLine("Test");
            var model = JToken.Parse(File.ReadAllText(Path.Combine("../../../../../apps/__EAVFW__.__MainApp__/obj/", "manifest.g.json")));
            var models = Directory.EnumerateFiles("../../../../../apps/__EAVFW__.__MainApp__/manifests/")
                .Select(file => JToken.Parse(File.ReadAllText(file)))
                .OrderByDescending(k => Semver.SemVersion.Parse(k.SelectToken("$.version").ToString()))
                .ToArray();
            var optionsBuilder = new DbContextOptionsBuilder<DynamicContext>();
            //  optionsBuilder.UseInMemoryDatabase("test");
            optionsBuilder.UseSqlServer(configuration.GetValue<string>("ConnectionStrings:ApplicationDb") ?? "dummy", x => x.MigrationsHistoryTable("__MigrationsHistory", schema));
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>();

            var ctx = new DynamicContext(optionsBuilder.Options, Options.Create(
                 new DotNetDevOps.Extensions.EAVFramework.DynamicContextOptions
                 {
                     Manifests = new[] { model }.Concat(models).ToArray(),
                     PublisherPrefix = schema,
                     EnableDynamicMigrations = true,
                     Namespace = "__EAVFW__.Models",
                     DTOAssembly = typeof(Startup).Assembly,
                     DTOBaseClasses = new[] { typeof(BaseOwnerEntity<Identity>), typeof(BaseIdEntity<Identity>) }
                 }),
                 new MigrationManager(NullLogger<MigrationManager>.Instance, Options.Create(new MigrationManagerOptions() { SkipValidateSchemaNameForRemoteTypes = true }))
                 , NullLogger<DynamicContext>.Instance);

            // var test = ctx.GetMigrations();

            var migrator = ctx.Database.GetInfrastructure().GetRequiredService<IMigrator>();
            var sql = migrator.GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
            //  await migrator.MigrateAsync("0"); //Clean up
            Directory.CreateDirectory("dbinit");
            await File.WriteAllTextAsync("dbinit/init.sql", sql);
            // await migrator.MigrateAsync(); //Move to latest migration
        }

        [TestMethod]
        public async Task InitializeSystemAdministrator()
        {
            // var schema = "kfst";
            var configuration = new ConfigurationBuilder()
           .AddEnvironmentVariables()
           .AddUserSecrets(this.GetType().Assembly)
           .Build();

            Console.WriteLine(configuration.GetValue<string>("AddPasswordLess"));
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                Console.WriteLine(item.Key + " " + item.Value);
            }


            var model = JToken.Parse(File.ReadAllText(Path.Combine("../../../../../apps/__EAVFW__.__MainApp__/obj/", "manifest.g.json")));

            var sb = new StringBuilder();

            var adminSGId = "$(SystemAdminSecurityGroupId)";

            sb.AppendLine("DECLARE @adminSRId uniqueidentifier");
            sb.AppendLine("DECLARE @permissionId uniqueidentifier");
            sb.AppendLine($"SET @adminSRId = ISNULL((SELECT s.Id   FROM [$(DBName)].[$(DBSchema)].[SecurityRoles] s WHERE s.Name = 'System Administrator'),'{Guid.NewGuid()}')");


            sb.AppendLine($"IF NOT EXISTS(SELECT * FROM [$(DBName)].[$(DBSchema)].[Identities] WHERE [Id] = '{adminSGId}')");
            sb.AppendLine("BEGIN");


            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[Identities] (Id, Name, ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('{adminSGId}', 'System Administrator Group', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");
            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityGroups] (Id) VALUES('{adminSGId}')");


            //sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[Identities] (Id, Name,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES ('$(UserGuid)', '$(UserName)', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");
            //sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[Users] (Id,Email) VALUES ('$(UserGuid)', '$(UserEmail)');");

            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[Identities] (Id, Name,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES ('$(UserGuid)', '$(UserName)', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");
            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[SystemUsers] (Id,Email,PrincipalName) VALUES ('$(UserGuid)', '$(UserEmail)', '$(UserPrincipalName)');");



            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityRoles] (Name, Description, Id,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('System Administrator', 'Access to all permissions', @adminSRId, CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");

            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityRoleAssignments] (IdentityId, SecurityRoleId, Id,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('{adminSGId}', @adminSRId, '{Guid.NewGuid()}',CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");
            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityGroupMembers] (IdentityId, SecurityGroupId, Id,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('$(UserGuid)', '{adminSGId}', '{Guid.NewGuid()}',CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");


            sb.AppendLine("END;");

            foreach (var entitiy in model.SelectToken("$.entities").OfType<JProperty>())
            {


                WritePermissionStatement(sb, entitiy, "ReadGlobal", "Global Read", adminSGId, true);
                WritePermissionStatement(sb, entitiy, "Read", "Read", adminSGId);
                WritePermissionStatement(sb, entitiy, "UpdateGlobal", "Global Update", adminSGId, true);
                WritePermissionStatement(sb, entitiy, "Update", "Update", adminSGId);
                WritePermissionStatement(sb, entitiy, "CreateGlobal", "Global Create", adminSGId, true);
                WritePermissionStatement(sb, entitiy, "Create", "Create", adminSGId);
                WritePermissionStatement(sb, entitiy, "DeleteGlobal", "Global Delete", adminSGId, true);
                WritePermissionStatement(sb, entitiy, "Delete", "Delete", adminSGId);
                WritePermissionStatement(sb, entitiy, "ShareGlobal", "Global Share", adminSGId, true);
                WritePermissionStatement(sb, entitiy, "Share", "Share", adminSGId);
                WritePermissionStatement(sb, entitiy, "AssignGlobal", "Global Assign", adminSGId, true);
                WritePermissionStatement(sb, entitiy, "Assign", "Assign", adminSGId);

            }

            Directory.CreateDirectory("dbinit");
            await File.WriteAllTextAsync("dbinit/init-systemadmin.sql", sb.ToString());


        }

        private static void WritePermissionStatement(StringBuilder sb, JProperty entitiy, string permission, string permissionName, string adminSGId, bool adminSRId1 = false)
        {
            sb.AppendLine($"SET @permissionId = ISNULL((SELECT s.Id   FROM [$(DBName)].[$(DBSchema)].[Permissions] s WHERE s.Name = '{entitiy.Value.SelectToken("$.collectionSchemaName")}{permission}'),'{Guid.NewGuid()}')");


            sb.AppendLine($"IF NOT EXISTS(SELECT * FROM [$(DBName)].[$(DBSchema)].[Permissions] WHERE [Name] = '{entitiy.Value.SelectToken("$.collectionSchemaName")}{permission}')");
            sb.AppendLine("BEGIN");

            sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[Permissions] (Name, Description, Id, ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('{entitiy.Value.SelectToken("$.collectionSchemaName")}{permission}', '{permissionName} access to {entitiy.Value.SelectToken("$.pluralName")}', @permissionId, CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");

            sb.AppendLine("END");

            if (adminSRId1)
            {
                sb.AppendLine($"IF NOT EXISTS(SELECT * FROM [$(DBName)].[$(DBSchema)].[SecurityRolePermissions] WHERE [Name] = 'System Administrator - {entitiy.Value.SelectToken("$.collectionSchemaName")} - {permission}')");
                sb.AppendLine("BEGIN");



                sb.AppendLine($"INSERT INTO [$(DBName)].[$(DBSchema)].[SecurityRolePermissions] (Name, PermissionId, SecurityRoleId, Id,ModifiedOn,CreatedOn,CreatedById,ModifiedById,OwnerId) VALUES('System Administrator - {entitiy.Value.SelectToken("$.collectionSchemaName")} - {permission}', @permissionId, @adminSRId, '{Guid.NewGuid()}', CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,'{adminSGId}','{adminSGId}','{adminSGId}')");

                sb.AppendLine("END");
            }


        }
    }
}
