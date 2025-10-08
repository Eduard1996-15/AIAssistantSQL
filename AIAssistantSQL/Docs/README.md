# ?? AI Assistant SQL

**Asistente de IA para generar consultas SQL desde lenguaje natural usando Ollama.**

Convierte preguntas en espa�ol (o ingl�s) como *"�Cu�ntos usuarios activos hay?"* en consultas SQL precisas, las ejecuta autom�ticamente y presenta los resultados de forma clara.

---

## ? **Caracter�sticas Principales**

- ??? **Lenguaje Natural ? SQL**: Escribe preguntas normales, obt�n SQL correcto
- ?? **IA Local con Ollama**: Sin costos de API, privacidad total
- ?? **Validaci�n Inteligente**: Detecta errores antes de ejecutar
- ?? **Autocorrecci�n**: Reintenta autom�ticamente si algo falla
- ?? **M�ltiples Vistas**: Respuesta de IA o tabla de datos
- ??? **Multi-DB**: Soporta SQL Server y PostgreSQL
- ?? **Seguro**: Solo consultas SELECT, validaci�n estricta
- ?? **Esquemas Portables**: Exporta/importa esquemas en JSON

---

## ?? **Instalaci�n R�pida (5 minutos)**

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

Este script instalar� autom�ticamente:
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

### **4. Ejecutar la Aplicaci�n**
```bash
cd AIAssistantSQL
dotnet run
```

Abre: **http://localhost:5000**

---

## ?? **Uso B�sico**

### **1. Configurar Base de Datos**
1. Ve a **"Configuraci�n BD"**
2. Ingresa tu cadena de conexi�n
3. Click **"Conectar y Extraer Esquema Completo"**

**Ejemplo (SQL Server):**
```
Server=localhost;Database=MiDB;Trusted_Connection=True;TrustServerCertificate=True;
```

### **2. Hacer Consultas**
1. Ve a **"Consultas"**
2. Escribe en lenguaje natural:
   - *"�Cu�ntos usuarios activos hay?"*
   - *"Muestra los documentos con m�s de 15 l�neas"*
   - *"Lista usuarios con sus roles"*

3. �Listo! La IA genera el SQL, lo ejecuta y muestra los resultados.

---

## ?? **Modelos de IA Disponibles**

| Modelo | Tama�o | Precisi�n SQL | Recomendado Para |
|--------|--------|---------------|------------------|
| **codellama** ? | 3.8 GB | ????? | **SQL complejo** (por defecto) |
| **mistral** | 4.1 GB | ???? | Balance velocidad/precisi�n |
| **deepseek-coder** | 3.8 GB | ????? | SQL muy avanzado |
| llama3.2 | 2.0 GB | ?? | ? NO recomendado |

**Ver m�s:** [MODELOS_IA.md](MODELOS_IA.md)

---

## ?? **Documentaci�n**

- [?? README.md](README.md) - Este archivo
- [?? INICIO_RAPIDO.md](INICIO_RAPIDO.md) - Gu�a paso a paso
- [?? MODELOS_IA.md](MODELOS_IA.md) - Gu�a de modelos de IA
- [?? GUIA_DIAGNOSTICO.md](GUIA_DIAGNOSTICO.md) - Soluci�n de problemas
- [?? RESUMEN_PROYECTO.md](RESUMEN_PROYECTO.md) - Arquitectura t�cnica
- [? POR_QUE_CODELLAMA.md](POR_QUE_CODELLAMA.md) - Por qu� usar CodeLlama

---

## ?? **Ejemplos de Consultas**

```
? "cu�ntos usuarios activos hay"
? SELECT COUNT(*) FROM Usuario WHERE activo = 1

? "documentos con m�s de 15 l�neas"
? SELECT d.DocumentoId, COUNT(*) FROM DOCUMENTO d 
   JOIN lineas_documento l ON d.DocumentoId = l.DocumentoId 
   GROUP BY d.DocumentoId HAVING COUNT(*) > 15

? "usuarios con sus roles"
? SELECT u.UserName, r.NombreRol FROM Usuario u
   JOIN Roles_usuario ru ON u.UsuarioId = ru.UsuarioId
   JOIN Roles r ON ru.RolId = r.RolId

? "�ltimos 10 documentos creados"
? SELECT TOP 10 * FROM DOCUMENTO ORDER BY FechaCreacion DESC
```

---

## ?? **Soluci�n de Problemas**

### **Ollama no est� disponible**
```bash
# Iniciar Ollama
ollama serve

# Verificar que est� corriendo
curl http://localhost:11434/api/tags
```

### **SQL generado es incorrecto**
1. Verifica que est�s usando **CodeLlama** (no llama3.2)
2. Ve a "Configuraci�n" ? Cambiar a codellama
3. S� m�s espec�fico en tu pregunta

### **Error: columna no existe**
1. Ve a "Diagn�stico" para ver las columnas exactas
2. El sistema autocorrige variaciones de nombres (guiones, may�sculas)
3. Si persiste, usa nombres exactos: `SELECT UsuarioId FROM Usuario`

**Ver m�s:** [GUIA_DIAGNOSTICO.md](GUIA_DIAGNOSTICO.md)

---

## ?? **Caracter�sticas Destacadas**

### **Validaci�n Inteligente**
- ? Verifica tablas y columnas antes de ejecutar
- ? Autocorrige variaciones de nombres (case-insensitive)
- ? Normaliza guiones bajos y espacios
- ? Sugiere alternativas si hay errores

### **Autocorrecci�n Autom�tica**
```
1. IA genera: SELECT * FROM lineasdocumento
   ?
2. Validaci�n detecta: tabla "lineasdocumento" no existe
   ?
3. Autocorrecci�n: lineasdocumento ? lineas_documento
   ?
4. ? Ejecuta: SELECT * FROM lineas_documento
```

### **Doble Vista de Resultados**
- ?? **Respuesta Natural**: La IA interpreta y explica los resultados
- ?? **Tabla de Datos**: Datos crudos en tabla HTML
- ?? Alterna entre vistas con un click

### **Comprende Relaciones**
- ?? Entiende Foreign Keys
- ?? Hace JOINs autom�ticos cuando es necesario
- ?? Agrupa y cuenta registros relacionados (GROUP BY, HAVING)

---

## ??? **Arquitectura**

```
???????????????????????????????????????????????????
?  FRONTEND (Razor Pages)                         ?
?  ?? Consultas en lenguaje natural               ?
?  ?? Configuraci�n de BD                         ?
?  ?? Visualizaci�n de resultados                 ?
???????????????????????????????????????????????????
                    ?
???????????????????????????????????????????????????
?  BACKEND (.NET 8)                               ?
?  ?? OllamaService (IA)                          ?
?  ?? SchemaLoaderService (Esquemas)              ?
?  ?? QueryRepository (Ejecuci�n SQL)             ?
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

## ?? **Stack Tecnol�gico**

- **Backend**: .NET 8, ASP.NET Core MVC
- **Frontend**: Razor Pages, Bootstrap 5, jQuery
- **IA**: Ollama (CodeLlama)
- **Base de Datos**: SQL Server, PostgreSQL
- **ORM**: Dapper
- **Librer�as**: Microsoft.Data.SqlClient, Npgsql

---

## ?? **Seguridad**

- ? Solo consultas SELECT (no INSERT/UPDATE/DELETE)
- ? Validaci�n de SQL antes de ejecutar
- ? Sanitizaci�n de entradas
- ? Sin ejecuci�n de c�digo arbitrario
- ? IA local (sin env�o de datos a internet)

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

�Encontraste un bug? �Tienes una idea?
- ?? Reporta issues
- ?? Sugiere mejoras
- ?? Contribuye con PRs

---

**�Disfruta consultando bases de datos con lenguaje natural! ??**
