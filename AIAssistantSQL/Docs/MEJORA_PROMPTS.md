# ?? Mejora de Prompts - Enfoque Genérico y Simple

## ?? **Problema Anterior:**

### **Prompts Demasiado Complejos:**
```
? 150+ líneas de instrucciones
? Ejemplos hardcodeados
? Muchas reglas y excepciones
? La IA se confundía
? No manejaba errores SQL
```

**Resultado:** CodeLlama funcionaba bien solo, pero con nuestro prompt se confundía.

---

## ? **Solución Implementada:**

### **1. Prompt de Generación SQL: Simple y Genérico**

#### **Antes (150+ líneas):**
```
?? CRITICAL RULES:
1. Generate ONLY...
2. Use SIMPLEST...
3. FOCUS ON...
4. COPY EXACT...
...
(50+ líneas más de instrucciones)

AGGREGATION PATTERNS:
Pattern 1: ...
Pattern 2: ...
Pattern 3: ...
...
(40+ líneas de ejemplos)

SPECIFIC EXAMPLES:
Example 1: ...
Example 2: ...
...
```

#### **Ahora (30 líneas):**
```
You are an expert SQL query generator.

TASK: Generate a SQL SELECT query.

RULES:
1. Return ONLY the SQL query
2. Use EXACT table/column names
3. For SQL Server, use TOP

DATABASE: BD_SCU (SqlServer)

AVAILABLE TABLES:
TABLE: Usuario
  Columns:
    - UsuarioId (int) [PK]
    - UserName (varchar)
  Foreign Keys:
    - RolId ? Roles.RolId

USER QUESTION: documentos con más de 15 líneas

GENERATE SQL QUERY:
```

**Resultado:** La IA solo necesita:
- ? Las tablas con sus columnas
- ? Las relaciones (Foreign Keys)
- ? La pregunta del usuario

---

### **2. Manejo de Errores SQL Automático**

#### **Flujo Nuevo:**

```
1. Usuario: "documentos con más de 15 líneas"
   ?
2. IA genera SQL
   ?
3. Sistema ejecuta SQL
   ?
4. ? Error SQL: "Invalid column 'nombre'"
   ?
5. ?? Sistema detecta error
   ?
6. Envía feedback a IA:
   "Tu SQL falló con este error: 'Invalid column nombre'"
   "Tabla Usuario tiene: UsuarioId, UserName, Email"
   "Corrígelo"
   ?
7. IA corrige: 'nombre' ? 'UserName'
   ?
8. Sistema ejecuta SQL corregido
   ?
9. ? Funciona!
```

#### **Código:**

```csharp
try
{
    results = ExecuteSQL(sql);
}
catch (Exception sqlEx)
{
    // ?? Reintentar con feedback
    var feedback = $@"
ERROR: {sqlEx.Message}
FAILED SQL: {sql}
Please fix this error.";

    var correctedSql = GenerateSQL(feedback, schema);
    results = ExecuteSQL(correctedSql);
}
```

---

### **3. Prompt de Interpretación: Simple**

#### **Antes (80+ líneas):**
```
Eres un asistente...
?? REGLAS CRÍTICAS:
1. RESPONDE...
2. ENFÓCATE...
...
(40+ líneas de reglas)

?? DETECTA:
...
(20+ líneas)

Ejemplos:
A) Si hay 1...
B) Si hay múltiples...
...
```

#### **Ahora (20 líneas):**
```
You are a database assistant.

RULES:
1. Always respond in SPANISH
2. Be clear and concise
3. Use ONLY the results provided

USER QUESTION: documentos con más líneas
SQL: SELECT TOP 1...

RESULTS: 1 row
| DocumentoId | NumLineas |
| 57 | 56 |

RESPOND IN SPANISH:
```

---

## ?? **Comparación:**

| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Prompt SQL** | 150+ líneas | 30 líneas |
| **Prompt Interpretación** | 80+ líneas | 20 líneas |
| **Ejemplos hardcodeados** | ? Sí | ? No |
| **Manejo errores SQL** | ? No | ? **Sí** |
| **Claridad** | ?? | ????? |
| **Flexibilidad** | ?? | ????? |

---

## ?? **Por Qué Funciona Mejor:**

### **1. CodeLlama Es Inteligente**
```
? ANTES: Le dábamos 150 líneas de instrucciones
   ? Se confundía con tanta información

? AHORA: Le damos solo lo esencial
   ? CodeLlama sabe SQL, no necesita tanta guía
```

### **2. Schema Es Suficiente**
```
CodeLlama + Schema + Pregunta = SQL Correcto

No necesita:
? Ejemplos específicos
? Patrones hardcodeados
? Reglas complejas
```

### **3. Autocorrección con Feedback**
```
Primera vez: Puede fallar
Segunda vez (con error): ? Corrige

El feedback del error SQL es MÁS útil
que 100 líneas de ejemplos preventivos.
```

---

## ?? **Resultados Esperados:**

### **Test 1: Query Simple**
```
Pregunta: "cuántos usuarios hay"
SQL 1: SELECT COUNT(*) FROM Usuario
? Funciona a la primera
```

### **Test 2: Query con Error**
```
Pregunta: "usuarios con su nombre"
SQL 1: SELECT nombre FROM Usuario
? Error: "Invalid column 'nombre'"

Feedback: "Error: columna 'nombre' no existe.
          Columnas: UsuarioId, UserName, Email"

SQL 2: SELECT UserName FROM Usuario
? Funciona después de corrección
```

### **Test 3: Query Compleja**
```
Pregunta: "documento con más líneas"
SQL 1: SELECT TOP 1 DocumentoId, COUNT(*)
       FROM lineas_documento
       GROUP BY DocumentoId
       ORDER BY COUNT(*) DESC
? Funciona a la primera (CodeLlama sabe GROUP BY)
```

---

## ?? **Ventajas:**

### **1. Más Genérico**
```
? Funciona con CUALQUIER base de datos
? No depende de nombres específicos
? No necesita ejemplos hardcodeados
```

### **2. Más Robusto**
```
? Si falla, se autocorrige
? Aprende del error SQL real
? Feedback específico del problema
```

### **3. Más Simple**
```
? Prompts cortos y claros
? Fácil de mantener
? CodeLlama trabaja mejor
```

---

## ?? **Resumen:**

```
???????????????????????????????????????????????????
?  FILOSOFÍA ANTERIOR:                            ?
?  "Darle TODAS las instrucciones posibles"      ?
?  ? 150+ líneas de prompt                        ?
?  ? Muchos ejemplos                              ?
?  ? Reglas para cada caso                        ?
?  ? La IA se confunde                           ?
???????????????????????????????????????????????????

???????????????????????????????????????????????????
?  FILOSOFÍA NUEVA:                               ?
?  "Confiar en CodeLlama + Autocorrección"       ?
?  ? 30 líneas de prompt                          ?
?  ? Solo schema + pregunta                       ?
?  ? Si falla, feedback con error real            ?
?  ? La IA trabaja mejor                         ?
???????????????????????????????????????????????????
```

---

## ?? **Conclusión:**

**Menos es más.** CodeLlama es lo suficientemente inteligente para SQL. Solo necesita:

1. ? Schema claro
2. ? Pregunta del usuario
3. ? Feedback cuando falla

No necesita:
- ? 100+ líneas de instrucciones
- ? Ejemplos hardcodeados
- ? Patrones pre-definidos

**El error SQL real es el mejor profesor.** ??
