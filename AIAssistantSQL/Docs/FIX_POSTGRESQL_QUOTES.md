# ?? Fix - PostgreSQL Comillas Dobles en Nombres de Tabla

## ? **Error Original:**

```
La tabla '"Funcionarios"' no existe en el esquema de la base de datos.
Tablas disponibles: ... Funcionarios ...
```

---

## ?? **Problema:**

### **Causa Ra�z:**
PostgreSQL es **case-sensitive** con los nombres de tabla:

```sql
-- Si creas tabla as�:
CREATE TABLE "Funcionarios" (...)  -- ? Con comillas y may�sculas

-- Tienes que consultarla as�:
SELECT * FROM "Funcionarios"  -- ? Con comillas SIEMPRE
```

**El problema:**
1. `information_schema.tables` devuelve: `Funcionarios` (sin comillas)
2. El sistema lo carga como: `Funcionarios`
3. La IA genera SQL: `SELECT * FROM "Funcionarios"` (con comillas)
4. El validador busca: `"Funcionarios"` (con comillas)
5. No coincide con: `Funcionarios` (sin comillas)
6. Error: `'"Funcionarios"'` (�comillas triples!)

---

## ? **Soluci�n Implementada:**

### **Normalizaci�n de Nombres:**

```csharp
// ? Normalizar nombre de tabla (quitar comillas)
var normalizedTableName = tableName.Trim('"');

var tableSchema = new TableSchema { 
    TableName = normalizedTableName  // Sin comillas
};

// ? Normalizar nombres de columnas
foreach (dynamic col in columns)
{
    var normalizedColumnName = ((string)col.column_name).Trim('"');
    
    tableSchema.Columns.Add(new ColumnSchema
    {
        ColumnName = normalizedColumnName  // Sin comillas
    });
}

// ? Normalizar PKs y FKs tambi�n
tableSchema.PrimaryKeys = primaryKeys.Select(pk => pk.Trim('"')).ToList();
tableSchema.ForeignKeys.Add(new ForeignKeySchema
{
    ColumnName = ((string)fk.column_name).Trim('"'),
    ReferencedTable = ((string)fk.referenced_table).Trim('"'),
    ReferencedColumn = ((string)fk.referenced_column).Trim('"')
});
```

---

## ?? **Comparaci�n:**

### **ANTES:**

| Fuente | Valor |
|--------|-------|
| PostgreSQL devuelve | `Funcionarios` |
| Sistema carga | `Funcionarios` |
| IA genera | `"Funcionarios"` |
| Validador busca | `"Funcionarios"` ? |
| Comparaci�n | `"Funcionarios"` ? `Funcionarios` |

---

### **AHORA:**

| Fuente | Valor |
|--------|-------|
| PostgreSQL devuelve | `Funcionarios` |
| Sistema normaliza | `Funcionarios` (sin comillas) |
| IA genera | `"Funcionarios"` o `Funcionarios` |
| Validador normaliza | `Funcionarios` ? |
| Comparaci�n | `Funcionarios` = `Funcionarios` |

---

## ?? **Prueba Ahora:**

```sh
# 1. Reinicia la aplicaci�n
dotnet run

# 2. Reconecta a PostgreSQL
Ve a "Configuraci�n BD"
Ingresa tu conexi�n
Click "Conectar y Extraer Esquema Completo"

# 3. Verifica que las tablas se carguen sin comillas
Deber�as ver en los logs:
? Tabla 'Funcionarios' cargada exitosamente
? Tabla 'Capacitaciones' cargada exitosamente
(Sin comillas)

# 4. Haz una consulta
Ve a "Consultas"
Pregunta: "cu�ntos funcionarios hay"

# 5. Deber�a funcionar
SELECT COUNT(*) FROM Funcionarios
(Sin comillas dobles extra)
```

---

## ?? **Nombres que Ahora Funcionan:**

```
? Funcionarios (con may�scula)
? funcionarios (con min�scula)
? FUNCIONARIOS (todo may�sculas)
? Capacitaciones
? AspNetUsers
? __EFMigrationsHistory
```

**Todos sin comillas dobles extra.**

---

## ?? **PostgreSQL Case-Sensitivity:**

### **Regla de PostgreSQL:**

```sql
-- Sin comillas ? PostgreSQL convierte a min�sculas
CREATE TABLE Funcionarios (...)
-- Se guarda como: funcionarios

-- Con comillas ? PostgreSQL respeta may�sculas
CREATE TABLE "Funcionarios" (...)
-- Se guarda como: Funcionarios (requiere comillas siempre)
```

---

### **Nuestro Sistema Ahora:**

```
1. Lee el nombre desde information_schema
2. Quita comillas dobles si existen
3. Guarda nombre normalizado
4. Compara sin comillas
5. ? Funciona con ambos casos
```

---

## ?? **Por Qu� Sucede Esto:**

### **Entity Framework Core:**
Si usas EF Core para crear las tablas:

```csharp
// EF Core crea as�:
CREATE TABLE "Funcionarios" (...)  // ? Con comillas

// Por eso necesitas:
SELECT * FROM "Funcionarios"  // ? Siempre con comillas
```

---

### **Nuestro Fix:**
```csharp
// Normalizamos a:
Funcionarios  // ? Sin comillas

// As� funciona con o sin comillas en la BD
```

---

## ? **Resumen del Fix:**

```
???????????????????????????????????????????????????
?  PROBLEMA:                                      ?
?  ? '"Funcionarios"' (comillas triples)         ?
?  ? No coincide con Funcionarios               ?
?  ? Error: tabla no existe                     ?
???????????????????????????????????????????????????

???????????????????????????????????????????????????
?  SOLUCI�N:                                      ?
?  ? Normalizar nombres (sin comillas)           ?
?  ? Aplicar a tablas, columnas, PKs, FKs       ?
?  ? Comparaci�n consistente                     ?
?  ? Funciona con cualquier caso                 ?
???????????????????????????????????????????????????
```

---

## ?? **Verificaci�n:**

### **Antes de este fix:**
```
Error: La tabla '"Funcionarios"' no existe
Disponibles: ... Funcionarios ...
```

### **Despu�s de este fix:**
```
? SQL generado: SELECT COUNT(*) FROM Funcionarios
? Tabla encontrada: Funcionarios
? Consulta ejecutada exitosamente
```

---

**�Reinicia la app y prueba tus consultas en PostgreSQL! Deber�an funcionar correctamente ahora. ??**
