

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
# --- Configuration ---
$ProjectName  = "Pipboy.Avalonia.Demo.Browser"
$ProjectPath  = "$ProjectName.csproj"
$OutputDir    = "publish"
$Framework    = "net10.0-browser"
$Configuration = "Release"

# --- Publish ---
Write-Host "Publishing $ProjectName..."

dotnet publish $ProjectPath `
    -c $Configuration `
    -f $Framework `
    -o $OutputDir `
    -p:RunAOTCompilation=true `
    -p:WasmBuildNative=true `
    -p:PublishTrimmed=true `
    -p:TrimMode=full `
    -p:InvariantGlobalization=true `
    -p:DebuggerSupport=false

Write-Host "Done -> $OutputDir"
Write-Host "Starting local server..." -ForegroundColor Cyan
dotnet serve -d $OutputDir -p 8080 -o

Read-Host "Press Enter to exit"