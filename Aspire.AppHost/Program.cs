using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var postgresdb = postgres.AddDatabase("LoglivenDB");

var logliven = builder.AddProject<Logliven>("logliven")
    .WithReference(postgresdb);

logliven
    .AddWebAssemblyClient<Logliven_Client>("logliven-client")
    .WithReference(logliven);

builder.Build().Run();