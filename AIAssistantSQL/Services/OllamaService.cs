using AIAssistantSQL.Interfaces;
using AIAssistantSQL.Models;
using System.Text;
using System.Text.Json;

namespace AIAssistantSQL.Services
{
    /// <summary>
    /// Servicio para interactuar con Ollama y generar consultas SQL
    /// </summary>
    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OllamaService> _logger;
        private string _model; // Cambiado de readonly para poder modificarlo
        private const string OllamaGenerateEndpoint = "api/generate";

        // Constructor que recibe HttpClient por inyecci�n de dependencias (como tu app SCU)
        public OllamaService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration;
            _logger = logger;
            _model = _configuration["Ollama:Model"] ?? "llama3.2";
            
            // El HttpClient ya viene configurado desde Program.cs
            _logger.LogInformation($"?? OllamaService inicializado - Modelo: {_model}");
            _logger.LogInformation($"?? BaseAddress: {_httpClient.BaseAddress}");
        }

        /// <summary>
        /// Obtiene la lista de modelos disponibles en Ollama
        /// </summary>
        public async Task<List<OllamaModel>> GetAvailableModelsAsync()
        {
            try
            {
                _logger.LogInformation("?? Obteniendo lista de modelos disponibles...");

                var response = await _httpClient.GetAsync("api/tags");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"?? Error obteniendo modelos: {response.StatusCode}");
                    return new List<OllamaModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);

                var models = new List<OllamaModel>();

                if (jsonDoc.RootElement.TryGetProperty("models", out JsonElement modelsElement))
                {
                    foreach (var modelElement in modelsElement.EnumerateArray())
                    {
                        var name = modelElement.GetProperty("name").GetString() ?? "";
                        var sizeBytes = modelElement.GetProperty("size").GetInt64();
                        var modifiedAt = modelElement.GetProperty("modified_at").GetDateTime();

                        // Convertir tama�o a formato legible
                        var sizeGB = sizeBytes / (1024.0 * 1024.0 * 1024.0);
                        var sizeFormatted = sizeGB >= 1 
                            ? $"{sizeGB:F1} GB" 
                            : $"{sizeBytes / (1024.0 * 1024.0):F0} MB";

                        // Determinar si es recomendado para SQL
                        var isRecommendedForSQL = name.Contains("codellama", StringComparison.OrdinalIgnoreCase) ||
                                                   name.Contains("code", StringComparison.OrdinalIgnoreCase) ||
                                                   name.Contains("sql", StringComparison.OrdinalIgnoreCase);

                        var description = GetModelDescription(name);

                        models.Add(new OllamaModel
                        {
                            Name = name,
                            Size = sizeFormatted,
                            ModifiedAt = modifiedAt,
                            Description = description,
                            IsRecommendedForSQL = isRecommendedForSQL
                        });
                    }
                }

                _logger.LogInformation($"? Encontrados {models.Count} modelos");
                return models.OrderByDescending(m => m.IsRecommendedForSQL).ThenBy(m => m.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error obteniendo modelos de Ollama");
                return new List<OllamaModel>();
            }
        }

        /// <summary>
        /// Obtiene el modelo actualmente en uso
        /// </summary>
        public string GetCurrentModel()
        {
            return _model;
        }

        /// <summary>
        /// Cambia el modelo a usar y lo guarda en configuración
        /// </summary>
        public void SetModel(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                throw new ArgumentException("El nombre del modelo no puede estar vac�o", nameof(modelName));
            }

            _logger.LogInformation($"?? Cambiando modelo de '{_model}' a '{modelName}'");
            _model = modelName;
            
            // ✅ GUARDAR EN CONFIGURACIÓN para que todas las instancias lo usen
            _configuration["Ollama:Model"] = modelName;
            
            _logger.LogInformation($"? Modelo actualizado a: {_model}");
        }

        /// <summary>
        /// Limpia la caché de modelos para forzar una actualización
        /// </summary>
        public void ClearModelsCache()
        {
            // En este servicio base no hay caché, pero agregamos el método por compatibilidad
            _logger.LogInformation("🗑️ Limpieza de caché solicitada (sin implementación en servicio base)");
        }

        private string GetModelDescription(string modelName)
        {
            var lowerName = modelName.ToLower();

            if (lowerName.Contains("codellama"))
                return "?? Especializado en c�digo y SQL - Recomendado";
            else if (lowerName.Contains("code"))
                return "?? Optimizado para programaci�n y SQL";
            else if (lowerName.Contains("sql"))
                return "??? Especializado en bases de datos";
            else if (lowerName.Contains("llama3.2"))
                return "?? Modelo r�pido y ligero - Buen balance";
            else if (lowerName.Contains("llama3"))
                return "? Modelo general versi�n 3";
            else if (lowerName.Contains("llama2"))
                return "?? Modelo general versi�n 2";
            else if (lowerName.Contains("mistral"))
                return "?? Modelo eficiente y preciso";
            else if (lowerName.Contains("phi"))
                return "?? Modelo peque�o y eficiente";
            else if (lowerName.Contains("gemma"))
                return "?? Modelo de Google - Alta calidad";
            else
                return "?? Modelo de prop�sito general";
        }

        public async Task<string> GenerateSQLFromNaturalLanguageAsync(string naturalLanguageQuery, DatabaseSchema schema)
        {
            CancellationTokenSource? cts = null;
            HttpResponseMessage? response = null;
            
            try
            {
                var prompt = BuildPrompt(naturalLanguageQuery, schema);

                _logger.LogInformation($"?? Enviando consulta a Ollama con modelo: {_model}");

                // Request EXACTO como tu app SCU
                var requestData = new
                {
                    model = _model,
                    prompt = prompt,
                    stream = false,
                    options = new 
                    {
                        temperature = 0.3,
                        top_p = 0.8,
                        max_tokens = 1024,
                        num_predict = 512,
                        repeat_penalty = 1.1
                    }
                };

                var json = JsonSerializer.Serialize(requestData);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"?? Enviando petici�n a: {OllamaGenerateEndpoint}");
                _logger.LogInformation($"?? URL completa: {_httpClient.BaseAddress}{OllamaGenerateEndpoint}");

                // Timeout ajustado según el modelo:
                // - codellama (7B): 30s es suficiente
                // - deepseek-coder:33b: necesita 90-120s
                // - modelos grandes (70B+): necesitan 180-300s
                int timeoutSeconds = _model.Contains("33b") || _model.Contains("34b") ? 120 : 
                                    _model.Contains("70b") || _model.Contains("72b") ? 180 : 
                                    60; // Default 60s para modelos medianos
                
                _logger.LogInformation($"⏱️ Timeout configurado: {timeoutSeconds} segundos para modelo {_model}");
                
                cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

                try
                {
                    // USAR URL RELATIVA como en tu app SCU
                    response = await _httpClient.PostAsync(OllamaGenerateEndpoint, content, cts.Token);
                    
                    _logger.LogInformation($"?? Respuesta recibida: {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                        _logger.LogWarning($"?? Ollama respondi� con error: {response.StatusCode} - {response.ReasonPhrase}");
                        _logger.LogWarning($"?? Contenido del error: {errorContent}");
                        
                        return "**Error de conexion con IA**\n\n" +
                               $"**Estado:** {response.StatusCode} - {response.ReasonPhrase}\n" +
                               "**Solucion:**\n" +
                               "1. Verificar que Ollama este ejecutandose: `ollama serve`\n" +
                               "2. Comprobar que el modelo este instalado: `ollama pull llama3.2`\n" +
                               "3. Verificar conectividad en: http://localhost:11434\n\n" +
                               "**Nota:** El sistema continua funcionando normalmente.";
                    }

                    string responseContent;
                    try
                    {
                        responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                        _logger.LogInformation($"?? Contenido recibido: {responseContent.Length} caracteres");
                    }
                    catch (Exception readEx)
                    {
                        _logger.LogError(readEx, "? Error leyendo contenido de respuesta");
                        throw new Exception("Error leyendo respuesta de IA");
                    }

                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        _logger.LogWarning("?? Respuesta vac�a de Ollama");
                        throw new Exception("Respuesta vac�a del servicio de IA");
                    }

                    JsonDocument jsonResponse;
                    try
                    {
                        jsonResponse = JsonDocument.Parse(responseContent);
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "? Error deserializando JSON");
                        throw new Exception("Error procesando respuesta de IA");
                    }

                    // Parsear respuesta IGUAL que tu app SCU
                    if (jsonResponse.RootElement.TryGetProperty("response", out JsonElement responseElement))
                    {
                        var generatedText = responseElement.GetString();
                        if (!string.IsNullOrWhiteSpace(generatedText))
                        {
                            _logger.LogInformation("? Respuesta de IA procesada exitosamente");
                            
                            // Peque�a pausa para asegurar completion
                            await Task.Delay(10, cts.Token);
                            
                            var sql = ExtractSqlFromResponse(generatedText);
                            _logger.LogInformation($"? SQL generado exitosamente: {sql}");
                            
                            return sql;
                        }
                        else
                        {
                            _logger.LogWarning("?? Respuesta de IA vac�a en campo 'response'");
                            throw new Exception("Respuesta vac�a. Intente con una pregunta m�s espec�fica.");
                        }
                    }

                    // Verificar si hay un error en la respuesta
                    if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                    {
                        var errorMessage = errorElement.GetString();
                        _logger.LogError($"? Error reportado por Ollama: {errorMessage}");
                        throw new Exception($"Error del servicio de IA: {errorMessage}");
                    }

                    _logger.LogWarning("?? Estructura de respuesta inesperada");
                    throw new Exception("Respuesta inesperada del servicio de IA");
                }
                catch (OperationCanceledException) when (cts?.Token.IsCancellationRequested == true)
                {
                    // Calcular timeout usado
                    int timeoutUsed = _model.Contains("33b") || _model.Contains("34b") ? 120 : 
                                     _model.Contains("70b") || _model.Contains("72b") ? 180 : 
                                     60;
                    
                    _logger.LogWarning($"⏱️ Timeout de {timeoutUsed} segundos alcanzado para modelo {_model}");
                    
                    string suggestions = _model.Contains("33b") || _model.Contains("70b") 
                        ? "- El modelo es grande, considere usar un modelo más pequeño (codellama, llama3.2)\n" +
                          "- O espere un poco más (primera consulta suele ser más lenta)\n" +
                          "- Verifique que tiene suficiente RAM (mínimo 16GB para 33b)\n"
                        : "- Intente con una pregunta más corta\n" +
                          "- Verifique recursos del sistema\n";
                    
                    throw new Exception(
                        $"⏱️ Timeout del servicio de IA\n\n" +
                        $"La consulta tardó más de {timeoutUsed} segundos.\n\n" +
                        "Sugerencias:\n" +
                        suggestions +
                        "- Reinicie Ollama si persiste: ollama serve");
                }
                finally
                {
                    // CR�TICO: Liberar recursos HTTP
                    if (response != null)
                    {
                        try
                        {
                            response.Dispose();
                        }
                        catch (Exception disposeEx)
                        {
                            _logger.LogWarning(disposeEx, "?? Error liberando HttpResponseMessage");
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "?? Error de conexi�n HTTP");
                throw new Exception(
                    $"? Error de conectividad\n\n" +
                    $"Detalle: {httpEx.Message}\n\n" +
                    "Soluciones:\n" +
                    "1. Verificar que Ollama est� ejecut�ndose: ollama serve\n" +
                    "2. Comprobar puerto 11434\n" +
                    "3. Verificar firewall\n" +
                    "4. Probar: curl http://localhost:11434/api/tags",
                    httpEx);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && ex is not HttpRequestException)
            {
                _logger.LogError(ex, "? Error inesperado");
                throw new Exception($"Error al comunicarse con Ollama: {ex.Message}", ex);
            }
            finally
            {
                // CR�TICO: Liberar CancellationTokenSource
                try
                {
                    cts?.Dispose();
                }
                catch (Exception ctsEx)
                {
                    _logger.LogWarning(ctsEx, "?? Error liberando CancellationTokenSource");
                }

                // CR�TICO: Garbage collection
                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception gcEx)
                {
                    _logger.LogWarning(gcEx, "?? Error en GC");
                }
            }
        }

        /// <summary>
        /// Interpreta los resultados de una consulta SQL y genera una respuesta en lenguaje natural
        /// </summary>
        public async Task<string> InterpretQueryResultsAsync(
            string originalQuestion,
            string executedSql,
            List<Dictionary<string, object>> results,
            List<string>? conversationHistory = null)
        {
            CancellationTokenSource? cts = null;
            HttpResponseMessage? response = null;
            
            try
            {
                var prompt = BuildInterpretationPrompt(originalQuestion, executedSql, results, conversationHistory);

                _logger.LogInformation($"?? Interpretando resultados con IA...");

                var requestData = new
                {
                    model = _model,
                    prompt = prompt,
                    stream = false,
                    options = new 
                    {
                        temperature = 0.7, // M�s creativo para respuestas naturales
                        top_p = 0.9,
                        max_tokens = 2048,
                        num_predict = 1024,
                        repeat_penalty = 1.1
                    }
                };

                var json = JsonSerializer.Serialize(requestData);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Timeout ajustado para interpretación (generalmente más rápido que generación SQL)
                int timeoutSeconds = _model.Contains("33b") || _model.Contains("34b") ? 90 : 
                                    _model.Contains("70b") || _model.Contains("72b") ? 120 : 
                                    45;
                
                cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

                response = await _httpClient.PostAsync(OllamaGenerateEndpoint, content, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("?? Error al interpretar resultados");
                    return FormatResultsAsTable(results); // Fallback a tabla simple
                }

                string responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                var jsonResponse = JsonDocument.Parse(responseContent);

                if (jsonResponse.RootElement.TryGetProperty("response", out JsonElement responseElement))
                {
                    var interpretation = responseElement.GetString();
                    if (!string.IsNullOrWhiteSpace(interpretation))
                    {
                        _logger.LogInformation("? Interpretaci�n generada exitosamente");
                        return interpretation;
                    }
                }

                return FormatResultsAsTable(results); // Fallback
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error interpretando resultados");
                return FormatResultsAsTable(results); // Fallback
            }
            finally
            {
                response?.Dispose();
                cts?.Dispose();
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                _logger.LogInformation("?? Verificando disponibilidad de Ollama...");

                var response = await _httpClient.GetAsync("api/tags");

                _logger.LogInformation($"?? Respuesta: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("? Ollama disponible");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"?? Ollama respondi�: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Ollama no disponible");
                return false;
            }
        }

        private string BuildPrompt(string naturalLanguageQuery, DatabaseSchema schema)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Generate a SQL query for this question.");
            sb.AppendLine();
            sb.AppendLine($"Database: {schema.DatabaseName} ({schema.DatabaseType})");
            sb.AppendLine();
            
            sb.AppendLine("Schema:");
            foreach (var table in schema.Tables)
            {
                sb.AppendLine($"\nTABLE {table.TableName}:");
                foreach (var column in table.Columns)
                {
                    var pk = table.PrimaryKeys.Contains(column.ColumnName) ? " [PK]" : "";
                    sb.AppendLine($"  - {column.ColumnName} ({column.DataType}){pk}");
                }
            }
            
            sb.AppendLine();
            sb.AppendLine($"Question: {naturalLanguageQuery}");
            sb.AppendLine();
            sb.AppendLine("Rules:");
            sb.AppendLine("1. Use EXACT column names from schema above (case-sensitive)");
            sb.AppendLine("2. Return only SQL query, no explanations");
            sb.AppendLine("3. For SQL Server use TOP, not LIMIT");
            sb.AppendLine();
            sb.AppendLine("SQL Query:");

            return sb.ToString();
        }

        private string BuildInterpretationPrompt(
            string originalQuestion, 
            string executedSql, 
            List<Dictionary<string, object>> results,
            List<string>? conversationHistory)
        {
            var sb = new StringBuilder();

            sb.AppendLine("You are a database assistant. Your job is to explain query results in Spanish.");
            sb.AppendLine();
            sb.AppendLine("RULES:");
            sb.AppendLine("1. Always respond in SPANISH");
            sb.AppendLine("2. Be clear and concise");
            sb.AppendLine("3. Use ONLY the data from the results provided");
            sb.AppendLine("4. If there's 1 result and user asked for 'the most/least', say 'EL documento es...' (singular)");
            sb.AppendLine("5. If there are multiple results, say 'Encontr� X documentos...' (plural)");
            sb.AppendLine("6. Present data in markdown tables");
            sb.AppendLine();
            sb.AppendLine($"USER QUESTION: {originalQuestion}");
            sb.AppendLine($"SQL EXECUTED: {executedSql}");
            sb.AppendLine();
            
            if (results == null || !results.Any())
            {
                sb.AppendLine("RESULTS: No data found (0 rows)");
                sb.AppendLine();
                sb.AppendLine("Respond: 'No se encontraron resultados.'");
            }
            else
            {
                sb.AppendLine($"RESULTS: {results.Count} rows");
                sb.AppendLine();

                var columns = results[0].Keys.ToList();
                
                // Header
                sb.AppendLine("| " + string.Join(" | ", columns) + " |");
                sb.AppendLine("|" + string.Join("|", columns.Select(_ => "---")) + "|");
                
                // Rows (mostrar todos)
                foreach (var row in results)
                {
                    var values = columns.Select(col => 
                    {
                        var val = row[col];
                        return val == null ? "NULL" : val.ToString();
                    });
                    sb.AppendLine("| " + string.Join(" | ", values) + " |");
                }
            }

            sb.AppendLine();
            sb.AppendLine("RESPOND IN SPANISH:");
            sb.AppendLine("Format: [Brief answer] + [markdown table] + Total: X registros");

            return sb.ToString();
        }

        private string FormatResultsAsTable(List<Dictionary<string, object>> results)
        {
            if (results == null || !results.Any())
            {
                return "**No se encontraron resultados.**";
            }

            var sb = new StringBuilder();
            
            // Si es un solo valor (como COUNT)
            if (results.Count == 1 && results[0].Count == 1)
            {
                var value = results[0].First().Value;
                return $"**Resultado:** {value}";
            }

            // Tabla markdown
            sb.AppendLine();
            
            // Headers
            var columns = results[0].Keys.ToList();
            sb.AppendLine("| " + string.Join(" | ", columns) + " |");
            sb.AppendLine("|" + string.Join("|", columns.Select(c => "---")) + "|");

            // Rows (m�ximo 50 para no saturar la vista)
            foreach (var row in results.Take(50))
            {
                sb.AppendLine("| " + string.Join(" | ", columns.Select(c => row[c]?.ToString() ?? "NULL")) + " |");
            }

            if (results.Count > 50)
            {
                sb.AppendLine();
                sb.AppendLine($"*... y {results.Count - 50} filas m�s*");
            }

            sb.AppendLine();
            sb.AppendLine($"**Total de registros:** {results.Count}");

            return sb.ToString();
        }

        private string ExtractSqlFromResponse(string response)
        {
            // Limpiar markdown code blocks
            var sql = response.Trim();

            // Remover ```sql o ``` al inicio
            if (sql.StartsWith("```sql", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Substring(6).Trim();
            }
            else if (sql.StartsWith("```"))
            {
                sql = sql.Substring(3).Trim();
            }

            // Remover ``` al final
            if (sql.EndsWith("```"))
            {
                sql = sql.Substring(0, sql.Length - 3).Trim();
            }

            // Tomar solo la primera sentencia SQL
            var lines = sql.Split('\n');
            var sqlLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Si la l�nea parece ser SQL
                if (trimmedLine.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                    trimmedLine.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
                    sqlLines.Any())
                {
                    sqlLines.Add(trimmedLine);

                    // Si termina con ; es el final
                    if (trimmedLine.EndsWith(";"))
                        break;
                }
            }

            return sqlLines.Any() ? string.Join(" ", sqlLines) : sql;
        }
    }
}
