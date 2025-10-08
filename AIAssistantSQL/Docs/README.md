# ?? AI Assistant SQL

**Asistente de IA para generar consultas SQL desde lenguaje natural usando Ollama.**

Convierte preguntas en español (o inglés) como *"¿Cuántos usuarios activos hay?"* en consultas SQL precisas, las ejecuta automáticamente y presenta los resultados de forma clara.

---

## ? **Características Principales**

- ??? **Lenguaje Natural ? SQL**: Escribe preguntas normales, obtén SQL correcto
- ?? **IA Local con Ollama**: Sin costos de API, privacidad total
- ?? **Validación Inteligente**: Detecta errores antes de ejecutar
- ?? **Autocorrección**: Reintenta automáticamente si algo falla
- ?? **Múltiples Vistas**: Respuesta de IA o tabla de datos
- ??? **Multi-DB**: Soporta SQL Server y PostgreSQL
- ?? **Seguro**: Solo consultas SELECT, validación estricta
- ?? **Esquemas Portables**: Exporta/importa esquemas en JSON

---

## ?? **Instalación Rápida (5 minutos)**

### **1. Pre-requisitos**
- .NET 8 SDK ([descargar](https://dotnet.microsoft.com/download/dotnet/8.0))
- Ollama ([descargar](https://ollama.ai))

### **2. Instalar Modelos de IA (IMPORTANTE)**

El proyecto usa **CodeLlama** por defecto (el mejor para SQL).

#### **Windows:**
```cmd
cd AIAssistantSQL
install-models.bat
```

#### **Linux/Mac:**
```bash
cd AIAssistantSQL
chmod +x install-models.sh
./install-models.sh
```

Este script instalará automáticamente:
- ? **CodeLlama** (3.8 GB) - Modelo principal
- ? **Mistral** (4.1 GB) - Alternativa
- ? **DeepSeek Coder** (3.8 GB) - Para SQL avanzado

**O manualmente:**
```bash
ollama pull codellama
```

### **3. Iniciar Ollama**
```bash
ollama serve
```

### **4. Ejecutar la Aplicación**
```bash
cd AIAssistantSQL
dotnet run
```

Abre: **http://localhost:5000**

---

## ?? **Uso Básico**

### **1. Configurar Base de Datos**
1. Ve a **"Configuración BD"**
2. Ingresa tu cadena de conexión
3. Click **"Conectar y Extraer Esquema Completo"**

**Ejemplo (SQL Server):**
```
Server=localhost;Database=MiDB;Trusted_Connection=True;TrustServerCertificate=True;
```

### **2. Hacer Consultas**
1. Ve a **"Consultas"**
2. Escribe en lenguaje natural:
   - *"¿Cuántos usuarios activos hay?"*
   - *"Muestra los documentos con más de 15 líneas"*
   - *"Lista usuarios con sus roles"*

3. ¡Listo! La IA genera el SQL, lo ejecuta y muestra los resultados.

---

## ?? **Modelos de IA Disponibles**

| Modelo | Tamaño | Precisión SQL | Recomendado Para |
|--------|--------|---------------|------------------|
| **codellama** ? | 3.8 GB | ????? | **SQL complejo** (por defecto) |
| **mistral** | 4.1 GB | ???? | Balance velocidad/precisión |
| **deepseek-coder** | 3.8 GB | ????? | SQL muy avanzado |
| llama3.2 | 2.0 GB | ?? | ? NO recomendado |

**Ver más:** [MODELOS_IA.md](MODELOS_IA.md)

---

## ?? **Documentación**

- [?? README.md](README.md) - Este archivo
- [?? INICIO_RAPIDO.md](INICIO_RAPIDO.md) - Guía paso a paso
- [?? MODELOS_IA.md](MODELOS_IA.md) - Guía de modelos de IA
- [?? GUIA_DIAGNOSTICO.md](GUIA_DIAGNOSTICO.md) - Solución de problemas
- [?? RESUMEN_PROYECTO.md](RESUMEN_PROYECTO.md) - Arquitectura técnica
- [? POR_QUE_CODELLAMA.md](POR_QUE_CODELLAMA.md) - Por qué usar CodeLlama

---

## ?? **Ejemplos de Consultas**

```
? "cuántos usuarios activos hay"
? SELECT COUNT(*) FROM Usuario WHERE activo = 1

? "documentos con más de 15 líneas"
? SELECT d.DocumentoId, COUNT(*) FROM DOCUMENTO d 
   JOIN lineas_documento l ON d.DocumentoId = l.DocumentoId 
   GROUP BY d.DocumentoId HAVING COUNT(*) > 15

? "usuarios con sus roles"
? SELECT u.UserName, r.NombreRol FROM Usuario u
   JOIN Roles_usuario ru ON u.UsuarioId = ru.UsuarioId
   JOIN Roles r ON ru.RolId = r.RolId

? "últimos 10 documentos creados"
? SELECT TOP 10 * FROM DOCUMENTO ORDER BY FechaCreacion DESC
```

---

## ?? **Solución de Problemas**

### **Ollama no está disponible**
```bash
# Iniciar Ollama
ollama serve

# Verificar que esté corriendo
curl http://localhost:11434/api/tags
```

### **SQL generado es incorrecto**
1. Verifica que estés usando **CodeLlama** (no llama3.2)
2. Ve a "Configuración" ? Cambiar a codellama
3. Sé más específico en tu pregunta

### **Error: columna no existe**
1. Ve a "Diagnóstico" para ver las columnas exactas
2. El sistema autocorrige variaciones de nombres (guiones, mayúsculas)
3. Si persiste, usa nombres exactos: `SELECT UsuarioId FROM Usuario`

**Ver más:** [GUIA_DIAGNOSTICO.md](GUIA_DIAGNOSTICO.md)

---

## ?? **Características Destacadas**

### **Validación Inteligente**
- ? Verifica tablas y columnas antes de ejecutar
- ? Autocorrige variaciones de nombres (case-insensitive)
- ? Normaliza guiones bajos y espacios
- ? Sugiere alternativas si hay errores

### **Autocorrección Automática**
```
1. IA genera: SELECT * FROM lineasdocumento
   ?
2. Validación detecta: tabla "lineasdocumento" no existe
   ?
3. Autocorrección: lineasdocumento ? lineas_documento
   ?
4. ? Ejecuta: SELECT * FROM lineas_documento
```

### **Doble Vista de Resultados**
- ?? **Respuesta Natural**: La IA interpreta y explica los resultados
- ?? **Tabla de Datos**: Datos crudos en tabla HTML
- ?? Alterna entre vistas con un click

### **Comprende Relaciones**
- ?? Entiende Foreign Keys
- ?? Hace JOINs automáticos cuando es necesario
- ?? Agrupa y cuenta registros relacionados (GROUP BY, HAVING)

---

## ??? **Arquitectura**

```
???????????????????????????????????????????????????
?  FRONTEND (Razor Pages)                         ?
?  ?? Consultas en lenguaje natural               ?
?  ?? Configuración de BD                         ?
?  ?? Visualización de resultados                 ?
???????????????????????????????????????????????????
                    ?
???????????????????????????????????????????????????
?  BACKEND (.NET 8)                               ?
?  ?? OllamaService (IA)                          ?
?  ?? SchemaLoaderService (Esquemas)              ?
?  ?? QueryRepository (Ejecución SQL)             ?
?  ?? SqlValidatorService (Seguridad)             ?
???????????????????????????????????????????????????
                    ?
???????????????????????????????????????????????????
?  OLLAMA (IA Local)                              ?
?  ?? CodeLlama (Modelo por defecto)              ?
???????????????????????????????????????????????????
                    ?
???????????????????????????????????????????????????
?  BASE DE DATOS                                  ?
?  ?? SQL Server                                  ?
?  ?? PostgreSQL                                  ?
???????????????????????????????????????????????????
```

---

## ?? **Stack Tecnológico**

- **Backend**: .NET 8, ASP.NET Core MVC
- **Frontend**: Razor Pages, Bootstrap 5, jQuery
- **IA**: Ollama (CodeLlama)
- **Base de Datos**: SQL Server, PostgreSQL
- **ORM**: Dapper
- **Librerías**: Microsoft.Data.SqlClient, Npgsql

---

## ?? **Seguridad**

- ? Solo consultas SELECT (no INSERT/UPDATE/DELETE)
- ? Validación de SQL antes de ejecutar
- ? Sanitización de entradas
- ? Sin ejecución de código arbitrario
- ? IA local (sin envío de datos a internet)

---

## ?? **Licencia**

MIT License - Ver [LICENSE](LICENSE)

---

## ?? **Agradecimientos**

- [Ollama](https://ollama.ai) - IA local sin costo
- [CodeLlama](https://github.com/facebookresearch/codellama) - Modelo de Meta
- [Dapper](https://github.com/DapperLib/Dapper) - Micro ORM
- [Bootstrap](https://getbootstrap.com) - Framework CSS

---

## ?? **Contacto y Contribuciones**

¿Encontraste un bug? ¿Tienes una idea?
- ?? Reporta issues
- ?? Sugiere mejoras
- ?? Contribuye con PRs

---

**¡Disfruta consultando bases de datos con lenguaje natural! ??**
