# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/SPC.Stream.Emulator/*" -Force -Recurse
dotnet publish "./SPC.Stream.Emulator.csproj" -c Release -o "$env:RELOADEDIIMODS/SPC.Stream.Emulator" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location