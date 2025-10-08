# ?? Gu�a de Diagn�stico - Errores de Columnas Inexistentes

## ?? Problema: La IA genera SQL con columnas que no existen

### Ejemplo:
```
Pregunta: "documentos originales"
SQL Generado: SELECT ... Email ... FROM Documento
Error: Invalid column name 'Email'
```

---

## ? **Soluciones Implementadas:**

### 1. **Validaci�n Estricta de Columnas**
Antes de ejecutar ANY SQL, el sistema ahora:
- ? Verifica que todas las tablas existen
- ? Verifica que todas las columnas en SELECT existen
- ? Verifica que todas las columnas en WHERE existen
- ? Sugiere columnas similares si hay error
- ? Reintenta con feedback del error

### 2. **Logging Detallado**
Puedes ver en la consola exactamente qu� est� pasando:
```
?? Esquema cargado: BD_SCU con 13 tablas
?? Tablas disponibles: Usuario, Documento, Logs, ...
?? Procesando consulta: 'documentos originales'
?? Enviando esquema completo a la IA (13 tablas)
?? SQL generado por IA: SELECT ... Email ...
?? Validando SQL contra el esquema...
? VALIDACI�N FALLIDA: La columna 'Email' no existe en ninguna tabla
?? Reintentando generaci�n de SQL con feedback del error...
```

---

## ?? **C�mo Diagnosticar el Problema:**

### **Paso 1: Ve a "Diagn�stico"**
1. Abre tu navegador
2. Ve a la secci�n **"Diagn�stico"** en el men�
3. Busca la tabla **"Documento"**
4. Ver�s EXACTAMENTE qu� columnas tiene

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
1. Ve a **"Configuraci�n"**
2. Verifica qu� modelo est�s usando
3. Si es `llama3.2` ? Considera cambiar a `codellama`

---

## ?? **Por Qu� Puede Estar Fallando:**

### **Causa 1: El Modelo No Es Lo Suficientemente Bueno**
- **llama3.2** es r�pido pero no tan preciso para SQL
- **Soluci�n:** Cambia a `codellama`

```bash
ollama pull codellama
```

Luego en "Configuraci�n" ? "Usar este Modelo" ? codellama

### **Causa 2: La Pregunta Es Ambigua**
- "documentos originales" puede significar muchas cosas
- La IA asume que "originales" implica un email o usuario

**Mejor pregunta:**
```
"muestra los documentos de la tabla Documento"
"lista todos los documentos con su versi�n"
"documentos con su n�mero y fecha"
```

### **Causa 3: El Esquema No Se Carg� Correctamente**
Verifica en "Diagn�stico":
- �Aparece la tabla "Documento"?
- �Tiene las columnas correctas?

Si no:
1. Ve a "Configuraci�n BD"
2. "Conectar y Extraer Esquema Completo" de nuevo

---

## ?? **Soluci�n Inmediata:**

### **Opci�n A: S� M�s Espec�fico**
? "documentos originales"
? "SELECT * FROM Documento"
? "muestra todas las columnas de la tabla Documento"
? "lista documentos con su DocumentoId y Titulo"

### **Opci�n B: Usa CodeLlama**
```bash
# Instalar
ollama pull codellama

# Usar en la app
Ir a "Configuraci�n" ? Click "Usar este Modelo" en codellama
```

### **Opci�n C: Verifica el Esquema**
1. Ve a "Diagn�stico"
2. Abre la tabla "Documento"
3. Copia los nombres EXACTOS de las columnas
4. Pregunta usando esos nombres:
   ```
   "muestra DocumentoId, Titulo, Version de la tabla Documento"
   ```

---

## ?? **Flujo de Validaci�n (Actualizado):**

```
Usuario pregunta: "documentos originales"
   ?
IA recibe esquema completo (13 tablas)
   ?
IA genera: SELECT ... Email ... FROM Documento
   ?
? VALIDACI�N DETECTA: "Email" no existe en Documento
   ?
? BLOQUEADO: No ejecuta el SQL
   ?
?? REINTENTO: IA genera SQL nuevo con feedback
   ?
IA genera: SELECT DocumentoId, Titulo, Version FROM Documento
   ?
? VALIDACI�N: Todas las columnas existen
   ?
? EJECUTA: Query exitosa
```

---

## ?? **Prueba Esto:**

### **Test 1: Query Gen�rico (puede fallar)**
```
"documentos"
```

### **Test 2: Query Espec�fico (deber�a funcionar)**
```
"SELECT * FROM Documento"
```

### **Test 3: Query con Columnas Exactas**
```
"muestra DocumentoId y Titulo de Documento"
```

### **Test 4: Verificar Columnas Disponibles**
```
"qu� columnas tiene la tabla Documento"
```

La IA deber�a responder listando las columnas del esquema.

---

## ?? **Si A�n Falla:**

### **Debug Step-by-Step:**

1. **Reinicia Ollama:**
```bash
# Det�n Ollama (Ctrl+C si est� corriendo)
ollama serve
```

2. **Verifica Modelo:**
```bash
ollama list
# Deber�as ver: codellama, llama3.2
```

3. **Reinicia la App:**
```bash
cd AIAssistantSQL
dotnet run
```

4. **Verifica Logs en Consola:**
Busca estas l�neas:
```
?? Esquema cargado: ...
?? SQL generado por IA: ...
? Columna 'XXX' NO existe ...
```

5. **Si Dice "Esquema cargado: 0 tablas":**
   - Ve a "Configuraci�n BD"
   - Reconecta y extrae esquema

---

## ?? **Conclusi�n:**

El sistema AHORA tiene:
- ? Validaci�n completa antes de ejecutar
- ? Reintento autom�tico con feedback
- ? Sugerencias de columnas similares
- ? Logging detallado para debug

**Si sigue fallando, el problema est� en:**
1. Modelo de IA no lo suficientemente bueno (usa codellama)
2. Pregunta muy ambigua (s� m�s espec�fico)
3. Esquema no cargado correctamente (verifica en Diagn�stico)

---

**Prueba ahora con CodeLlama y preguntas m�s espec�ficas. ??**
