# AI Assistant SQL - Instalador de Modelos Avanzado (PowerShell)
# Versión: 2.0
# Autor: AI Assistant SQL

param(
    [string]$Action = "menu",
    [string[]]$Models = @(),
    [switch]$Force,
    [switch]$Quiet
)

# Configuración
$ErrorActionPreference = "Stop"
$OllamaUrl = "http://localhost:11434"
$MinDiskSpaceGB = 10

# Colores para output
function Write-ColorText {
    param(
        [string]$Text,
        [string]$Color = "White"
    )
    
    if (-not $Quiet) {
        Write-Host $Text -ForegroundColor $Color
    }
}

function Write-Success { param([string]$Text) Write-ColorText "✅ $Text" "Green" }
function Write-Info { param([string]$Text) Write-ColorText "ℹ️  $Text" "Cyan" }
function Write-Warning { param([string]$Text) Write-ColorText "⚠️  $Text" "Yellow" }
function Write-Error { param([string]$Text) Write-ColorText "❌ $Text" "Red" }

# Función para verificar Ollama
function Test-OllamaInstallation {
    Write-Info "Verificando instalación de Ollama..."
    
    try {
        $ollamaVersion = & ollama version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Ollama está instalado: $($ollamaVersion[0])"
            return $true
        }
    }
    catch {
        Write-Error "Ollama no está instalado"
        Write-Info "Descarga Ollama desde: https://ollama.ai/download"
        return $false
    }
    
    return $false
}

# Función para verificar si Ollama está ejecutándose
function Test-OllamaRunning {
    Write-Info "Verificando si Ollama está ejecutándose..."
    
    try {
        $response = Invoke-WebRequest -Uri "$OllamaUrl/api/tags" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Success "Ollama está ejecutándose correctamente"
            return $true
        }
    }
    catch {
        Write-Warning "Ollama no está ejecutándose"
        Write-Info "Iniciando Ollama en segundo plano..."
        
        try {
            Start-Process -FilePath "ollama" -ArgumentList "serve" -WindowStyle Hidden
            Start-Sleep -Seconds 5
            
            $response = Invoke-WebRequest -Uri "$OllamaUrl/api/tags" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Success "Ollama iniciado correctamente"
                return $true
            }
        }
        catch {
            Write-Error "No se pudo iniciar Ollama automáticamente"
            Write-Info "Por favor, ejecuta manualmente: ollama serve"
            return $false
        }
    }
    
    return $false
}

# Función para verificar espacio en disco
function Test-DiskSpace {
    Write-Info "Verificando espacio en disco disponible..."
    
    $drive = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'"
    $freeSpaceGB = [math]::Round($drive.FreeSpace / 1GB, 2)
    
    if ($freeSpaceGB -lt $MinDiskSpaceGB) {
        Write-Warning "Espacio disponible: $freeSpaceGB GB"
        Write-Warning "Se recomienda al menos $MinDiskSpaceGB GB libres"
        
        if (-not $Force) {
            $continue = Read-Host "¿Desea continuar? (s/N)"
            if ($continue -notmatch '^[sS]$') {
                Write-Info "Instalación cancelada por el usuario"
                exit 0
            }
        }
    }
    else {
        Write-Success "Espacio disponible: $freeSpaceGB GB"
    }
}

# Función para obtener modelos instalados
function Get-InstalledModels {
    try {
        $output = & ollama list 2>$null
        if ($LASTEXITCODE -eq 0) {
            return $output | Select-Object -Skip 1 | ForEach-Object {
                if ($_ -match '^(\S+)\s+') {
                    $matches[1]
                }
            }
        }
    }
    catch {
        return @()
    }
    
    return @()
}

# Función para instalar un modelo
function Install-OllamaModel {
    param(
        [string]$ModelName,
        [string]$Description,
        [string]$Size
    )
    
    Write-Info "Instalando modelo: $ModelName ($Description) - Tamaño: $Size"
    
    # Verificar si ya está instalado
    $installedModels = Get-InstalledModels
    if ($installedModels -contains $ModelName) {
        Write-Success "Modelo $ModelName ya está instalado"
        return $true
    }
    
    try {
        Write-Info "Descargando $ModelName... (esto puede tomar varios minutos)"
        
        $job = Start-Job -ScriptBlock {
            param($model)
            & ollama pull $model 2>&1
        } -ArgumentList $ModelName
        
        # Mostrar progreso
        $timeout = 600 # 10 minutos
        $elapsed = 0
        
        while ($job.State -eq "Running" -and $elapsed -lt $timeout) {
            Start-Sleep -Seconds 5
            $elapsed += 5
            
            if ($elapsed % 30 -eq 0) {
                Write-Info "Descargando $ModelName... ($elapsed segundos transcurridos)"
            }
        }
        
        $result = Receive-Job -Job $job -Wait
        Remove-Job -Job $job
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Modelo $ModelName instalado correctamente"
            
            # Verificar funcionamiento
            Write-Info "Probando modelo $ModelName..."
            $testResult = & ollama run $ModelName "SELECT 1 as test" --timeout 10 2>$null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Modelo $ModelName funciona correctamente"
            }
            else {
                Write-Warning "Modelo $ModelName instalado pero no responde correctamente"
            }
            
            return $true
        }
        else {
            Write-Error "Error instalando modelo $ModelName"
            Write-Info "Salida del comando: $result"
            return $false
        }
    }
    catch {
        Write-Error "Excepción instalando modelo $ModelName`: $($_.Exception.Message)"
        return $false
    }
}

# Función para mostrar menú
function Show-Menu {
    if ($Quiet) { return }
    
    Write-Host ""
    Write-Info "Modelos disponibles para instalación:"
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host "MODELOS RECOMENDADOS PARA SQL:" -ForegroundColor Yellow
    Write-Host "1. codellama:7b-code          - Especializado en código (3.8GB) 🎯" -ForegroundColor White
    Write-Host "2. codellama:13b-code         - Más preciso, más lento (7.3GB) 🎯" -ForegroundColor White
    Write-Host "3. deepseek-coder:6.7b        - Excelente para SQL (3.8GB) 🚀" -ForegroundColor White
    Write-Host "4. deepseek-coder:33b         - Máxima precisión (18GB) 🚀" -ForegroundColor White
    Write-Host ""
    Write-Host "MODELOS GENERALES RÁPIDOS:" -ForegroundColor Yellow
    Write-Host "5. llama3.2:3b               - Muy rápido, ligero (2GB) ⚡" -ForegroundColor White
    Write-Host "6. llama3.1:8b               - Balance rendimiento/tamaño (4.7GB) ⚡" -ForegroundColor White
    Write-Host "7. mistral:7b                - Eficiente y preciso (4.1GB) 🎭" -ForegroundColor White
    Write-Host "8. phi3:3.8b                 - Pequeño y eficiente (2.3GB) 📱" -ForegroundColor White
    Write-Host ""
    Write-Host "MODELOS AVANZADOS:" -ForegroundColor Yellow
    Write-Host "9. qwen2.5-coder:7b          - Nuevo modelo para código (4.2GB) 💻" -ForegroundColor White
    Write-Host "10. granite-code:8b          - IBM, optimizado para SQL (4.9GB) 💎" -ForegroundColor White
    Write-Host ""
    Write-Host "OPCIONES ESPECIALES:" -ForegroundColor Yellow
    Write-Host "11. Instalar TODOS los recomendados (1, 3, 5)" -ForegroundColor Green
    Write-Host "12. Instalar solo los LIGEROS (5, 8)" -ForegroundColor Green
    Write-Host "13. Instalar solo los ESPECIALIZADOS (1, 3)" -ForegroundColor Green
    Write-Host "14. Mostrar modelos instalados" -ForegroundColor Cyan
    Write-Host "0. Salir" -ForegroundColor Red
    Write-Host ""
}

# Definición de modelos
$ModelDefinitions = @{
    1 = @{ Name = "codellama:7b-code"; Description = "Especializado en código y SQL"; Size = "3.8GB" }
    2 = @{ Name = "codellama:13b-code"; Description = "Más preciso para consultas complejas"; Size = "7.3GB" }
    3 = @{ Name = "deepseek-coder:6.7b"; Description = "Excelente rendimiento en SQL"; Size = "3.8GB" }
    4 = @{ Name = "deepseek-coder:33b"; Description = "Máxima precisión y capacidad"; Size = "18GB" }
    5 = @{ Name = "llama3.2:3b"; Description = "Muy rápido y ligero"; Size = "2GB" }
    6 = @{ Name = "llama3.1:8b"; Description = "Balance entre velocidad y precisión"; Size = "4.7GB" }
    7 = @{ Name = "mistral:7b"; Description = "Eficiente y confiable"; Size = "4.1GB" }
    8 = @{ Name = "phi3:3.8b"; Description = "Optimizado para dispositivos pequeños"; Size = "2.3GB" }
    9 = @{ Name = "qwen2.5-coder:7b"; Description = "Nuevo modelo especializado"; Size = "4.2GB" }
    10 = @{ Name = "granite-code:8b"; Description = "IBM, optimizado para SQL"; Size = "4.9GB" }
}

# Función para instalar modelos por selección
function Install-SelectedModels {
    param([int[]]$Selections)
    
    $totalInstalled = 0
    $totalFailed = 0
    
    Write-Info "Iniciando instalación de modelos seleccionados..."
    
    foreach ($selection in $Selections) {
        if ($ModelDefinitions.ContainsKey($selection)) {
            $model = $ModelDefinitions[$selection]
            
            if (Install-OllamaModel -ModelName $model.Name -Description $model.Description -Size $model.Size) {
                $totalInstalled++
            }
            else {
                $totalFailed++
            }
            
            Write-Host "----------------------------------------" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Info "Resumen de instalación:"
    Write-Success "Modelos instalados exitosamente: $totalInstalled"
    if ($totalFailed -gt 0) {
        Write-Error "Modelos que fallaron: $totalFailed"
    }
}

# Función para instalar presets
function Install-Preset {
    param([int]$PresetNumber)
    
    switch ($PresetNumber) {
        11 {
            Write-Info "Instalando todos los modelos recomendados..."
            Install-SelectedModels -Selections @(1, 3, 5)
        }
        12 {
            Write-Info "Instalando solo modelos ligeros..."
            Install-SelectedModels -Selections @(5, 8)
        }
        13 {
            Write-Info "Instalando solo modelos especializados..."
            Install-SelectedModels -Selections @(1, 3)
        }
    }
}

# Función para mostrar modelos instalados
function Show-InstalledModels {
    Write-Info "Modelos actualmente instalados:"
    Write-Host "================================" -ForegroundColor Cyan
    
    try {
        $output = & ollama list 2>$null
        if ($LASTEXITCODE -eq 0) {
            $output | ForEach-Object { Write-Host $_ -ForegroundColor White }
            
            $modelCount = ($output | Measure-Object).Count - 1
            if ($modelCount -gt 0) {
                Write-Host ""
                Write-Success "Total de modelos instalados: $modelCount"
                Write-Host ""
                Write-Info "💡 Recomendaciones de uso:"
                Write-Host "• Para consultas rápidas y simples: llama3.2:3b" -ForegroundColor Gray
                Write-Host "• Para consultas SQL complejas: codellama:7b-code" -ForegroundColor Gray
                Write-Host "• Para máxima precisión: deepseek-coder:6.7b" -ForegroundColor Gray
                Write-Host "• Para dispositivos con poca RAM: phi3:3.8b" -ForegroundColor Gray
            }
        }
        else {
            Write-Warning "No se pueden mostrar los modelos (Ollama no disponible)"
        }
    }
    catch {
        Write-Error "Error obteniendo lista de modelos: $($_.Exception.Message)"
    }
    
    if (-not $Quiet) {
        Write-Host ""
        Read-Host "Presiona Enter para continuar"
    }
}

# Función principal
function Main {
    Write-Host ""
    Write-Success "🚀 Instalador de Modelos Ollama para SQL - Versión 2.0 (PowerShell)"
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Verificaciones previas
    if (-not (Test-OllamaInstallation)) {
        exit 1
    }
    
    if (-not (Test-OllamaRunning)) {
        exit 1
    }
    
    Test-DiskSpace
    
    # Mostrar modelos instalados
    if (-not $Quiet) {
        Show-InstalledModels
    }
    
    # Procesar acción
    switch ($Action.ToLower()) {
        "install" {
            if ($Models.Count -gt 0) {
                foreach ($modelName in $Models) {
                    $modelInfo = $ModelDefinitions.Values | Where-Object { $_.Name -eq $modelName }
                    if ($modelInfo) {
                        Install-OllamaModel -ModelName $modelInfo.Name -Description $modelInfo.Description -Size $modelInfo.Size
                    }
                    else {
                        Write-Warning "Modelo no reconocido: $modelName"
                    }
                }
            }
            else {
                Write-Error "No se especificaron modelos para instalar"
            }
        }
        "preset" {
            if ($Models.Count -gt 0) {
                $presetNumber = [int]$Models[0]
                Install-Preset -PresetNumber $presetNumber
            }
        }
        "list" {
            Show-InstalledModels
        }
        "menu" {
            # Menú interactivo
            do {
                Show-Menu
                $choice = Read-Host "Seleccione una opción (0-14)"
                
                switch ($choice) {
                    "0" { 
                        Write-Info "Saliendo del instalador..."
                        break 
                    }
                    "11" { Install-Preset -PresetNumber 11 }
                    "12" { Install-Preset -PresetNumber 12 }
                    "13" { Install-Preset -PresetNumber 13 }
                    "14" { Show-InstalledModels }
                    default {
                        $choiceInt = [int]$choice
                        if ($choiceInt -ge 1 -and $choiceInt -le 10) {
                            Install-SelectedModels -Selections @($choiceInt)
                        }
                        else {
                            Write-Warning "Opción no válida. Por favor seleccione un número del 0 al 14."
                            continue
                        }
                    }
                }
                
                if ($choice -ne "0" -and $choice -ne "14") {
                    Write-Host ""
                    $continue = Read-Host "¿Desea instalar más modelos? (s/N)"
                    if ($continue -notmatch '^[sS]$') {
                        break
                    }
                }
            } while ($choice -ne "0")
        }
    }
    
    # Resumen final
    Write-Host ""
    Write-Success "🎉 Proceso completado!"
    Write-Info "Los modelos están listos para usar en AI Assistant SQL"
    Write-Info "Puede cambiar el modelo activo desde la interfaz web en:"
    Write-Host "http://localhost:5000/Settings/ModelSelector" -ForegroundColor Yellow
    Write-Host ""
    
    Show-InstalledModels
}

# Ejecutar función principal
try {
    Main
}
catch {
    Write-Error "Error crítico: $($_.Exception.Message)"
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
    exit 1
}