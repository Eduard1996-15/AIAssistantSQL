# 🚀 Optimización de GPU para Ollama

## Tu Hardware
- **GPU:** NVIDIA RTX 4070 ✅ (Excelente para IA)
- **RAM:** 64GB DDR5 ✅ (Más que suficiente)
- **CPU:** Intel i9-13900HX ✅ (Potente)

## Estado Actual
```
deepseek-coder:33b    67% CPU / 33% GPU
```

**Problema:** Ollama está usando más CPU que GPU (debería ser al revés).

---

## ✅ PASO 1: Configurar Variables de Entorno

### Opción A: Configuración Permanente (Recomendado)

1. **Abrir Variables de Entorno:**
   - Presiona `Win + X` → "Sistema" → "Configuración avanzada del sistema"
   - Click en "Variables de entorno"

2. **Crear nuevas variables (Sistema):**

```
Variable: OLLAMA_NUM_GPU
Valor: 1

Variable: OLLAMA_GPU_OVERHEAD
Valor: 0

Variable: OLLAMA_MAX_LOADED_MODELS
Valor: 1

Variable: OLLAMA_NUM_PARALLEL
Valor: 1

Variable: CUDA_VISIBLE_DEVICES
Valor: 0
```

3. **Reiniciar Ollama:**
```powershell
# Cerrar Ollama completamente
taskkill /F /IM ollama.exe

# Iniciar de nuevo
ollama serve
```

### Opción B: Configuración Temporal (Para probar)

```powershell
# En PowerShell ANTES de iniciar Ollama:
$env:OLLAMA_NUM_GPU = "1"
$env:OLLAMA_GPU_OVERHEAD = "0"
$env:OLLAMA_MAX_LOADED_MODELS = "1"
$env:OLLAMA_NUM_PARALLEL = "1"
$env:CUDA_VISIBLE_DEVICES = "0"

# Luego iniciar Ollama
ollama serve
```

---

## ✅ PASO 2: Verificar CUDA

```powershell
# Verificar que NVIDIA drivers están instalados
nvidia-smi
```

**Deberías ver:**
```
+-----------------------------------------------------------------------------+
| NVIDIA-SMI 535.xx       Driver Version: 535.xx       CUDA Version: 12.x    |
|-------------------------------+----------------------+----------------------+
| GPU  Name            TCC/WDDM | Bus-Id        Disp.A | Volatile Uncorr. ECC |
| Fan  Temp  Perf  Pwr:Usage/Cap|         Memory-Usage | GPU-Util  Compute M. |
|===============================+======================+======================|
|   0  NVIDIA GeForce ... WDDM  | 00000000:01:00.0 Off |                  N/A |
| N/A   45C    P8    10W /  N/A |      0MiB / 12288MiB |      0%      Default |
+-------------------------------+----------------------+----------------------+
```

---

## ✅ PASO 3: Ejecutar con Más GPU

Después de configurar variables de entorno:

```powershell
# Cerrar todo
taskkill /F /IM ollama.exe

# Iniciar Ollama con configuración GPU
ollama serve

# En otra terminal, cargar el modelo
ollama run deepseek-coder:33b

# Verificar uso de GPU
ollama ps
```

**Resultado esperado:**
```
NAME                  ID              SIZE     PROCESSOR          CONTEXT    UNTIL
deepseek-coder:33b    acec7c0b0fd9    21 GB    10%/90% CPU/GPU    4096       Now
                                               ^^^^^^^ MÁS GPU!
```

---

## 🎯 Rendimiento Esperado

| Configuración | Tiempo Consulta Simple | Tiempo Consulta Compleja |
|---------------|------------------------|--------------------------|
| **Solo CPU** (actual) | 45-90s | 90-180s |
| **CPU + GPU (33%)** | 30-60s | 60-120s |
| **CPU + GPU (90%)** | **10-25s** ⚡ | **25-60s** ⚡ |

---

## 🔧 Troubleshooting

### GPU no se detecta

```powershell
# 1. Verificar drivers NVIDIA
nvidia-smi

# 2. Reinstalar Ollama (asegura que detecte GPU)
# Descargar desde: https://ollama.ai/download

# 3. Verificar versión
ollama --version
```

### Sigue usando mucho CPU

```powershell
# Forzar solo GPU (experimental)
$env:OLLAMA_NUM_GPU = "1"
$env:OLLAMA_GPU_LAYERS = "999"  # Forzar TODAS las capas a GPU

ollama serve
```

---

## 📊 Monitoreo en Tiempo Real

### Ver uso de GPU mientras procesas consultas:

```powershell
# Terminal 1: Ollama
ollama serve

# Terminal 2: Monitoreo GPU (actualiza cada 1 segundo)
nvidia-smi -l 1

# Terminal 3: Tu aplicación
cd C:\Users\Admin\Documents\PROYECTOS DGAT\AIAssistantSQL\AIAssistantSQL
dotnet run
```

---

## ✅ Configuración Óptima para RTX 4070

```powershell
# Variables de entorno recomendadas:
OLLAMA_NUM_GPU=1
OLLAMA_GPU_OVERHEAD=0
OLLAMA_MAX_LOADED_MODELS=1
OLLAMA_NUM_PARALLEL=1
CUDA_VISIBLE_DEVICES=0
OLLAMA_KEEP_ALIVE=30m        # Mantener modelo en memoria 30 min
OLLAMA_NUM_THREAD=8          # Threads CPU (tu i9 tiene 24, usa 8)
```

---

## 🚀 Resultado Final

Con GPU optimizada + timeouts corregidos:

- **Primera consulta:** 15-30 segundos (carga modelo)
- **Siguientes consultas:** 5-15 segundos ⚡⚡⚡
- **Uso GPU:** 80-90%
- **Uso RAM:** ~22GB (de tus 64GB)

---

## 📝 Notas

1. **Primera consulta siempre más lenta:** Ollama carga el modelo (20GB) en GPU
2. **Mantener modelo "caliente":** Configurar `OLLAMA_KEEP_ALIVE=30m`
3. **Si falla GPU:** Ollama automáticamente vuelve a CPU (más lento pero funcional)
4. **RTX 4070 perfecta:** 12GB VRAM es suficiente para deepseek-coder:33b

---

## ✅ Checklist

- [ ] Configurar variables de entorno
- [ ] Verificar `nvidia-smi` funciona
- [ ] Reiniciar Ollama completamente
- [ ] Ejecutar `ollama run deepseek-coder:33b`
- [ ] Verificar `ollama ps` muestra 80-90% GPU
- [ ] Probar consulta en tu app
- [ ] Medir tiempo (debería ser ~10-20s)
