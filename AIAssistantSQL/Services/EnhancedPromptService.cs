using AIAssistantSQL.Models;
using System.Text;

namespace AIAssistantSQL.Services
{
    /// <summary>
    /// Servicio especializado para generar prompts optimizados para consultas SQL
    /// </summary>
    public class EnhancedPromptService
    {
        private readonly ILogger<EnhancedPromptService> _logger;

        public EnhancedPromptService(ILogger<EnhancedPromptService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Construye un prompt avanzado con contexto detallado del esquema
        /// </summary>
        public string BuildAdvancedSQLPrompt(string naturalLanguageQuery, DatabaseSchema schema, string model = "general")
        {
            var sb = new StringBuilder(4096);

            // Instrucciones principales optimizadas por modelo
            AddModelSpecificInstructions(sb, model);
            
            // Reglas críticas mejoradas
            AddCriticalRules(sb, schema.DatabaseType);
            
            // Contexto de la base de datos
            AddDatabaseContext(sb, schema);
            
            // Análisis inteligente de tablas relevantes
            var relevantTables = AnalyzeRelevantTables(naturalLanguageQuery, schema.Tables);
            AddRelevantTablesContext(sb, relevantTables, schema.DatabaseType);
            
            // Ejemplos específicos basados en el tipo de consulta
            AddQueryTypeExamples(sb, naturalLanguageQuery, schema.DatabaseType);
            
            // Consulta del usuario
            AddUserQuery(sb, naturalLanguageQuery);
            
            // Instrucciones finales específicas
            AddFinalInstructions(sb, schema.DatabaseType);

            var prompt = sb.ToString();
            _logger.LogInformation($"🔧 Prompt generado: {prompt.Length} caracteres");
            
            return prompt;
        }

        /// <summary>
        /// Añade instrucciones específicas según el modelo de IA
        /// </summary>
        private void AddModelSpecificInstructions(StringBuilder sb, string model)
        {
            sb.AppendLine("You are an expert SQL query generator with deep knowledge of database systems.");
            sb.AppendLine();

            if (model.Contains("codellama", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("As CodeLlama, focus on generating syntactically perfect, optimized SQL queries.");
                sb.AppendLine("Prioritize code quality, proper indexing hints, and efficient JOIN strategies.");
            }
            else if (model.Contains("deepseek", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("As DeepSeek Coder, apply advanced reasoning to understand complex query requirements.");
                sb.AppendLine("Generate efficient, well-structured queries with proper error handling considerations.");
            }
            else if (model.Contains("mistral", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("As Mistral, focus on precise, concise SQL generation with attention to performance.");
                sb.AppendLine("Ensure queries are both accurate and optimized for the specific database type.");
            }
            else
            {
                sb.AppendLine("Generate accurate, optimized SQL queries following best practices.");
            }

            sb.AppendLine();
        }

        /// <summary>
        /// Añade reglas críticas mejoradas
        /// </summary>
        private void AddCriticalRules(StringBuilder sb, DatabaseType dbType)
        {
            sb.AppendLine("CRITICAL RULES (MUST FOLLOW):");
            sb.AppendLine("1. Return ONLY the SQL query - no explanations, markdown, or code blocks");
            sb.AppendLine("2. Use EXACT table and column names from schema (case-sensitive)");
            sb.AppendLine("3. Always use proper aliases for tables (t1, t2, etc.)");
            
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    sb.AppendLine("4. SQL Server: Use TOP N instead of LIMIT N");
                    sb.AppendLine("5. SQL Server: Use ISNULL() for null handling");
                    sb.AppendLine("6. SQL Server: Use square brackets [table] for reserved words");
                    sb.AppendLine("7. SQL Server: Use GETDATE() for current date");
                    break;
                    
                case DatabaseType.PostgreSQL:
                    sb.AppendLine("4. PostgreSQL: Use LIMIT N instead of TOP N");
                    sb.AppendLine("5. PostgreSQL: Use COALESCE() for null handling");
                    sb.AppendLine("6. PostgreSQL: Use double quotes \"table\" for case-sensitive names");
                    sb.AppendLine("7. PostgreSQL: Use NOW() or CURRENT_TIMESTAMP for current date");
                    break;
                    
                case DatabaseType.MySQL:
                    sb.AppendLine("4. MySQL: Use LIMIT N instead of TOP N");
                    sb.AppendLine("5. MySQL: Use IFNULL() for null handling");
                    sb.AppendLine("6. MySQL: Use backticks `table` for reserved words");
                    sb.AppendLine("7. MySQL: Use NOW() for current date");
                    break;
                    
                default:
                    sb.AppendLine("4. Use appropriate LIMIT/TOP syntax for the database");
                    sb.AppendLine("5. Handle NULL values appropriately");
                    break;
            }
            
            sb.AppendLine("8. Use proper JOIN syntax (INNER/LEFT/RIGHT JOIN)");
            sb.AppendLine("9. Always specify join conditions in ON clause");
            sb.AppendLine("10. Use meaningful aliases and avoid ambiguous column references");
            sb.AppendLine();
        }

        /// <summary>
        /// Añade contexto de la base de datos
        /// </summary>
        private void AddDatabaseContext(StringBuilder sb, DatabaseSchema schema)
        {
            sb.AppendLine($"DATABASE CONTEXT:");
            sb.AppendLine($"• Name: {schema.DatabaseName}");
            sb.AppendLine($"• Type: {schema.DatabaseType}");
            sb.AppendLine($"• Total Tables: {schema.Tables.Count}");
            sb.AppendLine($"• Domain: {InferDatabaseDomain(schema.Tables)}");
            sb.AppendLine();
        }

        /// <summary>
        /// Infiere el dominio de la base de datos basado en los nombres de las tablas
        /// </summary>
        private string InferDatabaseDomain(List<TableSchema> tables)
        {
            var tableNames = tables.Select(t => t.TableName.ToLower()).ToList();
            
            // Detectar dominio basado en patrones comunes de nombres de tablas
            // Usamos patrones genéricos sin hardcodear nombres específicos
            
            // E-commerce: productos, ventas, clientes, pedidos, inventario
            if (tableNames.Any(n => n.Contains("producto") || n.Contains("product") || 
                                    n.Contains("venta") || n.Contains("sale") || 
                                    n.Contains("cliente") || n.Contains("customer") ||
                                    n.Contains("pedido") || n.Contains("order") ||
                                    n.Contains("inventario") || n.Contains("inventory")))
                return "E-commerce/Ventas";
            
            // Gestión de usuarios: autenticación, perfiles, sesiones, roles, permisos
            if (tableNames.Any(n => n.Contains("usuario") || n.Contains("user") || 
                                    n.Contains("perfil") || n.Contains("profile") || 
                                    n.Contains("sesion") || n.Contains("session") ||
                                    n.Contains("rol") || n.Contains("role") ||
                                    n.Contains("permiso") || n.Contains("permission") ||
                                    n.Contains("auth") || n.Contains("login")))
                return "Gestión de Usuarios";
            
            // RRHH: empleados, departamentos, nóminas, asistencias
            if (tableNames.Any(n => n.Contains("empleado") || n.Contains("employee") || 
                                    n.Contains("departamento") || n.Contains("department") || 
                                    n.Contains("nomina") || n.Contains("payroll") ||
                                    n.Contains("asistencia") || n.Contains("attendance")))
                return "Recursos Humanos";
            
            // Salud: pacientes, médicos, citas, diagnósticos, tratamientos
            if (tableNames.Any(n => n.Contains("paciente") || n.Contains("patient") || 
                                    n.Contains("medico") || n.Contains("doctor") || 
                                    n.Contains("cita") || n.Contains("appointment") ||
                                    n.Contains("diagnostico") || n.Contains("diagnosis") ||
                                    n.Contains("tratamiento") || n.Contains("treatment")))
                return "Sistema Médico";
            
            // Educación: estudiantes, cursos, profesores, materias, calificaciones
            if (tableNames.Any(n => n.Contains("estudiante") || n.Contains("student") || 
                                    n.Contains("curso") || n.Contains("course") || 
                                    n.Contains("profesor") || n.Contains("teacher") ||
                                    n.Contains("materia") || n.Contains("subject") ||
                                    n.Contains("calificacion") || n.Contains("grade")))
                return "Sistema Educativo";
            
            // Finanzas: transacciones, cuentas, pagos, facturas, gastos
            if (tableNames.Any(n => n.Contains("transaccion") || n.Contains("transaction") || 
                                    n.Contains("cuenta") || n.Contains("account") || 
                                    n.Contains("pago") || n.Contains("payment") ||
                                    n.Contains("factura") || n.Contains("invoice") ||
                                    n.Contains("gasto") || n.Contains("expense")))
                return "Sistema Financiero";
                
            return "Sistema General";
        }

        /// <summary>
        /// Analiza y determina tablas relevantes para la consulta
        /// </summary>
        private List<TableSchema> AnalyzeRelevantTables(string query, List<TableSchema> allTables)
        {
            var queryLower = query.ToLower();
            var relevantTables = new List<TableSchema>();
            
            // Palabras clave por contexto
            var keywords = ExtractKeywords(queryLower);
            
            // Buscar tablas por nombres exactos
            foreach (var table in allTables)
            {
                var tableName = table.TableName.ToLower();
                
                // Coincidencia directa con nombre de tabla
                if (keywords.Any(k => tableName.Contains(k) || k.Contains(tableName)))
                {
                    relevantTables.Add(table);
                    continue;
                }
                
                // Coincidencia por columnas importantes
                if (table.Columns.Any(col => keywords.Any(k => col.ColumnName.ToLower().Contains(k))))
                {
                    relevantTables.Add(table);
                    continue;
                }
            }
            
            // Si no encontramos tablas específicas, usar heurísticas
            if (!relevantTables.Any())
            {
                relevantTables = GetTablesByHeuristics(queryLower, allTables);
            }
            
            // Agregar tablas relacionadas por FK
            var relatedTables = GetRelatedTables(relevantTables, allTables);
            relevantTables.AddRange(relatedTables);
            
            // Limitar a máximo 8 tablas para rendimiento
            return relevantTables.Distinct().Take(8).ToList();
        }

        /// <summary>
        /// Extrae palabras clave de la consulta
        /// </summary>
        private List<string> ExtractKeywords(string query)
        {
            // Remover palabras comunes
            var stopWords = new HashSet<string> 
            { 
                "el", "la", "los", "las", "un", "una", "de", "en", "con", "por", "para", 
                "que", "se", "es", "son", "tiene", "tienen", "me", "te", "le", "nos",
                "and", "or", "the", "a", "an", "in", "on", "at", "to", "for", "of", "with"
            };
            
            var words = query.Split(new[] { ' ', ',', '.', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(w => w.Length > 2 && !stopWords.Contains(w.ToLower()))
                             .Select(w => w.ToLower())
                             .ToList();
                             
            return words;
        }

        /// <summary>
        /// Obtiene tablas usando heurísticas cuando no hay coincidencias directas
        /// </summary>
        private List<TableSchema> GetTablesByHeuristics(string query, List<TableSchema> allTables)
        {
            var result = new List<TableSchema>();
            
            // Heurística 1: Tablas con más columnas (principales)
            result.AddRange(allTables.OrderByDescending(t => t.Columns.Count).Take(3));
            
            // Heurística 2: Tablas con claves foráneas (relacionales)
            result.AddRange(allTables.Where(t => t.ForeignKeys.Any()).Take(3));
            
            return result.Distinct().ToList();
        }

        /// <summary>
        /// Obtiene tablas relacionadas por claves foráneas
        /// </summary>
        private List<TableSchema> GetRelatedTables(List<TableSchema> baseTables, List<TableSchema> allTables)
        {
            var relatedTables = new List<TableSchema>();
            
            foreach (var baseTable in baseTables)
            {
                // Tablas que referencian a esta tabla
                var referencingTables = allTables.Where(t => 
                    t.ForeignKeys.Any(fk => fk.ReferencedTable.Equals(baseTable.TableName, StringComparison.OrdinalIgnoreCase)))
                    .Take(2);
                    
                relatedTables.AddRange(referencingTables);
                
                // Tablas referenciadas por esta tabla
                var referencedTableNames = baseTable.ForeignKeys.Select(fk => fk.ReferencedTable).Distinct();
                var referencedTables = allTables.Where(t => 
                    referencedTableNames.Contains(t.TableName, StringComparer.OrdinalIgnoreCase))
                    .Take(2);
                    
                relatedTables.AddRange(referencedTables);
            }
            
            return relatedTables.Where(t => !baseTables.Contains(t)).ToList();
        }

        /// <summary>
        /// Añade contexto de tablas relevantes con información detallada
        /// </summary>
        private void AddRelevantTablesContext(StringBuilder sb, List<TableSchema> relevantTables, DatabaseType dbType)
        {
            sb.AppendLine("RELEVANT TABLES WITH RELATIONSHIPS:");
            sb.AppendLine();

            foreach (var table in relevantTables)
            {
                sb.AppendLine($"TABLE: {table.TableName}");
                
                // Propósito inferido de la tabla
                var purpose = InferTablePurpose(table);
                if (!string.IsNullOrEmpty(purpose))
                {
                    sb.AppendLine($"  Purpose: {purpose}");
                }
                
                sb.AppendLine("  Columns:");
                
                // Agrupar columnas por tipo
                var pkColumns = table.Columns.Where(c => table.PrimaryKeys.Contains(c.ColumnName)).ToList();
                var fkColumns = table.Columns.Where(c => table.ForeignKeys.Any(fk => fk.ColumnName == c.ColumnName)).ToList();
                var regularColumns = table.Columns.Except(pkColumns).Except(fkColumns).ToList();
                
                // Mostrar PKs primero
                foreach (var col in pkColumns)
                {
                    sb.AppendLine($"    🔑 {col.ColumnName} ({col.DataType}) [PRIMARY KEY]");
                }
                
                // Luego FKs
                foreach (var col in fkColumns)
                {
                    var fk = table.ForeignKeys.First(f => f.ColumnName == col.ColumnName);
                    sb.AppendLine($"    🔗 {col.ColumnName} ({col.DataType}) → {fk.ReferencedTable}.{fk.ReferencedColumn}");
                }
                
                // Finalmente columnas regulares (máximo 10)
                foreach (var col in regularColumns.Take(10))
                {
                    var nullable = col.IsNullable ? " [NULL]" : " [NOT NULL]";
                    sb.AppendLine($"    📊 {col.ColumnName} ({col.DataType}){nullable}");
                }
                
                if (regularColumns.Count > 10)
                {
                    sb.AppendLine($"    ... y {regularColumns.Count - 10} columnas más");
                }
                
                // Mostrar índices si es relevante
                if (table.Columns.Count > 10)
                {
                    sb.AppendLine("  💡 Tip: Esta tabla tiene muchas columnas, usa solo las necesarias");
                }
                
                sb.AppendLine();
            }
        }

        /// <summary>
        /// Infiere el propósito de una tabla basado en su estructura
        /// </summary>
        private string InferTablePurpose(TableSchema table)
        {
            var tableName = table.TableName.ToLower();
            var columnNames = table.Columns.Select(c => c.ColumnName.ToLower()).ToList();
            
            // Patrones comunes
            if (tableName.Contains("audit") || tableName.Contains("log"))
                return "Tabla de auditoría/registro";
            if (tableName.Contains("config") || tableName.Contains("setting"))
                return "Configuración del sistema";
            if (table.ForeignKeys.Count >= 2 && table.Columns.Count <= 5)
                return "Tabla de relación muchos-a-muchos";
            if (columnNames.Any(c => c.Contains("fecha") || c.Contains("date")) && 
                columnNames.Any(c => c.Contains("monto") || c.Contains("amount")))
                return "Tabla transaccional";
            if (columnNames.Any(c => c.Contains("nombre") || c.Contains("name")) && 
                table.PrimaryKeys.Any())
                return "Tabla maestra/catálogo";
                
            return "";
        }

        /// <summary>
        /// Añade ejemplos específicos basados en el tipo de consulta
        /// </summary>
        private void AddQueryTypeExamples(StringBuilder sb, string query, DatabaseType dbType)
        {
            var queryType = ClassifyQueryType(query);
            
            sb.AppendLine($"QUERY TYPE: {queryType}");
            sb.AppendLine();
            
            switch (queryType)
            {
                case QueryType.Count:
                    AddCountExamples(sb, dbType);
                    break;
                case QueryType.Aggregation:
                    AddAggregationExamples(sb, dbType);
                    break;
                case QueryType.Filtering:
                    AddFilteringExamples(sb, dbType);
                    break;
                case QueryType.Join:
                    AddJoinExamples(sb, dbType);
                    break;
                case QueryType.Ranking:
                    AddRankingExamples(sb, dbType);
                    break;
                default:
                    AddGeneralExamples(sb, dbType);
                    break;
            }
            
            sb.AppendLine();
        }

        /// <summary>
        /// Clasifica el tipo de consulta basado en palabras clave
        /// </summary>
        private QueryType ClassifyQueryType(string query)
        {
            var queryLower = query.ToLower();
            
            if (queryLower.Contains("cuantos") || queryLower.Contains("cantidad") || queryLower.Contains("total de") || 
                queryLower.Contains("count") || queryLower.Contains("número de"))
                return QueryType.Count;
                
            if (queryLower.Contains("suma") || queryLower.Contains("promedio") || queryLower.Contains("máximo") || 
                queryLower.Contains("mínimo") || queryLower.Contains("sum") || queryLower.Contains("avg") || 
                queryLower.Contains("max") || queryLower.Contains("min"))
                return QueryType.Aggregation;
                
            if (queryLower.Contains("donde") || queryLower.Contains("que tengan") || queryLower.Contains("con") || 
                queryLower.Contains("where") || queryLower.Contains("filter"))
                return QueryType.Filtering;
                
            if (queryLower.Contains("y") || queryLower.Contains("con") || queryLower.Contains("relacionados") || 
                queryLower.Contains("join") || queryLower.Contains("junto"))
                return QueryType.Join;
                
            if (queryLower.Contains("mejor") || queryLower.Contains("peor") || queryLower.Contains("top") || 
                queryLower.Contains("mayor") || queryLower.Contains("menor") || queryLower.Contains("primeros"))
                return QueryType.Ranking;
                
            return QueryType.General;
        }

        private void AddCountExamples(StringBuilder sb, DatabaseType dbType)
        {
            sb.AppendLine("COUNT QUERY EXAMPLES:");
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    sb.AppendLine("SELECT COUNT(*) FROM [tabla]");
                    sb.AppendLine("SELECT COUNT(DISTINCT columna) FROM [tabla]");
                    break;
                default:
                    sb.AppendLine("SELECT COUNT(*) FROM tabla");
                    sb.AppendLine("SELECT COUNT(DISTINCT columna) FROM tabla");
                    break;
            }
        }

        private void AddAggregationExamples(StringBuilder sb, DatabaseType dbType)
        {
            sb.AppendLine("AGGREGATION QUERY EXAMPLES:");
            sb.AppendLine("SELECT SUM(columna), AVG(columna), MAX(columna), MIN(columna) FROM tabla");
            sb.AppendLine("SELECT columna_grupo, COUNT(*), SUM(valor) FROM tabla GROUP BY columna_grupo");
        }

        private void AddFilteringExamples(StringBuilder sb, DatabaseType dbType)
        {
            sb.AppendLine("FILTERING QUERY EXAMPLES:");
            sb.AppendLine("SELECT * FROM tabla WHERE columna = 'valor'");
            sb.AppendLine("SELECT * FROM tabla WHERE fecha >= '2024-01-01'");
            sb.AppendLine("SELECT * FROM tabla WHERE columna IN ('val1', 'val2')");
        }

        private void AddJoinExamples(StringBuilder sb, DatabaseType dbType)
        {
            sb.AppendLine("JOIN QUERY EXAMPLES:");
            sb.AppendLine("SELECT t1.col1, t2.col2 FROM tabla1 t1 INNER JOIN tabla2 t2 ON t1.id = t2.tabla1_id");
            sb.AppendLine("SELECT t1.*, t2.* FROM tabla1 t1 LEFT JOIN tabla2 t2 ON t1.id = t2.fk_id");
        }

        private void AddRankingExamples(StringBuilder sb, DatabaseType dbType)
        {
            sb.AppendLine("RANKING QUERY EXAMPLES:");
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    sb.AppendLine("SELECT TOP 10 * FROM tabla ORDER BY columna DESC");
                    break;
                case DatabaseType.PostgreSQL:
                case DatabaseType.MySQL:
                    sb.AppendLine("SELECT * FROM tabla ORDER BY columna DESC LIMIT 10");
                    break;
            }
        }

        private void AddGeneralExamples(StringBuilder sb, DatabaseType dbType)
        {
            sb.AppendLine("GENERAL QUERY PATTERNS:");
            sb.AppendLine("SELECT columns FROM table WHERE condition ORDER BY column");
            sb.AppendLine("Use proper aliases and avoid SELECT *");
        }

        /// <summary>
        /// Añade la consulta del usuario con análisis
        /// </summary>
        private void AddUserQuery(StringBuilder sb, string naturalLanguageQuery)
        {
            sb.AppendLine("USER QUESTION ANALYSIS:");
            sb.AppendLine($"Original: {naturalLanguageQuery}");
            
            var keywords = ExtractKeywords(naturalLanguageQuery.ToLower());
            if (keywords.Any())
            {
                sb.AppendLine($"Key terms: {string.Join(", ", keywords.Take(10))}");
            }
            
            sb.AppendLine();
            sb.AppendLine("GENERATE OPTIMIZED SQL QUERY:");
        }

        /// <summary>
        /// Añade instrucciones finales específicas
        /// </summary>
        private void AddFinalInstructions(StringBuilder sb, DatabaseType dbType)
        {
            sb.AppendLine();
            sb.AppendLine("FINAL REQUIREMENTS:");
            sb.AppendLine("✓ Use only tables and columns from the schema above");
            sb.AppendLine("✓ Return a single, executable SQL statement");
            sb.AppendLine("✓ No explanations, comments, or markdown formatting");
            sb.AppendLine("✓ Ensure proper syntax for " + dbType.ToString());
            sb.AppendLine("✓ Use meaningful aliases for better readability");
            sb.AppendLine("✓ Include proper error handling considerations");
        }

        /// <summary>
        /// Tipos de consulta para clasificación
        /// </summary>
        private enum QueryType
        {
            General,
            Count,
            Aggregation,
            Filtering,
            Join,
            Ranking
        }
    }
}