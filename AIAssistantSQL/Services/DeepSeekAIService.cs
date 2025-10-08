using AIAssistantSQL.Interfaces;
using AIAssistantSQL.Models;
using System.Text;
using System.Text.Json;

namespace AIAssistantSQL.Services
{
    public class DeepSeekAIService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DeepSeekAIService> _logger;
        private readonly string _apiKey;
        private readonly string _model;

        public DeepSeekAIService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<DeepSeekAIService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DeepSeek");
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["DeepSeekAI:ApiKey"] ?? string.Empty;
            _model = _configuration["DeepSeekAI:Model"] ?? "deepseek-chat";
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_apiKey))
                {
                    _logger.LogWarning("DeepSeek AI API Key no configurada");
                    return false;
                }

                // Verificar conectividad con un request simple
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.deepseek.com/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        model = _model,
                        messages = new[] { new { role = "user", content = "test" } },
                        max_tokens = 1
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando disponibilidad de DeepSeek AI");
                return false;
            }
        }

        public string GetCurrentModel()
        {
            return $"DeepSeek - {_model}";
        }

        public void SetModel(string modelName)
        {
            _configuration["DeepSeekAI:Model"] = modelName;
        }

        public async Task<List<OllamaModel>> GetAvailableModelsAsync()
        {
            return new List<OllamaModel>
            {
                new OllamaModel
                {
                    Name = "deepseek-chat",
                    Description = "Modelo principal de DeepSeek - Excelente para SQL",
                    Size = "Cloud (gratis con límites)"
                },
                new OllamaModel
                {
                    Name = "deepseek-coder",
                    Description = "Especializado en código - Muy preciso para SQL complejo",
                    Size = "Cloud (gratis con límites)"
                }
            };
        }

        public async Task<string> GenerateSQLFromNaturalLanguageAsync(
            string naturalLanguageQuery,
            DatabaseSchema schema)
        {
            try
            {
                var prompt = BuildPrompt(naturalLanguageQuery, schema);

                _logger.LogInformation($"?? Enviando consulta a DeepSeek AI ({_model})");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.deepseek.com/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                
                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are an expert SQL query generator." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3,
                    max_tokens = 1024
                };

                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"? DeepSeek error: {response.StatusCode} - {errorContent}");
                    throw new Exception($"DeepSeek API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseContent);

                if (jsonResponse.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                {
                    var message = choices[0].GetProperty("message");
                    var content = message.GetProperty("content").GetString();

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var sql = CleanSqlResponse(content);
                        _logger.LogInformation($"? SQL generado por DeepSeek: {sql}");
                        return sql;
                    }
                }

                throw new Exception("No se pudo obtener respuesta de DeepSeek");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando SQL con DeepSeek AI");
                throw new Exception($"Error con DeepSeek AI: {ex.Message}", ex);
            }
        }

        public async Task<string> InterpretQueryResultsAsync(
            string originalQuestion,
            string executedSql,
            List<Dictionary<string, object>> results,
            List<string> conversationHistory = null)
        {
            try
            {
                var prompt = BuildInterpretationPrompt(originalQuestion, executedSql, results, conversationHistory);

                _logger.LogInformation($"?? Interpretando resultados con DeepSeek AI");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.deepseek.com/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");

                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful database assistant that explains results in Spanish." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 2048
                };

                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("? Error al interpretar resultados con DeepSeek");
                    return $"? Consulta ejecutada exitosamente ({results.Count} registros encontrados).\n\n*Nota: No se pudo generar interpretación con IA.*";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseContent);

                if (jsonResponse.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                {
                    var message = choices[0].GetProperty("message");
                    var content = message.GetProperty("content").GetString();

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        _logger.LogInformation("? Interpretación generada");
                        return content;
                    }
                }

                return $"? Consulta ejecutada exitosamente ({results.Count} registros encontrados).";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interpretando resultados con DeepSeek AI");
                return $"? Consulta ejecutada exitosamente ({results.Count} registros encontrados).\n\n*Nota: No se pudo generar interpretación con IA.*";
            }
        }

        private string BuildPrompt(string naturalLanguageQuery, DatabaseSchema schema)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("You are an expert SQL query generator.");
            sb.AppendLine();
            sb.AppendLine("TASK: Generate a SQL SELECT query based on the user's question and the database schema provided.");
            sb.AppendLine();
            sb.AppendLine("RULES:");
            sb.AppendLine("1. Return ONLY the SQL query (no explanations, no markdown, no code blocks)");
            sb.AppendLine("2. Use EXACT table and column names from the schema (case-sensitive, including underscores)");
            sb.AppendLine("3. For SQL Server, use TOP instead of LIMIT");
            sb.AppendLine("4. Analyze which tables are relevant to the question before building the query");
            sb.AppendLine();

            sb.AppendLine($"DATABASE: {schema.DatabaseName} ({schema.DatabaseType})");
            sb.AppendLine();
            sb.AppendLine("AVAILABLE TABLES:");
            sb.AppendLine();

            foreach (var table in schema.Tables)
            {
                sb.AppendLine($"TABLE: {table.TableName}");
                sb.AppendLine("  Columns:");
                foreach (var column in table.Columns)
                {
                    var pk = table.PrimaryKeys.Contains(column.ColumnName) ? " [PK]" : "";
                    sb.AppendLine($"    - {column.ColumnName} ({column.DataType}){pk}");
                }

                if (table.ForeignKeys.Any())
                {
                    sb.AppendLine("  Foreign Keys:");
                    foreach (var fk in table.ForeignKeys)
                    {
                        sb.AppendLine($"    - {fk.ColumnName} ? {fk.ReferencedTable}.{fk.ReferencedColumn}");
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine($"USER QUESTION: {naturalLanguageQuery}");
            sb.AppendLine();
            sb.AppendLine("GENERATE SQL QUERY:");

            return sb.ToString();
        }

        private string BuildInterpretationPrompt(
            string originalQuestion,
            string executedSql,
            List<Dictionary<string, object>> results,
            List<string> conversationHistory)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("You are a database assistant. Your job is to explain query results in Spanish.");
            sb.AppendLine();
            sb.AppendLine("RULES:");
            sb.AppendLine("1. Always respond in SPANISH");
            sb.AppendLine("2. Be clear and concise");
            sb.AppendLine("3. Use ONLY the data from the results provided");
            sb.AppendLine("4. If there's 1 result and user asked for 'the most/least', say 'EL documento es...' (singular)");
            sb.AppendLine("5. If there are multiple results, say 'Encontré X documentos...' (plural)");
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

                // Rows
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

        private string CleanSqlResponse(string sql)
        {
            sql = sql.Trim();

            if (sql.StartsWith("```sql", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Substring(6);
            }
            else if (sql.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Substring(3);
            }

            if (sql.EndsWith("```"))
            {
                sql = sql.Substring(0, sql.Length - 3);
            }

            return sql.Trim();
        }
    }
}
