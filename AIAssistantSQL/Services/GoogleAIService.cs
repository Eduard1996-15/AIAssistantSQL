using AIAssistantSQL.Interfaces;
using AIAssistantSQL.Models;
using Mscc.GenerativeAI;

namespace AIAssistantSQL.Services
{
    public class GoogleAIService : IOllamaService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAIService> _logger;
        private readonly string _apiKey;
        private string _model;

        public GoogleAIService(
            IConfiguration configuration,
            ILogger<GoogleAIService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["GoogleAI:ApiKey"] ?? string.Empty;
            _model = _configuration["GoogleAI:Model"] ?? "gemini-1.5-flash";
            
            // Corregir nombre del modelo para la API v1
            _model = NormalizeModelName(_model);
        }

        private string NormalizeModelName(string modelName)
        {
            // La librería Mscc.GenerativeAI usa la API v1beta
            // Los nombres correctos son:
            // - gemini-1.5-flash-latest
            // - gemini-1.5-pro-latest
            // - gemini-pro
            
            return modelName.ToLower() switch
            {
                "gemini-1.5-flash" => "gemini-1.5-flash-latest",
                "gemini-1.5-pro" => "gemini-1.5-pro-latest",
                "gemini-1.0-pro" => "gemini-pro",
                "gemini-pro" => "gemini-pro",
                _ => "gemini-1.5-flash-latest" // Por defecto
            };
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_apiKey))
                {
                    _logger.LogWarning("Google AI API Key no configurada");
                    return false;
                }

                // Intentar crear el modelo para verificar conectividad
                var googleAI = new GoogleAI(apiKey: _apiKey);
                var model = googleAI.GenerativeModel(model: _model);
                
                // Hacer una prueba simple
                var testPrompt = "Hello";
                var response = await model.GenerateContent(testPrompt);
                
                return response != null && !string.IsNullOrEmpty(response.Text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando disponibilidad de Google AI");
                return false;
            }
        }

        public string GetCurrentModel()
        {
            return $"Google AI - {_model}";
        }

        public void SetModel(string modelName)
        {
            _model = NormalizeModelName(modelName);
            _configuration["GoogleAI:Model"] = modelName;
        }

        public async Task<List<OllamaModel>> GetAvailableModelsAsync()
        {
            return new List<OllamaModel>
            {
                new OllamaModel
                {
                    Name = "gemini-1.5-flash",
                    Description = "Rápido y eficiente - RECOMENDADO para SQL",
                    Size = "Cloud (gratis con límites)"
                },
                new OllamaModel
                {
                    Name = "gemini-1.5-pro",
                    Description = "Más potente pero más lento - Para SQL muy complejo",
                    Size = "Cloud (gratis con límites menores)"
                },
                new OllamaModel
                {
                    Name = "gemini-pro",
                    Description = "Versión anterior - Modelo estable",
                    Size = "Cloud"
                }
            };
        }

        public async Task<string> GenerateSQLFromNaturalLanguageAsync(
            string naturalLanguageQuery, 
            DatabaseSchema schema)
        {
            try
            {
                var googleAI = new GoogleAI(apiKey: _apiKey);
                var model = googleAI.GenerativeModel(model: _model);

                var prompt = BuildPrompt(naturalLanguageQuery, schema);

                _logger.LogInformation($"?? Enviando consulta a Google AI ({_model})");

                var response = await model.GenerateContent(prompt);
                var generatedSql = response?.Text ?? string.Empty;

                // Limpiar respuesta (remover markdown si lo incluye)
                generatedSql = CleanSqlResponse(generatedSql);

                _logger.LogInformation($"? SQL generado por Google AI: {generatedSql}");

                return generatedSql;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando SQL con Google AI");
                throw new Exception($"Error con Google AI: {ex.Message}", ex);
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
                var googleAI = new GoogleAI(apiKey: _apiKey);
                var model = googleAI.GenerativeModel(model: _model);

                var prompt = BuildInterpretationPrompt(originalQuestion, executedSql, results, conversationHistory);

                _logger.LogInformation($"?? Interpretando resultados con Google AI");

                var response = await model.GenerateContent(prompt);
                var interpretation = response?.Text ?? "No se pudo interpretar los resultados.";

                _logger.LogInformation($"? Interpretación generada");

                return interpretation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interpretando resultados con Google AI");
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
