using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using Spectre.Console;
using Spectre.Console.Cli;
using System.IO.Ports;
using System.Text;

namespace BtPrint;

internal sealed class PrintCommand : AsyncCommand<PrintCommandSettings>
{
    private static int CalculateBytesPerSecond(int baudRate)
        => baudRate / 10;

    private static long GetByteCount(byte[][] bytes)
    {
        long count = 0;
        foreach (var row in bytes)
        {
            count += row.Length;
        }
        return count;
    }

    private static async Task WaitTillDone(long byteCount, long bytesPerSecond)
    {
        const int minimumWaitTimeInSeconds = 2;
        long secondsToWait = (byteCount / bytesPerSecond) + minimumWaitTimeInSeconds;
        do
        {
            await Task.Delay(1000);
            --secondsToWait;
        }
        while (secondsToWait > 0);
    }

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

            AnsiConsole.MarkupLine("[green]Waiting for the printer to finish...[/]");
            await WaitTillDone(GetByteCount(payload), CalculateBytesPerSecond(settings.BaudRate)).Spinner(Spinner.Known.BouncingBar);

            printer.Connected -= OnConnected;
            printer.StatusChanged -= OnStatusChange;
        }

        return 0;
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
