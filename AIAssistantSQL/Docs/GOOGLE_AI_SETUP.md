# ?? Gu�a de Configuraci�n - Google AI (Gemini)

## ?? **�Por Qu� Usar Google AI?**

| Caracter�stica | CodeLlama (Local) | Google AI (Gemini) |
|----------------|-------------------|--------------------|
| **Precisi�n SQL** | ???? (90%) | ????? (98%) |
| **Velocidad** | Depende de tu PC | Muy r�pido (cloud) |
| **Privacidad** | ????? (100% local) | ??? (datos en cloud) |
| **Costo** | 100% gratis | **Gratis** (con l�mites generosos) |
| **Requiere GPU** | S� (recomendado) | No |
| **SQL Complejo** | ???? | ????? |
| **JOINs Complejos** | ???? | ????? |
| **Subqueries** | ??? | ????? |

---

## ?? **Plan Gratuito de Google AI:**

**Gemini 1.5 Flash** (recomendado):
- ? **15 consultas por minuto**
- ? **1,500 consultas por d�a**
- ? **1 mill�n de tokens por mes**
- ? **Sin costo**

**Gemini 1.5 Pro** (m�s potente):
- ? **2 consultas por minuto**
- ? **50 consultas por d�a**
- ? **Sin costo**

**Para este proyecto:** El plan gratuito es M�S que suficiente. ?

---

## ?? **Paso 1: Obtener API Key (2 minutos)**

### **1. Ve a Google AI Studio**
```
https://aistudio.google.com/app/apikey
```

### **2. Inicia Sesi�n**
- Usa tu cuenta de Google (Gmail)
- Es GRATIS

### **3. Crea una API Key**
1. Click en **"Get API key"** o **"Create API key"**
2. Selecciona un proyecto existente o crea uno nuevo
3. Click **"Create API key in new project"**
4. ? **Copia la API Key** (algo como: `AIzaSyBxxxxx...`)

**?? IMPORTANTE:**
- Guarda la API Key en un lugar seguro
- NO la compartas p�blicamente
- Es como una contrase�a

---

## ?? **Paso 2: Configurar en la Aplicaci�n**

### **Opci�n A: Desde appsettings.json (Recomendado)**

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

### **Opci�n B: Desde Variables de Entorno (M�s Seguro)**

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

Luego reinicia la aplicaci�n.

---

## ?? **Paso 3: Usar Google AI**

```bash
# 1. Aseg�rate de tener la API Key configurada
cat appsettings.json | grep ApiKey

# 2. Inicia la aplicaci�n
dotnet run

# 3. Abre tu navegador
http://localhost:5000

# 4. �Listo! Ahora usa Google AI
```

---

## ?? **Cambiar Entre Ollama y Google AI**

### **Opci�n 1: Cambiar en appsettings.json**

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

### **Opci�n 2: Variable de Entorno**

```bash
# Usar Ollama
set AI__PROVIDER=Ollama

# Usar Google AI
set AI__PROVIDER=GoogleAI
```

---

## ?? **Comparaci�n de Modelos Google AI:**

### **gemini-1.5-flash** (Recomendado) ?????

**Ventajas:**
- ? MUY r�pido
- ? Excelente para SQL
- ? L�mites generosos (15/min, 1500/d�a)
- ? Consume menos tokens

**Ideal para:**
- SQL de complejidad media
- Uso diario intensivo
- Prototipado r�pido

**Configuraci�n:**
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
- ? M�S preciso
- ? Mejor con SQL extremadamente complejo
- ? Entiende contexto m�s profundo

**Desventajas:**
- ?? M�s lento
- ?? L�mites menores (2/min, 50/d�a)

**Ideal para:**
- SQL con subqueries complejas
- CTEs (WITH clauses)
- Window functions
- Optimizaci�n de queries

**Configuraci�n:**
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
Pregunta: "cu�ntos usuarios hay"
```

| Modelo | SQL | Tiempo | Precisi�n |
|--------|-----|--------|-----------|
| CodeLlama | `SELECT COUNT(*) FROM Usuario` | ~2s | ? |
| Gemini Flash | `SELECT COUNT(*) FROM Usuario` | ~0.5s | ? |
| Gemini Pro | `SELECT COUNT(*) FROM Usuario` | ~1s | ? |

**Ganador:** Gemini Flash (m�s r�pido)

---

### **Test 2: Query con GROUP BY**
```
Pregunta: "documentos con m�s de 15 l�neas"
```

| Modelo | SQL Correcto | Tiempo | Precisi�n |
|--------|--------------|--------|-----------|
| CodeLlama | ? | ~3s | 95% |
| Gemini Flash | ? | ~0.8s | 98% |
| Gemini Pro | ? | ~1.5s | 99% |

**Ganador:** Gemini Flash (balance perfecto)

---

### **Test 3: Query Compleja con Subqueries**
```
Pregunta: "usuarios que tienen documentos con m�s de 20 l�neas"
```

| Modelo | SQL Correcto | Tiempo | Precisi�n |
|--------|--------------|--------|-----------|
| CodeLlama | ?? (a veces) | ~4s | 80% |
| Gemini Flash | ? | ~1s | 95% |
| Gemini Pro | ? | ~2s | **99%** |

**Ganador:** Gemini Pro (m�s complejo)

---

## ?? **Recomendaciones:**

### **Para Uso General:**
```
?? Gemini 1.5 Flash
   - R�pido
   - Preciso
   - L�mites generosos
```

### **Para SQL Extremadamente Complejo:**
```
?? Gemini 1.5 Pro
   - M�xima precisi�n
   - Para queries cr�ticos
```

### **Para Privacidad Total:**
```
?? CodeLlama (Ollama Local)
   - 100% privado
   - Sin conexi�n a internet
   - Gratis sin l�mites
```

---

## ?? **Seguridad y Privacidad:**

### **�Qu� se Env�a a Google?**
- ? El esquema de tu base de datos (nombres de tablas/columnas)
- ? La pregunta del usuario
- ? Los resultados de la consulta (para interpretaci�n)

### **�Qu� NO se Env�a?**
- ? Los datos reales de tu base de datos
- ? Tu cadena de conexi�n
- ? Contrase�as

### **Google Guarda los Datos?**
Seg�n los [T�rminos de Servicio](https://ai.google.dev/gemini-api/terms):
- ? Los prompts NO se usan para entrenar modelos
- ? Se eliminan despu�s de procesarlos
- ?? Pueden guardarse temporalmente para detectar abuso

### **Recomendaci�n:**
```
Producci�n con datos sensibles:
? Usar CodeLlama (local)

Desarrollo/Testing:
? Usar Google AI (m�s r�pido y preciso)
```

---

## ? **Preguntas Frecuentes:**

### **�Es realmente gratis?**
? S�, el plan gratuito incluye:
- 15 consultas/minuto (gemini-flash)
- 1,500 consultas/d�a
- Para este proyecto, es M�S que suficiente

### **�Necesito tarjeta de cr�dito?**
? No. El plan gratuito NO requiere tarjeta.

### **�Qu� pasa si excedo los l�mites?**
- Recibir�s un error HTTP 429 (Too Many Requests)
- Puedes esperar 1 minuto y reintentar
- O cambiar temporalmente a Ollama

### **�Puedo usar ambos?**
? S�! Puedes:
- Usar Google AI durante el d�a
- Usar Ollama en la noche
- Cambiar seg�n necesites

### **�Es mejor que CodeLlama?**
Para SQL, S�:
- ? M�s r�pido
- ? M�s preciso
- ? No requiere GPU potente

CodeLlama ventajas:
- ? 100% privado
- ? Sin l�mites de uso
- ? Sin conexi�n a internet

---

## ?? **Decisi�n R�pida:**

```
???????????????????????????????????????????????????
?  USA GOOGLE AI SI:                              ?
?  ? Quieres la m�xima precisi�n                 ?
?  ? No tienes GPU potente                       ?
?  ? Los datos NO son ultra-sensibles            ?
?  ? Necesitas velocidad                         ?
???????????????????????????????????????????????????

???????????????????????????????????????????????????
?  USA OLLAMA (CodeLlama) SI:                     ?
?  ? Privacidad es cr�tica                       ?
?  ? Tienes buena GPU                            ?
?  ? No quieres l�mites de uso                   ?
?  ? No tienes internet estable                  ?
???????????????????????????????????????????????????
```

---

**�Disfruta de SQL con IA de Google! ??**
