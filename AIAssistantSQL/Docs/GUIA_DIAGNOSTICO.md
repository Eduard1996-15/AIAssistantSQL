# ?? Guía de Diagnóstico - Errores de Columnas Inexistentes

## ?? Problema: La IA genera SQL con columnas que no existen

### Ejemplo:
```
Pregunta: "documentos originales"
SQL Generado: SELECT ... Email ... FROM Documento
Error: Invalid column name 'Email'
```

---

## ? **Soluciones Implementadas:**

### 1. **Validación Estricta de Columnas**
Antes de ejecutar ANY SQL, el sistema ahora:
- ? Verifica que todas las tablas existen
- ? Verifica que todas las columnas en SELECT existen
- ? Verifica que todas las columnas en WHERE existen
- ? Sugiere columnas similares si hay error
- ? Reintenta con feedback del error

### 2. **Logging Detallado**
Puedes ver en la consola exactamente qué está pasando:
```
?? Esquema cargado: BD_SCU con 13 tablas
?? Tablas disponibles: Usuario, Documento, Logs, ...
?? Procesando consulta: 'documentos originales'
?? Enviando esquema completo a la IA (13 tablas)
?? SQL generado por IA: SELECT ... Email ...
?? Validando SQL contra el esquema...
? VALIDACIÓN FALLIDA: La columna 'Email' no existe en ninguna tabla
?? Reintentando generación de SQL con feedback del error...
```

---

## ?? **Cómo Diagnosticar el Problema:**

### **Paso 1: Ve a "Diagnóstico"**
1. Abre tu navegador
2. Ve a la sección **"Diagnóstico"** en el menú
3. Busca la tabla **"Documento"**
4. Verás EXACTAMENTE qué columnas tiene

### **Paso 2: Revisa los Logs**
En la consola donde ejecutaste `dotnet run`, busca:
```
?? Esquema cargado: ...
?? Tablas disponibles: ...
?? SQL generado por IA: ...
? Columna 'XXX' NO existe en el esquema
? Columna 'YYY' existe en el esquema
```

### **Paso 3: Verifica el Modelo de IA**
1. Ve a **"Configuración"**
2. Verifica qué modelo estás usando
3. Si es `llama3.2` ? Considera cambiar a `codellama`

---

## ?? **Por Qué Puede Estar Fallando:**

### **Causa 1: El Modelo No Es Lo Suficientemente Bueno**
- **llama3.2** es rápido pero no tan preciso para SQL
- **Solución:** Cambia a `codellama`

```bash
ollama pull codellama
```

Luego en "Configuración" ? "Usar este Modelo" ? codellama

### **Causa 2: La Pregunta Es Ambigua**
- "documentos originales" puede significar muchas cosas
- La IA asume que "originales" implica un email o usuario

**Mejor pregunta:**
```
"muestra los documentos de la tabla Documento"
"lista todos los documentos con su versión"
"documentos con su número y fecha"
```

### **Causa 3: El Esquema No Se Cargó Correctamente**
Verifica en "Diagnóstico":
- ¿Aparece la tabla "Documento"?
- ¿Tiene las columnas correctas?

Si no:
1. Ve a "Configuración BD"
2. "Conectar y Extraer Esquema Completo" de nuevo

---

## ?? **Solución Inmediata:**

### **Opción A: Sé Más Específico**
? "documentos originales"
? "SELECT * FROM Documento"
? "muestra todas las columnas de la tabla Documento"
? "lista documentos con su DocumentoId y Titulo"

### **Opción B: Usa CodeLlama**
```bash
# Instalar
ollama pull codellama

# Usar en la app
Ir a "Configuración" ? Click "Usar este Modelo" en codellama
```

### **Opción C: Verifica el Esquema**
1. Ve a "Diagnóstico"
2. Abre la tabla "Documento"
3. Copia los nombres EXACTOS de las columnas
4. Pregunta usando esos nombres:
   ```
   "muestra DocumentoId, Titulo, Version de la tabla Documento"
   ```

---

## ?? **Flujo de Validación (Actualizado):**

```
Usuario pregunta: "documentos originales"
   ?
IA recibe esquema completo (13 tablas)
   ?
IA genera: SELECT ... Email ... FROM Documento
   ?
? VALIDACIÓN DETECTA: "Email" no existe en Documento
   ?
? BLOQUEADO: No ejecuta el SQL
   ?
?? REINTENTO: IA genera SQL nuevo con feedback
   ?
IA genera: SELECT DocumentoId, Titulo, Version FROM Documento
   ?
? VALIDACIÓN: Todas las columnas existen
   ?
? EJECUTA: Query exitosa
```

---

## ?? **Prueba Esto:**

### **Test 1: Query Genérico (puede fallar)**
```
"documentos"
```

### **Test 2: Query Específico (debería funcionar)**
```
"SELECT * FROM Documento"
```

### **Test 3: Query con Columnas Exactas**
```
"muestra DocumentoId y Titulo de Documento"
```

### **Test 4: Verificar Columnas Disponibles**
```
"qué columnas tiene la tabla Documento"
```

La IA debería responder listando las columnas del esquema.

---

## ?? **Si Aún Falla:**

### **Debug Step-by-Step:**

1. **Reinicia Ollama:**
```bash
# Detén Ollama (Ctrl+C si está corriendo)
ollama serve
```

2. **Verifica Modelo:**
```bash
ollama list
# Deberías ver: codellama, llama3.2
```

3. **Reinicia la App:**
```bash
cd AIAssistantSQL
dotnet run
```

4. **Verifica Logs en Consola:**
Busca estas líneas:
```
?? Esquema cargado: ...
?? SQL generado por IA: ...
? Columna 'XXX' NO existe ...
```

5. **Si Dice "Esquema cargado: 0 tablas":**
   - Ve a "Configuración BD"
   - Reconecta y extrae esquema

---

## ?? **Conclusión:**

El sistema AHORA tiene:
- ? Validación completa antes de ejecutar
- ? Reintento automático con feedback
- ? Sugerencias de columnas similares
- ? Logging detallado para debug

**Si sigue fallando, el problema está en:**
1. Modelo de IA no lo suficientemente bueno (usa codellama)
2. Pregunta muy ambigua (sé más específico)
3. Esquema no cargado correctamente (verifica en Diagnóstico)

---

**Prueba ahora con CodeLlama y preguntas más específicas. ??**
