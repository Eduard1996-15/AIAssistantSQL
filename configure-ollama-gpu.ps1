# ========================================
# Script de Optimización GPU para Ollama
# RTX 4070 + DeepSeek-Coder:33b
# ========================================

Write-Host "🚀 Configurando Ollama para RTX 4070..." -ForegroundColor Green
Write-Host ""

# 1. Cerrar Ollama si está ejecutándose
Write-Host "1️⃣ Cerrando Ollama..." -ForegroundColor Yellow
Get-Process ollama -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

# 2. Configurar variables de entorno para esta sesión
Write-Host "2️⃣ Configurando variables de entorno..." -ForegroundColor Yellow

$env:OLLAMA_NUM_GPU = "1"
$env:OLLAMA_GPU_OVERHEAD = "0"
$env:OLLAMA_MAX_LOADED_MODELS = "1"
$env:OLLAMA_NUM_PARALLEL = "1"
$env:CUDA_VISIBLE_DEVICES = "0"
$env:OLLAMA_KEEP_ALIVE = "30m"
$env:OLLAMA_NUM_THREAD = "8"

Write-Host "   ✅ OLLAMA_NUM_GPU = 1" -ForegroundColor Green
Write-Host "   ✅ OLLAMA_GPU_OVERHEAD = 0" -ForegroundColor Green
Write-Host "   ✅ OLLAMA_MAX_LOADED_MODELS = 1" -ForegroundColor Green
Write-Host "   ✅ OLLAMA_NUM_PARALLEL = 1" -ForegroundColor Green
Write-Host "   ✅ OLLAMA_KEEP_ALIVE = 30m" -ForegroundColor Green
Write-Host "   ✅ OLLAMA_NUM_THREAD = 8" -ForegroundColor Green
Write-Host ""

# 3. Verificar NVIDIA GPU
Write-Host "3️⃣ Verificando GPU NVIDIA..." -ForegroundColor Yellow
try {
    $nvidiaInfo = nvidia-smi --query-gpu=name,memory.total --format=csv,noheader 2>$null
    if ($nvidiaInfo) {
        Write-Host "   ✅ GPU detectada: $nvidiaInfo" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️ GPU no detectada con nvidia-smi" -ForegroundColor Red
        Write-Host "   Verifica drivers NVIDIA instalados" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ⚠️ nvidia-smi no disponible" -ForegroundColor Red
}
Write-Host ""

# 4. Iniciar Ollama con configuración GPU
Write-Host "4️⃣ Iniciando Ollama con configuración GPU..." -ForegroundColor Yellow
Write-Host "   Ejecutando: ollama serve" -ForegroundColor Cyan
Write-Host ""
Write-Host "   ⏳ Espera 5 segundos para que Ollama inicie..." -ForegroundColor Yellow
Write-Host ""

# Iniciar Ollama en segundo plano
Start-Process -FilePath "ollama" -ArgumentList "serve" -WindowStyle Hidden

Start-Sleep -Seconds 5

# 5. Verificar que Ollama está corriendo
Write-Host "5️⃣ Verificando Ollama..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434/api/tags" -Method GET -TimeoutSec 3 -UseBasicParsing
    Write-Host "   ✅ Ollama está corriendo correctamente" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Ollama no responde en http://localhost:11434" -ForegroundColor Red
    Write-Host "   Verifica que Ollama esté instalado correctamente" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# 6. Pre-cargar el modelo
Write-Host "6️⃣ Pre-cargando DeepSeek-Coder:33b en GPU..." -ForegroundColor Yellow
Write-Host "   Este proceso puede tardar 30-60 segundos..." -ForegroundColor Cyan
Write-Host ""

try {
    # Enviar una consulta simple para cargar el modelo
    $body = @{
        model = "deepseek-coder:33b"
        prompt = "SELECT 1"
        stream = $false
    } | ConvertTo-Json

    Write-Host "   ⏳ Cargando modelo en GPU (20GB)..." -ForegroundColor Yellow
    
    $response = Invoke-RestMethod -Uri "http://localhost:11434/api/generate" `
        -Method POST `
        -Body $body `
        -ContentType "application/json" `
        -TimeoutSec 120
    
    Write-Host "   ✅ Modelo cargado exitosamente en GPU" -ForegroundColor Green
} catch {
    Write-Host "   ⚠️ Error cargando modelo: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   El modelo se cargará en la primera consulta" -ForegroundColor Yellow
}
Write-Host ""

# 7. Verificar uso de GPU
Write-Host "7️⃣ Verificando uso de GPU..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $ollamaPs = ollama ps 2>$null
    Write-Host ""
    Write-Host "   📊 Estado de Ollama:" -ForegroundColor Cyan
    Write-Host "   $ollamaPs" -ForegroundColor White
    Write-Host ""
    
    if ($ollamaPs -match "(\d+)%/(\d+)% CPU/GPU") {
        $cpuPercent = $Matches[1]
        $gpuPercent = $Matches[2]
        
        if ([int]$gpuPercent -gt 50) {
            Write-Host "   ✅ GPU optimizada: $gpuPercent% GPU" -ForegroundColor Green
        } elseif ([int]$gpuPercent -gt 0) {
            Write-Host "   ⚠️ GPU en uso pero no optimizada: $gpuPercent% GPU" -ForegroundColor Yellow
            Write-Host "   Espera a que termine de cargar el modelo" -ForegroundColor Yellow
        } else {
            Write-Host "   ❌ GPU no en uso: usando solo CPU" -ForegroundColor Red
            Write-Host "   Verifica drivers NVIDIA y CUDA" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "   ⚠️ No se pudo verificar estado GPU" -ForegroundColor Yellow
}
Write-Host ""

# 8. Resumen
Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ CONFIGURACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "🎯 Próximos pasos:" -ForegroundColor Cyan
Write-Host "   1. Inicia tu aplicación ASP.NET" -ForegroundColor White
Write-Host "   2. Cambia modelo a 'deepseek-coder:33b' en Configuración" -ForegroundColor White
Write-Host "   3. Haz consultas SQL (primera: 30-60s, siguientes: 10-20s)" -ForegroundColor White
Write-Host ""
Write-Host "📊 Monitoreo GPU en tiempo real:" -ForegroundColor Cyan
Write-Host "   nvidia-smi -l 1" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔄 Para verificar estado:" -ForegroundColor Cyan
Write-Host "   ollama ps" -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️ NOTA: Variables de entorno SOLO aplican en esta terminal" -ForegroundColor Yellow
Write-Host "   Para configuración permanente, ver: Docs\OLLAMA_GPU_OPTIMIZATION.md" -ForegroundColor Yellow
Write-Host ""
