# ?? Fix FINAL - Referencias Inconsistentes de Tablas en PostgreSQL

## ? **Error:**

```
42P01: falta una entrada para la tabla «funcionarios» en la cláusula FROM
POSITION: 151
```

**SQL problemático:**
```sql
SELECT Funcionarios."FuncionarioID", COUNT(Capacitaciones."CapacitacionID") 
FROM "Funcionarios" 
LEFT JOIN "Capacitaciones" 
ON Funcionarios."FuncionarioID" = Capacitaciones."FuncionarioID"
   ^^^^^^^^^^^ ? SIN comillas          ^^^^^^^^^^^^^^ ? SIN comillas
GROUP BY Funcionarios."FuncionarioID"
         ^^^^^^^^^^^ ? SIN comillas
```

---

## ?? **Causa:**

La función `AddQuotesToPostgreSQLTables` solo agregaba comillas a tablas **después de FROM y JOIN**, pero NO en:
- ? Cláusulas **ON** (`ON Funcionarios.Col`)
- ? Cláusulas **GROUP BY** (`GROUP BY Funcionarios.Col`)
- ? Cláusulas **ORDER BY** (`ORDER BY Funcionarios.Col`)

**Resultado:** PostgreSQL buscaba `funcionarios` (minúsculas) en lugar de `"Funcionarios"` (con comillas y mayúsculas).

---

## ? **Solución Implementada (2 mejoras):**

### **1. Mejorar `AddQuotesToPostgreSQLTables`**

Agregamos un **patrón adicional** para capturar referencias standalone de tablas:

```csharp
// ? NUEVO: Patrón 3 - Tabla sola sin alias (ej: ON Funcionarios.Id)
var standaloneTablePattern = $@"\b{Regex.Escape(tableName)}\.";
var standaloneTableReplacement = $@"""{tableName}"".";

sql = Regex.Replace(sql, standaloneTablePattern, standaloneTableReplacement, RegexOptions.IgnoreCase);
```

**Esto captura:**
- `ON Funcionarios.Id` ? `ON "Funcionarios".Id`
- `GROUP BY Funcionarios.Name` ? `GROUP BY "Funcionarios".Name`
- `WHERE Funcionarios.Status = 1` ? `WHERE "Funcionarios".Status = 1`

---

### **2. Informar a la IA sobre el Tipo de BD**

Modificamos el esquema enviado a la IA para incluir el tipo de base de datos:

```csharp
// ? MEJORADO: Obtener tipo de BD temprano
var databaseType = currentConnection?.DatabaseType ?? currentSchema.DatabaseType;

// Crear esquema con contexto
var schemaWithContext = new DatabaseSchema
{
    DatabaseName = $"{currentSchema.DatabaseName} ({databaseType})",  // ? Tipo de BD visible
    DatabaseType = databaseType,
    Tables = currentSchema.Tables,
    LoadedAt = currentSchema.LoadedAt
};

// Generar SQL con contexto
var generatedSql = await _ollamaService.GenerateSQLFromNaturalLanguageAsync(
    naturalLanguageQuery, 
    schemaWithContext  // ? IA sabe si es PostgreSQL o SQL Server
);
```

**Beneficios:**
- ? IA sabe usar `LIMIT` (PostgreSQL) vs `TOP` (SQL Server)
- ? IA sabe usar comillas si es necesario
- ? SQL más compatible con el motor específico

---

## ?? **Transformación Completa:**

### **SQL Generado (con bug):**
```sql
SELECT Funcionarios."FuncionarioID", COUNT(Capacitaciones."CapacitacionID") 
FROM "Funcionarios" 
LEFT JOIN "Capacitaciones" 
ON Funcionarios."FuncionarioID" = Capacitaciones."FuncionarioID"
GROUP BY Funcionarios."FuncionarioID"
```

### **SQL Corregido (después del fix):**
```sql
SELECT "Funcionarios"."FuncionarioID", COUNT("Capacitaciones"."CapacitacionID") 
FROM "Funcionarios" 
LEFT JOIN "Capacitaciones" 
ON "Funcionarios"."FuncionarioID" = "Capacitaciones"."FuncionarioID"
GROUP BY "Funcionarios"."FuncionarioID"
```

**? Todas las referencias consistentes con comillas!**

---

## ?? **Casos Manejados:**

### **Caso 1: JOIN con ON**
```sql
-- Entrada:
SELECT * FROM Funcionarios f
JOIN Capacitaciones c ON Funcionarios.Id = Capacitaciones.FuncionarioId

-- Salida:
SELECT * FROM "Funcionarios" f
JOIN "Capacitaciones" c ON "Funcionarios".Id = "Capacitaciones".FuncionarioId
```

---

### **Caso 2: GROUP BY con tabla**
```sql
-- Entrada:
SELECT Funcionarios.Nombre, COUNT(*) 
FROM Funcionarios 
GROUP BY Funcionarios.Nombre

-- Salida:
SELECT "Funcionarios".Nombre, COUNT(*) 
FROM "Funcionarios" 
GROUP BY "Funcionarios".Nombre
```

---

### **Caso 3: ORDER BY con tabla**
```sql
-- Entrada:
SELECT * FROM Funcionarios ORDER BY Funcionarios.Nombre

-- Salida:
SELECT * FROM "Funcionarios" ORDER BY "Funcionarios".Nombre
```

---

### **Caso 4: WHERE con tabla**
```sql
-- Entrada:
SELECT * FROM Funcionarios WHERE Funcionarios.Activo = TRUE

-- Salida:
SELECT * FROM "Funcionarios" WHERE "Funcionarios".Activo = TRUE
```

---

### **Caso 5: Múltiples referencias (mix)**
```sql
-- Entrada:
SELECT Funcionarios.Nombre, Capacitaciones.Titulo
FROM Funcionarios
JOIN Capacitaciones ON Funcionarios.Id = Capacitaciones.FuncionarioId
WHERE Funcionarios.Activo = TRUE
GROUP BY Funcionarios.Nombre, Capacitaciones.Titulo
ORDER BY Funcionarios.Nombre

-- Salida:
SELECT "Funcionarios".Nombre, "Capacitaciones".Titulo
FROM "Funcionarios"
JOIN "Capacitaciones" ON "Funcionarios".Id = "Capacitaciones".FuncionarioId
WHERE "Funcionarios".Activo = TRUE
GROUP BY "Funcionarios".Nombre, "Capacitaciones".Titulo
ORDER BY "Funcionarios".Nombre
```

---

## ?? **Patrones de Regex:**

### **Patrón 1: FROM/JOIN** (ya existía)
```regex
\b(FROM|JOIN|UPDATE|DELETE FROM)\s+Funcionarios\b
? $1 "Funcionarios"
```

### **Patrón 2: Tabla con alias** (ya existía)
```regex
\bFuncionarios\s+(AS\s+)?([a-z]+)\b
? "Funcionarios" $1$2
```

### **Patrón 3: Tabla standalone** (? NUEVO)
```regex
\bFuncionarios\.
? "Funcionarios".
```

**Ejemplos:**
- `Funcionarios.Id` ? `"Funcionarios".Id` ?
- `FROM Funcionarios` ? `FROM "Funcionarios"` ? (patrón 1)
- `Funcionarios f` ? `"Funcionarios" f` ? (patrón 2)

---

## ?? **Diferencia Clave:**

### **ANTES (Bug):**
```
FROM "Funcionarios"          ? Con comillas
JOIN "Capacitaciones"        ? Con comillas
ON Funcionarios.Id           ? SIN comillas ? Error
GROUP BY Funcionarios.Name   ? SIN comillas ? Error
```

### **AHORA (Corregido):**
```
FROM "Funcionarios"          ? Con comillas
JOIN "Capacitaciones"        ? Con comillas
ON "Funcionarios".Id         ? Con comillas
GROUP BY "Funcionarios".Name ? Con comillas
```

---

## ?? **Prueba Ahora:**

```sh
# 1. Reinicia la aplicación
dotnet run

# 2. Haz una consulta con JOIN:
"funcionarios con más capacitaciones"

# 3. Verifica logs:
info: ?? Procesando consulta para PostgreSQL
info: ? SQL generado por IA: SELECT ... FROM Funcionarios ... ON Funcionarios.Id ...
info: ?? SQL con comillas aplicadas: SELECT ... FROM "Funcionarios" ... ON "Funcionarios".Id ...
info: ? Consulta exitosa: X filas retornadas

# 4. Debería funcionar sin error 42P01
```

---

## ?? **Logs Esperados:**

### **ANTES (Error):**
```
info: SELECT ... ON Funcionarios.Id ...
fail: ? Error: falta una entrada para la tabla «funcionarios» en la cláusula FROM
```

### **AHORA (Éxito):**
```
info: ?? Procesando consulta para PostgreSQL  ? IA sabe el tipo de BD
info: ? SQL generado por IA: SELECT ... ON Funcionarios.Id ...
info: ?? SQL con comillas aplicadas: SELECT ... ON "Funcionarios".Id ...
info: ? Consulta ejecutada exitosamente
info: ? Consulta exitosa: 5 filas retornadas
```

---

## ? **Resumen del Fix:**

```
???????????????????????????????????????????????????
?  PROBLEMA:                                      ?
?  ? ON Funcionarios.Id (sin comillas)           ?
?  ? GROUP BY Funcionarios.Name (sin comillas)   ?
?  ? Referencias inconsistentes                   ?
?  ? Error 42P01: tabla no encontrada            ?
???????????????????????????????????????????????????
                    ?
???????????????????????????????????????????????????
?  SOLUCIÓN:                                      ?
?  ? Patrón 3: \bTableName\. ? "TableName".     ?
?  ? Captura TODAS las referencias               ?
?  ? Comillas consistentes                       ?
?  ? IA informada del tipo de BD                 ?
???????????????????????????????????????????????????
```

---

## ?? **Resultado Final:**

| Cláusula | Antes | Ahora |
|----------|-------|-------|
| FROM | `"Funcionarios"` ? | `"Funcionarios"` ? |
| JOIN | `"Capacitaciones"` ? | `"Capacitaciones"` ? |
| ON | `Funcionarios.Id` ? | `"Funcionarios".Id` ? |
| GROUP BY | `Funcionarios.Name` ? | `"Funcionarios".Name` ? |
| ORDER BY | `Funcionarios.Name` ? | `"Funcionarios".Name` ? |
| WHERE | `Funcionarios.Status` ? | `"Funcionarios".Status` ? |

---

**¡Reinicia la app y prueba! Ahora TODAS las referencias de tabla tendrán comillas consistentemente. ??**
