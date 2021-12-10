using System.Threading.Tasks;
using EAVFW.Models;
using DotNetDevOps.Extensions.EAVFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace EAVFW.HelperScripts
{
    [TestClass]
    public class ManifestTests
    {
        [TestMethod]
        [DeploymentItem(@"specs/DynamicFormManifest001.sql")]
        [DeploymentItem(@"specs/DynamicFormManifest001.json")]
        public async Task BlanketBk001Test()
        {
            var manifest = JToken.Parse(await System.IO.File.ReadAllTextAsync(@"specs\DynamicFormManifest001.json"));
            var sql = RunDbWithSchema("BK001", manifest);

            var expectedSql = await System.IO.File.ReadAllTextAsync(@"specs\DynamicFormManifest001.sql");

            Assert.AreEqual(expectedSql, sql);
        }

        [TestMethod]
        public void RowVersionTest()
        {
            var manifest = JToken.FromObject(new
            {
                entities = new
                {
                    CustomEntity = new
                    {
                        schemaName = "CustomEntity",
                        logicalName = "customEntity",
                        pluralName = "customentities",
                        collectionSchemaName = "CustomEntities",
                        attributes = new
                        {
                            id = new
                            {
                                isPrimaryKey = true,
                                schemaName = "Id",
                                logicalName = "id",
                                type = "guid"
                            },
                            name = new
                            {
                                schemaName = "Name",
                                logicalName = "name",
                                type = "string",
                                isPrimaryField = true
                            },
                            rowversion = new
                            {
                                schemaName = "RowVersion",
                                logicalName = "rowversion",
                                type = new
                                {
                                    type = "binary",
                                    sql = new
                                    {
                                        rowVersion = true,
                                    },
                                },

                                isPrimaryField = true,
                                isRowVersion = true,
                                isRequired = true,
                            }
                        }
                    }
                }
            });

            var sql = RunDbWithSchema("manifest_rowversion", manifest);
        }

        [TestMethod]
        public void Binary()
        {
            var manifest = JToken.FromObject(new
            {
                entities = new
                {
                    CustomEntity = new
                    {
                        schemaName = "CustomEntity",
                        logicalName = "customEntity",
                        pluralName = "customentities",
                        collectionSchemaName = "CustomEntities",
                        attributes = new
                        {
                            id = new
                            {
                                isPrimaryKey = true,
                                schemaName = "Id",
                                logicalName = "id",
                                type = "guid"
                            },
                            name = new
                            {
                                schemaName = "Name",
                                logicalName = "name",
                                type = "string",
                                isPrimaryField = true
                            },
                            blob = new
                            {
                                schemaName = "Data",
                                logicalName = "Data",
                                type = "binary",
                                isPrimaryField = true
                            }
                        }
                    }
                }
            });

            RunDbWithSchema("manifest_binary", manifest);
        }

        private object CreateCustomEntity(string name, string pluralName)
        {
            return new
            {
                schemaName = name,
                logicalName = name.Replace(" ", "").ToLower(),
                pluralName = pluralName,
                collectionSchemaName = pluralName.Replace(" ", ""),
                attributes = new
                {
                    id = new
                    {
                        isPrimaryKey = true,
                        schemaName = "Id",
                        logicalName = "id",
                        type = "guid"
                    },
                    name = new
                    {
                        schemaName = "Name",
                        logicalName = "name",
                        type = "string",
                        isPrimaryField = true
                    },
                }
            };
        }

        [TestMethod]
        [DeploymentItem(@"specs/CarsAndTrucksModel.sql")]
        public async Task CarsAndTrucksModelTest()
        {
            //Arrange
            var manifest = JToken.FromObject(new
            {
                version = "1.0.0",
                entities = new
                {
                    Car = CreateCustomEntity("Car", "Cars"),
                    Truck = CreateCustomEntity("Truck", "Trucks")
                }
            });

            //Act
            var sql = RunDbWithSchema("manifest_migrations", manifest);

            //Assure
            var expectedSql = await System.IO.File.ReadAllTextAsync(@"specs\CarsAndTrucksModel.sql");

            Assert.AreEqual(expectedSql, sql);
        }

        [TestMethod]
        [DeploymentItem(@"specs/CarsAndTrucksModel_AddEntity.sql")]
        public async Task CarsAndTrucksModel_AddEntityTest()
        {
            var manifestA = JToken.FromObject(new
            {
                version = "1.0.0",
                entities = new
                {
                    Car = CreateCustomEntity("Car", "Cars")
                }
            });

            var manifestB = JToken.FromObject(new
            {
                version = "1.0.1",
                entities = new
                {
                    Car = CreateCustomEntity("Car", "Cars"),
                    Truck = CreateCustomEntity("Truck", "Trucks")
                }
            });

            var sql = RunDbWithSchema("manifest_migrations", manifestB, manifestA);

            var expectedSql = await System.IO.File.ReadAllTextAsync(@"specs\CarsAndTrucksModel_AddEntity.sql");

            Assert.AreEqual(expectedSql, sql);
        }

        [TestMethod]
        [DeploymentItem(@"specs/CarsAndTrucksModel_AddAttribute.sql")]
        public async Task CarsAndTrucksModel_AddAttributeTest()
        {
            var manifestA = JToken.FromObject(new
            {
                version = "1.0.0",
                entities = new
                {
                    Car = CreateCustomEntity("Car", "Cars")
                }
            });

            var manifestB = JToken.FromObject(new
            {
                version = "1.0.1",
                entities = new
                {
                    Car = CreateCustomEntity("Car", "Cars"),
                    Truck = CreateCustomEntity("Truck", "Trucks")
                }
            });

            var manifestC = JToken.FromObject(new
            {
                version = "1.0.2",
                entities = new
                {
                    Car = CreateCustomEntity("Car", "Cars"),
                    Truck = CreateCustomEntity("Truck", "Trucks")
                }
            });
            AppendAttribute(manifestC, "Truck", "Version", new
            {
                schemaName = "Version",
                logicalName = "version",
                type = "string",
            });

            var sql = RunDbWithSchema("manifest_migrations", manifestC, manifestB, manifestA);

            var expectedSql = await System.IO.File.ReadAllTextAsync(@"specs\CarsAndTrucksModel_AddAttribute.sql");

            Assert.AreEqual(expectedSql, sql);
        }

        private void AppendAttribute(JToken manifestC, string entityKey, string attributeKey, object attribute)
        {
            manifestC["entities"][entityKey]["attributes"][attributeKey] = JToken.FromObject(attribute);
        }

        private string RunDbWithSchema(string schema, params JToken[] manifests)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(GetType().Assembly)
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IMigrationManager, MigrationManager>();

            services.AddOptions<DynamicContextOptions>().Configure((o) =>
            {
                o.Manifests = manifests;
                o.PublisherPrefix = "tests";
                o.EnableDynamicMigrations = true;
                o.Namespace = "DummyNamespace";
                o.DTOBaseClasses = new[] { typeof(BaseOwnerEntity), typeof(BaseIdEntity) };
                o.DTOAssembly = typeof(BaseOwnerEntity).Assembly;
            });

            services.AddDbContext<DynamicContext>((sp, optionsBuilder) =>
            {
                optionsBuilder.UseSqlServer(configuration.GetValue<string>("ConnectionString") ?? "dummy",
                    x => x.MigrationsHistoryTable("__MigrationsHistory", schema));
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();

                optionsBuilder.ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>();
            });

            var sp = services.BuildServiceProvider();
            var ctx = sp.GetRequiredService<DynamicContext>();

            var migrator = ctx.Database.GetInfrastructure().GetRequiredService<IMigrator>();
            var sql = migrator.GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);

            return sql;
        }

        [TestMethod]
        public void MutualReferenceTest()
        {
            var manifest = JToken.FromObject(new
            {
                entities = new
                {
                    Account = new
                    {
                        schemaName = "AccountEntity",
                        logicalName = "accountentity",
                        pluralName = "accountentities",
                        collectionSchemaName = "AccountEntities",
                        attributes = new
                        {
                            id = new
                            {
                                isPrimaryKey = true,
                                schemaName = "Id",
                                logicalName = "id",
                                type = "guid"
                            },
                            name = new
                            {
                                schemaName = "Name",
                                logicalName = "name",
                                type = "string",
                                isPrimaryField = true
                            },
                            PrimaryAccountCode = new
                            {
                                schemaName = "PrimaryAccountCode",
                                logicalName = "primaryaccountcode",
                                type = new
                                {
                                    type = "lookup",
                                    referenceType = "AccountCode"
                                },
                                isRequired = true,
                            }
                        }
                    },
                    AccountCode = new
                    {
                        schemaName = "AccountCodeEntity",
                        logicalName = "accountcodeentity",
                        pluralName = "accountcodeentities",
                        collectionSchemaName = "AccountCodeEntities",
                        attributes = new
                        {
                            id = new
                            {
                                isPrimaryKey = true,
                                schemaName = "Id",
                                logicalName = "id",
                                type = "guid"
                            },
                            name = new
                            {
                                schemaName = "Name",
                                logicalName = "name",
                                type = "string",
                                isPrimaryField = true
                            },
                            Account = new
                            {
                                schemaName = "account",
                                logicalName = "account",
                                type = new
                                {
                                    type = "lookup",
                                    referenceType = "Account"
                                },
                                isRequired = true,
                            }
                        }
                    }
                }
            });

            var sql = RunDbWithSchema("MutualReferenceTest", manifest);
        }
    }
}