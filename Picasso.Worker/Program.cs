using Picasso;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => 
{ 
    options.ServiceName = "Picasso";
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
