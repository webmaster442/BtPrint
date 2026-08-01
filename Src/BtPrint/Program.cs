using BtPrint;
using Spectre.Console.Cli;

var app = new CommandApp<PrintCommand>();
app.Configure(c =>
{
    c.SetApplicationName("BtPrint");
    c.AddExample("BtPrint", "-p", "COM3", "test.jpg");
    c.AddExample("BtPrint", "--port", "COM3", "test.txt");
});
return await app.RunAsync(args);