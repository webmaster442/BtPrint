using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO.Ports;

namespace BtPrint;

internal class PrintCommandSettings : CommandSettings
{
    public static readonly HashSet<string> SupportedExtensions
        = new([".jpg", ".jpeg", ".bmp", ".png", ".txt", ".gif"], StringComparer.InvariantCultureIgnoreCase);

    [CommandOption("-p|--port <PORT>")]
    [Description("The serial port to which the printer is connected. For example, COM3 on Windows or /dev/ttyUSB0 on Linux.")]
    public string Port { get; set; } = "";

    [CommandOption("-b|--baudrate")]
    [Description("The baud rate for the serial connection. Supported values are 115200, 9600, and 4800. Default is 115200.")]
    public int BaudRate { get; set; } = 115200;

    [CommandOption("-w|--max-width")]
    [Description("The maximum width of the printed image in pixels. This setting is ignored for text files. Default is 400")]
    public int MaxWidth { get; set; } = 400;

    [CommandArgument(0, "<filename>")]
    [Description("The path to the file to print. Supported formats are .jpg, .jpeg, .bmp, .png, .txt, and .gif.")]
    public string FileName { get; set; } = "";

    public bool IsText => Path.GetExtension(FileName).Equals(".txt", StringComparison.InvariantCultureIgnoreCase);

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(Port))
        {
            return ValidationResult.Error("Port is required.");
        }

        if (!SerialPort.GetPortNames().Contains(Port))
        {
            return ValidationResult.Error($"Port '{Port}' is not available. Available ports: {string.Join(", ", SerialPort.GetPortNames())}");
        }

        if (BaudRate != 115200 && BaudRate != 9600 && BaudRate != 4800)
        {
            return ValidationResult.Error("BaudRate must be 115200, 9600, or 4800.");
        }

        if (MaxWidth < 1)
        {
            return ValidationResult.Error("MaxWidth must be a positive integer.");
        }

        if (!File.Exists(FileName))
        {
            return ValidationResult.Error($"File '{FileName}' does not exist.");
        }

        if (!SupportedExtensions.Contains(Path.GetExtension(FileName)))
        {
            return ValidationResult.Error($"File extension of '{FileName}' is not supported. Supported extensions are: {string.Join(", ", SupportedExtensions)}.");
        }

        return ValidationResult.Success();
    }
}
