using AIAssistantSQL.Interfaces;
using AIAssistantSQL.Models;
using AIAssistantSQL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistantSQL.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly ISchemaLoaderService _schemaLoaderService;
        private readonly IQueryRepository _queryRepository;
        private readonly ILogger<DatabaseController> _logger;
        private readonly IConfiguration _configuration;

        // Guardar la configuraci�n de conexi�n en memoria (en producci�n usar BD o cach� distribuido)
        private static DatabaseConnectionConfig? _currentConnection;

        public DatabaseController(
            ISchemaLoaderService schemaLoaderService,
            IQueryRepository queryRepository,
            ILogger<DatabaseController> logger,
            IConfiguration configuration)
        {
            _schemaLoaderService = schemaLoaderService;
            _queryRepository = queryRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var currentSchema = _schemaLoaderService.GetCurrentSchema();

            var viewModel = new DatabaseConfigViewModel
            {
                IsSchemaLoaded = currentSchema != null,
                LoadedDatabaseName = currentSchema?.DatabaseName,
                LoadedAt = currentSchema?.LoadedAt,
                TableCount = currentSchema?.Tables.Count ?? 0,
                DatabaseType = currentSchema?.DatabaseType ?? DatabaseType.SqlServer,
                IsConnected = _currentConnection != null,
                CurrentConnectionString = _currentConnection?.ConnectionString
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadFromFile(IFormFile schemaFile)
        {
            try
            {
                if (schemaFile == null || schemaFile.Length == 0)
                {
                    TempData["Error"] = "Por favor seleccione un archivo v�lido";
                    return RedirectToAction(nameof(Index));
                }

                // Guardar archivo temporalmente
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, schemaFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await schemaFile.CopyToAsync(stream);
                }

                // Cargar esquema
                var schema = await _schemaLoaderService.LoadSchemaFromFileAsync(filePath);

                TempData["Success"] = $"Esquema cargado exitosamente: {schema.DatabaseName} - {schema.Tables.Count} tablas";
                TempData["Warning"] = "Recuerda configurar la cadena de conexi�n para poder ejecutar consultas";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar esquema desde archivo");
                TempData["Error"] = $"Error al cargar esquema: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConnectAndLoadSchema(string connectionString, DatabaseType databaseType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    TempData["Error"] = "Por favor ingrese una cadena de conexi�n v�lida";
                    return RedirectToAction(nameof(Index));
                }

                // CORRECCI�N: Limpiar dobles barras invertidas (de \\ a \)
                // El string literal en C# necesita escape, entonces:
                // - Buscamos: @"\\" (que es \\)
                // - Reemplazamos con: @"\" (que es \)
                connectionString = connectionString.Replace(@"\\", @"\");
                
                _logger.LogInformation($"Intentando conectar a {databaseType}...");
                _logger.LogInformation($"Cadena de conexi�n original recibida");
                _logger.LogInformation($"Cadena de conexi�n limpia: {connectionString}");

                // Probar conexi�n primero
                var isConnected = await _queryRepository.TestConnectionAsync(connectionString, databaseType);
                if (!isConnected)
                {
                    TempData["Error"] = $"No se pudo conectar a la base de datos. Verifique la cadena de conexi�n.<br/>Cadena utilizada: {connectionString}";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Conexi�n exitosa, extrayendo esquema...");

                // Cargar esquema
                var schema = await _schemaLoaderService.LoadSchemaFromConnectionStringAsync(connectionString, databaseType);

                // Guardar la configuraci�n de conexi�n
                _currentConnection = new DatabaseConnectionConfig
                {
                    ConnectionString = connectionString,
                    DatabaseType = databaseType,
                    DatabaseName = schema.DatabaseName
                };

                // Tambi�n guardar en sesi�n como fallback
                HttpContext.Session.SetString("ConnectionString", connectionString);
                HttpContext.Session.SetString("DatabaseType", databaseType.ToString());
                HttpContext.Session.SetString("DatabaseName", schema.DatabaseName);

                _logger.LogInformation($"Esquema cargado: {schema.Tables.Count} tablas encontradas");

                TempData["Success"] = $"? Conectado exitosamente a: {schema.DatabaseName}<br/>?? {schema.Tables.Count} tablas cargadas<br/>?? Ya puedes hacer consultas!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar y cargar esquema");
                TempData["Error"] = $"? Error: {ex.Message}<br/>Cadena de conexi�n: {connectionString}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult SetConnectionString(string connectionString, DatabaseType databaseType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    TempData["Error"] = "Por favor ingrese una cadena de conexi�n v�lida";
                    return RedirectToAction(nameof(Index));
                }

                // Limpiar dobles barras invertidas correctamente
                connectionString = connectionString.Replace(@"\\", @"\");
                
                _logger.LogInformation($"Configurando cadena de conexi�n limpia: {connectionString}");

                var currentSchema = _schemaLoaderService.GetCurrentSchema();
                if (currentSchema == null)
                {
                    TempData["Error"] = "Primero debe cargar el esquema de la base de datos";
                    return RedirectToAction(nameof(Index));
                }

                // Guardar la configuraci�n de conexi�n
                _currentConnection = new DatabaseConnectionConfig
                {
                    ConnectionString = connectionString,
                    DatabaseType = databaseType,
                    DatabaseName = currentSchema.DatabaseName
                };

                HttpContext.Session.SetString("ConnectionString", connectionString);
                HttpContext.Session.SetString("DatabaseType", databaseType.ToString());

                TempData["Success"] = "Cadena de conexi�n configurada correctamente. Ya puedes ejecutar consultas.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar cadena de conexi�n");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSchemaToFile(string fileName)
        {
            try
            {
                var currentSchema = _schemaLoaderService.GetCurrentSchema();
                if (currentSchema == null)
                {
                    TempData["Error"] = "No hay esquema cargado para guardar";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"{currentSchema.DatabaseName}_schema.json";
                }

                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".json";
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);
                await _schemaLoaderService.SaveSchemaToFileAsync(currentSchema, filePath);

                TempData["Success"] = $"Esquema guardado exitosamente: {fileName}<br/>Ubicaci�n: wwwroot/uploads/{fileName}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar esquema");
                TempData["Error"] = $"Error al guardar esquema: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult Disconnect()
        {
            _currentConnection = null;
            HttpContext.Session.Remove("ConnectionString");
            HttpContext.Session.Remove("DatabaseType");
            HttpContext.Session.Remove("DatabaseName");

            TempData["Success"] = "Desconectado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewSchema()
        {
            var currentSchema = _schemaLoaderService.GetCurrentSchema();
            if (currentSchema == null)
            {
                TempData["Error"] = "No hay esquema cargado";
                return RedirectToAction(nameof(Index));
            }

            return View(currentSchema);
        }

        public static DatabaseConnectionConfig? GetCurrentConnection()
        {
            return _currentConnection;
        }
    }
}
