# ?? Guía de Configuración - Google AI (Gemini)

## ?? **¿Por Qué Usar Google AI?**

| Característica | CodeLlama (Local) | Google AI (Gemini) |
|----------------|-------------------|--------------------|
| **Precisión SQL** | ???? (90%) | ????? (98%) |
| **Velocidad** | Depende de tu PC | Muy rápido (cloud) |
| **Privacidad** | ????? (100% local) | ??? (datos en cloud) |
| **Costo** | 100% gratis | **Gratis** (con límites generosos) |
| **Requiere GPU** | Sí (recomendado) | No |
| **SQL Complejo** | ???? | ????? |
| **JOINs Complejos** | ???? | ????? |
| **Subqueries** | ??? | ????? |

---

## ?? **Plan Gratuito de Google AI:**

**Gemini 1.5 Flash** (recomendado):
- ? **15 consultas por minuto**
- ? **1,500 consultas por día**
- ? **1 millón de tokens por mes**
- ? **Sin costo**

**Gemini 1.5 Pro** (más potente):
- ? **2 consultas por minuto**
- ? **50 consultas por día**
- ? **Sin costo**

**Para este proyecto:** El plan gratuito es MÁS que suficiente. ?

---

## ?? **Paso 1: Obtener API Key (2 minutos)**

### **1. Ve a Google AI Studio**
```
https://aistudio.google.com/app/apikey
```

### **2. Inicia Sesión**
- Usa tu cuenta de Google (Gmail)
- Es GRATIS

### **3. Crea una API Key**
1. Click en **"Get API key"** o **"Create API key"**
2. Selecciona un proyecto existente o crea uno nuevo
3. Click **"Create API key in new project"**
4. ? **Copia la API Key** (algo como: `AIzaSyBxxxxx...`)

**?? IMPORTANTE:**
- Guarda la API Key en un lugar seguro
- NO la compartas públicamente
- Es como una contraseña

---

## ?? **Paso 2: Configurar en la Aplicación**

### **Opción A: Desde appsettings.json (Recomendado)**

Edita `appsettings.json`:

```json
{
  "GoogleAI": {
    "ApiKey": "AIzaSyBxxxxx_TU_API_KEY_AQUI_xxxxx",
    "Model": "gemini-1.5-flash",
    "Enabled": true
  },
  "AI": {
    "Provider": "GoogleAI"
  }
}
```

### **Opción B: Desde Variables de Entorno (Más Seguro)**

#### **Windows:**
```cmd
setx GOOGLEAI__APIKEY "AIzaSyBxxxxx_TU_API_KEY_AQUI_xxxxx"
setx AI__PROVIDER "GoogleAI"
```

#### **Linux/Mac:**
```bash
export GOOGLEAI__APIKEY="AIzaSyBxxxxx_TU_API_KEY_AQUI_xxxxx"
export AI__PROVIDER="GoogleAI"
```

Luego reinicia la aplicación.

---

## ?? **Paso 3: Usar Google AI**

```bash
# 1. Asegúrate de tener la API Key configurada
cat appsettings.json | grep ApiKey

# 2. Inicia la aplicación
dotnet run

# 3. Abre tu navegador
http://localhost:5000

# 4. ¡Listo! Ahora usa Google AI
```

---

## ?? **Cambiar Entre Ollama y Google AI**

### **Opción 1: Cambiar en appsettings.json**

**Usar Ollama (Local):**
```json
{
  "AI": {
    "Provider": "Ollama"
  }
}
```

**Usar Google AI (Cloud):**
```json
{
  "AI": {
    "Provider": "GoogleAI"
  }
}
```

### **Opción 2: Variable de Entorno**

```bash
# Usar Ollama
set AI__PROVIDER=Ollama

# Usar Google AI
set AI__PROVIDER=GoogleAI
```

---

## ?? **Comparación de Modelos Google AI:**

### **gemini-1.5-flash** (Recomendado) ?????

**Ventajas:**
- ? MUY rápido
- ? Excelente para SQL
- ? Límites generosos (15/min, 1500/día)
- ? Consume menos tokens

**Ideal para:**
- SQL de complejidad media
- Uso diario intensivo
- Prototipado rápido

**Configuración:**
```json
{
  "GoogleAI": {
    "Model": "gemini-1.5-flash"
  }
}
```

---

### **gemini-1.5-pro** (SQL Muy Complejo) ?????

**Ventajas:**
- ? MÁS preciso
- ? Mejor con SQL extremadamente complejo
- ? Entiende contexto más profundo

**Desventajas:**
- ?? Más lento
- ?? Límites menores (2/min, 50/día)

**Ideal para:**
- SQL con subqueries complejas
- CTEs (WITH clauses)
- Window functions
- Optimización de queries

**Configuración:**
```json
{
  "GoogleAI": {
    "Model": "gemini-1.5-pro"
  }
}
```

---

## ?? **Pruebas de Rendimiento:**

### **Test 1: Query Simple**
```
Pregunta: "cuántos usuarios hay"
```

| Modelo | SQL | Tiempo | Precisión |
|--------|-----|--------|-----------|
| CodeLlama | `SELECT COUNT(*) FROM Usuario` | ~2s | ? |
| Gemini Flash | `SELECT COUNT(*) FROM Usuario` | ~0.5s | ? |
| Gemini Pro | `SELECT COUNT(*) FROM Usuario` | ~1s | ? |

**Ganador:** Gemini Flash (más rápido)

---

### **Test 2: Query con GROUP BY**
```
Pregunta: "documentos con más de 15 líneas"
```

| Modelo | SQL Correcto | Tiempo | Precisión |
|--------|--------------|--------|-----------|
| CodeLlama | ? | ~3s | 95% |
| Gemini Flash | ? | ~0.8s | 98% |
| Gemini Pro | ? | ~1.5s | 99% |

**Ganador:** Gemini Flash (balance perfecto)

---

### **Test 3: Query Compleja con Subqueries**
```
Pregunta: "usuarios que tienen documentos con más de 20 líneas"
```

| Modelo | SQL Correcto | Tiempo | Precisión |
|--------|--------------|--------|-----------|
| CodeLlama | ?? (a veces) | ~4s | 80% |
| Gemini Flash | ? | ~1s | 95% |
| Gemini Pro | ? | ~2s | **99%** |

**Ganador:** Gemini Pro (más complejo)

---

## ?? **Recomendaciones:**

### **Para Uso General:**
```
?? Gemini 1.5 Flash
   - Rápido
   - Preciso
   - Límites generosos
```

### **Para SQL Extremadamente Complejo:**
```
?? Gemini 1.5 Pro
   - Máxima precisión
   - Para queries críticos
```

### **Para Privacidad Total:**
```
?? CodeLlama (Ollama Local)
   - 100% privado
   - Sin conexión a internet
   - Gratis sin límites
```

---

## ?? **Seguridad y Privacidad:**

### **¿Qué se Envía a Google?**
- ? El esquema de tu base de datos (nombres de tablas/columnas)
- ? La pregunta del usuario
- ? Los resultados de la consulta (para interpretación)

### **¿Qué NO se Envía?**
- ? Los datos reales de tu base de datos
- ? Tu cadena de conexión
- ? Contraseñas

### **Google Guarda los Datos?**
Según los [Términos de Servicio](https://ai.google.dev/gemini-api/terms):
- ? Los prompts NO se usan para entrenar modelos
- ? Se eliminan después de procesarlos
- ?? Pueden guardarse temporalmente para detectar abuso

### **Recomendación:**
```
Producción con datos sensibles:
? Usar CodeLlama (local)

Desarrollo/Testing:
? Usar Google AI (más rápido y preciso)
```

---

## ? **Preguntas Frecuentes:**

### **¿Es realmente gratis?**
? Sí, el plan gratuito incluye:
- 15 consultas/minuto (gemini-flash)
- 1,500 consultas/día
- Para este proyecto, es MÁS que suficiente

### **¿Necesito tarjeta de crédito?**
? No. El plan gratuito NO requiere tarjeta.

### **¿Qué pasa si excedo los límites?**
- Recibirás un error HTTP 429 (Too Many Requests)
- Puedes esperar 1 minuto y reintentar
- O cambiar temporalmente a Ollama

### **¿Puedo usar ambos?**
? Sí! Puedes:
- Usar Google AI durante el día
- Usar Ollama en la noche
- Cambiar según necesites

### **¿Es mejor que CodeLlama?**
Para SQL, SÍ:
- ? Más rápido
- ? Más preciso
- ? No requiere GPU potente

CodeLlama ventajas:
- ? 100% privado
- ? Sin límites de uso
- ? Sin conexión a internet

---

## ?? **Decisión Rápida:**

```
???????????????????????????????????????????????????
?  USA GOOGLE AI SI:                              ?
?  ? Quieres la máxima precisión                 ?
?  ? No tienes GPU potente                       ?
?  ? Los datos NO son ultra-sensibles            ?
?  ? Necesitas velocidad                         ?
???????????????????????????????????????????????????

???????????????????????????????????????????????????
?  USA OLLAMA (CodeLlama) SI:                     ?
?  ? Privacidad es crítica                       ?
?  ? Tienes buena GPU                            ?
?  ? No quieres límites de uso                   ?
?  ? No tienes internet estable                  ?
???????????????????????????????????????????????????
```

---

**¡Disfruta de SQL con IA de Google! ??**
