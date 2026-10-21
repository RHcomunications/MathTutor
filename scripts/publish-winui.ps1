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
if (-not (Test-Path $project)) {
    throw "No se encontró el proyecto WinUI: $project"
}

if (-not $Version) {
    $versionNode = Select-Xml -Path $project -XPath '/Project/PropertyGroup/Version' | Select-Object -First 1
    if (-not $versionNode) {
        throw "No se encontró <Version> en $project"
    }
    $Version = $versionNode.Node.InnerText.Trim()
}

$publishDir = Join-Path $root "artifacts\publish\MathTutor.WinUI\$($Configuration.ToLower())-$Runtime"
$releaseDir = Join-Path $root 'artifacts\releases'
$zipPath = Join-Path $releaseDir "MathTutor-v$Version-winui3-$Runtime.zip"

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

Push-Location $root
try {
    & dotnet publish $project -c $Configuration -r $Runtime --self-contained true -p:Version=$Version -o $publishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish falló con código $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

$priPath = Join-Path $publishDir 'MathTutor.WinUI.pri'
if (-not (Test-Path $priPath)) {
    throw "La publicación no contiene MathTutor.WinUI.pri: $publishDir"
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -CompressionLevel Optimal -Force

$fileCount = (Get-ChildItem $publishDir -Recurse -File).Count
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
Write-Output "Publicación: $publishDir"
Write-Output "Archivos: $fileCount | .pri: presente"
Write-Output "ZIP: $zipPath ($zipSize MB)"
