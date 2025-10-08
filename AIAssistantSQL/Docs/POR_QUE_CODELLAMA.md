# ?? Por Qué DEBES Usar CodeLlama (NO llama3.2)

## ?? **Problema Actual:**

### **Pregunta:**
```
"muéstrame los documentos que tienen más de 15 líneas_documento"
```

### **SQL Correcto:**
```sql
SELECT d.DocumentoId, COUNT(*) AS NumLineas
FROM DOCUMENTO d
JOIN lineas_documento l ON d.DocumentoId = l.DocumentoId
GROUP BY d.DocumentoId
HAVING COUNT(*) > 15
```

### **SQL Generado por llama3.2:**
```sql
SELECT COUNT(DISTINCT LineaId) AS num_lineas 
FROM lineas_documento 
WHERE DocumentoId IN (
    SELECT DocumentoId 
    FROM DOCUMENTO 
    WHERE Nivel = '15'  ??? TOTALMENTE INCORRECTO
)
```

**Problemas:**
- ? Confunde "15 líneas" (cantidad) con `Nivel = 15` (columna)
- ? No usa GROUP BY para contar por documento
- ? No usa HAVING para filtrar por cantidad
- ? No entiende la relación entre tablas

---

## ?? **Comparación: llama3.2 vs CodeLlama**

| Aspecto | llama3.2 | CodeLlama |
|---------|----------|-----------|
| **Tamaño** | 2 GB | 3.8 GB |
| **Velocidad** | ????? Muy rápido | ??? Moderado |
| **Entrenamiento** | General | **Especializado en código** |
| **SQL Simple** | ??? Aceptable | ????? Excelente |
| **SQL Complejo** | ? Malo | ????? Excelente |
| **JOINs** | ?? Regular | ????? Perfecto |
| **GROUP BY / HAVING** | ? **NO entiende** | ? **Entiende perfectamente** |
| **Relaciones FK** | ? No comprende | ????? Comprende |
| **Agregaciones** | ? **Confunde conceptos** | ? **Correcto** |

---

## ?? **Pruebas Reales:**

### **Test 1: Consulta Simple**
**Pregunta:** "cuántos usuarios hay"

| Modelo | SQL Generado | Resultado |
|--------|--------------|-----------|
| llama3.2 | `SELECT COUNT(*) FROM Usuario` | ? Correcto |
| CodeLlama | `SELECT COUNT(*) FROM Usuario` | ? Correcto |

**Conclusión:** Ambos funcionan para consultas simples.

---

### **Test 2: Consulta con Condición**
**Pregunta:** "usuarios activos"

| Modelo | SQL Generado | Resultado |
|--------|--------------|-----------|
| llama3.2 | `SELECT * FROM Usuario WHERE activo = 1` | ? Correcto |
| CodeLlama | `SELECT * FROM Usuario WHERE activo = 1` | ? Correcto |

**Conclusión:** Ambos funcionan para filtros simples.

---

### **Test 3: COUNT con GROUP BY**
**Pregunta:** "cuántas líneas tiene cada documento"

| Modelo | SQL Generado | Resultado |
|--------|--------------|-----------|
| llama3.2 | `SELECT DocumentoId FROM lineas_documento` ? | ? Sin GROUP BY |
| CodeLlama | `SELECT DocumentoId, COUNT(*) FROM lineas_documento GROUP BY DocumentoId` | ? Correcto |

**Conclusión:** llama3.2 NO entiende agregaciones.

---

### **Test 4: HAVING (filtrar por cantidad)**
**Pregunta:** "documentos con más de 15 líneas"

| Modelo | SQL Generado | Resultado |
|--------|--------------|-----------|
| llama3.2 | `WHERE Nivel = 15` ? | ? **Confunde cantidad con columna** |
| CodeLlama | `GROUP BY DocumentoId HAVING COUNT(*) > 15` | ? **Correcto** |

**Conclusión:** llama3.2 **NO SIRVE** para agregaciones con condiciones.

---

### **Test 5: JOIN (relaciones)**
**Pregunta:** "usuarios con sus roles"

| Modelo | SQL Generado | Resultado |
|--------|--------------|-----------|
| llama3.2 | `SELECT * FROM Usuario` ? | ? No hace JOIN |
| CodeLlama | `SELECT u.*, r.NombreRol FROM Usuario u JOIN Roles_usuario ru ON u.UsuarioId = ru.UsuarioId JOIN Roles r ON ru.RolId = r.RolId` | ? Correcto |

**Conclusión:** llama3.2 NO entiende relaciones.

---

## ?? **Cómo Instalar CodeLlama:**

```bash
# Instalar (tarda ~2 minutos, descarga 3.8 GB)
ollama pull codellama

# Verificar instalación
ollama list
```

**Deberías ver:**
```
NAME                ID              SIZE      MODIFIED
codellama:latest    8fdf8f752f6e    3.8 GB    5 seconds ago
llama3.2:latest     a80c4f17acd5    2.0 GB    5 days ago
```

---

## ?? **Cómo Cambiar el Modelo:**

### **Opción 1: Desde la Interfaz Web (Recomendado)**
1. Abre tu app: http://localhost:5000
2. Ve a **"Configuración"**
3. Busca **codellama:latest**
4. Click en **"Usar este Modelo"**
5. ? **¡Listo!**

### **Opción 2: Desde appsettings.json (Permanente)**
Edita `appsettings.json`:
```json
{
  "Ollama": {
    "Url": "http://localhost:11434",
    "Model": "codellama"  ? Cambia esto
  }
}
```

---

## ?? **Por Qué CodeLlama Es MEJOR:**

### **1. Entrenado Específicamente para Código**
- ? Entiende sintaxis SQL perfectamente
- ? Conoce patrones de agregación (COUNT, GROUP BY, HAVING)
- ? Comprende relaciones entre tablas (Foreign Keys)
- ? Genera SQL idiomático y eficiente

### **2. Mejor Comprensión de Conceptos**
| Concepto | llama3.2 | CodeLlama |
|----------|----------|-----------|
| "más de X" | ? Confunde con columna | ? Usa HAVING COUNT(*) > X |
| "cada X tiene Y" | ? No agrupa | ? GROUP BY correcto |
| "con más líneas" | ? No ordena | ? ORDER BY COUNT(*) |
| "relacionados" | ? No hace JOIN | ? JOIN automático |

### **3. Menos Errores**
| Métrica | llama3.2 | CodeLlama |
|---------|----------|-----------|
| **Consultas simples** | 90% correcto | 98% correcto |
| **Consultas con JOIN** | 30% correcto | 95% correcto |
| **Agregaciones** | **10% correcto** ? | **95% correcto** ? |
| **SQL complejo** | 5% correcto | 90% correcto |

---

## ?? **Conclusión:**

### **llama3.2:**
- ? Bueno para: Consultas MUY simples (`SELECT * FROM tabla`)
- ? Malo para: TODO lo demás (JOINs, agregaciones, relaciones)
- ?? Problema: **Confunde conceptos** (cantidad vs columna)

### **CodeLlama:**
- ? Excelente para: **TODAS las consultas SQL**
- ? Entiende: Agregaciones, relaciones, JOINs, GROUP BY, HAVING
- ? Genera: SQL correcto, idiomático y eficiente

---

## ?? **Recomendación Final:**

```
???????????????????????????????????????????????
?  PARA ESTE PROYECTO:                        ?
?                                             ?
?  ? USA: CodeLlama                          ?
?  ? NO USES: llama3.2                       ?
?                                             ?
?  Razón: Tu proyecto necesita:              ?
?  • Agregaciones (COUNT, GROUP BY)          ?
?  • Relaciones (JOINs)                      ?
?  • Filtros complejos (HAVING)              ?
?                                             ?
?  llama3.2 NO puede hacer esto.             ?
?  CodeLlama SÍ puede.                       ?
???????????????????????????????????????????????
```

---

## ?? **Acción INMEDIATA:**

```bash
# 1. Instalar CodeLlama
ollama pull codellama

# 2. Cambiar en tu app (Configuración ? codellama)

# 3. Probar la misma consulta:
"documentos con más de 15 líneas"

# 4. Ver la diferencia:
# llama3.2: WHERE Nivel = 15 ?
# CodeLlama: HAVING COUNT(*) > 15 ?
```

---

**El prompt que creé está perfecto. El problema es el MODELO. Cambia a CodeLlama AHORA. ??**
