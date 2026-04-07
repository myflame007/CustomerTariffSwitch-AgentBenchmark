using CustomerTariffSwitch.Data.Services;
using CustomerTariffSwitch.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Error);
});

services.AddSingleton<CsvReaderService>();
services.AddSingleton<DecisionRepository>();
services.AddSingleton<SlaCalculationService>();
services.AddSingleton<RequestValidationService>();
services.AddSingleton<TariffSwitchProcessor>();

var provider = services.BuildServiceProvider();

try
{
    var processor = provider.GetRequiredService<TariffSwitchProcessor>();
    var exitCode = processor.Run();
    Environment.Exit(exitCode);
}
catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
{
    Console.Error.WriteLine($"FATAL: {ex.Message}");
    Environment.Exit(1);
}
