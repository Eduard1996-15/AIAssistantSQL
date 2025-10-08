# ?? Gu�a de Uso con tu Base de Datos BD_SCU

## Conexi�n R�pida

### Tu Cadena de Conexi�n:
```
Server=SKILAINE3\SQLEXPRESS;Database=BD_SCU;Trusted_Connection=True;TrustServerCertificate=True;Integrated Security=True;
```

## Pasos para Configurar

### 1. Aseg�rate que Ollama est� corriendo

```bash
ollama serve
```

Si no tienes el modelo, desc�rgalo:
```bash
ollama pull llama2
# O mejor a�n, usa codellama para SQL:
ollama pull codellama
```

### 2. Ejecuta la Aplicaci�n

```bash
cd AIAssistantSQL
dotnet run
```

Abre: **https://localhost:5001**

### 3. Configura tu Base de Datos

1. Ve a **"Configuraci�n BD"**
2. Usa la **OPCI�N RECOMENDADA** (arriba en la p�gina)
3. Selecciona: **SQL Server**
4. Pega tu cadena de conexi�n:
   ```
   Server=SKILAINE3\SQLEXPRESS;Database=BD_SCU;Trusted_Connection=True;TrustServerCertificate=True;Integrated Security=True;
   ```
5. Click en **"Conectar y Extraer Esquema Completo"**

**�Listo!** El sistema autom�ticamente:
- ? Se conecta a tu BD_SCU
- ? Extrae todas las tablas (Usuario, Documento, Roles, Logs, etc.)
- ? Mapea columnas, claves primarias y for�neas
- ? Deja todo listo para consultar

---

## ?? Consultas Espec�ficas para tu BD_SCU

Ahora puedes ir a **"Consultas"** y preguntar:

### Consultas sobre Usuarios
```
"Mu�strame todos los usuarios activos"
"�Cu�ntos usuarios hay por nivel organizacional?"
"Lista los usuarios con rol de administrador"
"Usuarios registrados en el �ltimo mes"
"Mu�strame los usuarios inactivos"
```

### Consultas sobre Documentos
```
"�Cu�ntos documentos hay en total?"
"Lista los documentos por nivel organizacional"
"Mu�strame los �ltimos 10 documentos creados"
"Documentos pendientes de aprobaci�n"
"�Cu�ntos documentos hay en cada versi�n?"
"Documentos que necesitan revisi�n"
```

### Consultas sobre Roles y Permisos
```
"Lista todos los roles disponibles"
"�Qu� roles tiene el usuario con ID 5?"
"Usuarios con m�ltiples roles"
"�Cu�ntos usuarios hay por rol?"
```

### Consultas sobre Logs y Auditor�a
```
"Mu�strame los �ltimos 20 logs del sistema"
"Logs de seguridad del �ltimo d�a"
"�Qu� usuarios han generado m�s logs?"
"Logs de errores o ataques detectados"
```

### Consultas sobre Notificaciones
```
"�Cu�ntas notificaciones no le�das hay?"
"Notificaciones por rol"
"�ltimas notificaciones creadas"
```

### Consultas sobre Estructura Organizacional
```
"Lista todos los niveles organizacionales"
"�Cu�ntos subniveles hay por nivel?"
"Mu�strame la estructura completa de niveles y subniveles"
```

### Consultas sobre Revisiones de Documentos
```
"Documentos en revisi�n"
"�Cu�ntas revisiones hay pendientes?"
"�ltimas revisiones creadas"
"Revisiones por estado"
```

### Consultas Complejas
```
"Mu�strame usuarios con documentos pendientes"
"�Qu� usuarios tienen acceso a determinado nivel organizacional?"
"Documentos con m�ltiples revisiones"
"Usuarios m�s activos en el sistema (por logs)"
```

---

## ?? Tablas que el Sistema Detectar�

Seg�n tu DbContext, estas son las tablas que se extraer�n autom�ticamente:

1. **SecurityLogs** - Logs de seguridad y ataques
2. **Documento** - Documentos del sistema
3. **DocumentoRevision** - Revisiones de documentos
4. **LineasDocumento** - L�neas/detalles de documentos
5. **Logs** - Logs generales del sistema
6. **NivelOrganizacional** - Niveles de la organizaci�n
7. **Notificacion** - Notificaciones del sistema
8. **Role** - Roles de usuario
9. **RolesUsuario** - Relaci�n usuarios-roles
10. **Subnivel** - Subniveles organizacionales
11. **UserPassword** - Historial de contrase�as
12. **Usuario** - Usuarios del sistema

---

## ?? Guardar el Esquema para Reutilizar

Despu�s de conectarte y extraer el esquema:

1. Ve a **"Configuraci�n BD"**
2. Baja hasta **"Guardar Esquema Actual"**
3. Nombre sugerido: `BD_SCU_schema.json`
4. Click en **"Guardar Esquema"**

**Ahora puedes:**
- Usar ese archivo en otros proyectos
- Compartirlo con tu equipo
- Subirlo en otro equipo sin necesidad de la base de datos

---

## ?? Ejemplo de Flujo Completo

```
USUARIO: "Mu�strame todos los usuarios activos del sistema"

IA GENERA: 
SELECT * FROM Usuario WHERE activo = 1

RESULTADO: 
+----------+------------------+----------------+--------+
| UsuarioId | Name            | Email          | Activo |
+----------+------------------+----------------+--------+
| 1        | Juan P�rez      | juan@mail.com  | 1      |
| 2        | Mar�a Garc�a    | maria@mail.com | 1      |
...
```

---

## ?? Importante para tu BD_SCU

### Columnas espec�ficas de tu base de datos:

**Usuario:**
- `Activo` (bit) - Usa "usuarios activos" o "activo = 1"
- `Nivel` (varchar) - Relacionado con NivelOrganizacional
- `SubNivel` (int) - Relacionado con Subnivel

**Documento:**
- `DocResponsable` - Documento del responsable
- `GradoResponsable` - Grado del responsable
- `Version` - Versi�n del documento
- `FechaAprobacion` - Fecha de aprobaci�n

**Logs:**
- `Action` - Tipo de acci�n
- `TableName` - Tabla afectada
- `UserId` - Usuario que gener� el log

---

## ?? Tips para Mejores Resultados

1. **S� espec�fico con los nombres de columnas** que aparecen en tu BD
2. **Usa t�rminos relacionados** con tu dominio (documento, usuario, nivel, etc.)
3. **Prueba consultas simples primero** antes de las complejas
4. **Revisa el SQL generado** para aprender c�mo la IA interpreta tus preguntas

---

## ?? Si algo no funciona:

### Problema: No se conecta a la BD
**Soluci�n:**
- Verifica que SQL Server est� corriendo
- Confirma que tienes permisos de lectura en BD_SCU
- Prueba la cadena de conexi�n en SQL Server Management Studio

### Problema: Ollama no responde
**Soluci�n:**
```bash
# Reinicia Ollama
ollama serve

# Verifica que est� corriendo
curl http://localhost:11434/api/tags
```

### Problema: SQL generado es incorrecto
**Soluci�n:**
- Cambia a un modelo mejor: `ollama pull codellama`
- S� m�s espec�fico en tu pregunta
- Menciona nombres exactos de tablas/columnas

---

## ?? Recomendaci�n Final

**Para tu BD_SCU espec�ficamente:**

Cambia en `appsettings.json` el modelo a `codellama` que es mejor para SQL:

```json
{
  "Ollama": {
    "Url": "http://localhost:11434",
    "Model": "codellama"
  }
}
```

Luego ejecuta:
```bash
ollama pull codellama
```

**�Disfruta consultando tu BD_SCU con lenguaje natural! ??**
