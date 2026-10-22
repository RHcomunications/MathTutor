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
$privacyPath = Join-Path $root 'PRIVACIDAD.md'
$bibliographyPath = Join-Path $root 'BIBLIOGRAFIA.md'
$launcherSource = Join-Path $root 'scripts\launcher\MathTutorLauncher.cs'
$iconPath = Join-Path $root 'src\MathTutor.WinUI\Assets\AppIcon.ico'
if (-not (Test-Path $project)) {
    throw "No se encontró el proyecto WinUI: $project"
}
if (-not (Test-Path $guidePath)) {
    throw "No se encontró la guía para usuarios: $guidePath"
}
if (-not (Test-Path $privacyPath)) {
    throw "No se encontró el aviso de privacidad: $privacyPath"
}
if (-not (Test-Path $bibliographyPath)) {
    throw "No se encontró la bibliografía: $bibliographyPath"
}
if (-not (Test-Path $launcherSource)) {
    throw "No se encontró el lanzador: $launcherSource"
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
$libsDir = Join-Path $packageDir 'libs'
$modulesDir = Join-Path $packageDir 'modules'
$releaseDir = Join-Path $root 'artifacts\releases'
$zipPath = Join-Path $releaseDir "MathTutor-v$Version-Windows.zip"
$launcherPath = Join-Path $packageDir 'MathTutor.exe'

foreach ($dir in @($publishDir, $packageDir)) {
    if (Test-Path $dir) {
        Remove-Item $dir -Recurse -Force
    }
}
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $libsDir -Force | Out-Null
New-Item -ItemType Directory -Path $modulesDir -Force | Out-Null
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

Push-Location $root
try {
    & dotnet publish $project -c $Configuration -r $Runtime --self-contained true `
        -p:Version=$Version `
        -p:EnableMsixTooling=false `
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

Copy-Item -Path (Join-Path $publishDir '*') -Destination $libsDir -Recurse -Force
Get-ChildItem $libsDir -File -Filter '*.winmd' | Move-Item -Destination $modulesDir
Get-ChildItem $libsDir -File -Filter '*.pdb' | Remove-Item -Force
Copy-Item -LiteralPath $guidePath -Destination (Join-Path $packageDir 'LEEME.txt') -Force
Copy-Item -LiteralPath $privacyPath -Destination (Join-Path $packageDir 'PRIVACIDAD.md') -Force
Copy-Item -LiteralPath $bibliographyPath -Destination (Join-Path $packageDir 'BIBLIOGRAFIA.md') -Force

$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path $csc)) {
    throw "No se encontró el compilador de C# de .NET Framework (csc.exe)"
}

$cscArgs = @(
    '/nologo'
    '/target:winexe'
    '/optimize+'
    "/out:$launcherPath"
)
if (Test-Path $iconPath) {
    $cscArgs += "/win32icon:$iconPath"
}
$cscArgs += $launcherSource

& $csc @cscArgs
if ($LASTEXITCODE -ne 0) {
    throw "No se pudo compilar el lanzador MathTutor.exe"
}

foreach ($required in @('MathTutor.exe', 'LEEME.txt', 'PRIVACIDAD.md', 'BIBLIOGRAFIA.md', 'libs\MathTutor.exe', 'libs\MathTutor.pri')) {
    if (-not (Test-Path (Join-Path $packageDir $required))) {
        throw "El paquete no contiene $required"
    }
}
$moduleCount = (Get-ChildItem $modulesDir -File -Filter '*.winmd').Count
if ($moduleCount -eq 0) {
    throw "El paquete no contiene módulos .winmd"
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
foreach ($required in @('MathTutor.exe', 'LEEME.txt', 'PRIVACIDAD.md', 'BIBLIOGRAFIA.md', 'libs\MathTutor.exe', 'libs\MathTutor.pri')) {
    $entry = "MathTutor-v$Version\$required"
    if ($entries -notcontains $entry) {
        throw "El ZIP no contiene $entry"
    }
}

$fileCount = (Get-ChildItem $packageDir -Recurse -File).Count
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
Write-Output "Paquete: $packageDir"
Write-Output "Archivos: $fileCount | Lanzador: MathTutor.exe | App: libs\MathTutor.exe | Módulos: $moduleCount | LEEME.txt, PRIVACIDAD.md y BIBLIOGRAFIA.md: presentes"
Write-Output "ZIP: $zipPath ($zipSize MB)"
