using EAVFramework;
using EAVFramework.Extensions.Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using __EAVFW__.Models;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var portal = builder
    .AddEAVFWApp<__EAVFW_____MainApp__>("__databaseName__-portal", "build-app",
        builder.Configuration.GetValue<string>("Aspire_LaunchProfile") ?? "http")
    .WithHotReload()
#if (useNpmLink)
    .WithNpmLink("__npmLinkPath__")
#endif
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "local")
    .WithEnvironment("DOTNET_ENVIRONMENT", "local")
    .ForwardEnvironmentVariables<__EAVFW_____MainApp__>()
    .WithSqlServerDB(configureSqlServer: sql => sql.WithDbGate(dbgate => dbgate.WithParentRelationship(sql)))
    .WithMailPit()
    .WithEAVModel<__EAVFW___Models, DynamicContext, Identity, Signin>(
        "__databaseName__-model",
        "__userEmail__",
        Guid.Parse("__userGuid__"),
        "__userName__");

builder.Build().Run();
