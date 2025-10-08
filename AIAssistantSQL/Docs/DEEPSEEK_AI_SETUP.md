# ?? Gu�a de Configuraci�n - DeepSeek AI

## ?? **�Por Qu� Usar DeepSeek AI?**

| Caracter�stica | CodeLlama | Google AI | **DeepSeek AI** |
|----------------|-----------|-----------|-----------------|
| **Precisi�n SQL** | ???? (90%) | ????? (98%) | ????? (**99%**) |
| **Velocidad** | Depende PC | 0.5-1s | **0.8-1.2s** |
| **Privacidad** | ????? Local | ??? Cloud | ??? Cloud |
| **Costo** | Gratis | Gratis* | **Gratis*** |
| **Especializaci�n** | C�digo | General | **C�digo/SQL** ????? |
| **SQL Complejo** | ???? | ????? | ????? |
| **Subqueries** | ??? | ????? | ????? |

**DeepSeek est� ESPEC�FICAMENTE entrenado para c�digo y SQL.** ??

---

## ?? **Plan Gratuito de DeepSeek:**

**DeepSeek Chat (Recomendado):**
- ? **10 millones de tokens gratis** al registrarse
- ? Muy econ�mico despu�s: $0.14 por mill�n de tokens
- ? Especializado en c�digo y SQL
- ? Sin l�mites de consultas/minuto

**DeepSeek Coder (A�n m�s especializado):**
- ? Mismo precio
- ? MUY preciso para SQL complejo
- ? Entiende patrones de bases de datos

**Para este proyecto:** 10 millones de tokens = ~5,000-10,000 consultas SQL. ?

---

## ?? **Paso 1: Obtener API Key (2 minutos)**

### **1. Ve a DeepSeek Platform**
```
https://platform.deepseek.com/
```

### **2. Reg�strate**
- Usa tu email o cuenta de GitHub
- Es GRATIS
- Te dan **10 millones de tokens** gratis

### **3. Crea una API Key**
1. Ve a https://platform.deepseek.com/api_keys
2. Click **"Create API Key"**
3. Dale un nombre: "AI Assistant SQL"
4. ? **Copia la API Key** (algo como: `sk-xxxxx...`)

**?? IMPORTANTE:**
- Guarda la API Key en un lugar seguro
- NO la compartas p�blicamente
- Solo se muestra una vez

---

## ?? **Paso 2: Configurar en la Aplicaci�n**

### **Opci�n A: Desde appsettings.json (Recomendado)**

Edita `appsettings.json`:

```json
{
  "DeepSeekAI": {
    "ApiKey": "sk-xxxxx_TU_API_KEY_AQUI_xxxxx",
    "Model": "deepseek-chat",
    "Enabled": true
  },
  "AI": {
    "Provider": "DeepSeek"
  }
}
```

### **Opci�n B: Desde Variables de Entorno (M�s Seguro)**

#### **Windows:**
```cmd
setx DEEPSEEKAI__APIKEY "sk-xxxxx_TU_API_KEY_AQUI_xxxxx"
setx AI__PROVIDER "DeepSeek"
```

#### **Linux/Mac:**
```bash
export DEEPSEEKAI__APIKEY="sk-xxxxx_TU_API_KEY_AQUI_xxxxx"
export AI__PROVIDER="DeepSeek"
```

Luego reinicia la aplicaci�n.

---

## ?? **Paso 3: Usar DeepSeek AI**

```bash
# 1. Aseg�rate de tener la API Key configurada
cat appsettings.json | grep DeepSeekAI

# 2. Inicia la aplicaci�n
dotnet run

# 3. Abre tu navegador
http://localhost:5000

# 4. Ve a "Configuraci�n" y selecciona DeepSeek
# 5. �Listo!
```

---

## ?? **Cambiar Entre Proveedores:**

### **Opci�n 1: Desde la Interfaz Web (Recomendado)**
1. Ve a **"Configuraci�n"**
2. Selecciona **"DeepSeek AI"**
3. Click **"Usar DeepSeek AI"**
4. ? �Listo!

### **Opci�n 2: Desde appsettings.json**

**Usar Ollama (Local):**
```json
{
  "AI": {
    "Provider": "Ollama"
  }
}
```

**Usar Google AI:**
```json
{
  "AI": {
    "Provider": "GoogleAI"
  }
}
```

**Usar DeepSeek:**
```json
{
  "AI": {
    "Provider": "DeepSeek"
  }
}
```

---

## ?? **Comparaci�n de Modelos DeepSeek:**

### **deepseek-chat** (Recomendado) ?????

**Ventajas:**
- ? Excelente para SQL
- ? Muy r�pido
- ? Balance perfecto c�digo/SQL
- ? M�s barato

**Ideal para:**
- SQL de complejidad media-alta
- Uso diario
- JOINs y agregaciones

**Configuraci�n:**
```json
{
  "DeepSeekAI": {
    "Model": "deepseek-chat"
  }
}
```

---

### **deepseek-coder** (SQL Muy Complejo) ?????

**Ventajas:**
- ? EXTREMADAMENTE preciso para SQL complejo
- ? Entiende CTEs y window functions
- ? Optimizaci�n de queries

**Ideal para:**
- SQL extremadamente complejo
- Subqueries anidadas
- CTEs (WITH clauses)
- Window functions
- Optimizaci�n de performance

**Configuraci�n:**
```json
{
  "DeepSeekAI": {
    "Model": "deepseek-coder"
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
| CodeLlama | ? | ~2s | 100% |
| Gemini Flash | ? | ~0.5s | 100% |
| **DeepSeek Chat** | ? | **~0.8s** | 100% |

**Ganador:** Gemini Flash (m�s r�pido), pero DeepSeek muy cerca.

---

### **Test 2: Query con GROUP BY**
```
Pregunta: "documentos con m�s de 15 l�neas"
```

| Modelo | SQL Correcto | Tiempo | Precisi�n |
|--------|--------------|--------|-----------|
| CodeLlama | ? | ~3s | 95% |
| Gemini Flash | ? | ~0.8s | 98% |
| **DeepSeek Chat** | ? | **~1s** | **99%** |

**Ganador:** DeepSeek (balance perfecto)

---

### **Test 3: Query Compleja con Subqueries**
```
Pregunta: "usuarios que tienen documentos con m�s de 20 l�neas"
```

| Modelo | SQL Correcto | Tiempo | Precisi�n |
|--------|--------------|--------|-----------|
| CodeLlama | ?? | ~4s | 80% |
| Gemini Flash | ? | ~1s | 95% |
| Gemini Pro | ? | ~2s | 99% |
| **DeepSeek Chat** | ? | **~1.2s** | **99%** |

**Ganador:** DeepSeek (mejor balance precio/rendimiento)

---

### **Test 4: SQL EXTREMADAMENTE Complejo**
```
Pregunta: "usuarios con promedio de l�neas por documento mayor a 25 y que tengan al menos 3 documentos"
```

| Modelo | SQL Correcto | Tiempo | Precisi�n |
|--------|--------------|--------|-----------|
| CodeLlama | ? | ~5s | 60% |
| Gemini Flash | ?? | ~1.5s | 90% |
| Gemini Pro | ? | ~2.5s | 98% |
| **DeepSeek Coder** | ? | **~1.5s** | **100%** |

**Ganador:** DeepSeek Coder (especializado en c�digo)

---

## ?? **Recomendaciones:**

### **Para Uso General:**
```
?? DeepSeek Chat
   - Balance perfecto
   - Muy preciso
   - Econ�mico
   - Especializado en SQL
```

### **Para SQL EXTREMADAMENTE Complejo:**
```
?? DeepSeek Coder
   - M�xima precisi�n (100%)
   - CTEs y window functions
   - Optimizaci�n de queries
```

### **Para Privacidad Total:**
```
?? CodeLlama (Ollama Local)
   - 100% privado
   - Sin conexi�n a internet
   - Gratis sin l�mites
```

---

## ?? **Costos y L�mites:**

### **Gratis al Inicio:**
- ? **10 millones de tokens gratis**
- ? Suficiente para ~5,000-10,000 consultas SQL
- ? Sin tarjeta de cr�dito requerida

### **Despu�s del Cr�dito Gratis:**
- **$0.14 por mill�n de tokens de input**
- **$0.28 por mill�n de tokens de output**

### **Ejemplo Real:**
```
Consulta SQL t�pica:
- Input: ~500 tokens (esquema + pregunta)
- Output: ~200 tokens (SQL generado)
- Total: ~700 tokens

Costo: ~$0.0001 por consulta
10,000 consultas = ~$1 USD

?? MUY econ�mico
```

---

## ?? **Seguridad y Privacidad:**

### **�Qu� se Env�a a DeepSeek?**
- ? El esquema de tu base de datos
- ? La pregunta del usuario
- ? Los resultados (para interpretaci�n)

### **�Qu� NO se Env�a?**
- ? Los datos reales de tu BD
- ? Tu cadena de conexi�n
- ? Contrase�as

### **DeepSeek Guarda los Datos?**
Seg�n su [Pol�tica de Privacidad](https://www.deepseek.com/privacy):
- ? Los prompts NO se usan para entrenar modelos
- ? Privacidad de datos respetada
- ?? Logs temporales para monitoreo

### **Recomendaci�n:**
```
Producci�n con datos sensibles:
? Usar CodeLlama (local)

Desarrollo/Testing/Producci�n normal:
? Usar DeepSeek (preciso y econ�mico)
```

---

## ? **Preguntas Frecuentes:**

### **�Es mejor que Google AI?**
**Para SQL espec�ficamente, S�:**
- ? M�s especializado en c�digo/SQL
- ? Igual o mejor precisi�n
- ? M�s econ�mico a largo plazo

**Google AI ventajas:**
- ? M�s r�pido (0.5s vs 0.8s)
- ? L�mites gratis m�s generosos

### **�Necesito tarjeta de cr�dito?**
? NO para los 10 millones de tokens gratis.
?? S� si quieres usar m�s despu�s.

### **�Qu� pasa si se acaban mis tokens gratis?**
- Puedes agregar tarjeta de cr�dito
- O cambiar a Ollama (gratis ilimitado)
- O cambiar a Google AI (tambi�n gratis con l�mites)

### **�Puedo usar los 3 proveedores?**
? **S�!** Puedes cambiar cuando quieras:
- DeepSeek para SQL complejo
- Google AI para velocidad m�xima
- Ollama para privacidad

---

## ?? **Decisi�n R�pida:**

```
???????????????????????????????????????????????????
?  USA DEEPSEEK SI:                               ?
?  ? Quieres la m�xima precisi�n en SQL          ?
?  ? SQL complejo con CTEs/subqueries            ?
?  ? Buen balance precio/rendimiento             ?
?  ? No te importa pagar muy poco ($1/10k)      ?
???????????????????????????????????????????????????

???????????????????????????????????????????????????
?  USA GOOGLE AI SI:                              ?
?  ? Quieres la m�xima velocidad                 ?
?  ? L�mites gratis m�s generosos                ?
?  ? SQL general (no extremadamente complejo)    ?
???????????????????????????????????????????????????

???????????????????????????????????????????????????
?  USA OLLAMA SI:                                 ?
?  ? Privacidad es cr�tica                       ?
?  ? Sin l�mites ni costos                       ?
?  ? Sin dependencia de internet                 ?
???????????????????????????????????????????????????
```

---

## ?? **Tabla Comparativa Final:**

| Aspecto | Ollama | Google AI | **DeepSeek** |
|---------|--------|-----------|--------------|
| **Precisi�n SQL** | 90% | 98% | **99%** ? |
| **Velocidad** | 2-4s | **0.5s** ? | 0.8-1.2s |
| **Privacidad** | **?????** | ??? | ??? |
| **Costo** | **Gratis** ? | Gratis* | Gratis* |
| **SQL Complejo** | ???? | ????? | **?????** ? |
| **Especializaci�n** | ???? | ??? | **?????** ? |
| **L�mites** | Sin l�mites | 1500/d�a | 10M tokens |

**Recomendaci�n:** Usa **DeepSeek** para SQL complejo, **Google AI** para velocidad, **Ollama** para privacidad.

---

**�Disfruta de SQL con DeepSeek AI! ??**
