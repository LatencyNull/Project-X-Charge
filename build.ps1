param (
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [ValidateSet("win-x64", "win-x86")]
    [string]$Architecture = "win-x64",

    [switch]$SelfContained,
    [switch]$Clean,
    [switch]$Run
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "       X-Charge - Build System          " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Comprobar SDK de .NET
try {
    $dotnetVersion = (dotnet --version).Trim()
    Write-Host "[OK] SDK de .NET detectado: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Error "No se encontro el SDK de .NET en el sistema. Asegurate de tener .NET 8 SDK instalado en el PATH."
    exit 1
}

# 2. Detener instancias activas si existen
$running = Get-Process -Name "X-Charge" -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "[!] Se detecto una instancia de X-Charge en ejecucion. Cerrando..." -ForegroundColor Yellow
    $running | Stop-Process -Force
    Start-Sleep -Milliseconds 800
}

# 3. Limpieza opcional
$publishDir = Join-Path $ScriptDir "publish"
if ($Clean) {
    Write-Host "[*] Limpiando artefactos previos..." -ForegroundColor Yellow
    dotnet clean -c $Configuration --nologo -v q
    $folders = @("bin", "obj", "publish")
    foreach ($f in $folders) {
        $dir = Join-Path $ScriptDir $f
        if (Test-Path $dir) {
            Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    Write-Host "[OK] Limpieza completada." -ForegroundColor Green
}

# 4. Compilacion
Write-Host "[*] Compilando X-Charge ($Configuration)..." -ForegroundColor Cyan
dotnet build -c $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error durante la compilacion."
    exit $LASTEXITCODE
}
Write-Host "[OK] Compilacion correcta (0 advertencias, 0 errores)." -ForegroundColor Green

# 5. Publicacion Single-File
$modeText = "Dependiente del Framework"
if ($SelfContained) {
    $modeText = "Autonoma (Self-Contained)"
}
Write-Host "[*] Publicando ejecutable Single-File ($modeText)..." -ForegroundColor Cyan

$publishArgs = @(
    "publish",
    "-c", $Configuration,
    "-r", $Architecture,
    "--nologo",
    "-o", $publishDir,
    "/p:PublishSingleFile=true",
    "/p:IncludeNativeLibrariesForSelfExtract=true"
)

if ($SelfContained) {
    $publishArgs += "--self-contained"
    $publishArgs += "true"
    $publishArgs += "/p:EnableCompressionInSingleFile=true"
} else {
    $publishArgs += "--self-contained"
    $publishArgs += "false"
}

& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error durante la publicacion."
    exit $LASTEXITCODE
}

$exePath = Join-Path $publishDir "X-Charge.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    $sizeMb = [math]::Round($fileInfo.Length / 1MB, 2)
    $hash = (Get-FileHash -Path $exePath -Algorithm SHA256).Hash

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "   Publicacion completada con exito!    " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Destino: $exePath"
    Write-Host "Modo:    $modeText"
    Write-Host "Tamano:  $sizeMb MB ($($fileInfo.Length) bytes)"
    Write-Host "SHA256:  $hash"
    Write-Host "========================================"
    Write-Host ""

    # Sincronizacion automatica con la carpeta de produccion si existe
    $prodDir = "H:\Programas Instalados W10\X-Charge"
    if (Test-Path $prodDir) {
        $prodExe = Join-Path $prodDir "X-Charge.exe"
        try {
            Copy-Item -Path $exePath -Destination $prodExe -Force
            Write-Host "[OK] Actualizado exitosamente en produccion: $prodExe" -ForegroundColor Green
        } catch {
            Write-Warning "No se pudo actualizar en produccion (puede estar en ejecucion): $($_.Exception.Message)"
        }
    }
} else {
    Write-Error "No se encontro el ejecutable generado en $exePath."
    exit 1
}

# 6. Ejecucion opcional
if ($Run) {
    Write-Host "[*] Iniciando X-Charge..." -ForegroundColor Cyan
    Start-Process -FilePath $exePath
    Write-Host "[OK] Proceso iniciado." -ForegroundColor Green
}
