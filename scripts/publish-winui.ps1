[CmdletBinding()]
param(
    [string]$Version,
    [ValidateSet('win-x64', 'win-x86', 'win-arm64')]
    [string]$Runtime = 'win-x64',
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\MathTutor.WinUI\MathTutor.WinUI.csproj'
$guidePath = Join-Path $root 'LEEME.txt'
if (-not (Test-Path $project)) {
    throw "No se encontró el proyecto WinUI: $project"
}
if (-not (Test-Path $guidePath)) {
    throw "No se encontró la guía para usuarios: $guidePath"
}

if (-not $Version) {
    $versionNode = Select-Xml -Path $project -XPath '/Project/PropertyGroup/Version' | Select-Object -First 1
    if (-not $versionNode) {
        throw "No se encontró <Version> en $project"
    }
    $Version = $versionNode.Node.InnerText.Trim()
}

$publishDir = Join-Path $root "artifacts\publish\MathTutor.WinUI\$($Configuration.ToLower())-$Runtime"
$packageRoot = Join-Path $root 'artifacts\package'
$packageDir = Join-Path $packageRoot "MathTutor-v$Version"
$releaseDir = Join-Path $root 'artifacts\releases'
$zipPath = Join-Path $releaseDir "MathTutor-v$Version-Windows.zip"

foreach ($dir in @($publishDir, $packageDir)) {
    if (Test-Path $dir) {
        Remove-Item $dir -Recurse -Force
    }
}
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

Push-Location $root
try {
    & dotnet publish $project -c $Configuration -r $Runtime --self-contained true `
        -p:Version=$Version `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:EnableMsixTooling=true `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $publishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish falló con código $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

$priPath = Join-Path $publishDir 'MathTutor.pri'
if (-not (Test-Path $priPath)) {
    throw "La publicación no contiene MathTutor.pri: $publishDir"
}

Get-ChildItem $publishDir -File |
    Where-Object { $_.Extension -ne '.pdb' } |
    Copy-Item -Destination $packageDir -Force
Copy-Item -LiteralPath $guidePath -Destination (Join-Path $packageDir 'LEEME.txt') -Force

foreach ($required in @('MathTutor.exe', 'MathTutor.pri', 'LEEME.txt')) {
    if (-not (Test-Path (Join-Path $packageDir $required))) {
        throw "El paquete no contiene $required"
    }
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path $packageDir -DestinationPath $zipPath -CompressionLevel Optimal -Force

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entries = $zip.Entries.FullName
}
finally {
    $zip.Dispose()
}
foreach ($required in @('MathTutor.exe', 'MathTutor.pri', 'LEEME.txt')) {
    $entry = "MathTutor-v$Version\$required"
    if ($entries -notcontains $entry) {
        throw "El ZIP no contiene $entry"
    }
}

$fileCount = (Get-ChildItem $packageDir -Recurse -File).Count
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
Write-Output "Paquete: $packageDir"
Write-Output "Archivos: $fileCount | .pri: presente | LEEME.txt: presente"
Write-Output "ZIP: $zipPath ($zipSize MB)"
