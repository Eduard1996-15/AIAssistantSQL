@echo off
echo ========================================
echo  Instalacion Rapida de CodeLlama
echo ========================================
echo.
echo Este es el modelo RECOMENDADO para SQL.
echo Solo se instalara CodeLlama (3.8 GB).
echo.
echo Para instalar mas modelos, usa:
echo   install-models.bat
echo.
echo ========================================
pause

echo.
echo Instalando CodeLlama...
echo ========================================
ollama pull codellama

if %errorlevel% neq 0 (
    echo.
    echo ERROR: No se pudo instalar CodeLlama
    echo.
    echo Posibles causas:
    echo 1. Ollama no esta corriendo
    echo    Solucion: ollama serve
    echo.
    echo 2. No hay conexion a internet
    echo    Solucion: Verifica tu conexion
    echo.
    echo 3. No hay espacio en disco
    echo    Solucion: Libera al menos 4 GB
    echo.
    pause
    exit /b 1
)

echo.
echo ========================================
echo  INSTALACION COMPLETA
echo ========================================
echo.
echo ? CodeLlama instalado correctamente
echo.
echo Modelos disponibles:
ollama list
echo.
echo ========================================
echo  PROXIMOS PASOS:
echo ========================================
echo.
echo 1. Inicia Ollama (si no esta corriendo):
echo    ollama serve
echo.
echo 2. Inicia la aplicacion:
echo    dotnet run
echo.
echo 3. Abre tu navegador:
echo    http://localhost:5000
echo.
echo 4. ¡Empieza a consultar!
echo    El modelo por defecto ya es CodeLlama
echo.
echo ========================================
pause
