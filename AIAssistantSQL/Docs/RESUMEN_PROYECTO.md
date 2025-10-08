# ?? Resumen del Proyecto AI Assistant SQL

## ? Proyecto Completado Exitosamente

Se ha creado una aplicación web ASP.NET Core MVC completa para consultar bases de datos usando lenguaje natural con IA (Ollama).

---

## ?? Estructura del Proyecto

```
AIAssistantSQL/
??? Controllers/
?   ??? HomeController.cs          - Página principal e información
?   ??? DatabaseController.cs      - Gestión de esquemas de BD
?   ??? QueryController.cs         - Ejecución de consultas con IA
?
??? Models/
?   ??? DatabaseType.cs            - Enum de tipos de BD
?   ??? DatabaseSchema.cs          - Modelos de esquema (Tablas, Columnas, FKs)
?   ??? QueryModels.cs             - Request/Response de consultas
?   ??? ViewModels.cs              - ViewModels para las vistas
?
??? Views/
?   ??? Shared/
?   ?   ??? _Layout.cshtml         - Layout principal con Bootstrap
?   ??? Home/
?   ?   ??? Index.cshtml           - Dashboard principal
?   ?   ??? Privacy.cshtml         - Política de privacidad
?   ??? Database/
?   ?   ??? Index.cshtml           - Configuración de BD (3 opciones)
?   ?   ??? ViewSchema.cshtml      - Visualización de esquema cargado
?   ??? Query/
?       ??? Index.cshtml           - Interface de consultas con IA
?       ??? History.cshtml         - Historial de consultas
?
??? Services/
?   ??? OllamaService.cs           - Integración con Ollama para generar SQL
?   ??? SchemaLoaderService.cs     - Carga esquemas (archivo/BD)
?   ??? SqlValidatorService.cs     - Validación de seguridad (solo SELECT)
?
??? Repositories/
?   ??? QueryRepository.cs         - Ejecución de queries con Dapper
?
??? Interfaces/
?   ??? IOllamaService.cs
?   ??? ISchemaLoaderService.cs
?   ??? ISqlValidatorService.cs
?   ??? IQueryRepository.cs
?
??? wwwroot/
?   ??? uploads/
?       ??? ejemplo_schema.json    - Esquema de ejemplo (EjemploTienda)
?
??? Program.cs                      - Configuración de la aplicación
??? appsettings.json                - Configuración de Ollama
??? README.md                       - Documentación completa
??? INICIO_RAPIDO.md                - Guía de inicio rápido
??? .gitignore                      - Archivos a ignorar en Git
```

---

## ?? Características Implementadas

### 1. ? Integración con Ollama (IA Local)
- Comunicación con Ollama vía HTTP API
- Generación de SQL desde lenguaje natural
- Detección automática de disponibilidad
- Configuración flexible del modelo (llama2, codellama, etc.)

### 2. ? 3 Modos de Carga de Esquema
**Opción 1: Archivo JSON**
- Sube un archivo con el esquema previamente guardado
- Formato estructurado y reutilizable

**Opción 2: Conexión Directa**
- Conecta directamente a SQL Server o PostgreSQL
- Extrae automáticamente toda la estructura:
  - Tablas, columnas, tipos de datos
  - Claves primarias y foráneas
  - Restricciones y relaciones

**Opción 3: Guardar Esquema**
- Guarda el esquema actual en JSON
- Reutilizable en otros proyectos

### 3. ? Seguridad
- **Validación de consultas**: Solo permite SELECT
- **Bloqueo de operaciones peligrosas**: INSERT, UPDATE, DELETE, DROP, etc.
- **Detección de inyección SQL**: Patrones sospechosos
- **Sanitización de queries**: Limpieza automática

### 4. ? Interfaz de Usuario
- **Dashboard Principal**: Estado del sistema (Ollama, Esquema)
- **Configuración Intuitiva**: Interfaz clara para cargar esquemas
- **Vista de Consultas**: Input de lenguaje natural + resultados en tabla
- **Visualización de Esquema**: Accordion con todas las tablas y detalles
- **Historial**: Registro de consultas exitosas y fallidas
- **Diseño Responsive**: Bootstrap 5 + Bootstrap Icons

### 5. ? Soporte Multi-Base de Datos
- **SQL Server**: Usando Microsoft.Data.SqlClient
- **PostgreSQL**: Usando Npgsql
- Arquitectura extensible para agregar más motores

### 6. ? Arquitectura Limpia
- **Patrón Repository**: Separación de acceso a datos
- **Inyección de Dependencias**: Servicios desacoplados
- **Interfaces**: Contratos claros y testables
- **MVC**: Separación de responsabilidades
- **Logging**: ILogger en todos los servicios

---

## ?? Tecnologías Utilizadas

| Tecnología | Versión | Uso |
|-----------|---------|-----|
| .NET | 8.0 | Framework principal |
| ASP.NET Core MVC | 8.0 | Web framework |
| C# | 12.0 | Lenguaje |
| Ollama | Local | IA para generar SQL |
| Dapper | 2.1.35 | Micro ORM |
| Entity Framework Core | 9.0.9 | Introspección de esquemas |
| Microsoft.Data.SqlClient | 5.2.0 | SQL Server |
| Npgsql | 8.0.3 | PostgreSQL |
| OllamaSharp | 5.4.7 | Cliente Ollama |
| Bootstrap | 5.3.0 | UI Framework |
| jQuery | 3.7.0 | AJAX y DOM |

---

## ?? Flujo de Uso Completo

### Setup Inicial
1. Instalar Ollama: `ollama pull llama2`
2. Iniciar Ollama: `ollama serve`
3. Ejecutar aplicación: `dotnet run`
4. Abrir navegador: `https://localhost:5001`

### Primer Uso
1. **Ir a "Configuración BD"**
   - Opción A: Subir `ejemplo_schema.json` (incluido)
   - Opción B: Conectar con cadena de conexión real

2. **Verificar que Ollama esté online** (indicador verde en navbar)

3. **Ir a "Consultas"**
   - Escribir: "Muéstrame todos los productos"
   - La IA genera: `SELECT * FROM Productos`
   - Se ejecuta y muestra resultados en tabla

4. **Ver Historial** (mantiene registro de todas las consultas)

---

## ?? Ejemplos de Consultas Soportadas

Con el esquema de ejemplo (EjemploTienda):

```
? "Muéstrame los últimos 10 productos"
? "¿Cuántos clientes hay registrados?"
? "Lista las ventas ordenadas por fecha"
? "Productos con stock menor a 10"
? "Total de ventas por cliente"
? "Clientes que no han comprado nunca"
? "Producto más vendido del último mes"
? "Categorías con más de 5 productos"
```

---

## ?? Validaciones de Seguridad Implementadas

```csharp
? BLOQUEADAS: INSERT, UPDATE, DELETE, DROP, CREATE, ALTER, TRUNCATE
? BLOQUEADAS: EXEC, EXECUTE, sp_, xp_, MERGE
? BLOQUEADAS: GRANT, REVOKE, DENY, BACKUP, RESTORE
? PERMITIDAS: SELECT, WITH (para CTEs)
? VALIDACIÓN: Comentarios maliciosos
? VALIDACIÓN: Inyección SQL con UNION
```

---

## ?? Esquema de Ejemplo Incluido

**Base de Datos:** EjemploTienda  
**Tablas:**
- ? Productos (ProductoId, Nombre, Precio, CategoriaId, Stock)
- ? Categorias (CategoriaId, Nombre, Descripcion)
- ? Clientes (ClienteId, Nombre, Email, Telefono, FechaRegistro)
- ? Ventas (VentaId, ClienteId, FechaVenta, Total)
- ? DetalleVentas (DetalleId, VentaId, ProductoId, Cantidad, PrecioUnitario, Subtotal)

**Relaciones:**
- Productos ? Categorias
- Ventas ? Clientes
- DetalleVentas ? Ventas + Productos

---

## ?? Cómo Reutilizar en Otros Proyectos

### Método 1: Copiar Código
```
1. Copia las carpetas: Models, Services, Repositories, Interfaces
2. Instala los paquetes NuGet necesarios
3. Registra los servicios en Program.cs
4. ¡Listo!
```

### Método 2: Usar Esquemas Guardados
```
1. En proyecto original: carga BD y guarda esquema como JSON
2. En nuevo proyecto: sube ese JSON
3. Configurar cadena de conexión correspondiente
4. ¡A consultar!
```

---

## ?? Ventajas del Proyecto

? **Reutilizable**: Diseñado para usar en múltiples proyectos  
? **Seguro**: Solo consultas SELECT, validaciones estrictas  
? **Independiente**: IA local con Ollama (sin costos de API)  
? **Flexible**: Soporta SQL Server y PostgreSQL  
? **Intuitivo**: Interface clara y fácil de usar  
? **Portable**: Esquemas en JSON reutilizables  
? **Extensible**: Arquitectura limpia y desacoplada  
? **Documentado**: README completo + guía rápida  

---

## ?? Próximos Pasos Sugeridos (Opcionales)

### Mejoras Futuras
- [ ] Persistir historial en base de datos
- [ ] Autenticación y autorización de usuarios
- [ ] Soporte para MySQL y SQLite
- [ ] Export de resultados a CSV/Excel
- [ ] Guardar consultas favoritas
- [ ] Multi-idioma (español/inglés)
- [ ] Modo oscuro
- [ ] API REST independiente
- [ ] Paquete NuGet publicable

---

## ?? Notas Importantes

?? **Seguridad**: Solo usa credenciales de solo lectura  
?? **Ollama**: Debe estar corriendo en segundo plano  
?? **Performance**: Consultas complejas pueden tardar (depende del modelo de IA)  
?? **Historial**: Se pierde al reiniciar (está en memoria)  
?? **Sesión**: La cadena de conexión se guarda en sesión temporal  

---

## ? Build Exitoso

El proyecto compila sin errores y está listo para ejecutarse.

```bash
cd AIAssistantSQL
dotnet run
```

**¡Disfruta consultando bases de datos con lenguaje natural! ????**
