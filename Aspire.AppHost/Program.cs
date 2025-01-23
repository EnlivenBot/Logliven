var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var postgresdb = postgres.AddDatabase("LoglivenDB");

builder.AddProject<Projects.Logliven>("logliven")
    .WithReference(postgresdb);

builder.Build().Run();