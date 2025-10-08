# ?? Mejora de Prompts - Enfoque Gen�rico y Simple

## ?? **Problema Anterior:**

### **Prompts Demasiado Complejos:**
```
? 150+ l�neas de instrucciones
? Ejemplos hardcodeados
? Muchas reglas y excepciones
? La IA se confund�a
? No manejaba errores SQL
```

**Resultado:** CodeLlama funcionaba bien solo, pero con nuestro prompt se confund�a.

---

## ? **Soluci�n Implementada:**

### **1. Prompt de Generaci�n SQL: Simple y Gen�rico**

#### **Antes (150+ l�neas):**
```
?? CRITICAL RULES:
1. Generate ONLY...
2. Use SIMPLEST...
3. FOCUS ON...
4. COPY EXACT...
...
(50+ l�neas m�s de instrucciones)

AGGREGATION PATTERNS:
Pattern 1: ...
Pattern 2: ...
Pattern 3: ...
...
(40+ l�neas de ejemplos)

SPECIFIC EXAMPLES:
Example 1: ...
Example 2: ...
...
```

#### **Ahora (30 l�neas):**
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

USER QUESTION: documentos con m�s de 15 l�neas

GENERATE SQL QUERY:
```

**Resultado:** La IA solo necesita:
- ? Las tablas con sus columnas
- ? Las relaciones (Foreign Keys)
- ? La pregunta del usuario

---

### **2. Manejo de Errores SQL Autom�tico**

#### **Flujo Nuevo:**

```
1. Usuario: "documentos con m�s de 15 l�neas"
   ?
2. IA genera SQL
   ?
3. Sistema ejecuta SQL
   ?
4. ? Error SQL: "Invalid column 'nombre'"
   ?
5. ?? Sistema detecta error
   ?
6. Env�a feedback a IA:
   "Tu SQL fall� con este error: 'Invalid column nombre'"
   "Tabla Usuario tiene: UsuarioId, UserName, Email"
   "Corr�gelo"
   ?
7. IA corrige: 'nombre' ? 'UserName'
   ?
8. Sistema ejecuta SQL corregido
   ?
9. ? Funciona!
```

#### **C�digo:**

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

### **3. Prompt de Interpretaci�n: Simple**

#### **Antes (80+ l�neas):**
```
Eres un asistente...
?? REGLAS CR�TICAS:
1. RESPONDE...
2. ENF�CATE...
...
(40+ l�neas de reglas)

?? DETECTA:
...
(20+ l�neas)

Ejemplos:
A) Si hay 1...
B) Si hay m�ltiples...
...
```

#### **Ahora (20 l�neas):**
```
You are a database assistant.

RULES:
1. Always respond in SPANISH
2. Be clear and concise
3. Use ONLY the results provided

USER QUESTION: documentos con m�s l�neas
SQL: SELECT TOP 1...

RESULTS: 1 row
| DocumentoId | NumLineas |
| 57 | 56 |

RESPOND IN SPANISH:
```

---

## ?? **Comparaci�n:**

| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Prompt SQL** | 150+ l�neas | 30 l�neas |
| **Prompt Interpretaci�n** | 80+ l�neas | 20 l�neas |
| **Ejemplos hardcodeados** | ? S� | ? No |
| **Manejo errores SQL** | ? No | ? **S�** |
| **Claridad** | ?? | ????? |
| **Flexibilidad** | ?? | ????? |

---

## ?? **Por Qu� Funciona Mejor:**

### **1. CodeLlama Es Inteligente**
```
? ANTES: Le d�bamos 150 l�neas de instrucciones
   ? Se confund�a con tanta informaci�n

? AHORA: Le damos solo lo esencial
   ? CodeLlama sabe SQL, no necesita tanta gu�a
```

### **2. Schema Es Suficiente**
```
CodeLlama + Schema + Pregunta = SQL Correcto

No necesita:
? Ejemplos espec�ficos
? Patrones hardcodeados
? Reglas complejas
```

### **3. Autocorrecci�n con Feedback**
```
Primera vez: Puede fallar
Segunda vez (con error): ? Corrige

El feedback del error SQL es M�S �til
que 100 l�neas de ejemplos preventivos.
```

---

## ?? **Resultados Esperados:**

### **Test 1: Query Simple**
```
Pregunta: "cu�ntos usuarios hay"
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
? Funciona despu�s de correcci�n
```

### **Test 3: Query Compleja**
```
Pregunta: "documento con m�s l�neas"
SQL 1: SELECT TOP 1 DocumentoId, COUNT(*)
       FROM lineas_documento
       GROUP BY DocumentoId
       ORDER BY COUNT(*) DESC
? Funciona a la primera (CodeLlama sabe GROUP BY)
```

---

## ?? **Ventajas:**

### **1. M�s Gen�rico**
```
? Funciona con CUALQUIER base de datos
? No depende de nombres espec�ficos
? No necesita ejemplos hardcodeados
```

### **2. M�s Robusto**
```
? Si falla, se autocorrige
? Aprende del error SQL real
? Feedback espec�fico del problema
```

### **3. M�s Simple**
```
? Prompts cortos y claros
? F�cil de mantener
? CodeLlama trabaja mejor
```

---

## ?? **Resumen:**

```
???????????????????????????????????????????????????
?  FILOSOF�A ANTERIOR:                            ?
?  "Darle TODAS las instrucciones posibles"      ?
?  ? 150+ l�neas de prompt                        ?
?  ? Muchos ejemplos                              ?
?  ? Reglas para cada caso                        ?
?  ? La IA se confunde                           ?
???????????????????????????????????????????????????

???????????????????????????????????????????????????
?  FILOSOF�A NUEVA:                               ?
?  "Confiar en CodeLlama + Autocorrecci�n"       ?
?  ? 30 l�neas de prompt                          ?
?  ? Solo schema + pregunta                       ?
?  ? Si falla, feedback con error real            ?
?  ? La IA trabaja mejor                         ?
???????????????????????????????????????????????????
```

---

## ?? **Conclusi�n:**

**Menos es m�s.** CodeLlama es lo suficientemente inteligente para SQL. Solo necesita:

1. ? Schema claro
2. ? Pregunta del usuario
3. ? Feedback cuando falla

No necesita:
- ? 100+ l�neas de instrucciones
- ? Ejemplos hardcodeados
- ? Patrones pre-definidos

**El error SQL real es el mejor profesor.** ??
