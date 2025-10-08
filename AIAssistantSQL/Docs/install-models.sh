#!/bin/bash

echo "========================================"
echo " Instalador de Modelos de IA para SQL"
echo "========================================"
echo ""
echo "Este script descargara los MEJORES modelos"
echo "de IA especializados en SQL y codigo."
echo ""
echo "MODELOS QUE SE INSTALARAN:"
echo ""
echo "1. CodeLlama (3.8 GB) - RECOMENDADO"
echo "   - Especializado en codigo y SQL"
echo "   - Mejor precision en consultas complejas"
echo "   - GROUP BY, HAVING, JOINs perfectos"
echo ""
echo "2. Mistral (4.1 GB) - ALTERNATIVA"
echo "   - Buen balance precision/velocidad"
echo "   - Excelente para SQL y razonamiento"
echo ""
echo "3. DeepSeek Coder (3.8 GB) - OPCIONAL"
echo "   - Especializado en programacion"
echo "   - Muy bueno para SQL complejo"
echo ""
echo "ESPACIO TOTAL REQUERIDO: ~12 GB"
echo ""
echo "========================================"
echo ""
read -p "Presiona Enter para continuar..."

echo ""
echo "[1/3] Instalando CodeLlama (RECOMENDADO)..."
echo "========================================"
ollama pull codellama
if [ $? -ne 0 ]; then
    echo "ERROR: No se pudo descargar CodeLlama"
    echo "Verifica que Ollama este corriendo: ollama serve"
    read -p "Presiona Enter para salir..."
    exit 1
fi
echo ""
echo "? CodeLlama instalado correctamente"
echo ""

echo "[2/3] Instalando Mistral..."
echo "========================================"
ollama pull mistral
if [ $? -ne 0 ]; then
    echo "ADVERTENCIA: No se pudo descargar Mistral"
    echo "Continuando con el siguiente modelo..."
fi
echo ""
echo "? Mistral instalado correctamente"
echo ""

echo "[3/3] Instalando DeepSeek Coder..."
echo "========================================"
ollama pull deepseek-coder
if [ $? -ne 0 ]; then
    echo "ADVERTENCIA: No se pudo descargar DeepSeek Coder"
    echo "Continuando..."
fi
echo ""
echo "? DeepSeek Coder instalado correctamente"
echo ""

echo "========================================"
echo " INSTALACION COMPLETA"
echo "========================================"
echo ""
echo "Modelos instalados:"
ollama list
echo ""
echo "========================================"
echo ""
echo "PROXIMOS PASOS:"
echo "1. Inicia tu aplicacion: dotnet run"
echo "2. Abre: http://localhost:5000"
echo "3. Ve a 'Configuracion'"
echo "4. El modelo por defecto es: codellama"
echo ""
echo "NOTA: Puedes cambiar de modelo en la seccion"
echo "      'Configuracion' de la aplicacion."
echo ""
echo "========================================"
read -p "Presiona Enter para salir..."
