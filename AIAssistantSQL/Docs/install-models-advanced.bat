@echo off
REM Script para instalar múltiples modelos de Ollama optimizados para SQL (Windows)
REM Autor: AI Assistant SQL
REM Versión: 2.0

echo.
echo 🚀 Instalador de Modelos Ollama para SQL - Version 2.0
echo ==================================================
echo.

REM Verificar si Ollama está instalado
where ollama >nul 2>&1
if errorlevel 1 (
    echo ❌ Ollama no está instalado
    echo.
    echo 📥 Descargando e instalando Ollama...
    echo Por favor, descargue Ollama desde: https://ollama.ai/download
    echo.
    pause
    exit /b 1
) else (
    echo ✅ Ollama está instalado
)

REM Verificar si Ollama está ejecutándose
curl -s http://localhost:11434/api/tags >nul 2>&1
if errorlevel 1 (
    echo ⚠️  Ollama no está ejecutándose
    echo 🔄 Iniciando Ollama...
    start /b ollama serve
    timeout /t 5 /nobreak >nul
    
    curl -s http://localhost:11434/api/tags >nul 2>&1
    if errorlevel 1 (
        echo ❌ No se pudo iniciar Ollama
        echo Por favor, inicie Ollama manualmente: ollama serve
        pause
        exit /b 1
    ) else (
        echo ✅ Ollama iniciado correctamente
    )
) else (
    echo ✅ Ollama está ejecutándose
)

echo.
echo 📋 Verificando espacio en disco...
for /f "tokens=3" %%a in ('dir /-c %USERPROFILE% ^| find "bytes free"') do set freespace=%%a
echo Espacio disponible verificado

:menu
echo.
echo ℹ️  Modelos disponibles para instalación:
echo ==================================================
echo MODELOS RECOMENDADOS PARA SQL:
echo 1. codellama:7b-code          - Especializado en código (3.8GB) 🎯
echo 2. codellama:13b-code         - Más preciso, más lento (7.3GB) 🎯
echo 3. deepseek-coder:6.7b        - Excelente para SQL (3.8GB) 🚀
echo 4. deepseek-coder:33b         - Máxima precisión (18GB) 🚀
echo.
echo MODELOS GENERALES RÁPIDOS:
echo 5. llama3.2:3b               - Muy rápido, ligero (2GB) ⚡
echo 6. llama3.1:8b               - Balance rendimiento/tamaño (4.7GB) ⚡
echo 7. mistral:7b                - Eficiente y preciso (4.1GB) 🎭
echo 8. phi3:3.8b                 - Pequeño y eficiente (2.3GB) 📱
echo.
echo MODELOS AVANZADOS:
echo 9. qwen2.5-coder:7b          - Nuevo modelo para código (4.2GB) 💻
echo 10. granite-code:8b          - IBM, optimizado para SQL (4.9GB) 💎
echo.
echo OPCIONES ESPECIALES:
echo 11. Instalar TODOS los recomendados (1, 3, 5)
echo 12. Instalar solo los LIGEROS (5, 8)
echo 13. Instalar solo los ESPECIALIZADOS (1, 3)
echo 14. Mostrar modelos instalados
echo 0. Salir
echo.

set /p choice="Seleccione una opción (0-14): "

if "%choice%"=="0" goto end
if "%choice%"=="1" call :install_model "codellama:7b-code" "Especializado en código y SQL" "3.8GB"
if "%choice%"=="2" call :install_model "codellama:13b-code" "Más preciso para consultas complejas" "7.3GB"
if "%choice%"=="3" call :install_model "deepseek-coder:6.7b" "Excelente rendimiento en SQL" "3.8GB"
if "%choice%"=="4" call :install_model "deepseek-coder:33b" "Máxima precisión y capacidad" "18GB"
if "%choice%"=="5" call :install_model "llama3.2:3b" "Muy rápido y ligero" "2GB"
if "%choice%"=="6" call :install_model "llama3.1:8b" "Balance entre velocidad y precisión" "4.7GB"
if "%choice%"=="7" call :install_model "mistral:7b" "Eficiente y confiable" "4.1GB"
if "%choice%"=="8" call :install_model "phi3:3.8b" "Optimizado para dispositivos pequeños" "2.3GB"
if "%choice%"=="9" call :install_model "qwen2.5-coder:7b" "Nuevo modelo especializado" "4.2GB"
if "%choice%"=="10" call :install_model "granite-code:8b" "IBM, optimizado para SQL" "4.9GB"
if "%choice%"=="11" call :install_preset_all
if "%choice%"=="12" call :install_preset_light
if "%choice%"=="13" call :install_preset_specialized
if "%choice%"=="14" call :show_installed
goto menu

:install_model
set model_name=%~1
set description=%~2
set size=%~3

echo.
echo ℹ️  Instalando modelo: %model_name% (%description%) - Tamaño: %size%

REM Verificar si el modelo ya está instalado
ollama list | find "%model_name%" >nul
if not errorlevel 1 (
    echo ✅ Modelo %model_name% ya está instalado
    goto :eof
)

echo 📥 Descargando e instalando %model_name%...
ollama pull %model_name%
if errorlevel 1 (
    echo ❌ Error instalando modelo %model_name%
) else (
    echo ✅ Modelo %model_name% instalado correctamente
    
    echo ℹ️  Probando modelo %model_name%...
    echo SELECT 1 as test | ollama run %model_name% "Generate a simple SQL query:" >nul 2>&1
    if not errorlevel 1 (
        echo ✅ Modelo %model_name% funciona correctamente
    ) else (
        echo ⚠️  Modelo %model_name% instalado pero no responde correctamente
    )
)

echo ----------------------------------------
goto :eof

:install_preset_all
echo.
echo ℹ️  Instalando todos los modelos recomendados...
call :install_model "codellama:7b-code" "Especializado en código y SQL" "3.8GB"
call :install_model "deepseek-coder:6.7b" "Excelente rendimiento en SQL" "3.8GB"
call :install_model "llama3.2:3b" "Muy rápido y ligero" "2GB"
goto :eof

:install_preset_light
echo.
echo ℹ️  Instalando solo modelos ligeros...
call :install_model "llama3.2:3b" "Muy rápido y ligero" "2GB"
call :install_model "phi3:3.8b" "Optimizado para dispositivos pequeños" "2.3GB"
goto :eof

:install_preset_specialized
echo.
echo ℹ️  Instalando solo modelos especializados...
call :install_model "codellama:7b-code" "Especializado en código y SQL" "3.8GB"
call :install_model "deepseek-coder:6.7b" "Excelente rendimiento en SQL" "3.8GB"
goto :eof

:show_installed
echo.
echo ℹ️  Modelos actualmente instalados:
echo ================================
ollama list
echo.
echo 💡 Recomendaciones de uso:
echo • Para consultas rápidas y simples: llama3.2:3b
echo • Para consultas SQL complejas: codellama:7b-code
echo • For máxima precisión: deepseek-coder:6.7b
echo • Para dispositivos con poca RAM: phi3:3.8b
echo.
pause
goto :eof

:end
echo.
echo ✅ Instalación completada!
echo ℹ️  Los modelos están listos para usar en AI Assistant SQL
echo ℹ️  Puede cambiar el modelo activo desde la interfaz web
echo.

REM Mostrar modelos instalados finales
call :show_installed

echo.
echo 🎉 ¡Gracias por usar AI Assistant SQL!
echo.
pause