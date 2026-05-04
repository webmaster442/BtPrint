using System.Text;

using ESCPOS_NET;
using ESCPOS_NET.Emitters;

using Spectre.Console;
using Spectre.Console.Cli;

namespace BtPrint;

internal sealed class PrintCommand : AsyncCommand<PrintCommandSettings>
{
    private static byte[][] PrepareImagePayload(BaseCommandEmitter e, string fileName, int maxWidth)
    {
        return
        [
            e.PrintLine(" "),
            e.PrintImage(File.ReadAllBytes(fileName), isHiDPI: true, isLegacy: true, maxWidth: maxWidth),
            e.PrintLine(" "),
            e.PrintLine(" ")
        ];
    }

    private static byte[][] PrepareTextPayload(BaseCommandEmitter e, string fileName)
    {
        var text = File.ReadAllText(fileName);
        return
        [
            e.PrintLine(" "),
            e.Print(text),
            e.PrintLine(" ")
        ];
    }

    protected override async Task<int> ExecuteAsync(CommandContext context,
                                              PrintCommandSettings settings,
                                              CancellationToken cancellationToken)
    {
        var fileToPrint = Path.GetFullPath(settings.FileName);

        AnsiConsole.MarkupLine("[green]Connecting to printer...[/]");

        using (var printer = new SerialPrinter(portName: settings.Port, baudRate: settings.BaudRate))
        {
            var ecoder = new EPSON { Encoding = Encoding.ASCII };
            printer.Connected += OnConnected;
            printer.StatusChanged += OnStatusChange;

            printer.Write(ecoder.Initialize());
            printer.Write(ecoder.Enable());
            printer.Write(ecoder.EnableAutomaticStatusBack());

            AnsiConsole.MarkupLine("[green]Creating payload...[/]");
            
            byte[][] payload;
            if (settings.IsText)
            {
                payload = PrepareTextPayload(ecoder, fileToPrint);
                printer.Write(payload);
            }
            else
            {
                payload = PrepareImagePayload(ecoder, fileToPrint, settings.MaxWidth);
                printer.Write(payload);
            }

            AnsiConsole.MarkupLine("Printing.... Press the ESC key when printer is finished, to disconnect");
            WaitForESCKey();

            printer.Connected -= OnConnected;
            printer.StatusChanged -= OnStatusChange;
        }

        return 0;
    }

    private static void WaitForESCKey()
    {
        while (true)
        {
            var key = Console.ReadKey(false);
            if (key.Key == ConsoleKey.Escape)
            {
                break;
            }
        }
    }

    private void OnStatusChange(object? sender, PrinterStatusEventArgs e)
    {
        if (e.IsPaperOut == true)
        {
            AnsiConsole.MarkupLine("[red]Paper is out![/]");
        }
        if (e.IsPaperLow == true)
        {
            AnsiConsole.MarkupLine("[yellow]Paper is low.[/]");
        }
        if (e.IsCoverOpen == true)
        {
            AnsiConsole.MarkupLine("[red]Cover is open![/]");
        }
    }

    private void OnConnected(object? sender, EventArgs e)
        => AnsiConsole.MarkupLine("[yellow]Printer connected.[/]");
}
