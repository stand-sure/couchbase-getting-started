using Microsoft.Extensions.Hosting;

using StartUsingConsole;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureHostBuilder();

await builder.RunConsoleAsync();