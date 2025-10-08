# ?? Resumen del Proyecto AI Assistant SQL

## ? Proyecto Completado Exitosamente

Se ha creado una aplicaci�n web ASP.NET Core MVC completa para consultar bases de datos usando lenguaje natural con IA (Ollama).

---

## ?? Estructura del Proyecto

```
AIAssistantSQL/
??? Controllers/
?   ??? HomeController.cs          - P�gina principal e informaci�n
?   ??? DatabaseController.cs      - Gesti�n de esquemas de BD
?   ??? QueryController.cs         - Ejecuci�n de consultas con IA
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
?   ?   ??? Privacy.cshtml         - Pol�tica de privacidad
?   ??? Database/
?   ?   ??? Index.cshtml           - Configuraci�n de BD (3 opciones)
?   ?   ??? ViewSchema.cshtml      - Visualizaci�n de esquema cargado
?   ??? Query/
?       ??? Index.cshtml           - Interface de consultas con IA
?       ??? History.cshtml         - Historial de consultas
?
??? Services/
?   ??? OllamaService.cs           - Integraci�n con Ollama para generar SQL
?   ??? SchemaLoaderService.cs     - Carga esquemas (archivo/BD)
?   ??? SqlValidatorService.cs     - Validaci�n de seguridad (solo SELECT)
?
??? Repositories/
?   ??? QueryRepository.cs         - Ejecuci�n de queries con Dapper
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
??? Program.cs                      - Configuraci�n de la aplicaci�n
??? appsettings.json                - Configuraci�n de Ollama
??? README.md                       - Documentaci�n completa
??? INICIO_RAPIDO.md                - Gu�a de inicio r�pido
??? .gitignore                      - Archivos a ignorar en Git
```

---

## ?? Caracter�sticas Implementadas

### 1. ? Integraci�n con Ollama (IA Local)
- Comunicaci�n con Ollama v�a HTTP API
- Generaci�n de SQL desde lenguaje natural
- Detecci�n autom�tica de disponibilidad
- Configuraci�n flexible del modelo (llama2, codellama, etc.)

### 2. ? 3 Modos de Carga de Esquema
**Opci�n 1: Archivo JSON**
- Sube un archivo con el esquema previamente guardado
- Formato estructurado y reutilizable

**Opci�n 2: Conexi�n Directa**
- Conecta directamente a SQL Server o PostgreSQL
- Extrae autom�ticamente toda la estructura:
  - Tablas, columnas, tipos de datos
  - Claves primarias y for�neas
  - Restricciones y relaciones

**Opci�n 3: Guardar Esquema**
- Guarda el esquema actual en JSON
- Reutilizable en otros proyectos

### 3. ? Seguridad
- **Validaci�n de consultas**: Solo permite SELECT
- **Bloqueo de operaciones peligrosas**: INSERT, UPDATE, DELETE, DROP, etc.
- **Detecci�n de inyecci�n SQL**: Patrones sospechosos
- **Sanitizaci�n de queries**: Limpieza autom�tica

### 4. ? Interfaz de Usuario
- **Dashboard Principal**: Estado del sistema (Ollama, Esquema)
- **Configuraci�n Intuitiva**: Interfaz clara para cargar esquemas
- **Vista de Consultas**: Input de lenguaje natural + resultados en tabla
- **Visualizaci�n de Esquema**: Accordion con todas las tablas y detalles
- **Historial**: Registro de consultas exitosas y fallidas
- **Dise�o Responsive**: Bootstrap 5 + Bootstrap Icons

### 5. ? Soporte Multi-Base de Datos
- **SQL Server**: Usando Microsoft.Data.SqlClient
- **PostgreSQL**: Usando Npgsql
- Arquitectura extensible para agregar m�s motores

### 6. ? Arquitectura Limpia
- **Patr�n Repository**: Separaci�n de acceso a datos
- **Inyecci�n de Dependencias**: Servicios desacoplados
- **Interfaces**: Contratos claros y testables
- **MVC**: Separaci�n de responsabilidades
- **Logging**: ILogger en todos los servicios

---

## ?? Tecnolog�as Utilizadas

| Tecnolog�a | Versi�n | Uso |
|-----------|---------|-----|
| .NET | 8.0 | Framework principal |
| ASP.NET Core MVC | 8.0 | Web framework |
| C# | 12.0 | Lenguaje |
| Ollama | Local | IA para generar SQL |
| Dapper | 2.1.35 | Micro ORM |
| Entity Framework Core | 9.0.9 | Introspecci�n de esquemas |
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
3. Ejecutar aplicaci�n: `dotnet run`
4. Abrir navegador: `https://localhost:5001`

### Primer Uso
1. **Ir a "Configuraci�n BD"**
   - Opci�n A: Subir `ejemplo_schema.json` (incluido)
   - Opci�n B: Conectar con cadena de conexi�n real

2. **Verificar que Ollama est� online** (indicador verde en navbar)

3. **Ir a "Consultas"**
   - Escribir: "Mu�strame todos los productos"
   - La IA genera: `SELECT * FROM Productos`
   - Se ejecuta y muestra resultados en tabla

4. **Ver Historial** (mantiene registro de todas las consultas)

---

## ?? Ejemplos de Consultas Soportadas

Con el esquema de ejemplo (EjemploTienda):

```
? "Mu�strame los �ltimos 10 productos"
? "�Cu�ntos clientes hay registrados?"
? "Lista las ventas ordenadas por fecha"
? "Productos con stock menor a 10"
? "Total de ventas por cliente"
? "Clientes que no han comprado nunca"
? "Producto m�s vendido del �ltimo mes"
? "Categor�as con m�s de 5 productos"
```

---

## ?? Validaciones de Seguridad Implementadas

```csharp
? BLOQUEADAS: INSERT, UPDATE, DELETE, DROP, CREATE, ALTER, TRUNCATE
? BLOQUEADAS: EXEC, EXECUTE, sp_, xp_, MERGE
? BLOQUEADAS: GRANT, REVOKE, DENY, BACKUP, RESTORE
? PERMITIDAS: SELECT, WITH (para CTEs)
? VALIDACI�N: Comentarios maliciosos
? VALIDACI�N: Inyecci�n SQL con UNION
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

## ?? C�mo Reutilizar en Otros Proyectos

### M�todo 1: Copiar C�digo
```
1. Copia las carpetas: Models, Services, Repositories, Interfaces
2. Instala los paquetes NuGet necesarios
3. Registra los servicios en Program.cs
4. �Listo!
```

### M�todo 2: Usar Esquemas Guardados
```
1. En proyecto original: carga BD y guarda esquema como JSON
2. En nuevo proyecto: sube ese JSON
3. Configurar cadena de conexi�n correspondiente
4. �A consultar!
```

---

## ?? Ventajas del Proyecto

? **Reutilizable**: Dise�ado para usar en m�ltiples proyectos  
? **Seguro**: Solo consultas SELECT, validaciones estrictas  
? **Independiente**: IA local con Ollama (sin costos de API)  
? **Flexible**: Soporta SQL Server y PostgreSQL  
? **Intuitivo**: Interface clara y f�cil de usar  
? **Portable**: Esquemas en JSON reutilizables  
? **Extensible**: Arquitectura limpia y desacoplada  
? **Documentado**: README completo + gu�a r�pida  

---

## ?? Pr�ximos Pasos Sugeridos (Opcionales)

### Mejoras Futuras
- [ ] Persistir historial en base de datos
- [ ] Autenticaci�n y autorizaci�n de usuarios
- [ ] Soporte para MySQL y SQLite
- [ ] Export de resultados a CSV/Excel
- [ ] Guardar consultas favoritas
- [ ] Multi-idioma (espa�ol/ingl�s)
- [ ] Modo oscuro
- [ ] API REST independiente
- [ ] Paquete NuGet publicable

---

## ?? Notas Importantes

?? **Seguridad**: Solo usa credenciales de solo lectura  
?? **Ollama**: Debe estar corriendo en segundo plano  
?? **Performance**: Consultas complejas pueden tardar (depende del modelo de IA)  
?? **Historial**: Se pierde al reiniciar (est� en memoria)  
?? **Sesi�n**: La cadena de conexi�n se guarda en sesi�n temporal  

---

## ? Build Exitoso

El proyecto compila sin errores y est� listo para ejecutarse.

```bash
cd AIAssistantSQL
dotnet run
```

**�Disfruta consultando bases de datos con lenguaje natural! ????**
