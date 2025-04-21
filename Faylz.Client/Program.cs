using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Faylz.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register services
builder.Services.AddScoped<FileService>();

await builder.Build().RunAsync();
