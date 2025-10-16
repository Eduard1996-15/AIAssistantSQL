# AI Assistant SQL - Instalador de Modelos Avanzado (PowerShell)
# Version: 2.0
# Autor: AI Assistant SQL

param(
    [string]$Action = "menu",
    [string[]]$Models = @(),
    [switch]$Force,
    [switch]$Quiet
)

# Configuracion
$ErrorActionPreference = "Stop"
$OllamaUrl = "http://localhost:11434"
$MinDiskSpaceGB = 10

# Funciones de output con colores
function Write-ColorText {
    param(
        [string]$Text,
        [string]$Color = "White"
    )
    
    if (-not $Quiet) {
        Write-Host $Text -ForegroundColor $Color
    }
}

function Write-Success { param([string]$Text) Write-ColorText "✓ $Text" "Green" }
function Write-Info { param([string]$Text) Write-ColorText "i $Text" "Cyan" }
function Write-Warning { param([string]$Text) Write-ColorText "! $Text" "Yellow" }
function Write-Error { param([string]$Text) Write-ColorText "X $Text" "Red" }

# Funcion para verificar Ollama
function Test-OllamaInstallation {
    Write-Info "Verificando instalacion de Ollama..."
    
    try {
        $ollamaVersion = & ollama version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Ollama esta instalado correctamente"
            return $true
        }
    }
    catch {
        Write-Error "Ollama no esta instalado"
        Write-Info "Descarga Ollama desde: https://ollama.ai/download"
        return $false
    }
    
    return $false
}

# Funcion para verificar si Ollama esta ejecutandose
function Test-OllamaRunning {
    Write-Info "Verificando si Ollama esta ejecutandose..."
    
    try {
        $response = Invoke-WebRequest -Uri "$OllamaUrl/api/tags" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Success "Ollama esta ejecutandose correctamente"
            return $true
        }
    }
    catch {
        Write-Warning "Ollama no esta ejecutandose"
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
            Write-Error "No se pudo iniciar Ollama automaticamente"
            Write-Info "Por favor, ejecuta manualmente: ollama serve"
            return $false
        }
    }
    
    return $false
}

# Funcion para verificar espacio en disco
function Test-DiskSpace {
    Write-Info "Verificando espacio en disco disponible..."
    
    $drive = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'"
    $freeSpaceGB = [math]::Round($drive.FreeSpace / 1GB, 2)
    
    if ($freeSpaceGB -lt $MinDiskSpaceGB) {
        Write-Warning "Espacio disponible: $freeSpaceGB GB"
        Write-Warning "Se recomienda al menos $MinDiskSpaceGB GB libres"
        
        if (-not $Force) {
            $continue = Read-Host "Desea continuar? (s/N)"
            if ($continue -notmatch '^[sS]$') {
                Write-Info "Instalacion cancelada por el usuario"
                exit 0
            }
        }
    }
    else {
        Write-Success "Espacio disponible: $freeSpaceGB GB"
    }
}

# Funcion para obtener modelos instalados
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

# Funcion para instalar un modelo
function Install-OllamaModel {
    param(
        [string]$ModelName,
        [string]$Description,
        [string]$Size
    )
    
    Write-Info "Instalando modelo: $ModelName ($Description) - Tamano: $Size"
    
    # Verificar si ya esta instalado
    $installedModels = Get-InstalledModels
    if ($installedModels -contains $ModelName) {
        Write-Success "Modelo $ModelName ya esta instalado"
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
            return $false
        }
    }
    catch {
        Write-Error "Excepcion instalando modelo $ModelName"
        return $false
    }
}

# Funcion para mostrar menu
function Show-Menu {
    if ($Quiet) { return }
    
    Write-Host ""
    Write-Info "Modelos disponibles para instalacion:"
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host "MODELOS RECOMENDADOS PARA SQL:" -ForegroundColor Yellow
    Write-Host "1. codellama:7b-code          - Especializado en codigo (3.8GB)" -ForegroundColor White
    Write-Host "2. codellama:13b-code         - Mas preciso, mas lento (7.3GB)" -ForegroundColor White
    Write-Host "3. deepseek-coder:6.7b        - Excelente para SQL (3.8GB)" -ForegroundColor White
    Write-Host "4. deepseek-coder:33b         - Maxima precision (18GB)" -ForegroundColor White
    Write-Host ""
    Write-Host "MODELOS GENERALES RAPIDOS:" -ForegroundColor Yellow
    Write-Host "5. llama3.2:3b               - Muy rapido, ligero (2GB)" -ForegroundColor White
    Write-Host "6. llama3.1:8b               - Balance rendimiento/tamano (4.7GB)" -ForegroundColor White
    Write-Host "7. mistral:7b                - Eficiente y preciso (4.1GB)" -ForegroundColor White
    Write-Host "8. phi3:3.8b                 - Pequeno y eficiente (2.3GB)" -ForegroundColor White
    Write-Host ""
    Write-Host "MODELOS AVANZADOS:" -ForegroundColor Yellow
    Write-Host "9. qwen2.5-coder:7b          - Nuevo modelo para codigo (4.2GB)" -ForegroundColor White
    Write-Host "10. granite-code:8b          - IBM, optimizado para SQL (4.9GB)" -ForegroundColor White
    Write-Host ""
    Write-Host "OPCIONES ESPECIALES:" -ForegroundColor Yellow
    Write-Host "11. Instalar TODOS los recomendados (1, 3, 5)" -ForegroundColor Green
    Write-Host "12. Instalar solo los LIGEROS (5, 8)" -ForegroundColor Green
    Write-Host "13. Instalar solo los ESPECIALIZADOS (1, 3)" -ForegroundColor Green
    Write-Host "14. Mostrar modelos instalados" -ForegroundColor Cyan
    Write-Host "0. Salir" -ForegroundColor Red
    Write-Host ""
}

# Definicion de modelos
$ModelDefinitions = @{
    1 = @{ Name = "codellama:7b-code"; Description = "Especializado en codigo y SQL"; Size = "3.8GB" }
    2 = @{ Name = "codellama:13b-code"; Description = "Mas preciso para consultas complejas"; Size = "7.3GB" }
    3 = @{ Name = "deepseek-coder:6.7b"; Description = "Excelente rendimiento en SQL"; Size = "3.8GB" }
    4 = @{ Name = "deepseek-coder:33b"; Description = "Maxima precision y capacidad"; Size = "18GB" }
    5 = @{ Name = "llama3.2:3b"; Description = "Muy rapido y ligero"; Size = "2GB" }
    6 = @{ Name = "llama3.1:8b"; Description = "Balance entre velocidad y precision"; Size = "4.7GB" }
    7 = @{ Name = "mistral:7b"; Description = "Eficiente y confiable"; Size = "4.1GB" }
    8 = @{ Name = "phi3:3.8b"; Description = "Optimizado para dispositivos pequenos"; Size = "2.3GB" }
    9 = @{ Name = "qwen2.5-coder:7b"; Description = "Nuevo modelo especializado"; Size = "4.2GB" }
    10 = @{ Name = "granite-code:8b"; Description = "IBM, optimizado para SQL"; Size = "4.9GB" }
}

# Funcion para instalar modelos por seleccion
function Install-SelectedModels {
    param([int[]]$Selections)
    
    $totalInstalled = 0
    $totalFailed = 0
    
    Write-Info "Iniciando instalacion de modelos seleccionados..."
    
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
    Write-Info "Resumen de instalacion:"
    Write-Success "Modelos instalados exitosamente: $totalInstalled"
    if ($totalFailed -gt 0) {
        Write-Error "Modelos que fallaron: $totalFailed"
    }
}

# Funcion para instalar presets
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

# Funcion para mostrar modelos instalados
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
                Write-Info "Recomendaciones de uso:"
                Write-Host "• Para consultas rapidas: llama3.2:3b" -ForegroundColor Gray
                Write-Host "• Para consultas SQL complejas: codellama:7b-code" -ForegroundColor Gray
                Write-Host "• Para maxima precision: deepseek-coder:6.7b" -ForegroundColor Gray
                Write-Host "• Para dispositivos con poca RAM: phi3:3.8b" -ForegroundColor Gray
            }
        }
        else {
            Write-Warning "No se pueden mostrar los modelos (Ollama no disponible)"
        }
    }
    catch {
        Write-Error "Error obteniendo lista de modelos"
    }
    
    if (-not $Quiet) {
        Write-Host ""
        Read-Host "Presiona Enter para continuar"
    }
}

# Funcion principal
function Main {
    Write-Host ""
    Write-Success "Instalador de Modelos Ollama para SQL - Version 2.0 (PowerShell)"
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
    
    # Procesar accion
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
            # Menu interactivo
            do {
                Show-Menu
                $choice = Read-Host "Seleccione una opcion (0-14)"
                
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
                        try {
                            $choiceInt = [int]$choice
                            if ($choiceInt -ge 1 -and $choiceInt -le 10) {
                                Install-SelectedModels -Selections @($choiceInt)
                            }
                            else {
                                Write-Warning "Opcion no valida. Seleccione un numero del 0 al 14."
                                continue
                            }
                        }
                        catch {
                            Write-Warning "Entrada no valida. Ingrese un numero."
                            continue
                        }
                    }
                }
                
                if ($choice -ne "0" -and $choice -ne "14") {
                    Write-Host ""
                    $continue = Read-Host "Desea instalar mas modelos? (s/N)"
                    if ($continue -notmatch '^[sS]$') {
                        break
                    }
                }
            } while ($choice -ne "0")
        }
    }
    
    # Resumen final
    Write-Host ""
    Write-Success "Proceso completado!"
    Write-Info "Los modelos estan listos para usar en AI Assistant SQL"
    Write-Info "Puede cambiar el modelo activo desde la interfaz web en:"
    Write-Host "http://localhost:5000/Settings/ModelSelector" -ForegroundColor Yellow
    Write-Host ""
    
    Show-InstalledModels
}

# Ejecutar funcion principal
try {
    Main
}
catch {
    Write-Error "Error critico: $($_.Exception.Message)"
    exit 1
}