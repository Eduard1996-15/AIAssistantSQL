# AI Assistant SQL - Advanced Model Installer (PowerShell)
# Version: 2.0
# Author: AI Assistant SQL

param(
    [string]$Action = "menu",
    [string[]]$Models = @(),
    [switch]$Force,
    [switch]$Quiet
)

# Configuration
$ErrorActionPreference = "Stop"
$OllamaUrl = "http://localhost:11434"
$MinDiskSpaceGB = 10

# Color output functions
function Write-ColorText {
    param(
        [string]$Text,
        [string]$Color = "White"
    )
    
    if (-not $Quiet) {
        Write-Host $Text -ForegroundColor $Color
    }
}

function Write-Success { param([string]$Text) Write-ColorText "[SUCCESS] $Text" "Green" }
function Write-Info { param([string]$Text) Write-ColorText "[INFO] $Text" "Cyan" }
function Write-Warning { param([string]$Text) Write-ColorText "[WARNING] $Text" "Yellow" }
function Write-Error { param([string]$Text) Write-ColorText "[ERROR] $Text" "Red" }

# Function to check Ollama installation
function Test-OllamaInstallation {
    Write-Info "Checking Ollama installation..."
    
    try {
        $ollamaVersion = & ollama version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Ollama is correctly installed"
            return $true
        }
    }
    catch {
        Write-Error "Ollama is not installed"
        Write-Info "Download Ollama from: https://ollama.ai/download"
        return $false
    }
    
    return $false
}

# Function to check if Ollama is running
function Test-OllamaRunning {
    Write-Info "Checking if Ollama is running..."
    
    try {
        $response = Invoke-WebRequest -Uri "$OllamaUrl/api/tags" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Success "Ollama is running correctly"
            return $true
        }
    }
    catch {
        Write-Warning "Ollama is not running"
        Write-Info "Starting Ollama in background..."
        
        try {
            Start-Process -FilePath "ollama" -ArgumentList "serve" -WindowStyle Hidden
            Start-Sleep -Seconds 5
            
            $response = Invoke-WebRequest -Uri "$OllamaUrl/api/tags" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Success "Ollama started successfully"
                return $true
            }
        }
        catch {
            Write-Error "Could not start Ollama automatically"
            Write-Info "Please run manually: ollama serve"
            return $false
        }
    }
    
    return $false
}

# Function to check disk space
function Test-DiskSpace {
    Write-Info "Checking available disk space..."
    
    $drive = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'"
    $freeSpaceGB = [math]::Round($drive.FreeSpace / 1GB, 2)
    
    if ($freeSpaceGB -lt $MinDiskSpaceGB) {
        Write-Warning "Available space: $freeSpaceGB GB"
        Write-Warning "Recommended at least $MinDiskSpaceGB GB free"
        
        if (-not $Force) {
            $continue = Read-Host "Do you want to continue? (y/N)"
            if ($continue -notmatch '^[yY]$') {
                Write-Info "Installation canceled by user"
                exit 0
            }
        }
    }
    else {
        Write-Success "Available space: $freeSpaceGB GB"
    }
}

# Function to get installed models
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

# Function to install a model
function Install-OllamaModel {
    param(
        [string]$ModelName,
        [string]$Description,
        [string]$Size
    )
    
    Write-Info "Installing model: $ModelName ($Description) - Size: $Size"
    
    # Check if already installed
    $installedModels = Get-InstalledModels
    if ($installedModels -contains $ModelName) {
        Write-Success "Model $ModelName is already installed"
        return $true
    }
    
    try {
        Write-Info "Downloading $ModelName... (this may take several minutes)"
        
        $job = Start-Job -ScriptBlock {
            param($model)
            & ollama pull $model 2>&1
        } -ArgumentList $ModelName
        
        # Show progress
        $timeout = 600 # 10 minutes
        $elapsed = 0
        
        while ($job.State -eq "Running" -and $elapsed -lt $timeout) {
            Start-Sleep -Seconds 5
            $elapsed += 5
            
            if ($elapsed % 30 -eq 0) {
                Write-Info "Downloading $ModelName... $elapsed seconds elapsed"
            }
        }
        
        $result = Receive-Job -Job $job -Wait
        Remove-Job -Job $job
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Model $ModelName installed successfully"
            
            # Test functionality
            Write-Info "Testing model $ModelName..."
            $testResult = & ollama run $ModelName "SELECT 1 as test" --timeout 10 2>$null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Model $ModelName works correctly"
            }
            else {
                Write-Warning "Model $ModelName installed but not responding correctly"
            }
            
            return $true
        }
        else {
            Write-Error "Error installing model $ModelName"
            return $false
        }
    }
    catch {
        Write-Error "Exception installing model $ModelName"
        return $false
    }
}

# Function to show menu
function Show-Menu {
    if ($Quiet) { return }
    
    Write-Host ""
    Write-Info "Available models for installation:"
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host "RECOMMENDED MODELS FOR SQL:" -ForegroundColor Yellow
    Write-Host "1. codellama:7b-code          - Code specialized (3.8GB)" -ForegroundColor White
    Write-Host "2. codellama:13b-code         - More precise, slower (7.3GB)" -ForegroundColor White
    Write-Host "3. deepseek-coder:6.7b        - Excellent for SQL (3.8GB)" -ForegroundColor White
    Write-Host "4. deepseek-coder:33b         - Maximum precision (18GB)" -ForegroundColor White
    Write-Host ""
    Write-Host "FAST GENERAL MODELS:" -ForegroundColor Yellow
    Write-Host "5. llama3.2:3b               - Very fast, lightweight (2GB)" -ForegroundColor White
    Write-Host "6. llama3.1:8b               - Balance performance/size (4.7GB)" -ForegroundColor White
    Write-Host "7. mistral:7b                - Efficient and precise (4.1GB)" -ForegroundColor White
    Write-Host "8. phi3:3.8b                 - Small and efficient (2.3GB)" -ForegroundColor White
    Write-Host ""
    Write-Host "ADVANCED MODELS:" -ForegroundColor Yellow
    Write-Host "9. qwen2.5-coder:7b          - New model for code (4.2GB)" -ForegroundColor White
    Write-Host "10. granite-code:8b          - IBM, SQL optimized (4.9GB)" -ForegroundColor White
    Write-Host ""
    Write-Host "SPECIAL OPTIONS:" -ForegroundColor Yellow
    Write-Host "11. Install ALL recommended (1, 3, 5)" -ForegroundColor Green
    Write-Host "12. Install only LIGHTWEIGHT (5, 8)" -ForegroundColor Green
    Write-Host "13. Install only SPECIALIZED (1, 3)" -ForegroundColor Green
    Write-Host "14. Show installed models" -ForegroundColor Cyan
    Write-Host "0. Exit" -ForegroundColor Red
    Write-Host ""
}

# Model definitions
$ModelDefinitions = @{
    1 = @{ Name = "codellama:7b-code"; Description = "Code and SQL specialized"; Size = "3.8GB" }
    2 = @{ Name = "codellama:13b-code"; Description = "More precise for complex queries"; Size = "7.3GB" }
    3 = @{ Name = "deepseek-coder:6.7b"; Description = "Excellent SQL performance"; Size = "3.8GB" }
    4 = @{ Name = "deepseek-coder:33b"; Description = "Maximum precision and capability"; Size = "18GB" }
    5 = @{ Name = "llama3.2:3b"; Description = "Very fast and lightweight"; Size = "2GB" }
    6 = @{ Name = "llama3.1:8b"; Description = "Balance between speed and precision"; Size = "4.7GB" }
    7 = @{ Name = "mistral:7b"; Description = "Efficient and reliable"; Size = "4.1GB" }
    8 = @{ Name = "phi3:3.8b"; Description = "Optimized for small devices"; Size = "2.3GB" }
    9 = @{ Name = "qwen2.5-coder:7b"; Description = "New specialized model"; Size = "4.2GB" }
    10 = @{ Name = "granite-code:8b"; Description = "IBM, SQL optimized"; Size = "4.9GB" }
}

# Function to install models by selection
function Install-SelectedModels {
    param([int[]]$Selections)
    
    $totalInstalled = 0
    $totalFailed = 0
    
    Write-Info "Starting installation of selected models..."
    
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
    Write-Info "Installation summary:"
    Write-Success "Models installed successfully: $totalInstalled"
    if ($totalFailed -gt 0) {
        Write-Error "Models that failed: $totalFailed"
    }
}

# Function to install presets
function Install-Preset {
    param([int]$PresetNumber)
    
    switch ($PresetNumber) {
        11 {
            Write-Info "Installing all recommended models..."
            Install-SelectedModels -Selections @(1, 3, 5)
        }
        12 {
            Write-Info "Installing only lightweight models..."
            Install-SelectedModels -Selections @(5, 8)
        }
        13 {
            Write-Info "Installing only specialized models..."
            Install-SelectedModels -Selections @(1, 3)
        }
    }
}

# Function to show installed models
function Show-InstalledModels {
    Write-Info "Currently installed models:"
    Write-Host "================================" -ForegroundColor Cyan
    
    try {
        $output = & ollama list 2>$null
        if ($LASTEXITCODE -eq 0) {
            $output | ForEach-Object { Write-Host $_ -ForegroundColor White }
            
            $modelCount = ($output | Measure-Object).Count - 1
            if ($modelCount -gt 0) {
                Write-Host ""
                Write-Success "Total installed models: $modelCount"
                Write-Host ""
                Write-Info "Usage recommendations:"
                Write-Host "• For fast queries: llama3.2:3b" -ForegroundColor Gray
                Write-Host "• For complex SQL queries: codellama:7b-code" -ForegroundColor Gray
                Write-Host "• For maximum precision: deepseek-coder:6.7b" -ForegroundColor Gray
                Write-Host "• For low RAM devices: phi3:3.8b" -ForegroundColor Gray
            }
        }
        else {
            Write-Warning "Cannot show models (Ollama not available)"
        }
    }
    catch {
        Write-Error "Error getting model list"
    }
    
    if (-not $Quiet) {
        Write-Host ""
        Read-Host "Press Enter to continue"
    }
}

# Main function
function Main {
    Write-Host ""
    Write-Success "Ollama SQL Model Installer - Version 2.0 (PowerShell)"
    Write-Host "=======================================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Previous checks
    if (-not (Test-OllamaInstallation)) {
        exit 1
    }
    
    if (-not (Test-OllamaRunning)) {
        exit 1
    }
    
    Test-DiskSpace
    
    # Show installed models
    if (-not $Quiet) {
        Show-InstalledModels
    }
    
    # Process action
    switch ($Action.ToLower()) {
        "install" {
            if ($Models.Count -gt 0) {
                foreach ($modelName in $Models) {
                    $modelInfo = $ModelDefinitions.Values | Where-Object { $_.Name -eq $modelName }
                    if ($modelInfo) {
                        Install-OllamaModel -ModelName $modelInfo.Name -Description $modelInfo.Description -Size $modelInfo.Size
                    }
                    else {
                        Write-Warning "Unrecognized model: $modelName"
                    }
                }
            }
            else {
                Write-Error "No models specified for installation"
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
            # Interactive menu
            do {
                Show-Menu
                $choice = Read-Host "Select an option (0-14)"
                
                switch ($choice) {
                    "0" { 
                        Write-Info "Exiting installer..."
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
                                Write-Warning "Invalid option. Select a number from 0 to 14."
                                continue
                            }
                        }
                        catch {
                            Write-Warning "Invalid input. Enter a number."
                            continue
                        }
                    }
                }
                
                if ($choice -ne "0" -and $choice -ne "14") {
                    Write-Host ""
                    $continue = Read-Host "Do you want to install more models? (y/N)"
                    if ($continue -notmatch '^[yY]$') {
                        break
                    }
                }
            } while ($choice -ne "0")
        }
    }
    
    # Final summary
    Write-Host ""
    Write-Success "Process completed!"
    Write-Info "Models are ready to use in AI Assistant SQL"
    Write-Info "You can change the active model from the web interface at:"
    Write-Host "http://localhost:5000/Settings/ModelSelector" -ForegroundColor Yellow
    Write-Host ""
    
    Show-InstalledModels
}

# Execute main function
try {
    Main
}
catch {
    Write-Error "Critical error: $($_.Exception.Message)"
    exit 1
}