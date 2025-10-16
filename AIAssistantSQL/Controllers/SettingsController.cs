using AIAssistantSQL.Interfaces;
using AIAssistantSQL.Services;
using AIAssistantSQL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistantSQL.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IOllamaService _ollamaService;
        private readonly AIServiceFactory _aiServiceFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            IOllamaService ollamaService,
            AIServiceFactory aiServiceFactory,
            IServiceProvider serviceProvider,
            ILogger<SettingsController> logger)
        {
            _ollamaService = ollamaService;
            _aiServiceFactory = aiServiceFactory;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var models = await _ollamaService.GetAvailableModelsAsync();
            var currentModel = _ollamaService.GetCurrentModel();
            var providers = _aiServiceFactory.GetAvailableProviders();
            var currentProvider = _aiServiceFactory.GetCurrentProvider();

            ViewBag.AvailableModels = models;
            ViewBag.CurrentModel = currentModel;
            ViewBag.Providers = providers;
            ViewBag.CurrentProvider = currentProvider;

            return View();
        }

        /// <summary>
        /// Vista del selector de modelos avanzado
        /// </summary>
        public async Task<IActionResult> ModelSelector()
        {
            try
            {
                var models = await _ollamaService.GetAvailableModelsAsync();
                var currentModel = _ollamaService.GetCurrentModel();
                var isOllamaAvailable = await _ollamaService.IsAvailableAsync();

                var viewModel = new ModelSelectorViewModel
                {
                    CurrentModel = currentModel,
                    AvailableModels = models,
                    IsOllamaAvailable = isOllamaAvailable,
                    LastUpdated = DateTime.Now
                };

                ViewBag.IsOllamaAvailable = isOllamaAvailable;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando selector de modelos");
                TempData["Error"] = "Error cargando la lista de modelos";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Actualiza la lista de modelos disponibles
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RefreshModels()
        {
            try
            {
                // Limpiar caché para forzar actualización
                _ollamaService.ClearModelsCache();
                
                var models = await _ollamaService.GetAvailableModelsAsync();
                
                return Json(new
                {
                    success = true,
                    message = "Modelos actualizados correctamente",
                    count = models.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando modelos");
                return Json(new
                {
                    success = false,
                    message = "Error actualizando modelos: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Cambia el modelo activo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetModel(string modelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modelName))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Nombre de modelo requerido"
                    });
                }

                // Verificar que el modelo existe
                var models = await _ollamaService.GetAvailableModelsAsync();
                if (!models.Any(m => m.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Modelo no encontrado"
                    });
                }

                _ollamaService.SetModel(modelName);
                
                _logger.LogInformation($"Modelo cambiado a: {modelName}");

                return Json(new
                {
                    success = true,
                    message = $"Modelo cambiado a: {modelName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando modelo");
                return Json(new
                {
                    success = false,
                    message = "Error cambiando modelo: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Prueba un modelo específico
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TestModel(string modelName, string testQuery)
        {
            var startTime = DateTime.Now;
            
            try
            {
                if (string.IsNullOrWhiteSpace(modelName) || string.IsNullOrWhiteSpace(testQuery))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Modelo y consulta son requeridos"
                    });
                }

                // Guardar modelo actual
                var originalModel = _ollamaService.GetCurrentModel();
                
                // Cambiar temporalmente al modelo de prueba
                _ollamaService.SetModel(modelName);

                try
                {
                    // Crear un esquema de prueba simple
                    var testSchema = new AIAssistantSQL.Models.DatabaseSchema
                    {
                        DatabaseName = "TestDB",
                        DatabaseType = AIAssistantSQL.Models.DatabaseType.SqlServer,
                        Tables = new List<AIAssistantSQL.Models.TableSchema>
                        {
                            new AIAssistantSQL.Models.TableSchema
                            {
                                TableName = "Productos",
                                Columns = new List<AIAssistantSQL.Models.ColumnSchema>
                                {
                                    new AIAssistantSQL.Models.ColumnSchema { ColumnName = "Id", DataType = "int", IsNullable = false },
                                    new AIAssistantSQL.Models.ColumnSchema { ColumnName = "Nombre", DataType = "varchar(100)", IsNullable = false },
                                    new AIAssistantSQL.Models.ColumnSchema { ColumnName = "Precio", DataType = "decimal(10,2)", IsNullable = false }
                                },
                                PrimaryKeys = new List<string> { "Id" },
                                ForeignKeys = new List<AIAssistantSQL.Models.ForeignKeySchema>()
                            }
                        }
                    };

                    // Generar consulta
                    var result = await _ollamaService.GenerateSQLFromNaturalLanguageAsync(testQuery, testSchema);
                    
                    var executionTime = (DateTime.Now - startTime).TotalMilliseconds;

                    return Json(new
                    {
                        success = true,
                        result = result,
                        executionTime = Math.Round(executionTime, 0)
                    });
                }
                finally
                {
                    // Restaurar modelo original
                    _ollamaService.SetModel(originalModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error probando modelo {modelName}");
                
                var executionTime = (DateTime.Now - startTime).TotalMilliseconds;
                
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    executionTime = Math.Round(executionTime, 0)
                });
            }
        }

        /// <summary>
        /// Obtiene la lista de modelos de Ollama disponibles (endpoint JSON para AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOllamaModels()
        {
            try
            {
                var models = await _ollamaService.GetAvailableModelsAsync();
                var currentModel = _ollamaService.GetCurrentModel();

                return Json(new
                {
                    success = true,
                    models = models.Select(m => new
                    {
                        name = m.Name,
                        size = m.Size,
                        modifiedAt = m.ModifiedAt
                    }),
                    currentModel = currentModel,
                    count = models.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo modelos de Ollama");
                return Json(new
                {
                    success = false,
                    message = "Error al obtener modelos: " + ex.Message,
                    models = new object[] { },
                    currentModel = _ollamaService.GetCurrentModel()
                });
            }
        }

        /// <summary>
        /// Cambia el modelo activo (endpoint JSON para AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeModel([FromBody] ChangeModelRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.ModelName))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Nombre de modelo requerido"
                    });
                }

                // Verificar que el modelo existe
                var models = await _ollamaService.GetAvailableModelsAsync();
                if (!models.Any(m => m.Name.Equals(request.ModelName, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Modelo no encontrado en Ollama"
                    });
                }

                _ollamaService.SetModel(request.ModelName);
                
                _logger.LogInformation($"Usuario cambió el modelo a: {request.ModelName}");

                return Json(new
                {
                    success = true,
                    message = $"Modelo cambiado exitosamente a: {request.ModelName}",
                    modelName = request.ModelName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar modelo");
                return Json(new
                {
                    success = false,
                    message = "Error al cambiar modelo: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Clase para recibir la petición de cambio de modelo
        /// </summary>
        public class ChangeModelRequest
        {
            public string ModelName { get; set; } = string.Empty;
        }

        [HttpPost]
        public IActionResult ChangeModelFromView(string modelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modelName))
                {
                    TempData["Error"] = "Por favor selecciona un modelo válido";
                    return RedirectToAction(nameof(Index));
                }

                _ollamaService.SetModel(modelName);
                
                TempData["Success"] = $"✅ Modelo cambiado exitosamente a: <strong>{modelName}</strong><br/>Las próximas consultas usarán este modelo.";
                
                _logger.LogInformation($"Usuario cambió el modelo a: {modelName}");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar modelo");
                TempData["Error"] = $"Error al cambiar modelo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeProvider(string providerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(providerName))
                {
                    TempData["Error"] = "Por favor selecciona un proveedor v�lido";
                    return RedirectToAction(nameof(Index));
                }

                var currentProvider = _aiServiceFactory.GetCurrentProvider();

                _logger.LogInformation($"?? Intentando cambiar de '{currentProvider}' a '{providerName}'...");

                // ? VALIDAR CONEXI�N antes de cambiar
                IOllamaService newService;
                
                switch (providerName.ToLower())
                {
                    case "ollama":
                        newService = _serviceProvider.GetRequiredService<OllamaService>();
                        break;
                    case "googleai":
                    case "gemini":
                        newService = _serviceProvider.GetRequiredService<GoogleAIService>();
                        break;
                    case "deepseek":
                    case "deepseekai":
                        newService = _serviceProvider.GetRequiredService<DeepSeekAIService>();
                        break;
                    default:
                        TempData["Error"] = $"Proveedor no reconocido: {providerName}";
                        return RedirectToAction(nameof(Index));
                }

                // Probar conexi�n
                _logger.LogInformation($"?? Verificando disponibilidad de {providerName}...");
                bool isAvailable = await newService.IsAvailableAsync();

                if (!isAvailable)
                {
                    _logger.LogWarning($"? {providerName} no est� disponible");
                    
                    TempData["Error"] = GenerateErrorMessage(providerName);
                    
                    return RedirectToAction(nameof(Index));
                }

                // ? Conexi�n exitosa, cambiar proveedor
                _aiServiceFactory.SetProvider(providerName);
                
                TempData["Success"] = $"? Proveedor cambiado exitosamente a: <strong>{providerName}</strong><br/>" +
                                     $"Conexi�n verificada correctamente. Todas las consultas usar�n este proveedor de IA.";
                
                _logger.LogInformation($"? Usuario cambi� el proveedor a: {providerName}");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar proveedor");
                TempData["Error"] = $"Error al cambiar proveedor: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestProvider(string providerName)
        {
            try
            {
                _logger.LogInformation($"?? Probando conexi�n a {providerName}...");

                IOllamaService service;
                
                switch (providerName.ToLower())
                {
                    case "ollama":
                        service = _serviceProvider.GetRequiredService<OllamaService>();
                        break;
                    case "googleai":
                    case "gemini":
                        service = _serviceProvider.GetRequiredService<GoogleAIService>();
                        break;
                    case "deepseek":
                    case "deepseekai":
                        service = _serviceProvider.GetRequiredService<DeepSeekAIService>();
                        break;
                    default:
                        return Json(new
                        {
                            success = false,
                            message = $"Proveedor no reconocido: {providerName}"
                        });
                }

                bool isAvailable = await service.IsAvailableAsync();

                if (isAvailable)
                {
                    _logger.LogInformation($"? {providerName} disponible");
                    
                    return Json(new
                    {
                        success = true,
                        message = $"? Conexi�n exitosa con {providerName}",
                        details = GetSuccessDetails(providerName)
                    });
                }
                else
                {
                    _logger.LogWarning($"? {providerName} no disponible");
                    
                    return Json(new
                    {
                        success = false,
                        message = $"? No se pudo conectar con {providerName}",
                        details = GetErrorDetails(providerName)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error probando {providerName}");
                
                return Json(new
                {
                    success = false,
                    message = $"Error al probar conexi�n: {ex.Message}",
                    details = ex.ToString()
                });
            }
        }

        private string GenerateErrorMessage(string providerName)
        {
            return providerName.ToLower() switch
            {
                "ollama" => @"
                    <strong>? Ollama no est� disponible</strong><br/>
                    <br/>
                    <strong>Posibles causas:</strong><br/>
                    � Ollama no est� ejecut�ndose<br/>
                    � Puerto 11434 bloqueado<br/>
                    � Modelo no instalado<br/>
                    <br/>
                    <strong>Soluciones:</strong><br/>
                    1. Ejecutar: <code>ollama serve</code><br/>
                    2. Instalar modelo: <code>ollama pull codellama</code><br/>
                    3. Verificar: <code>curl http://localhost:11434/api/tags</code>",

                "googleai" or "gemini" => @"
                    <strong>? Google AI no est� disponible</strong><br/>
                    <br/>
                    <strong>Posibles causas:</strong><br/>
                    � API Key inv�lida o expirada<br/>
                    � Sin conexi�n a internet<br/>
                    � L�mites de uso excedidos<br/>
                    <br/>
                    <strong>Soluciones:</strong><br/>
                    1. Verificar API Key en <code>appsettings.json</code><br/>
                    2. Obtener nueva key en: <a href='https://aistudio.google.com/app/apikey' target='_blank'>Google AI Studio</a><br/>
                    3. Verificar conexi�n a internet",

                "deepseek" or "deepseekai" => @"
                    <strong>? DeepSeek AI no est� disponible</strong><br/>
                    <br/>
                    <strong>Posibles causas:</strong><br/>
                    � API Key inv�lida o expirada<br/>
                    � Sin conexi�n a internet<br/>
                    � Cr�ditos agotados<br/>
                    <br/>
                    <strong>Soluciones:</strong><br/>
                    1. Verificar API Key en <code>appsettings.json</code><br/>
                    2. Obtener nueva key en: <a href='https://platform.deepseek.com/api_keys' target='_blank'>DeepSeek Platform</a><br/>
                    3. Verificar saldo de cr�ditos",

                _ => $"<strong>? No se pudo conectar con {providerName}</strong>"
            };
        }

        private string GetSuccessDetails(string providerName)
        {
            return providerName.ToLower() switch
            {
                "ollama" => "Ollama est� funcionando correctamente en http://localhost:11434",
                "googleai" or "gemini" => "Conexi�n exitosa con Google AI (Gemini). API Key v�lida.",
                "deepseek" or "deepseekai" => "Conexi�n exitosa con DeepSeek AI. API Key v�lida y cr�ditos disponibles.",
                _ => "Conexi�n exitosa"
            };
        }

        private string GetErrorDetails(string providerName)
        {
            return providerName.ToLower() switch
            {
                "ollama" => "Ollama no responde en http://localhost:11434. Aseg�rate de que est� ejecut�ndose: ollama serve",
                "googleai" or "gemini" => "No se pudo conectar con Google AI. Verifica tu API Key y conexi�n a internet.",
                "deepseek" or "deepseekai" => "No se pudo conectar con DeepSeek AI. Verifica tu API Key y cr�ditos disponibles.",
                _ => "Error de conexi�n"
            };
        }
    }
}
