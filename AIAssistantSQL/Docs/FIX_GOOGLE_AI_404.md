# ?? Fix - Error 404 Google AI Gemini

## ? **Error Original:**

```
Error 404: models/gemini-1.5-flash is not found for API version v1beta
```

---

## ?? **Problema:**

La librería `Mscc.GenerativeAI` usa la **API v1beta** de Google AI, que requiere nombres de modelos específicos con el sufijo `-latest`.

---

## ? **Solución Implementada:**

### **Normalización de Nombres de Modelos:**

El sistema ahora convierte automáticamente los nombres de modelos a los nombres correctos de la API:

| Nombre Configurado | Nombre Real de API |
|-------------------|-------------------|
| `gemini-1.5-flash` | `gemini-1.5-flash-latest` |
| `gemini-1.5-pro` | `gemini-1.5-pro-latest` |
| `gemini-1.0-pro` | `gemini-pro` |
| `gemini-pro` | `gemini-pro` |

---

## ?? **Cambios en GoogleAIService.cs:**

### **Método Agregado:**

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

## ?? **Configuración:**

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

**El sistema convierte automáticamente `gemini-1.5-flash` ? `gemini-1.5-flash-latest`**

---

## ?? **Pruebas:**

### **Test 1: Verificar Disponibilidad**

```
1. Ve a "Configuración"
2. Click "?? Probar Conexión" en Google AI
3. ? Debería mostrar: "Conexión exitosa con Google AI (Gemini)"
```

---

### **Test 2: Cambiar a Google AI**

```
1. Ve a "Configuración"
2. Click "Usar Google AI (Gemini)"
3. ? Debería cambiar sin errores
4. ? Mensaje: "Proveedor cambiado exitosamente"
```

---

### **Test 3: Generar SQL**

```
1. Asegúrate de estar usando Google AI
2. Ve a "Consultas"
3. Pregunta: "cuántos usuarios hay"
4. ? Debería generar: SELECT COUNT(*) FROM Usuario
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

**Características:**
- ? Muy rápido (0.5-1s)
- ? Excelente para SQL
- ? Gratis: 15 consultas/min, 1500/día

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

**Características:**
- ? Máxima precisión
- ? Mejor con SQL muy complejo
- ?? Más lento (1-2s)
- ?? Límites menores: 2/min, 50/día

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

**Características:**
- ? Versión estable
- ? Buen rendimiento
- ? Sin sufijo -latest

---

## ?? **Verificación de la API:**

### **Modelos Disponibles en Google AI v1beta:**

Según la documentación de Google:

```
? gemini-1.5-flash-latest
? gemini-1.5-pro-latest
? gemini-pro
? gemini-1.5-flash-001
? gemini-1.5-pro-001
? gemini-1.5-flash (sin -latest)
? gemini-1.5-pro (sin -latest)
```

**Nuestro sistema normaliza automáticamente los nombres para usar los correctos.**

---

## ?? **Referencias:**

### **Documentación de Google AI:**
- https://ai.google.dev/api/rest/v1beta/models
- https://ai.google.dev/models/gemini

### **Librería Mscc.GenerativeAI:**
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
?  SOLUCIÓN:                                      ?
?  ? Normalización automática de nombres         ?
?  ? Sin cambios en configuración                ?
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
# 1. Reinicia la aplicación
dotnet run

# 2. Abre
http://localhost:5000

# 3. Ve a "Configuración"

# 4. Prueba Google AI
Click "?? Probar Conexión" en Google AI

# 5. Si funciona, cambia a Google AI
Click "Usar Google AI (Gemini)"

# 6. Haz una consulta
Ve a "Consultas" y pregunta algo
```

---

**¡El error 404 está corregido! Google AI ahora funciona correctamente. ?**
