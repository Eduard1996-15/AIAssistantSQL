# ?? Guía de Uso con tu Base de Datos BD_SCU

## Conexión Rápida

### Tu Cadena de Conexión:
```
Server=SKILAINE3\SQLEXPRESS;Database=BD_SCU;Trusted_Connection=True;TrustServerCertificate=True;Integrated Security=True;
```

## Pasos para Configurar

### 1. Asegúrate que Ollama esté corriendo

```bash
ollama serve
```

Si no tienes el modelo, descárgalo:
```bash
ollama pull llama2
# O mejor aún, usa codellama para SQL:
ollama pull codellama
```

### 2. Ejecuta la Aplicación

```bash
cd AIAssistantSQL
dotnet run
```

Abre: **https://localhost:5001**

### 3. Configura tu Base de Datos

1. Ve a **"Configuración BD"**
2. Usa la **OPCIÓN RECOMENDADA** (arriba en la página)
3. Selecciona: **SQL Server**
4. Pega tu cadena de conexión:
   ```
   Server=SKILAINE3\SQLEXPRESS;Database=BD_SCU;Trusted_Connection=True;TrustServerCertificate=True;Integrated Security=True;
   ```
5. Click en **"Conectar y Extraer Esquema Completo"**

**¡Listo!** El sistema automáticamente:
- ? Se conecta a tu BD_SCU
- ? Extrae todas las tablas (Usuario, Documento, Roles, Logs, etc.)
- ? Mapea columnas, claves primarias y foráneas
- ? Deja todo listo para consultar

---

## ?? Consultas Específicas para tu BD_SCU

Ahora puedes ir a **"Consultas"** y preguntar:

### Consultas sobre Usuarios
```
"Muéstrame todos los usuarios activos"
"¿Cuántos usuarios hay por nivel organizacional?"
"Lista los usuarios con rol de administrador"
"Usuarios registrados en el último mes"
"Muéstrame los usuarios inactivos"
```

### Consultas sobre Documentos
```
"¿Cuántos documentos hay en total?"
"Lista los documentos por nivel organizacional"
"Muéstrame los últimos 10 documentos creados"
"Documentos pendientes de aprobación"
"¿Cuántos documentos hay en cada versión?"
"Documentos que necesitan revisión"
```

### Consultas sobre Roles y Permisos
```
"Lista todos los roles disponibles"
"¿Qué roles tiene el usuario con ID 5?"
"Usuarios con múltiples roles"
"¿Cuántos usuarios hay por rol?"
```

### Consultas sobre Logs y Auditoría
```
"Muéstrame los últimos 20 logs del sistema"
"Logs de seguridad del último día"
"¿Qué usuarios han generado más logs?"
"Logs de errores o ataques detectados"
```

### Consultas sobre Notificaciones
```
"¿Cuántas notificaciones no leídas hay?"
"Notificaciones por rol"
"Últimas notificaciones creadas"
```

### Consultas sobre Estructura Organizacional
```
"Lista todos los niveles organizacionales"
"¿Cuántos subniveles hay por nivel?"
"Muéstrame la estructura completa de niveles y subniveles"
```

### Consultas sobre Revisiones de Documentos
```
"Documentos en revisión"
"¿Cuántas revisiones hay pendientes?"
"Últimas revisiones creadas"
"Revisiones por estado"
```

### Consultas Complejas
```
"Muéstrame usuarios con documentos pendientes"
"¿Qué usuarios tienen acceso a determinado nivel organizacional?"
"Documentos con múltiples revisiones"
"Usuarios más activos en el sistema (por logs)"
```

---

## ?? Tablas que el Sistema Detectará

Según tu DbContext, estas son las tablas que se extraerán automáticamente:

1. **SecurityLogs** - Logs de seguridad y ataques
2. **Documento** - Documentos del sistema
3. **DocumentoRevision** - Revisiones de documentos
4. **LineasDocumento** - Líneas/detalles de documentos
5. **Logs** - Logs generales del sistema
6. **NivelOrganizacional** - Niveles de la organización
7. **Notificacion** - Notificaciones del sistema
8. **Role** - Roles de usuario
9. **RolesUsuario** - Relación usuarios-roles
10. **Subnivel** - Subniveles organizacionales
11. **UserPassword** - Historial de contraseñas
12. **Usuario** - Usuarios del sistema

---

## ?? Guardar el Esquema para Reutilizar

Después de conectarte y extraer el esquema:

1. Ve a **"Configuración BD"**
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
USUARIO: "Muéstrame todos los usuarios activos del sistema"

IA GENERA: 
SELECT * FROM Usuario WHERE activo = 1

RESULTADO: 
+----------+------------------+----------------+--------+
| UsuarioId | Name            | Email          | Activo |
+----------+------------------+----------------+--------+
| 1        | Juan Pérez      | juan@mail.com  | 1      |
| 2        | María García    | maria@mail.com | 1      |
...
```

---

## ?? Importante para tu BD_SCU

### Columnas específicas de tu base de datos:

**Usuario:**
- `Activo` (bit) - Usa "usuarios activos" o "activo = 1"
- `Nivel` (varchar) - Relacionado con NivelOrganizacional
- `SubNivel` (int) - Relacionado con Subnivel

**Documento:**
- `DocResponsable` - Documento del responsable
- `GradoResponsable` - Grado del responsable
- `Version` - Versión del documento
- `FechaAprobacion` - Fecha de aprobación

**Logs:**
- `Action` - Tipo de acción
- `TableName` - Tabla afectada
- `UserId` - Usuario que generó el log

---

## ?? Tips para Mejores Resultados

1. **Sé específico con los nombres de columnas** que aparecen en tu BD
2. **Usa términos relacionados** con tu dominio (documento, usuario, nivel, etc.)
3. **Prueba consultas simples primero** antes de las complejas
4. **Revisa el SQL generado** para aprender cómo la IA interpreta tus preguntas

---

## ?? Si algo no funciona:

### Problema: No se conecta a la BD
**Solución:**
- Verifica que SQL Server esté corriendo
- Confirma que tienes permisos de lectura en BD_SCU
- Prueba la cadena de conexión en SQL Server Management Studio

### Problema: Ollama no responde
**Solución:**
```bash
# Reinicia Ollama
ollama serve

# Verifica que esté corriendo
curl http://localhost:11434/api/tags
```

### Problema: SQL generado es incorrecto
**Solución:**
- Cambia a un modelo mejor: `ollama pull codellama`
- Sé más específico en tu pregunta
- Menciona nombres exactos de tablas/columnas

---

## ?? Recomendación Final

**Para tu BD_SCU específicamente:**

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

**¡Disfruta consultando tu BD_SCU con lenguaje natural! ??**
