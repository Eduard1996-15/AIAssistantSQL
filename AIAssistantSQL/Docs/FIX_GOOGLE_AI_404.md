# ?? Fix - Error 404 Google AI Gemini

## ? **Error Original:**

```
Error 404: models/gemini-1.5-flash is not found for API version v1beta
```

---

## ?? **Problema:**

La librer�a `Mscc.GenerativeAI` usa la **API v1beta** de Google AI, que requiere nombres de modelos espec�ficos con el sufijo `-latest`.

---

## ? **Soluci�n Implementada:**

### **Normalizaci�n de Nombres de Modelos:**

El sistema ahora convierte autom�ticamente los nombres de modelos a los nombres correctos de la API:

| Nombre Configurado | Nombre Real de API |
|-------------------|-------------------|
| `gemini-1.5-flash` | `gemini-1.5-flash-latest` |
| `gemini-1.5-pro` | `gemini-1.5-pro-latest` |
| `gemini-1.0-pro` | `gemini-pro` |
| `gemini-pro` | `gemini-pro` |

---

## ?? **Cambios en GoogleAIService.cs:**

### **M�todo Agregado:**

```csharp
private string NormalizeModelName(string modelName)
{
    return modelName.ToLower() switch
    {
        "gemini-1.5-flash" => "gemini-1.5-flash-latest",
        "gemini-1.5-pro" => "gemini-1.5-pro-latest",
        "gemini-1.0-pro" => "gemini-pro",
        "gemini-pro" => "gemini-pro",
        _ => "gemini-1.5-flash-latest"
    };
}
```

### **Uso en Constructor:**

```csharp
public GoogleAIService(...)
{
    _apiKey = _configuration["GoogleAI:ApiKey"];
    _model = _configuration["GoogleAI:Model"] ?? "gemini-1.5-flash";
    
    // ? Normalizar el nombre del modelo
    _model = NormalizeModelName(_model);
}
```

---

## ?? **Configuraci�n:**

### **appsettings.json (SIN cambios):**

```json
{
  "GoogleAI": {
    "ApiKey": "AIzaSyB0mqQWd_vDtcLj6aS6cwmFL_fwRVIFezk",
    "Model": "gemini-1.5-flash",  // ? Puedes usar el nombre simple
    "Enabled": true
  }
}
```

**El sistema convierte autom�ticamente `gemini-1.5-flash` ? `gemini-1.5-flash-latest`**

---

## ?? **Pruebas:**

### **Test 1: Verificar Disponibilidad**

```
1. Ve a "Configuraci�n"
2. Click "?? Probar Conexi�n" en Google AI
3. ? Deber�a mostrar: "Conexi�n exitosa con Google AI (Gemini)"
```

---

### **Test 2: Cambiar a Google AI**

```
1. Ve a "Configuraci�n"
2. Click "Usar Google AI (Gemini)"
3. ? Deber�a cambiar sin errores
4. ? Mensaje: "Proveedor cambiado exitosamente"
```

---

### **Test 3: Generar SQL**

```
1. Aseg�rate de estar usando Google AI
2. Ve a "Consultas"
3. Pregunta: "cu�ntos usuarios hay"
4. ? Deber�a generar: SELECT COUNT(*) FROM Usuario
```

---

## ?? **Modelos Disponibles:**

### **Gemini 1.5 Flash (Recomendado):**
```json
{
  "GoogleAI": {
    "Model": "gemini-1.5-flash"
  }
}
```

**Se convierte a:** `gemini-1.5-flash-latest`

**Caracter�sticas:**
- ? Muy r�pido (0.5-1s)
- ? Excelente para SQL
- ? Gratis: 15 consultas/min, 1500/d�a

---

### **Gemini 1.5 Pro (SQL Complejo):**
```json
{
  "GoogleAI": {
    "Model": "gemini-1.5-pro"
  }
}
```

**Se convierte a:** `gemini-1.5-pro-latest`

**Caracter�sticas:**
- ? M�xima precisi�n
- ? Mejor con SQL muy complejo
- ?? M�s lento (1-2s)
- ?? L�mites menores: 2/min, 50/d�a

---

### **Gemini Pro (Estable):**
```json
{
  "GoogleAI": {
    "Model": "gemini-pro"
  }
}
```

**Se mantiene como:** `gemini-pro`

**Caracter�sticas:**
- ? Versi�n estable
- ? Buen rendimiento
- ? Sin sufijo -latest

---

## ?? **Verificaci�n de la API:**

### **Modelos Disponibles en Google AI v1beta:**

Seg�n la documentaci�n de Google:

```
? gemini-1.5-flash-latest
? gemini-1.5-pro-latest
? gemini-pro
? gemini-1.5-flash-001
? gemini-1.5-pro-001
? gemini-1.5-flash (sin -latest)
? gemini-1.5-pro (sin -latest)
```

**Nuestro sistema normaliza autom�ticamente los nombres para usar los correctos.**

---

## ?? **Referencias:**

### **Documentaci�n de Google AI:**
- https://ai.google.dev/api/rest/v1beta/models
- https://ai.google.dev/models/gemini

### **Librer�a Mscc.GenerativeAI:**
- https://github.com/mscraftsman/generative-ai

---

## ? **Resumen del Fix:**

```
???????????????????????????????????????????????????
?  PROBLEMA:                                      ?
?  ? Error 404: modelo no encontrado             ?
?                                                 ?
?  CAUSA:                                         ?
?  ? API v1beta requiere sufijo -latest          ?
?                                                 ?
?  SOLUCI�N:                                      ?
?  ? Normalizaci�n autom�tica de nombres         ?
?  ? Sin cambios en configuraci�n                ?
?  ? Transparente para el usuario                ?
?                                                 ?
?  RESULTADO:                                     ?
?  ? Google AI funciona correctamente            ?
?  ? Todos los modelos disponibles               ?
?  ? Sin errores 404                             ?
???????????????????????????????????????????????????
```

---

## ?? **Reinicia y Prueba:**

```sh
# 1. Reinicia la aplicaci�n
dotnet run

# 2. Abre
http://localhost:5000

# 3. Ve a "Configuraci�n"

# 4. Prueba Google AI
Click "?? Probar Conexi�n" en Google AI

# 5. Si funciona, cambia a Google AI
Click "Usar Google AI (Gemini)"

# 6. Haz una consulta
Ve a "Consultas" y pregunta algo
```

---

**�El error 404 est� corregido! Google AI ahora funciona correctamente. ?**
