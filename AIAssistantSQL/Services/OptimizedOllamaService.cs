using AIAssistantSQL.Interfaces;
using AIAssistantSQL.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace AIAssistantSQL.Services
{
    /// <summary>
    /// Servicio optimizado para interactuar con Ollama con caché y mejoras de rendimiento
    /// </summary>
    public class OptimizedOllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OptimizedOllamaService> _logger;
        private readonly IMemoryCache _cache;
        private string _model;
        private const string OllamaGenerateEndpoint = "api/generate";
        
        // Cache para modelos disponibles
        private readonly ConcurrentDictionary<string, DateTime> _modelCache = new();
        private readonly SemaphoreSlim _modelSemaphore = new(1, 1);
        
        // Pool de conexiones reutilizables
        private readonly ConcurrentQueue<HttpRequestMessage> _requestPool = new();
        
        public OptimizedOllamaService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<OptimizedOllamaService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            _model = _configuration["Ollama:Model"] ?? "llama3.2";
            
            // Configurar timeouts optimizados
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // Aumentado para consultas complejas
            
            _logger.LogInformation($"🚀 OptimizedOllamaService inicializado - Modelo: {_model}");
        }

        /// <summary>
        /// Obtiene la lista de modelos con caché
        /// </summary>
        public async Task<List<OllamaModel>> GetAvailableModelsAsync()
        {
            const string cacheKey = "ollama_models";
            
            // Intentar obtener del caché
            if (_cache.TryGetValue(cacheKey, out List<OllamaModel>? cachedModels) && cachedModels != null)
            {
                _logger.LogInformation("📦 Modelos obtenidos desde caché");
                return cachedModels;
            }

            await _modelSemaphore.WaitAsync();
            try
            {
                _logger.LogInformation("🔍 Obteniendo lista de modelos desde Ollama...");

                using var response = await _httpClient.GetAsync("api/tags");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"⚠️ Error obteniendo modelos: {response.StatusCode}");
                    return new List<OllamaModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);

                var models = new List<OllamaModel>();

                if (jsonDoc.RootElement.TryGetProperty("models", out JsonElement modelsElement))
                {
                    await foreach (var model in ProcessModelsAsync(modelsElement))
                    {
                        models.Add(model);
                    }
                }

                // Cachear por 5 minutos
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    Priority = CacheItemPriority.High
                };

                _cache.Set(cacheKey, models, cacheOptions);
                
                _logger.LogInformation($"✅ Encontrados {models.Count} modelos (cacheados)");
                return models.OrderByDescending(m => m.IsRecommendedForSQL).ThenBy(m => m.Name).ToList();
            }
            finally
            {
                _modelSemaphore.Release();
            }
        }

        /// <summary>
        /// Procesa modelos de forma asíncrona
        /// </summary>
        private static async IAsyncEnumerable<OllamaModel> ProcessModelsAsync(JsonElement modelsElement)
        {
            await Task.Yield(); // Permitir que otros procesos continúen

            foreach (var modelElement in modelsElement.EnumerateArray())
            {
                var name = modelElement.GetProperty("name").GetString() ?? "";
                var sizeBytes = modelElement.GetProperty("size").GetInt64();
                var modifiedAt = modelElement.GetProperty("modified_at").GetDateTime();

                // Convertir tamaño de forma optimizada
                var sizeFormatted = FormatSize(sizeBytes);
                var isRecommendedForSQL = IsRecommendedForSQL(name);
                var description = GetModelDescription(name);

                yield return new OllamaModel
                {
                    Name = name,
                    Size = sizeFormatted,
                    ModifiedAt = modifiedAt,
                    Description = description,
                    IsRecommendedForSQL = isRecommendedForSQL
                };
            }
        }

        /// <summary>
        /// Formatea el tamaño de archivo de forma optimizada
        /// </summary>
        private static string FormatSize(long sizeBytes)
        {
            const double GB = 1024.0 * 1024.0 * 1024.0;
            const double MB = 1024.0 * 1024.0;
            
            return sizeBytes >= GB 
                ? $"{sizeBytes / GB:F1} GB" 
                : $"{sizeBytes / MB:F0} MB";
        }

        /// <summary>
        /// Determina si un modelo es recomendado para SQL
        /// </summary>
        private static bool IsRecommendedForSQL(string name)
        {
            var lowerName = name.ToLower();
            return lowerName.Contains("codellama", StringComparison.OrdinalIgnoreCase) ||
                   lowerName.Contains("deepseek-coder", StringComparison.OrdinalIgnoreCase) ||
                   lowerName.Contains("code", StringComparison.OrdinalIgnoreCase) ||
                   lowerName.Contains("sql", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si un modelo es de código
        /// </summary>
        private bool IsCodeModel(string name)
        {
            var lowerName = name.ToLower();
            return lowerName.Contains("codellama", StringComparison.OrdinalIgnoreCase) ||
                   lowerName.Contains("deepseek-coder", StringComparison.OrdinalIgnoreCase) ||
                   lowerName.Contains("code", StringComparison.OrdinalIgnoreCase) ||
                   lowerName.Contains("sql", StringComparison.OrdinalIgnoreCase);
        }

        public string GetCurrentModel() => _model;

        public void SetModel(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("El nombre del modelo no puede estar vacío", nameof(modelName));

            _logger.LogInformation($"🔄 Cambiando modelo de '{_model}' a '{modelName}'");
            _model = modelName;
            
            // ✅ GUARDAR EN CONFIGURACIÓN para que todas las instancias lo usen
            _configuration["Ollama:Model"] = modelName;
            
            // Limpiar caché de respuestas cuando cambia el modelo
            _cache.Remove($"sql_generation_{_model}");
        }

        /// <summary>
        /// Limpia la caché de modelos para forzar una actualización
        /// </summary>
        public void ClearModelsCache()
        {
            const string cacheKey = "ollama_models";
            _cache.Remove(cacheKey);
            _logger.LogInformation("🗑️ Caché de modelos limpiada - próxima consulta obtendrá modelos actualizados");
        }

        /// <summary>
        /// Genera SQL con caché y optimizaciones de rendimiento
        /// </summary>
        public async Task<string> GenerateSQLFromNaturalLanguageAsync(string naturalLanguageQuery, DatabaseSchema schema)
        {
            // Generar clave de caché basada en query y esquema
            var cacheKey = GenerateCacheKey(naturalLanguageQuery, schema, _model);
            
            // Verificar caché primero
            if (_cache.TryGetValue(cacheKey, out string? cachedSql) && cachedSql != null)
            {
                _logger.LogInformation("⚡ SQL obtenido desde caché");
                return cachedSql;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            
            try
            {
                var prompt = await BuildOptimizedPromptAsync(naturalLanguageQuery, schema);
                _logger.LogInformation($"🤖 Enviando consulta a Ollama con modelo: {_model}");

                var requestData = new
                {
                    model = _model,
                    prompt = prompt,
                    stream = false,
                    options = GetOptimizedModelOptions()
                };

                using var content = new StringContent(
                    JsonSerializer.Serialize(requestData), 
                    Encoding.UTF8, 
                    "application/json");

                using var response = await _httpClient.PostAsync(OllamaGenerateEndpoint, content, cts.Token);
                
                _logger.LogInformation($"📡 Respuesta recibida: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponse(response, cts.Token);
                }

                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    throw new Exception("Respuesta vacía del servicio de IA");
                }

                using var jsonResponse = JsonDocument.Parse(responseContent);

                if (jsonResponse.RootElement.TryGetProperty("response", out JsonElement responseElement))
                {
                    var generatedText = responseElement.GetString();
                    if (!string.IsNullOrWhiteSpace(generatedText))
                    {
                        var sql = ExtractSqlFromResponse(generatedText);
                        
                        // Cachear el resultado por 10 minutos
                        var cacheOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                            Priority = CacheItemPriority.Normal,
                            Size = sql.Length
                        };
                        
                        _cache.Set(cacheKey, sql, cacheOptions);
                        
                        _logger.LogInformation("✅ SQL generado y cacheado exitosamente");
                        return sql;
                    }
                }

                if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                {
                    var errorMessage = errorElement.GetString();
                    throw new Exception($"Error del servicio de IA: {errorMessage}");
                }

                throw new Exception("Respuesta inesperada del servicio de IA");
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("⏱️ Timeout alcanzado (45 segundos)");
                throw new Exception(
                    "⏱️ Timeout del servicio de IA\\n\\n" +
                    "La consulta tardó demasiado tiempo.\\n\\n" +
                    "Sugerencias:\\n" +
                    "- Intente con una pregunta más específica\\n" +
                    "- Verifique la carga del sistema\\n" +
                    "- Considere usar un modelo más ligero");
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "🔌 Error de conexión HTTP");
                throw new Exception(
                    $"🔌 Error de conectividad\\n\\n" +
                    $"Detalle: {httpEx.Message}\\n\\n" +
                    "Soluciones:\\n" +
                    "1. Verificar que Ollama esté ejecutándose\\n" +
                    "2. Comprobar puerto 11434\\n" +
                    "3. Verificar conectividad de red",
                    httpEx);
            }
        }

        /// <summary>
        /// Genera una clave de caché única para la consulta
        /// </summary>
        private static string GenerateCacheKey(string query, DatabaseSchema schema, string model)
        {
            var sb = new StringBuilder();
            sb.Append("sql_generation_");
            sb.Append(model);
            sb.Append("_");
            sb.Append(query.GetHashCode());
            sb.Append("_");
            sb.Append(schema.DatabaseName);
            sb.Append("_");
            sb.Append(schema.Tables.Count);
            
            return sb.ToString();
        }

        /// <summary>
        /// Construye un prompt optimizado de forma asíncrona
        /// </summary>
        private static async Task<string> BuildOptimizedPromptAsync(string naturalLanguageQuery, DatabaseSchema schema)
        {
            await Task.Yield();
            
            var sb = new StringBuilder(2048); // Pre-allocar capacidad

            sb.AppendLine("You are an expert SQL query generator specialized in creating accurate, optimized queries.");
            sb.AppendLine();
            sb.AppendLine("CRITICAL RULES:");
            sb.AppendLine("1. Return ONLY the SQL query (no explanations, markdown, or code blocks)");
            sb.AppendLine("2. Use EXACT table and column names from schema (case-sensitive)");
            sb.AppendLine("3. For SQL Server: use TOP, not LIMIT");
            sb.AppendLine("4. For PostgreSQL: use LIMIT, not TOP");
            sb.AppendLine("5. Always use proper JOIN syntax when relating tables");
            sb.AppendLine("6. Use aliases for better readability");
            sb.AppendLine();
            
            sb.AppendLine($"DATABASE: {schema.DatabaseName} ({schema.DatabaseType})");
            sb.AppendLine();
            
            // Agregar solo tablas relevantes para mejorar rendimiento
            var relevantTables = GetRelevantTables(naturalLanguageQuery, schema.Tables);
            
            sb.AppendLine("RELEVANT TABLES:");
            foreach (var table in relevantTables)
            {
                sb.AppendLine($"\\nTABLE: {table.TableName}");
                sb.AppendLine("  Columns:");
                
                foreach (var column in table.Columns.Take(20)) // Limitar columnas mostradas
                {
                    var pk = table.PrimaryKeys.Contains(column.ColumnName) ? " [PK]" : "";
                    sb.AppendLine($"    - {column.ColumnName} ({column.DataType}){pk}");
                }

                if (table.ForeignKeys.Any())
                {
                    sb.AppendLine("  Foreign Keys:");
                    foreach (var fk in table.ForeignKeys.Take(10))
                    {
                        sb.AppendLine($"    - {fk.ColumnName} → {fk.ReferencedTable}.{fk.ReferencedColumn}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine($"USER QUESTION: {naturalLanguageQuery}");
            sb.AppendLine();
            sb.AppendLine("GENERATE OPTIMIZED SQL QUERY:");

            return sb.ToString();
        }

        /// <summary>
        /// Obtiene tablas relevantes basadas en la consulta
        /// </summary>
        private static List<TableSchema> GetRelevantTables(string query, List<TableSchema> allTables)
        {
            var queryLower = query.ToLower();
            var relevantTables = new List<TableSchema>();
            
            // Buscar tablas mencionadas directamente
            foreach (var table in allTables)
            {
                if (queryLower.Contains(table.TableName.ToLower()) || 
                    IsTableRelevantByContext(queryLower, table))
                {
                    relevantTables.Add(table);
                }
            }
            
            // Si no encontramos tablas específicas, devolver las primeras 5
            if (!relevantTables.Any())
            {
                relevantTables = allTables.Take(5).ToList();
            }
            
            return relevantTables;
        }

        /// <summary>
        /// Determina si una tabla es relevante por contexto
        /// </summary>
        private static bool IsTableRelevantByContext(string query, TableSchema table)
        {
            // Buscar por nombres de columnas comunes
            var commonColumns = new[] { "nombre", "name", "id", "fecha", "date", "precio", "price", "usuario", "user" };
            
            return table.Columns.Any(col => 
                commonColumns.Any(common => 
                    col.ColumnName.ToLower().Contains(common) && query.Contains(common)));
        }

        /// <summary>
        /// Obtiene opciones optimizadas para el modelo
        /// </summary>
        private object GetOptimizedModelOptions()
        {
            return new
            {
                temperature = 0.1, // Más determinístico para SQL
                top_p = 0.9,
                max_tokens = 2048,
                num_predict = 512,
                repeat_penalty = 1.05,
                num_ctx = 4096, // Contexto más amplio
                num_thread = Environment.ProcessorCount, // Usar todos los núcleos disponibles
                num_gpu = 1 // Usar GPU si está disponible
            };
        }

        /// <summary>
        /// Maneja respuestas de error de forma optimizada
        /// </summary>
        private async Task<string> HandleErrorResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning($"⚠️ Ollama respondió con error: {response.StatusCode} - {response.ReasonPhrase}");
            
            return "**Error de conexión con IA**\\n\\n" +
                   $"**Estado:** {response.StatusCode} - {response.ReasonPhrase}\\n" +
                   "**Solución:**\\n" +
                   "1. Verificar que Ollama esté ejecutándose: `ollama serve`\\n" +
                   "2. Comprobar que el modelo esté instalado\\n" +
                   "3. Verificar conectividad en: http://localhost:11434\\n\\n" +
                   "**Nota:** El sistema continúa funcionando normalmente.";
        }

        public async Task<string> InterpretQueryResultsAsync(
            string originalQuestion,
            string executedSql,
            List<Dictionary<string, object>> results,
            List<string>? conversationHistory = null)
        {
            // Implementación similar con caché para interpretaciones
            var cacheKey = $"interpretation_{originalQuestion.GetHashCode()}_{results.Count}";
            
            if (_cache.TryGetValue(cacheKey, out string? cachedInterpretation) && cachedInterpretation != null)
            {
                return cachedInterpretation;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            try
            {
                var prompt = BuildInterpretationPrompt(originalQuestion, executedSql, results, conversationHistory);
                
                var requestData = new
                {
                    model = _model,
                    prompt = prompt,
                    stream = false,
                    options = new 
                    {
                        temperature = 0.7,
                        top_p = 0.9,
                        max_tokens = 1024,
                        num_predict = 512,
                        repeat_penalty = 1.1
                    }
                };

                using var content = new StringContent(
                    JsonSerializer.Serialize(requestData), 
                    Encoding.UTF8, 
                    "application/json");

                using var response = await _httpClient.PostAsync(OllamaGenerateEndpoint, content, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    return FormatResultsAsTable(results);
                }

                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                using var jsonResponse = JsonDocument.Parse(responseContent);

                if (jsonResponse.RootElement.TryGetProperty("response", out JsonElement responseElement))
                {
                    var interpretation = responseElement.GetString();
                    if (!string.IsNullOrWhiteSpace(interpretation))
                    {
                        // Cachear interpretación por 5 minutos
                        _cache.Set(cacheKey, interpretation, TimeSpan.FromMinutes(5));
                        return interpretation;
                    }
                }

                return FormatResultsAsTable(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error interpretando resultados");
                return FormatResultsAsTable(results);
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            const string cacheKey = "ollama_availability";
            
            // Cache de disponibilidad por 30 segundos
            if (_cache.TryGetValue(cacheKey, out bool cachedAvailability))
            {
                return cachedAvailability;
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                using var response = await _httpClient.GetAsync("api/tags", cts.Token);
                
                var isAvailable = response.IsSuccessStatusCode;
                
                _cache.Set(cacheKey, isAvailable, TimeSpan.FromSeconds(30));
                
                return isAvailable;
            }
            catch
            {
                _cache.Set(cacheKey, false, TimeSpan.FromSeconds(10));
                return false;
            }
        }

        // Métodos auxiliares optimizados...
        
        private static string GetModelDescription(string modelName)
        {
            var lowerName = modelName.ToLower();

            return lowerName switch
            {
                _ when lowerName.Contains("codellama") => "🎯 Especializado en código y SQL - Recomendado",
                _ when lowerName.Contains("deepseek-coder") => "🚀 Modelo avanzado para programación - Excelente",
                _ when lowerName.Contains("code") => "💻 Optimizado para programación y SQL",
                _ when lowerName.Contains("sql") => "🗃️ Especializado en bases de datos",
                _ when lowerName.Contains("llama3.2") => "⚡ Modelo rápido y ligero - Buen balance",
                _ when lowerName.Contains("llama3") => "🦙 Modelo general versión 3 - Bueno",
                _ when lowerName.Contains("mistral") => "🎭 Modelo eficiente y preciso",
                _ when lowerName.Contains("phi") => "📱 Modelo pequeño y eficiente",
                _ when lowerName.Contains("gemma") => "💎 Modelo de Google - Alta calidad",
                _ => "🔧 Modelo de propósito general"
            };
        }

        private string BuildInterpretationPrompt(
            string originalQuestion, 
            string executedSql, 
            List<Dictionary<string, object>> results,
            List<string>? conversationHistory)
        {
            var sb = new StringBuilder(1024);

            sb.AppendLine("You are a database assistant. Explain query results in Spanish clearly and concisely.");
            sb.AppendLine();
            sb.AppendLine("RULES:");
            sb.AppendLine("1. Always respond in SPANISH");
            sb.AppendLine("2. Be clear and professional");
            sb.AppendLine("3. Use ONLY the data provided");
            sb.AppendLine("4. Present data in markdown tables when appropriate");
            sb.AppendLine();
            sb.AppendLine($"USER QUESTION: {originalQuestion}");
            sb.AppendLine($"SQL EXECUTED: {executedSql}");
            sb.AppendLine();
            
            if (results == null || !results.Any())
            {
                sb.AppendLine("RESULTS: No data found");
                sb.AppendLine("Respond: 'No se encontraron resultados para la consulta.'");
            }
            else
            {
                sb.AppendLine($"RESULTS: {results.Count} rows found");
                // Agregar muestra de datos si es necesario
                if (results.Count <= 5)
                {
                    sb.AppendLine("Sample data:");
                    foreach (var row in results.Take(3))
                    {
                        sb.AppendLine($"- {string.Join(", ", row.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("RESPOND IN SPANISH with a brief explanation and the data formatted nicely:");

            return sb.ToString();
        }

        private string ExtractSqlFromResponse(string response)
        {
            var sql = response.Trim();

            // Limpiar markdown
            if (sql.StartsWith("```sql", StringComparison.OrdinalIgnoreCase))
                sql = sql[6..].Trim();
            else if (sql.StartsWith("```"))
                sql = sql[3..].Trim();

            if (sql.EndsWith("```"))
                sql = sql[..^3].Trim();

            // Extraer primera sentencia SQL válida
            var lines = sql.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var sqlLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                if (trimmedLine.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                    trimmedLine.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
                    sqlLines.Any())
                {
                    sqlLines.Add(trimmedLine);
                    if (trimmedLine.EndsWith(";")) break;
                }
            }

            return sqlLines.Any() ? string.Join(" ", sqlLines) : sql;
        }

        private string FormatResultsAsTable(List<Dictionary<string, object>> results)
        {
            if (results == null || !results.Any())
                return "**No se encontraron resultados.**";

            if (results.Count == 1 && results[0].Count == 1)
            {
                var value = results[0].First().Value;
                return $"**Resultado:** {value}";
            }

            var sb = new StringBuilder();
            var columns = results[0].Keys.ToList();
            
            // Headers
            sb.AppendLine("| " + string.Join(" | ", columns) + " |");
            sb.AppendLine("|" + string.Join("|", columns.Select(_ => "---")) + "|");

            // Rows (máximo 20 para rendimiento)
            foreach (var row in results.Take(20))
            {
                var values = columns.Select(c => row[c]?.ToString() ?? "NULL");
                sb.AppendLine("| " + string.Join(" | ", values) + " |");
            }

            if (results.Count > 20)
            {
                sb.AppendLine($"\n*... y {results.Count - 20} filas más*");
            }

            sb.AppendLine($"\n**Total de registros:** {results.Count}");
            return sb.ToString();
        }

        public void Dispose()
        {
            _modelSemaphore?.Dispose();
            _httpClient?.Dispose();
        }
    }
}