#!/bin/bash

# Script para instalar múltiples modelos de Ollama optimizados para SQL
# Autor: AI Assistant SQL
# Versión: 2.0

echo "🚀 Instalador de Modelos Ollama para SQL - Versión 2.0"
echo "=================================================="

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para mostrar mensajes con color
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Verificar si Ollama está instalado
check_ollama() {
    log_info "Verificando instalación de Ollama..."
    if ! command -v ollama &> /dev/null; then
        log_error "Ollama no está instalado"
        log_info "Instalando Ollama..."
        curl -fsSL https://ollama.ai/install.sh | sh
        
        if [ $? -eq 0 ]; then
            log_success "Ollama instalado correctamente"
        else
            log_error "Error instalando Ollama"
            exit 1
        fi
    else
        log_success "Ollama ya está instalado"
    fi
}

# Verificar si Ollama está ejecutándose
check_ollama_running() {
    log_info "Verificando si Ollama está ejecutándose..."
    if ! curl -s http://localhost:11434/api/tags > /dev/null 2>&1; then
        log_warning "Ollama no está ejecutándose"
        log_info "Iniciando Ollama en background..."
        nohup ollama serve > /dev/null 2>&1 &
        sleep 5
        
        if curl -s http://localhost:11434/api/tags > /dev/null 2>&1; then
            log_success "Ollama iniciado correctamente"
        else
            log_error "No se pudo iniciar Ollama"
            exit 1
        fi
    else
        log_success "Ollama está ejecutándose"
    fi
}

# Función para verificar espacio en disco
check_disk_space() {
    log_info "Verificando espacio en disco disponible..."
    available_space=$(df -h ~ | awk 'NR==2 {print $4}' | sed 's/G//')
    
    if [ "${available_space%.*}" -lt 20 ]; then
        log_warning "Espacio disponible: ${available_space}GB"
        log_warning "Se recomienda al menos 20GB libres para todos los modelos"
        read -p "¿Desea continuar? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            log_info "Instalación cancelada por el usuario"
            exit 0
        fi
    else
        log_success "Espacio disponible: ${available_space}GB"
    fi
}

# Función para instalar un modelo
install_model() {
    local model_name=$1
    local description=$2
    local size=$3
    
    log_info "Instalando modelo: $model_name ($description) - Tamaño: $size"
    
    # Verificar si el modelo ya está instalado
    if ollama list | grep -q "$model_name"; then
        log_success "Modelo $model_name ya está instalado"
        return 0
    fi
    
    # Intentar instalar el modelo
    if timeout 600 ollama pull "$model_name"; then
        log_success "Modelo $model_name instalado correctamente"
        
        # Probar el modelo con una consulta simple
        log_info "Probando modelo $model_name..."
        test_response=$(echo "SELECT 1 as test" | ollama run "$model_name" "Generate a simple SQL query to test database connection:" 2>/dev/null)
        
        if [ $? -eq 0 ]; then
            log_success "Modelo $model_name funciona correctamente"
        else
            log_warning "Modelo $model_name instalado pero no responde correctamente"
        fi
        
        return 0
    else
        log_error "Error instalando modelo $model_name"
        return 1
    fi
}

# Función para mostrar menú de selección
show_menu() {
    echo
    log_info "Modelos disponibles para instalación:"
    echo "=================================================="
    echo "MODELOS RECOMENDADOS PARA SQL:"
    echo "1. codellama:7b-code          - Especializado en código (3.8GB) 🎯"
    echo "2. codellama:13b-code         - Más preciso, más lento (7.3GB) 🎯"
    echo "3. deepseek-coder:6.7b        - Excelente para SQL (3.8GB) 🚀"
    echo "4. deepseek-coder:33b         - Máxima precisión (18GB) 🚀"
    echo
    echo "MODELOS GENERALES RÁPIDOS:"
    echo "5. llama3.2:3b               - Muy rápido, ligero (2GB) ⚡"
    echo "6. llama3.1:8b               - Balance rendimiento/tamaño (4.7GB) ⚡"
    echo "7. mistral:7b                - Eficiente y preciso (4.1GB) 🎭"
    echo "8. phi3:3.8b                 - Pequeño y eficiente (2.3GB) 📱"
    echo
    echo "MODELOS AVANZADOS:"
    echo "9. qwen2.5-coder:7b          - Nuevo modelo para código (4.2GB) 💻"
    echo "10. granite-code:8b          - IBM, optimizado para SQL (4.9GB) 💎"
    echo
    echo "OPCIONES ESPECIALES:"
    echo "11. Instalar TODOS los recomendados (codellama:7b, deepseek-coder:6.7b, llama3.2:3b)"
    echo "12. Instalar solo los LIGEROS (llama3.2:3b, phi3:3.8b)"
    echo "13. Instalar solo los ESPECIALIZADOS (codellama:7b, deepseek-coder:6.7b)"
    echo "0. Salir"
    echo
}

# Arrays con información de modelos
declare -A models_info
models_info[1]="codellama:7b-code|Especializado en código y SQL|3.8GB"
models_info[2]="codellama:13b-code|Más preciso para consultas complejas|7.3GB"
models_info[3]="deepseek-coder:6.7b|Excelente rendimiento en SQL|3.8GB"
models_info[4]="deepseek-coder:33b|Máxima precisión y capacidad|18GB"
models_info[5]="llama3.2:3b|Muy rápido y ligero|2GB"
models_info[6]="llama3.1:8b|Balance entre velocidad y precisión|4.7GB"
models_info[7]="mistral:7b|Eficiente y confiable|4.1GB"
models_info[8]="phi3:3.8b|Optimizado para dispositivos pequeños|2.3GB"
models_info[9]="qwen2.5-coder:7b|Nuevo modelo especializado|4.2GB"
models_info[10]="granite-code:8b|IBM, optimizado para SQL|4.9GB"

# Función principal de instalación
install_selected_models() {
    local selections=("$@")
    local total_installed=0
    local total_failed=0
    
    log_info "Iniciando instalación de modelos seleccionados..."
    
    for selection in "${selections[@]}"; do
        if [[ $selection =~ ^[1-9]|10$ ]]; then
            IFS='|' read -r model_name description size <<< "${models_info[$selection]}"
            
            if install_model "$model_name" "$description" "$size"; then
                ((total_installed++))
            else
                ((total_failed++))
            fi
            
            echo "----------------------------------------"
        fi
    done
    
    echo
    log_info "Resumen de instalación:"
    log_success "Modelos instalados exitosamente: $total_installed"
    if [ $total_failed -gt 0 ]; then
        log_error "Modelos que fallaron: $total_failed"
    fi
}

# Función para instalaciones predefinidas
install_preset() {
    case $1 in
        11) # Todos los recomendados
            log_info "Instalando todos los modelos recomendados..."
            install_selected_models 1 3 5
            ;;
        12) # Solo ligeros
            log_info "Instalando solo modelos ligeros..."
            install_selected_models 5 8
            ;;
        13) # Solo especializados
            log_info "Instalando solo modelos especializados..."
            install_selected_models 1 3
            ;;
    esac
}

# Función para mostrar modelos instalados
show_installed_models() {
    log_info "Modelos actualmente instalados:"
    echo "================================"
    
    if command -v ollama &> /dev/null && curl -s http://localhost:11434/api/tags > /dev/null 2>&1; then
        ollama list | grep -E "(NAME|codellama|deepseek|llama|mistral|phi|qwen|granite)" | head -20
        echo
        
        # Mostrar recomendación
        installed_count=$(ollama list | wc -l)
        if [ $installed_count -gt 1 ]; then
            log_success "Total de modelos instalados: $((installed_count - 1))"
            echo
            log_info "💡 Recomendaciones de uso:"
            echo "• Para consultas rápidas y simples: llama3.2:3b"
            echo "• Para consultas SQL complejas: codellama:7b-code"
            echo "• Para máxima precisión: deepseek-coder:6.7b"
            echo "• Para dispositivos con poca RAM: phi3:3.8b"
        fi
    else
        log_warning "No se pueden mostrar los modelos (Ollama no disponible)"
    fi
}

# Función para configurar el modelo por defecto
set_default_model() {
    echo
    log_info "¿Desea configurar un modelo por defecto? (recomendado)"
    echo "Modelos recomendados como predeterminados:"
    echo "1. codellama:7b-code (mejor para SQL)"
    echo "2. deepseek-coder:6.7b (excelente rendimiento)"
    echo "3. llama3.2:3b (más rápido)"
    echo "4. Mantener configuración actual"
    echo
    read -p "Seleccione una opción (1-4): " default_choice
    
    case $default_choice in
        1)
            echo "Ollama:Model=codellama:7b-code" >> ../appsettings.json.tmp 2>/dev/null || true
            log_success "Modelo por defecto configurado: codellama:7b-code"
            ;;
        2)
            echo "Ollama:Model=deepseek-coder:6.7b" >> ../appsettings.json.tmp 2>/dev/null || true
            log_success "Modelo por defecto configurado: deepseek-coder:6.7b"
            ;;
        3)
            echo "Ollama:Model=llama3.2:3b" >> ../appsettings.json.tmp 2>/dev/null || true
            log_success "Modelo por defecto configurado: llama3.2:3b"
            ;;
        4)
            log_info "Manteniendo configuración actual"
            ;;
        *)
            log_warning "Opción no válida, manteniendo configuración actual"
            ;;
    esac
}

# Función principal
main() {
    echo
    log_info "🔧 Iniciando proceso de instalación..."
    
    # Verificaciones previas
    check_ollama
    check_ollama_running
    check_disk_space
    
    # Mostrar modelos ya instalados
    show_installed_models
    
    # Menú principal
    while true; do
        show_menu
        read -p "Seleccione una opción (0-13): " choice
        
        case $choice in
            0)
                log_info "Saliendo del instalador..."
                break
                ;;
            11|12|13)
                install_preset $choice
                ;;
            [1-9]|10)
                install_selected_models $choice
                ;;
            *)
                log_warning "Opción no válida. Por favor seleccione un número del 0 al 13."
                continue
                ;;
        esac
        
        echo
        read -p "¿Desea instalar más modelos? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            break
        fi
    done
    
    # Configurar modelo por defecto
    set_default_model
    
    # Resumen final
    echo
    log_success "🎉 Instalación completada!"
    log_info "Los modelos están listos para usar en AI Assistant SQL"
    log_info "Puede cambiar el modelo activo desde la interfaz web"
    echo
    
    # Mostrar estado final
    show_installed_models
}

# Ejecutar función principal
main "$@"