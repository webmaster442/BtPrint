dotnet publish Src\BtPrint\BtPrint.csproj `
    -c Release `
    -o Publish\Windows-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -r win-x64 `
    -p:EnableCompressionInSingleFile=true

dotnet publish Src\BtPrint\BtPrint.csproj `
    -c Release `
    -o Publish\Linux-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -r linux-x64 `
     -p:EnableCompressionInSingleFile=true

Compress-Archive `
    -Path "Publish\Windows-x64\*" `
    -DestinationPath "Publish\BtPrint-Windows-x64.zip" `
    -Force

Compress-Archive `
    -Path "Publish\Linux-x64\*" `
    -DestinationPath "Publish\BtPrint-Linux-x64.zip" `
    -Force