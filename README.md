# BtPrint - A simple printing app for Bluetooth pocket printers

![BtPrint Logo](Img/printer.webp)
---

BtPrint is a simple C# console app for printing text and images to Bluetooth pocket printers. It supports both Windows and Linux platforms, and can be used with a variety of Bluetooth printers.

I tested it with my SilverCrest 5890 printer, and it works well.

## Usage

* Printing text file: `BtPrint.exe -p "COM3" "C:\path\to\file.txt"`
* Printing an image: `BtPrint.exe -p "COM3" "C:\path\to\image.png"`

If you start the program without any arguments, it will print the usage instructions:

```
USAGE:
    BtPrint <filename> [OPTIONS]

EXAMPLES:
    BtPrint BtPrint -p Com3 test.jpg
    BtPrint BtPrint --port Com3 test.txt

ARGUMENTS:
    <filename>    The path to the file to print. Supported formats are .jpg, .jpeg, .bmp, .png, .txt, and .gif

OPTIONS:
    -h, --help           Prints help information
    -p, --port <PORT>    The serial port to which the printer is connected. For example, COM3 on Windows or /dev/ttyUSB0
                         on Linux
    -b, --baudrate       The baud rate for the serial connection. Supported values are 115200, 9600, and 4800. Default
                         is 115200
    -w, --max-width      The maximum width of the printed image in pixels. This setting is ignored for text files.
                         Default is 500
```

## Credits

This program uses the ESCPOS.NET Library, which is a C# library for controlling ESC/POS compatible printers. You can find the library on GitHub: https://github.com/lukevp/ESC-POS-.NET