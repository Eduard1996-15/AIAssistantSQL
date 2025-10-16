using AIAssistantSQL.Interfaces;
using AIAssistantSQL.Models;
using AIAssistantSQL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AIAssistantSQL.Controllers
{
    public class QueryController : Controller
    {
        private readonly IOllamaService _ollamaService;
        private readonly ISchemaLoaderService _schemaLoaderService;
        private readonly ISqlValidatorService _sqlValidatorService;
        private readonly IQueryRepository _queryRepository;
        private readonly ILogger<QueryController> _logger;

        // En memoria para demo - en producci�n usar base de datos
        private static List<QueryHistory> _queryHistory = new();
        
        // NUEVO: Manejo de conversaciones
        private static Dictionary<Guid, Conversation> _conversations = new();
        private static Guid _currentConversationId = Guid.Empty;

        public QueryController(
            IOllamaService ollamaService,
            ISchemaLoaderService schemaLoaderService,
            ISqlValidatorService sqlValidatorService,
            IQueryRepository queryRepository,
            ILogger<QueryController> logger)
        {
            _ollamaService = ollamaService;
            _schemaLoaderService = schemaLoaderService;
            _sqlValidatorService = sqlValidatorService;
            _queryRepository = queryRepository;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var currentConnection = DatabaseController.GetCurrentConnection();
            var currentSchema = _schemaLoaderService.GetCurrentSchema();

            // Crear nueva conversaci�n si no existe una activa
            if (_currentConversationId == Guid.Empty)
            {
                _currentConversationId = Guid.NewGuid();
                _conversations[_currentConversationId] = new Conversation
                {
                    Id = _currentConversationId,
                    StartedAt = DateTime.Now,
                    LastMessageAt = DateTime.Now,
                    DatabaseName = currentSchema?.DatabaseName ?? "Unknown"
                };
            }

            var viewModel = new QueryViewModel
            {
                History = _queryHistory.OrderByDescending(q => q.Timestamp).Take(10).ToList(),
                HasActiveConnection = currentConnection != null,
                DatabaseName = currentSchema?.DatabaseName,
                DatabaseType = currentSchema?.DatabaseType.ToString(),
                TableNames = currentSchema?.Tables.Select(t => t.TableName).ToList() ?? new List<string>()
            };

            if (currentConnection == null || currentSchema == null)
            {
                TempData["Warning"] = "?? Primero debes configurar la conexi�n a tu base de datos. <a href='/Database'>Ir a Configuraci�n</a>";
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Execute(string naturalLanguageQuery)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var aiStopwatch = new Stopwatch();
            var dbStopwatch = new Stopwatch();
            var response = new QueryResponse();

            try
            {
                // Validar que hay esquema cargado
                var currentSchema = _schemaLoaderService.GetCurrentSchema();
                if (currentSchema == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "No hay esquema de base de datos cargado. Por favor cargue un esquema primero en la secci�n de Configuraci�n.";
                    return Json(response);
                }

                _logger.LogInformation($"?? Esquema cargado: {currentSchema.DatabaseName} con {currentSchema.Tables.Count} tablas");
                
                // Log de tablas disponibles
                var tableNames = string.Join(", ", currentSchema.Tables.Select(t => t.TableName));
                _logger.LogInformation($"?? Tablas disponibles: {tableNames}");

                // Validar que hay conexi�n configurada
                var currentConnection = DatabaseController.GetCurrentConnection();
                string? connStr = currentConnection?.ConnectionString;
                
                if (currentConnection == null)
                {
                    connStr = HttpContext.Session.GetString("ConnectionString");
                    if (string.IsNullOrWhiteSpace(connStr))
                    {
                        response.Success = false;
                        response.ErrorMessage = "No hay conexi�n configurada. Por favor configure la cadena de conexi�n en la secci�n de Configuraci�n.";
                        return Json(response);
                    }
                }

                // ? NUEVO: Obtener tipo de BD temprano para pasarlo a la IA
                var databaseType = currentConnection?.DatabaseType ?? currentSchema.DatabaseType;

                // Validar que Ollama est� disponible
                var isOllamaAvailable = await _ollamaService.IsAvailableAsync();
                if (!isOllamaAvailable)
                {
                    response.Success = false;
                    response.ErrorMessage = "Ollama no est� disponible. Verifique que est� ejecut�ndose en http://localhost:11434";
                    return Json(response);
                }

                // ? MEJORADO: Generar SQL con contexto del tipo de BD
                _logger.LogInformation($"?? Procesando consulta: '{naturalLanguageQuery}' para {databaseType}");
                _logger.LogInformation($"?? Enviando esquema completo a la IA ({currentSchema.Tables.Count} tablas)");
                
                // Modificar el schema para incluir info del tipo de BD en el DatabaseName
                var schemaWithContext = new DatabaseSchema
                {
                    DatabaseName = $"{currentSchema.DatabaseName} ({databaseType})",
                    DatabaseType = databaseType,
                    Tables = currentSchema.Tables,
                    LoadedAt = currentSchema.LoadedAt
                };
                
                // PASO 1: Análisis contextual de la consulta
                _logger.LogInformation($"🔍 Analizando contexto para: {naturalLanguageQuery}");
                
                // Crear un prompt de análisis que ayude a la IA a entender mejor
                var contextualPrompt = $@"USER QUESTION: {naturalLanguageQuery}

CONTEXT ANALYSIS: Before generating SQL, analyze what the user is asking for:
- What type of data do they want?
- Which tables might contain this information?
- What relationships between tables might be needed?

AVAILABLE DATABASE SCHEMA:
{string.Join("\n", schemaWithContext.Tables.Select(t => 
    $"Table: {t.TableName}\n" +
    $"  Columns: {string.Join(", ", t.Columns.Select(c => $"{c.ColumnName} ({c.DataType})"))}\n" +
    (t.ForeignKeys.Any() ? $"  Relations: {string.Join(", ", t.ForeignKeys.Select(fk => $"{fk.ColumnName} -> {fk.ReferencedTable}.{fk.ReferencedColumn}"))}" : "  Relations: None") + "\n"
))}

Based on this schema and the user question, generate the appropriate SQL query.
Focus on using exact table and column names from the schema above.";

                // Generar SQL con IA usando el contexto completo del esquema
                _logger.LogInformation($"🤖 Generando SQL con contexto completo del esquema...");
                aiStopwatch.Start();
                var generatedSql = await _ollamaService.GenerateSQLFromNaturalLanguageAsync(contextualPrompt, schemaWithContext);
                aiStopwatch.Stop();

                _logger.LogInformation($"✅ SQL generado por IA: {generatedSql}");
                response.GeneratedSQL = generatedSql;

                // Validar que el SQL sea seguro (solo SELECT)
                if (!_sqlValidatorService.IsValidSelectQuery(generatedSql))
                {
                    _logger.LogWarning($"?? SQL no v�lido o no es SELECT");
                    response.Success = false;
                    response.ErrorMessage = "La consulta generada no es v�lida o no es una consulta SELECT segura.";
                    response.NaturalLanguageResponse = "? No pude generar una consulta SQL v�lida para tu pregunta. Por favor intenta reformularla de manera m�s simple.";
                    
                    AddToHistory(naturalLanguageQuery, generatedSql, response.NaturalLanguageResponse, false);

                    return Json(response);
                }

                // NUEVO: Validar que las tablas y columnas existen en el esquema
                _logger.LogInformation($"?? Validando SQL contra el esquema...");
                var queryValidation = ValidateQueryAgainstSchema(generatedSql, currentSchema);
                
                if (!queryValidation.IsValid)
                {
                    _logger.LogError($"? VALIDACI�N FALLIDA: {queryValidation.Reason}");
                    _logger.LogError($"? SQL problem�tico: {generatedSql}");
                    
                    // Reintentar con un prompt m�s espec�fico que incluya el error
                    _logger.LogInformation("?? Reintentando generaci�n de SQL con feedback del error...");
                    var retryPrompt = $@"PREVIOUS ATTEMPT WAS INCORRECT!

ERROR: {queryValidation.Reason}

Your previous SQL was: {generatedSql}

USER'S ORIGINAL QUESTION: {naturalLanguageQuery}

IMPORTANT: Carefully review the schema and use ONLY columns that actually exist.
Generate a corrected SQL query now:";

                    generatedSql = await _ollamaService.GenerateSQLFromNaturalLanguageAsync(retryPrompt, currentSchema);
                    _logger.LogInformation($"? SQL corregido (reintento): {generatedSql}");
                    
                    response.GeneratedSQL = generatedSql;
                    
                    // Validar de nuevo
                    if (!_sqlValidatorService.IsValidSelectQuery(generatedSql))
                    {
                        _logger.LogError($"? Reintento tambi�n fall� en validaci�n SELECT");
                        response.Success = false;
                        response.ErrorMessage = "No se pudo generar una consulta SQL v�lida despu�s de varios intentos.";
                        response.NaturalLanguageResponse = $"? Tuve problemas generando la consulta SQL correcta.\n\n**Error detectado:** {queryValidation.Reason}\n\nIntenta ser m�s espec�fico o revisar en 'Diagn�stico' qu� columnas est�n disponibles.";
                        
                        AddToHistory(naturalLanguageQuery, generatedSql, response.NaturalLanguageResponse, false);
                        return Json(response);
                    }

                    // Validar segunda vez contra el esquema
                    var secondValidation = ValidateQueryAgainstSchema(generatedSql, currentSchema);
                    if (!secondValidation.IsValid)
                    {
                        _logger.LogError($"? Reintento tambi�n fall� en validaci�n de esquema: {secondValidation.Reason}");
                        response.Success = false;
                        response.ErrorMessage = secondValidation.Reason;
                        response.NaturalLanguageResponse = $"? No pude generar una consulta v�lida.\n\n**Error:** {secondValidation.Reason}\n\n**Sugerencia:** Ve a la secci�n 'Diagn�stico' para ver las columnas exactas disponibles en cada tabla.";
                        
                        AddToHistory(naturalLanguageQuery, generatedSql, response.NaturalLanguageResponse, false);
                        return Json(response);
                    }
                }

                _logger.LogInformation($"? SQL validado correctamente");

                // Limpiar SQL
                var cleanedSql = _sqlValidatorService.CleanSqlQuery(generatedSql);

                // ? Si es PostgreSQL, agregar comillas a nombres de tabla
                if (databaseType == DatabaseType.PostgreSQL)
                {
                    cleanedSql = AddQuotesToPostgreSQLTables(cleanedSql, currentSchema);
                    _logger.LogInformation($"?? SQL ajustado para PostgreSQL: {cleanedSql}");
                }

                // Obtener configuraci�n de conexi�n
                var connectionString = connStr ?? HttpContext.Session.GetString("ConnectionString");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    response.Success = false;
                    response.ErrorMessage = "No hay cadena de conexi�n configurada. Por favor configure la conexi�n primero.";
                    return Json(response);
                }

                // Ejecutar consulta
                _logger.LogInformation($"Ejecutando SQL: {cleanedSql}");
                
                List<Dictionary<string, object>> results = new();
                try
                {
                    // Medir tiempo de ejecución en BD
                    dbStopwatch.Start();
                    results = (await _queryRepository.ExecuteQueryAsync(cleanedSql, connectionString, databaseType))
                        .Select(dict => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? (object)""))
                        .ToList();
                    dbStopwatch.Stop();
                }
                catch (Exception sqlEx)
                {
                    // 🔄 SISTEMA DE AUTO-CORRECCIÓN INTELIGENTE
                    _logger.LogError($"❌ Error ejecutando SQL: {sqlEx.Message}");
                    
                    // Intentar hasta 3 correcciones
                    const int maxRetries = 3;
                    List<string> attemptedQueries = new() { cleanedSql };
                    List<string> errorMessages = new() { sqlEx.Message };
                    
                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                    {
                        _logger.LogInformation($"🔄 Intento de corrección {attempt}/{maxRetries}...");
                        
                        // Analizar el error para detectar columnas/tablas inválidas
                        var errorAnalysis = AnalyzeErrorForColumnFix(errorMessages.Last(), currentSchema);
                        
                        // Crear prompt de corrección cada vez más específico con esquema completo
                        var errorFeedback = $@"WARNING: URGENT CORRECTION NEEDED - ATTEMPT {attempt}/{maxRetries}

ORIGINAL USER QUESTION: {naturalLanguageQuery}

PREVIOUS FAILED ATTEMPTS:
{string.Join("\n", attemptedQueries.Select((q, i) => $"Attempt {i + 1}: {q}\nError: {errorMessages[i]}\n"))}

COMPLETE DATABASE SCHEMA (USE EXACT NAMES - CASE SENSITIVE):
{string.Join("\n", currentSchema.Tables.Select(t => 
    $"Table: {t.TableName}\n" +
    $"  Columns (COPY EXACTLY): {string.Join(", ", t.Columns.Select(c => $"{c.ColumnName} ({c.DataType})"))}\n" +
    (t.PrimaryKeys.Any() ? $"  Primary Keys: {string.Join(", ", t.PrimaryKeys)}\n" : "") +
    (t.ForeignKeys.Any() ? $"  Foreign Keys: {string.Join(", ", t.ForeignKeys.Select(fk => $"{fk.ColumnName} -> {fk.ReferencedTable}.{fk.ReferencedColumn}"))}\n" : "")
))}

ERROR ANALYSIS:
- Last error: {errorMessages.Last()}
- Type: {(errorMessages.Last().Contains("Invalid column name") || errorMessages.Last().Contains("no existe") ? "INVALID COLUMN NAME - Column does not exist in schema" : errorMessages.Last().Contains("Invalid object name") ? "INVALID TABLE NAME - Table does not exist" : "SQL syntax or logic error")}
{errorAnalysis}

CRITICAL CORRECTION RULES:
1. COPY column names EXACTLY from schema above (case-sensitive: cedula NOT Cedula)
2. If error says Invalid column name, find similar column in schema and use EXACT name
3. Use ONLY tables and columns that exist in schema
4. Verify spelling and case of ALL column names
5. Make sure query is different from previous failed attempts
6. Answer the original question: {naturalLanguageQuery}

Generate corrected SQL query (ONLY the query, no explanations):";

                        try 
                        {
                            var correctedSql = await _ollamaService.GenerateSQLFromNaturalLanguageAsync(errorFeedback, currentSchema);
                            _logger.LogInformation($"🛠️ SQL corregido (intento {attempt}): {correctedSql}");
                            
                            // Validar el nuevo SQL
                            if (!_sqlValidatorService.IsValidSelectQuery(correctedSql))
                            {
                                _logger.LogWarning($"⚠️ SQL corregido no es válido en intento {attempt}");
                                continue;
                            }
                            
                            // Limpiar y ejecutar el SQL corregido
                            var cleanedCorrectedSql = _sqlValidatorService.CleanSqlQuery(correctedSql);
                            
                            // Agregar comillas si es PostgreSQL
                            if (databaseType == DatabaseType.PostgreSQL)
                            {
                                cleanedCorrectedSql = AddQuotesToPostgreSQLTables(cleanedCorrectedSql, currentSchema);
                                _logger.LogInformation($"🐘 SQL ajustado para PostgreSQL: {cleanedCorrectedSql}");
                            }
                            
                            try
                            {
                                _logger.LogInformation($"🚀 Ejecutando SQL corregido (intento {attempt})...");
                                results = (await _queryRepository.ExecuteQueryAsync(cleanedCorrectedSql, connectionString, databaseType))
                                    .Select(dict => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? (object)""))
                                    .ToList();
                                
                                // ✅ ¡ÉXITO! Actualizar información de respuesta
                                cleanedSql = cleanedCorrectedSql;
                                response.GeneratedSQL = correctedSql;
                                _logger.LogInformation($"✅ SQL corregido ejecutado exitosamente en intento {attempt}");
                                break; // Salir del loop de reintentos
                            }
                            catch (Exception retryEx)
                            {
                                _logger.LogError($"❌ Intento {attempt} falló: {retryEx.Message}");
                                attemptedQueries.Add(cleanedCorrectedSql);
                                errorMessages.Add(retryEx.Message);
                                
                                // Si es el último intento, devolver error final
                                if (attempt == maxRetries)
                                {
                                    response.Success = false;
                                    response.ErrorMessage = $"Error después de {maxRetries} intentos de corrección";
                                    response.NaturalLanguageResponse = $"😞 **No pude resolver tu consulta después de {maxRetries} intentos de corrección.**\n\n" +
                                        $"**Error original:** {sqlEx.Message}\n\n" +
                                        $"**Intentos realizados:**\n" +
                                        string.Join("\n", attemptedQueries.Select((q, i) => $"{i + 1}. `{q.Substring(0, Math.Min(50, q.Length))}...`")) +
                                        $"\n\n💡 **Sugerencias:**\n" +
                                        $"- Intenta ser más específico con los nombres de tablas\n" +
                                        $"- Verifica que los datos que buscas existan\n" +
                                        $"- Prueba con una consulta más simple";
                                    
                                    AddToHistory(naturalLanguageQuery, string.Join(" | ", attemptedQueries), response.NaturalLanguageResponse, false);
                                    return Json(response);
                                }
                            }
                        }
                        catch (Exception aiEx)
                        {
                            _logger.LogError($"❌ Error en IA durante corrección {attempt}: {aiEx.Message}");
                            
                            if (attempt == maxRetries)
                            {
                                response.Success = false;
                                response.ErrorMessage = $"Error en sistema de corrección: {aiEx.Message}";
                                response.NaturalLanguageResponse = $"😞 Hubo un problema con el sistema de auto-corrección.\n\n**Error:** {aiEx.Message}";
                                AddToHistory(naturalLanguageQuery, cleanedSql, response.NaturalLanguageResponse, false);
                                return Json(response);
                            }
                        }
                    }
                    
                    // Si llegamos aquí y results es null, asignar lista vacía para evitar errores
                    results ??= new List<Dictionary<string, object>>();
                }

                // NUEVO: Obtener contexto de conversación SOLO para interpretación
                // NO para generación de SQL (evita confusión)
                var conversationHistory = GetConversationHistory();

                // NUEVO: Interpretar resultados con IA (con timeout y fallback)
                string naturalLanguageResponse;
                try
                {
                    _logger.LogInformation($"💬 Interpretando resultados con IA...");
                    naturalLanguageResponse = await _ollamaService.InterpretQueryResultsAsync(
                        naturalLanguageQuery,
                        cleanedSql,
                        results,
                        conversationHistory
                    );
                }
                catch (Exception interpretEx)
                {
                    // Si la interpretación falla (timeout), usar respuesta simple
                    _logger.LogWarning(interpretEx, "⚠️ Interpretación IA falló, usando respuesta simple");
                    naturalLanguageResponse = GenerateSimpleResponse(naturalLanguageQuery, results.Count, cleanedSql);
                }

                response.Success = true;
                response.Results = results;
                response.RowCount = results.Count;
                response.ExecutionTime = totalStopwatch.Elapsed;
                response.AIResponseTime = aiStopwatch.Elapsed;
                response.DatabaseResponseTime = dbStopwatch.Elapsed;
                response.TotalResponseTime = totalStopwatch.Elapsed;
                response.NaturalLanguageResponse = naturalLanguageResponse;

                // Agregar al historial con respuesta de IA
                AddToHistory(naturalLanguageQuery, cleanedSql, naturalLanguageResponse, true);

                // Agregar a la conversaci�n actual
                AddToConversation(naturalLanguageQuery, cleanedSql, naturalLanguageResponse, true);

                _logger.LogInformation($"? Consulta exitosa: {results.Count} filas retornadas");

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar consulta");
                
                totalStopwatch.Stop();

                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ExecutionTime = totalStopwatch.Elapsed;
                response.AIResponseTime = aiStopwatch.Elapsed;
                response.DatabaseResponseTime = dbStopwatch.Elapsed;
                response.TotalResponseTime = totalStopwatch.Elapsed;
                response.NaturalLanguageResponse = $"? Ocurri� un error al procesar tu consulta: {ex.Message}";

                AddToHistory(naturalLanguageQuery, response.GeneratedSQL ?? "", response.NaturalLanguageResponse, false);
                AddToConversation(naturalLanguageQuery, response.GeneratedSQL ?? "", response.NaturalLanguageResponse, false);

                return Json(response);
            }
        }

        [HttpPost]
        public IActionResult NewConversation()
        {
            _currentConversationId = Guid.NewGuid();
            var currentSchema = _schemaLoaderService.GetCurrentSchema();
            
            _conversations[_currentConversationId] = new Conversation
            {
                Id = _currentConversationId,
                StartedAt = DateTime.Now,
                LastMessageAt = DateTime.Now,
                DatabaseName = currentSchema?.DatabaseName ?? "Unknown"
            };

            TempData["Success"] = "? Nueva conversaci�n iniciada";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult History()
        {
            return View(_queryHistory.OrderByDescending(q => q.Timestamp).ToList());
        }

        public IActionResult Conversations()
        {
            var conversations = _conversations.Values
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return View(conversations);
        }

        [HttpPost]
        public IActionResult ClearHistory()
        {
            _queryHistory.Clear();
            TempData["Success"] = "Historial limpiado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ClearConversations()
        {
            _conversations.Clear();
            _currentConversationId = Guid.Empty;
            TempData["Success"] = "Todas las conversaciones eliminadas";
            return RedirectToAction(nameof(Index));
        }

        private void AddToHistory(string question, string sql, string aiResponse, bool success)
        {
            _queryHistory.Add(new QueryHistory
            {
                Timestamp = DateTime.Now,
                NaturalLanguageQuery = question,
                GeneratedSQL = sql,
                NaturalLanguageResponse = aiResponse,
                Success = success,
                ConversationId = _currentConversationId
            });
        }

        private void AddToConversation(string question, string sql, string aiResponse, bool success)
        {
            if (_conversations.TryGetValue(_currentConversationId, out var conversation))
            {
                conversation.Messages.Add(new ConversationMessage
                {
                    Timestamp = DateTime.Now,
                    UserQuestion = question,
                    GeneratedSQL = sql,
                    AIResponse = aiResponse,
                    Success = success
                });

                conversation.LastMessageAt = DateTime.Now;
            }
        }

        private List<string> GetConversationHistory()
        {
            if (_conversations.TryGetValue(_currentConversationId, out var conversation))
            {
                return conversation.Messages
                    .OrderBy(m => m.Timestamp)
                    .Select(m => $"Usuario: {m.UserQuestion}\nIA: {m.AIResponse}")
                    .ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Valida que la consulta SQL generada tenga sentido con el esquema de la base de datos
        /// </summary>
        private (bool IsValid, string Reason) ValidateQueryAgainstSchema(string sql, DatabaseSchema schema)
        {
            var sqlUpper = sql.ToUpper();
            
            // Extraer nombres de tablas del SQL (despu�s de FROM y JOIN)
            var tableNames = new List<string>();
            var words = sql.Split(new[] { ' ', '\n', '\t', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i].Equals("FROM", StringComparison.OrdinalIgnoreCase) ||
                    words[i].Equals("JOIN", StringComparison.OrdinalIgnoreCase))
                {
                    var tableName = words[i + 1].Trim();
                    // Remover alias (AS alias)
                    if (tableName.Contains("AS", StringComparison.OrdinalIgnoreCase))
                    {
                        tableName = tableName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    }
                    tableName = tableName.Replace(";", "").Trim();
                    tableNames.Add(tableName);
                }
            }

            // Verificar que todas las tablas mencionadas existen en el esquema
            foreach (var tableName in tableNames)
            {
                // Intentar coincidencia exacta (case-insensitive)
                var exists = schema.Tables.Any(t => 
                    t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                
                if (!exists)
                {
                    // Intentar coincidencia flexible (ignorar guiones bajos, espacios, capitalizaci�n)
                    var normalizedSearchTable = NormalizeTableName(tableName);
                    var flexibleMatch = schema.Tables.FirstOrDefault(t => 
                        NormalizeTableName(t.TableName) == normalizedSearchTable);
                    
                    if (flexibleMatch != null)
                    {
                        _logger.LogInformation($"? Tabla encontrada con coincidencia flexible: '{tableName}' ? '{flexibleMatch.TableName}'");
                        // Reemplazar el nombre en el SQL con el nombre correcto
                        sql = System.Text.RegularExpressions.Regex.Replace(
                            sql, 
                            $@"\b{tableName}\b", 
                            flexibleMatch.TableName, 
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        
                        _logger.LogInformation($"? SQL corregido autom�ticamente: {sql}");
                        continue;
                    }
                    
                    // Si no hay coincidencia ni exacta ni flexible, reportar error
                    var availableTables = string.Join(", ", schema.Tables.Select(t => t.TableName));
                    _logger.LogError($"? Tabla '{tableName}' no encontrada. Disponibles: {availableTables}");
                    
                    return (false, $"La tabla '{tableName}' no existe en el esquema de la base de datos. Tablas disponibles: {availableTables}");
                }
                else
                {
                    _logger.LogInformation($"? Tabla '{tableName}' existe (coincidencia exacta)");
                }
            }

            // NUEVO: Validar columnas mencionadas en SELECT
            var selectMatch = System.Text.RegularExpressions.Regex.Match(sql, @"SELECT\s+(.+?)\s+FROM", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (selectMatch.Success)
            {
                var selectClause = selectMatch.Groups[1].Value;
                
                _logger.LogInformation($"?? Validando SELECT clause: {selectClause}");
                
                // Si no es SELECT *, verificar columnas
                if (!selectClause.Trim().Equals("*") && !selectClause.Contains("COUNT(*)"))
                {
                    // Extraer nombres de columnas
                    var columnParts = selectClause
                        .Split(',')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .ToList();
                    
                    _logger.LogInformation($"?? Columnas a validar: {string.Join(", ", columnParts)}");
                    
                    foreach (var columnPart in columnParts)
                    {
                        var actualColumnName = columnPart;
                        
                        // Limpiar funciones agregadas (COUNT, SUM, MAX, MIN, AVG, etc.)
                        if (actualColumnName.Contains("(") && actualColumnName.Contains(")"))
                        {
                            // Verificar si es una funci�n sin columna espec�fica (ej: COUNT(*))
                            if (actualColumnName.Contains("COUNT(*)") || 
                                actualColumnName.Contains("COUNT (*)"))
                            {
                                _logger.LogInformation($"? Ignorando COUNT(*)");
                                continue;
                            }
                            
                            // Extraer nombre de columna de dentro de la funci�n (ej: COUNT(Email) -> Email)
                            var match = System.Text.RegularExpressions.Regex.Match(
                                actualColumnName, 
                                @"\w+\s*\(\s*(\w+\.)?(\w+)\s*\)", 
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            
                            if (match.Success && match.Groups.Count > 2)
                            {
                                actualColumnName = match.Groups[2].Value;
                                _logger.LogInformation($"?? Columna extra�da de funci�n: {actualColumnName}");
                            }
                            else
                            {
                                _logger.LogInformation($"? Ignorando expresi�n con funci�n: {columnPart}");
                                continue;
                            }
                        }
                        
                        // Manejar alias de tabla (u.Email -> Email, u.* -> válido)
                        if (actualColumnName.Contains("."))
                        {
                            var parts = actualColumnName.Split('.');
                            var tableAlias = parts[0].Trim();
                            var columnName = parts[parts.Length - 1].Trim();
                            
                            // Si es *, es válido cuando va precedido de alias de tabla
                            if (columnName == "*")
                            {
                                _logger.LogInformation($"✅ Asterisco con alias de tabla válido: {actualColumnName}");
                                continue; // Saltar validación para u.*, t.*, etc.
                            }
                            
                            actualColumnName = columnName;
                            _logger.LogInformation($"🔍 Columna sin alias de tabla: {actualColumnName}");
                        }
                        
                        // Remover alias de columna (Email AS Correo -> Email)
                        if (actualColumnName.Contains(" AS ", StringComparison.OrdinalIgnoreCase))
                        {
                            actualColumnName = actualColumnName.Split(new[] { " AS ", " as " }, StringSplitOptions.None)[0].Trim();
                            _logger.LogInformation($"?? Columna sin alias: {actualColumnName}");
                        }
                        
                        // Limpiar caracteres extra�os
                        actualColumnName = actualColumnName.Trim('[', ']', '`', '"', '\'', ' ');
                        
                        // Manejar el caso de * sin alias (SELECT *)
                        if (actualColumnName == "*")
                        {
                            _logger.LogInformation($"✅ SELECT * detectado - válido");
                            continue; // * sin alias también es válido
                        }
                        
                        // Verificar si la columna existe en alguna tabla (case-insensitive y flexible)
                        var columnExists = schema.Tables.Any(t => 
                            t.Columns.Any(c => c.ColumnName.Equals(actualColumnName, StringComparison.OrdinalIgnoreCase)));
                        
                        if (!columnExists)
                        {
                            // Intentar coincidencia flexible
                            var normalizedSearchColumn = NormalizeTableName(actualColumnName); // Usa el mismo m�todo de normalizaci�n
                            string? flexibleColumnMatch = null;
                            
                            foreach (var table in schema.Tables)
                            {
                                flexibleColumnMatch = table.Columns.FirstOrDefault(c => 
                                    NormalizeTableName(c.ColumnName) == normalizedSearchColumn)?.ColumnName;
                                
                                if (flexibleColumnMatch != null)
                                {
                                    _logger.LogInformation($"? Columna encontrada con coincidencia flexible: '{actualColumnName}' ? '{flexibleColumnMatch}'");
                                    columnExists = true;
                                    break;
                                }
                            }
                            
                            if (!columnExists)
                            {
                                _logger.LogError($"? Columna '{actualColumnName}' NO existe en el esquema");
                                
                                // Buscar columna similar para sugerir
                                var suggestion = FindSimilarColumn(actualColumnName, schema);
                                var suggestionText = suggestion != null 
                                    ? $" �Quisiste decir '{suggestion}'?" 
                                    : "";
                                
                                // Listar columnas disponibles de la tabla principal (si se puede identificar)
                                var availableColumns = schema.Tables
                                    .SelectMany(t => t.Columns.Select(c => c.ColumnName))
                                    .Distinct()
                                    .Take(10);
                                

                                var availableColumnsText = string.Join(", ", availableColumns);
                                
                                return (false, $"La columna '{actualColumnName}' no existe en ninguna tabla del esquema.{suggestionText}\n\nAlgunas columnas disponibles: {availableColumnsText}");
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"? Columna '{actualColumnName}' existe en el esquema");
                        }
                    }
                }
            }

            // Validar columnas en WHERE clause tambi�n
            var whereMatch = System.Text.RegularExpressions.Regex.Match(sql, @"WHERE\s+(.+?)(\s+ORDER BY|\s+GROUP BY|\s*;|\s*$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            if (whereMatch.Success)
            {
                var whereClause = whereMatch.Groups[1].Value;
                _logger.LogInformation($"?? Validando WHERE clause: {whereClause}");
                
                // Extraer nombres de columnas del WHERE (buscar patrones como columna = valor)
                var whereColumns = System.Text.RegularExpressions.Regex.Matches(
                    whereClause,
                    @"(\w+\.)?(\w+)\s*[=<>!]+",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                foreach (System.Text.RegularExpressions.Match match in whereColumns)
                {
                    if (match.Groups.Count > 2)
                    {
                        var columnName = match.Groups[2].Value.Trim();
                        
                        // Verificar si es una palabra clave SQL (AND, OR, etc.)
                        var sqlKeywords = new[] { "AND", "OR", "NOT", "IN", "LIKE", "IS", "NULL", "TRUE", "FALSE" };
                        if (sqlKeywords.Contains(columnName.ToUpper()))
                        {
                            continue;
                        }
                        
                        var columnExists = schema.Tables.Any(t => 
                            t.Columns.Any(c => c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)));
                        
                        if (!columnExists)
                        {
                            // Intentar coincidencia flexible
                            var normalizedSearchColumn = NormalizeTableName(columnName);
                            var flexibleMatch = schema.Tables
                                .SelectMany(t => t.Columns)
                                .FirstOrDefault(c => NormalizeTableName(c.ColumnName) == normalizedSearchColumn);
                            

                            if (flexibleMatch != null)
                            {
                                _logger.LogInformation($"? Columna en WHERE encontrada con coincidencia flexible: '{columnName}' ? '{flexibleMatch.ColumnName}'");
                                columnExists = true;
                            }
                        }
                        
                        if (!columnExists)
                        {
                            _logger.LogError($"? Columna '{columnName}' en WHERE clause NO existe");
                            var suggestion = FindSimilarColumn(columnName, schema);
                            var suggestionText = suggestion != null ? $" �Quisiste decir '{suggestion}'?" : "";
                            
                            return (false, $"La columna '{columnName}' en la cl�usula WHERE no existe.{suggestionText}");
                        }
                        else
                        {
                            _logger.LogInformation($"? Columna '{columnName}' en WHERE existe");
                        }
                    }
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Normaliza un nombre de tabla para comparaci�n flexible (sin guiones, espacios, min�sculas)
        /// </summary>
        private string NormalizeTableName(string tableName)
        {
            return tableName
                .ToLower()
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "")
                .Trim();
        }

        /// <summary>
        /// Busca una columna similar en el esquema usando an�lisis sem�ntico autom�tico (SIN mapeos hardcodeados)
        /// </summary>
        private string? FindSimilarColumn(string searchColumn, DatabaseSchema schema)
        {
            searchColumn = searchColumn.ToLower().Trim();

            // 1. B�squeda EXACTA (ignorando may�sculas)
            foreach (var table in schema.Tables)
            {
                var exactMatch = table.Columns.FirstOrDefault(c => 
                    c.ColumnName.Equals(searchColumn, StringComparison.OrdinalIgnoreCase));
                
                if (exactMatch != null)
                {
                    _logger.LogInformation($"? Columna encontrada (exacta): '{exactMatch.ColumnName}'");
                    return exactMatch.ColumnName;
                }
            }

            // 2. B�squeda CONTAINS (la columna contiene el t�rmino buscado)
            foreach (var table in schema.Tables)
            {
                var containsMatch = table.Columns.FirstOrDefault(c => 
                    c.ColumnName.Contains(searchColumn, StringComparison.OrdinalIgnoreCase));
                
                if (containsMatch != null)
                {
                    _logger.LogInformation($"? Columna encontrada (contains): '{containsMatch.ColumnName}' contiene '{searchColumn}'");
                    return containsMatch.ColumnName;
                }
            }

            // 3. B�squeda INVERSA (el t�rmino buscado contiene la columna)
            foreach (var table in schema.Tables)
            {
                var reverseMatch = table.Columns.FirstOrDefault(c => 
                    searchColumn.Contains(c.ColumnName.ToLower()));
                
                if (reverseMatch != null)
                {
                    _logger.LogInformation($"? Columna encontrada (reverse): '{searchColumn}' contiene '{reverseMatch.ColumnName}'");
                    return reverseMatch.ColumnName;
                }
            }

            // 4. B�squeda por SIMILITUD (Levenshtein Distance)
            string? bestMatch = null;
            int bestDistance = int.MaxValue;
            const int MAX_DISTANCE = 3; // M�ximo 3 caracteres de diferencia

            foreach (var table in schema.Tables)
            {
                foreach (var column in table.Columns)
                {
                    var distance = LevenshteinDistance(searchColumn, column.ColumnName.ToLower());
                    
                    if (distance < bestDistance && distance <= MAX_DISTANCE)
                    {
                        bestDistance = distance;
                        bestMatch = column.ColumnName;
                    }
                }
            }

            if (bestMatch != null)
            {
                _logger.LogInformation($"? Columna encontrada (similitud): '{bestMatch}' es similar a '{searchColumn}' (distancia: {bestDistance})");
                return bestMatch;
            }

            // 5. B�squeda por PALABRAS CLAVE comunes (gen�rico, no hardcodeado)
            var keywords = ExtractKeywords(searchColumn);
            foreach (var keyword in keywords)
            {
                foreach (var table in schema.Tables)
                {
                    var keywordMatch = table.Columns.FirstOrDefault(c => 
                        c.ColumnName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                    
                    if (keywordMatch != null)
                    {
                        _logger.LogInformation($"? Columna encontrada (keyword): '{keywordMatch.ColumnName}' contiene keyword '{keyword}'");
                        return keywordMatch.ColumnName;
                    }
                }
            }

            _logger.LogWarning($"? No se encontr� ninguna columna similar a '{searchColumn}'");
            return null;
        }

        /// <summary>
        /// Extrae palabras clave de un t�rmino de b�squeda (para b�squeda sem�ntica)
        /// </summary>
        private List<string> ExtractKeywords(string searchTerm)
        {
            var keywords = new List<string>();
            searchTerm = searchTerm.ToLower();

            // Palabras comunes que indican conceptos (gen�rico, no espec�fico a ninguna BD)
            var commonPrefixes = new[] { "is", "has", "get", "set", "user", "name", "date", "time" };
            var commonSuffixes = new[] { "id", "name", "date", "time", "number", "code", "type", "status" };

            // Agregar el t�rmino completo
            keywords.Add(searchTerm);

            // Si el t�rmino tiene m�s de 4 caracteres, agregar substrings
            if (searchTerm.Length > 4)
            {
                // Primeros 4 caracteres
                keywords.Add(searchTerm.Substring(0, 4));
                
                // �ltimos 4 caracteres
                keywords.Add(searchTerm.Substring(searchTerm.Length - 4));
            }

            // Detectar y remover prefijos comunes
            foreach (var prefix in commonPrefixes)
            {
                if (searchTerm.StartsWith(prefix))
                {
                    var withoutPrefix = searchTerm.Substring(prefix.Length);
                    if (withoutPrefix.Length > 2)
                    {
                        keywords.Add(withoutPrefix);
                    }
                }
            }

            // Detectar y remover sufijos comunes
            foreach (var suffix in commonSuffixes)
            {
                if (searchTerm.EndsWith(suffix))
                {
                    var withoutSuffix = searchTerm.Substring(0, searchTerm.Length - suffix.Length);
                    if (withoutSuffix.Length > 2)
                    {
                        keywords.Add(withoutSuffix);
                    }
                }
            }

            return keywords.Distinct().ToList();
        }

        /// <summary>
        /// Calcula la distancia de Levenshtein entre dos cadenas (mide similitud)
        /// </summary>
        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; i++)
                d[i, 0] = i;
            
            for (int j = 0; j <= m; j++)
                d[0, j] = j;

            for (int j = 1; j <= m; j++)
            {
                for (int i = 1; i <= n; i++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        /// <summary>
        /// Agrega comillas dobles a los nombres de tabla Y columnas en PostgreSQL cuando sea necesario
        /// </summary>
        private string AddQuotesToPostgreSQLTables(string sql, DatabaseSchema schema)
        {
            // 1. ? MEJORADO: Agregar comillas a TODAS las referencias de nombres de TABLAS
            foreach (var table in schema.Tables)
            {
                var tableName = table.TableName;
                
                // Solo agregar comillas si el nombre tiene may�sculas
                if (tableName.Any(char.IsUpper))
                {
                    // Patr�n 1: FROM/JOIN tableName
                    var fromJoinPattern = $@"\b(FROM|JOIN|UPDATE|DELETE FROM)\s+{System.Text.RegularExpressions.Regex.Escape(tableName)}\b";
                    var fromJoinReplacement = $@"$1 ""{tableName}""";
                    
                    sql = System.Text.RegularExpressions.Regex.Replace(
                        sql, 
                        fromJoinPattern, 
                        fromJoinReplacement, 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                    
                    // Patr�n 2: tableName con alias (tableName t, tableName AS t)
                    var aliasPattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(tableName)}\s+(AS\s+)?([a-z]+)\b";
                    var aliasReplacement = $@"""{tableName}"" $1$2";
                    
                    sql = System.Text.RegularExpressions.Regex.Replace(
                        sql, 
                        aliasPattern, 
                        aliasReplacement, 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                    
                    // ? NUEVO: Patr�n 3: Tabla sola sin alias (ej: ON Funcionarios.Id)
                    // Esto captura referencias como: ON Funcionarios."Col", WHERE Funcionarios."Col"
                    var standaloneTablePattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(tableName)}\.";
                    var standaloneTableReplacement = $@"""{tableName}""." ;
                    
                    sql = System.Text.RegularExpressions.Regex.Replace(
                        sql,
                        standaloneTablePattern,
                        standaloneTableReplacement,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                }
            }
            
            // 2. ? Agregar comillas a nombres de COLUMNAS (NO a alias personalizados)
            foreach (var table in schema.Tables)
            {
                foreach (var column in table.Columns)
                {
                    var columnName = column.ColumnName;
                    
                    // Solo agregar comillas si el nombre tiene may�sculas
                    if (columnName.Any(char.IsUpper))
                    {
                        // ? IMPORTANTE: Solo agregar comillas si NO hay "AS" inmediatamente despu�s
                        
                        // Patr�n 1: alias.ColumnName (ej: f.Cedula ? f."Cedula")
                        // SOLO si NO est� seguido de AS
                        var aliasColumnPattern = $@"\b([a-zA-Z_]+)\.{System.Text.RegularExpressions.Regex.Escape(columnName)}(?!\s+AS\s)";
                        var aliasColumnReplacement = $@"$1.""{columnName}""";
                        
                        sql = System.Text.RegularExpressions.Regex.Replace(
                            sql,
                            aliasColumnPattern,
                            aliasColumnReplacement,
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        );
                        
                        // Patr�n 2: ColumnName sin alias (SELECT ColumnName, WHERE ColumnName)
                        // SOLO si NO est� seguido de AS
                        var standAlonePattern = $@"\b(SELECT|WHERE|AND|OR|ORDER BY|GROUP BY|HAVING|ON)\s+{System.Text.RegularExpressions.Regex.Escape(columnName)}(?!\s+AS\s)";
                        var standAloneReplacement = $@"$1 ""{columnName}""";
                        
                        sql = System.Text.RegularExpressions.Regex.Replace(
                            sql,
                            standAlonePattern,
                            standAloneReplacement,
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        );
                        
                        // Patr�n 3: ColumnName despu�s de coma
                        // SOLO si NO est� seguido de AS
                        var commaPattern = $@",\s*{System.Text.RegularExpressions.Regex.Escape(columnName)}(?!\s+AS\s)";
                        var commaReplacement = $@", ""{columnName}""";
                        
                        sql = System.Text.RegularExpressions.Regex.Replace(
                            sql,
                            commaPattern,
                            commaReplacement,
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        );
                    }
                }
            }
            
            _logger.LogInformation($"?? SQL con comillas aplicadas: {sql}");
            
            return sql;
        }

        /// <summary>
        /// Genera una respuesta simple cuando la interpretación con IA falla
        /// </summary>
        private string GenerateSimpleResponse(string question, int rowCount, string sql)
        {
            if (rowCount == 0)
            {
                return $"✅ **Consulta ejecutada correctamente**\n\n" +
                       $"No se encontraron resultados para: *\"{question}\"*\n\n" +
                       $"💡 **Sugerencias:**\n" +
                       $"- Verifica los filtros de búsqueda\n" +
                       $"- Intenta ampliar los criterios\n" +
                       $"- Revisa la ortografía de los términos";
            }
            else if (rowCount == 1)
            {
                return $"✅ **Consulta ejecutada correctamente**\n\n" +
                       $"📊 Encontré **{rowCount} registro** que coincide con tu búsqueda: *\"{question}\"*\n\n" +
                       $"Los resultados se muestran en la tabla de datos. Puedes descargarlos en CSV o PDF usando los botones de exportación.";
            }
            else
            {
                return $"✅ **Consulta ejecutada correctamente**\n\n" +
                       $"📊 Encontré **{rowCount} registros** que coinciden con tu búsqueda: *\"{question}\"*\n\n" +
                       $"Los resultados se muestran en la tabla de datos. Puedes descargarlos en CSV o PDF usando los botones de exportación.";
            }
        }

        /// <summary>
        /// Analiza el mensaje de error para sugerir correcciones de nombres de columnas
        /// Detecta dinámicamente nombres inválidos en múltiples idiomas y formatos
        /// </summary>
        private string AnalyzeErrorForColumnFix(string errorMessage, DatabaseSchema schema)
        {
            var analysis = new System.Text.StringBuilder();
            
            // 🔍 DETECCIÓN DINÁMICA: Extraer nombres entre comillas/corchetes del mensaje de error
            var invalidColumn = ExtractInvalidIdentifierFromError(errorMessage, "column");
            
            if (!string.IsNullOrEmpty(invalidColumn))
            {
                analysis.AppendLine($"\nINVALID COLUMN DETECTED: '{invalidColumn}'");
                
                // Buscar columnas similares en el esquema
                var similarColumns = FindSimilarColumns(invalidColumn, schema);
                
                if (similarColumns.Any())
                {
                    analysis.AppendLine("\nSUGGESTED CORRECTIONS (use EXACT name):");
                    foreach (var (table, column, similarity) in similarColumns.Take(5))
                    {
                        analysis.AppendLine($"  -> Use: {table}.{column} (similarity: {similarity:P0})");
                    }
                }
                else
                {
                    analysis.AppendLine("\nNo similar columns found. Review the schema carefully.");
                }
            }
            
            // 🔍 DETECCIÓN DINÁMICA: Extraer nombre de tabla inválida
            var invalidTable = ExtractInvalidIdentifierFromError(errorMessage, "table");
            
            if (!string.IsNullOrEmpty(invalidTable))
            {
                analysis.AppendLine($"\nINVALID TABLE DETECTED: '{invalidTable}'");
                
                // Buscar tablas similares
                var similarTables = schema.Tables
                    .Select(t => new { Table = t.TableName, Similarity = CalculateSimilarity(invalidTable, t.TableName) })
                    .Where(x => x.Similarity > 0.5)
                    .OrderByDescending(x => x.Similarity)
                    .ToList();
                
                if (similarTables.Any())
                {
                    analysis.AppendLine("\nSUGGESTED TABLE CORRECTIONS:");
                    foreach (var item in similarTables.Take(3))
                    {
                        analysis.AppendLine($"  -> Use: {item.Table} (similarity: {item.Similarity:P0})");
                    }
                }
            }
            
            return analysis.ToString();
        }

        /// <summary>
        /// Extrae dinámicamente el nombre del identificador inválido del mensaje de error
        /// Funciona con múltiples idiomas y formatos de error usando patrones inteligentes
        /// </summary>
        private string ExtractInvalidIdentifierFromError(string errorMessage, string identifierType)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return string.Empty;

            // Lista de patrones para diferentes tipos de errores y idiomas
            var patterns = new List<string>();

            if (identifierType.ToLower() == "column")
            {
                // Patrones para errores de columna (múltiples idiomas y formatos)
                patterns.AddRange(new[]
                {
                    // Inglés - SQL Server, PostgreSQL, MySQL
                    @"Invalid column name\s+'([^']+)'",
                    @"column\s+'([^']+)'.*not found",
                    @"Unknown column\s+'([^']+)'",
                    @"no such column:\s+([^\s,]+)",
                    
                    // Español
                    @"columna\s+'([^']+)'.*no.*v[aá]lida",
                    @"no existe.*columna\s+'?([^\s']+)",
                    @"columna.*'([^']+)'.*no encontrada",
                    
                    // Portugués
                    @"coluna\s+'([^']+)'.*inv[aá]lida",
                    
                    // Genérico: cualquier identificador entre comillas después de "column"
                    @"column[:\s]+'([^']+)'",
                    
                    // Formato con corchetes [columnName]
                    @"column\s+\[([^\]]+)\]",
                    @"Invalid.*\[([^\]]+)\].*column",
                    
                    // Sin comillas (algunos motores)
                    @"column\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+not",
                    @"Unknown\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+column"
                });
            }
            else if (identifierType.ToLower() == "table")
            {
                // Patrones para errores de tabla (múltiples idiomas)
                patterns.AddRange(new[]
                {
                    // Inglés
                    @"Invalid object name\s+'([^']+)'",
                    @"table\s+'([^']+)'.*not found",
                    @"Unknown table\s+'([^']+)'",
                    @"no such table:\s+([^\s,]+)",
                    @"Table\s+'([^']+)'.*doesn't exist",
                    
                    // Español
                    @"tabla\s+'([^']+)'.*no.*existe",
                    @"objeto.*'([^']+)'.*no.*v[aá]lido",
                    @"no.*encontr.*tabla\s+'?([^\s']+)",
                    
                    // Portugués
                    @"tabela\s+'([^']+)'.*n[ãa]o.*existe",
                    
                    // Genérico
                    @"object[:\s]+'([^']+)'",
                    @"FROM\s+'?([^\s',;]+).*not.*exist",
                    
                    // Formato con corchetes [tableName]
                    @"object\s+\[([^\]]+)\]",
                    @"Invalid.*\[([^\]]+)\].*table",
                    
                    // Sin comillas
                    @"table\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+not",
                    @"Unknown\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+table"
                });
            }

            // 🔍 Intentar cada patrón hasta encontrar coincidencia
            foreach (var pattern in patterns)
            {
                try
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        errorMessage,
                        pattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );

                    if (match.Success && match.Groups.Count > 1)
                    {
                        var identifier = match.Groups[1].Value.Trim();
                        
                        // Limpiar prefijos de esquema si existen (ej: "dbo.Usuario" → "Usuario")
                        if (identifier.Contains('.'))
                        {
                            var parts = identifier.Split('.');
                            identifier = parts[parts.Length - 1];
                        }
                        
                        // Remover corchetes/comillas si existen
                        identifier = identifier.Trim('[', ']', '"', '\'', '`');
                        
                        if (!string.IsNullOrWhiteSpace(identifier))
                        {
                            _logger.LogInformation($"🔍 Detected invalid {identifierType}: '{identifier}' using pattern match");
                            return identifier;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"⚠️ Error applying regex pattern - {ex.Message}");
                    continue; // Intentar siguiente patrón
                }
            }

            // 🆕 FALLBACK DINÁMICO: Buscar cualquier identificador entre comillas en el mensaje
            var fallbackPattern = @"'([a-zA-Z_][a-zA-Z0-9_]*)'";
            var fallbackMatch = System.Text.RegularExpressions.Regex.Match(
                errorMessage,
                fallbackPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            if (fallbackMatch.Success && fallbackMatch.Groups.Count > 1)
            {
                var identifier = fallbackMatch.Groups[1].Value.Trim();
                _logger.LogInformation($"🔍 Detected invalid {identifierType} using fallback pattern: '{identifier}'");
                return identifier;
            }

            _logger.LogWarning($"⚠️ Could not dynamically extract {identifierType} name from error: {errorMessage}");
            return string.Empty;
        }

        /// <summary>
        /// Busca columnas similares en todo el esquema usando similitud de strings
        /// </summary>
        private List<(string Table, string Column, double Similarity)> FindSimilarColumns(string searchColumn, DatabaseSchema schema)
        {
            var results = new List<(string Table, string Column, double Similarity)>();
            
            foreach (var table in schema.Tables)
            {
                foreach (var column in table.Columns)
                {
                    var similarity = CalculateSimilarity(searchColumn, column.ColumnName);
                    
                    // Considerar similares si coinciden en al menos 60%
                    if (similarity > 0.6)
                    {
                        results.Add((table.TableName, column.ColumnName, similarity));
                    }
                }
            }
            
            return results.OrderByDescending(x => x.Similarity).ToList();
        }

        /// <summary>
        /// Calcula similitud entre dos strings usando Levenshtein distance
        /// </summary>
        private double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0;
            
            // Case-insensitive comparison
            source = source.ToLower();
            target = target.ToLower();
            
            // Coincidencia exacta
            if (source == target)
                return 1.0;
            
            // Coincidencia por contenido (uno contiene al otro)
            if (source.Contains(target) || target.Contains(source))
                return 0.9;
            
            // Levenshtein distance
            var distance = LevenshteinDistance(source, target);
            var maxLength = Math.Max(source.Length, target.Length);
            
            return 1.0 - ((double)distance / maxLength);
        }
    }
}
