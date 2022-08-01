using Serilog;
using Spectre.Cli;
using WebmResizer.Commands;

File.WriteAllText("log.txt", "");
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt")
    .MinimumLevel.Debug()
    .CreateLogger();

var app = new CommandApp<RunInteractiveCommand>();
app.Configure(c =>
{
    c.AddCommand<RunCommand>("run");
    c.AddCommand<ListTypesCommand>("list");
});
app.Run(args);
