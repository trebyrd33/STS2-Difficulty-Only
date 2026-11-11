param(
    [Parameter(Mandatory=$true)][string]$GameDirectory,
    [switch]$InstallBundledBaseLib
)
$ErrorActionPreference = "Stop"
$packageRoot = $PSScriptRoot
if (!(Test-Path -LiteralPath $GameDirectory -PathType Container)) { throw "Game directory does not exist: $GameDirectory" }
$source = Join-Path $packageRoot "mods/STS2DifficultyOnly"
$required = @("STS2DifficultyOnly.dll", "STS2DifficultyOnly.json", "STS2DifficultyOnly.pck")
foreach ($name in $required) {
    if (!(Test-Path -LiteralPath (Join-Path $source $name) -PathType Leaf)) { throw "Package file missing: $name" }
}
$mods = Join-Path $GameDirectory "mods"
$dependency = Join-Path $mods "BaseLib"
if (!(Test-Path -LiteralPath (Join-Path $dependency "BaseLib.dll"))) {
    if (!$InstallBundledBaseLib) { throw "Install BaseLib >=3.4.7 first, or pass -InstallBundledBaseLib to install the bundled local copy." }
    $dependencyArchive = Join-Path $packageRoot "dependencies/BaseLib.3.4.7.zip"
    if (!(Test-Path -LiteralPath $dependencyArchive)) { throw "Bundled BaseLib archive missing." }
    New-Item -ItemType Directory -Path $dependency -Force | Out-Null
    Expand-Archive -LiteralPath $dependencyArchive -DestinationPath $dependency -Force
}
$destination = Join-Path $mods "STS2DifficultyOnly"
if (Test-Path -LiteralPath $destination) {
    $backup = Join-Path $packageRoot ("backups/STS2DifficultyOnly-" + (Get-Date -Format "yyyyMMdd-HHmmss-fff"))
    New-Item -ItemType Directory -Path $backup -Force | Out-Null
    Copy-Item -LiteralPath $destination -Destination $backup -Recurse
    Write-Host "Previous mod files backed up to $backup"
}
New-Item -ItemType Directory -Path $destination -Force | Out-Null
foreach ($name in $required) { Copy-Item -LiteralPath (Join-Path $source $name) -Destination (Join-Path $destination $name) -Force }
Write-Host "Installed STS2 Difficulty Only 0.3.0-test1. Disable Multiplayer Expanded, enable Difficulty Only in the game loader, and configure the difficulty toggles and death mode through BaseLib before starting a fresh test run."
