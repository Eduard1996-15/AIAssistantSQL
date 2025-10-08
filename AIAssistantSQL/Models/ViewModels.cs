namespace AIAssistantSQL.Models
{
    /// <summary>
    /// ViewModel para la vista de configuración de base de datos
    /// </summary>
    public class DatabaseConfigViewModel
    {
        public DatabaseType DatabaseType { get; set; }
        public string? ConnectionString { get; set; }
        public string? SchemaFilePath { get; set; }
        public bool IsSchemaLoaded { get; set; }
        public string? LoadedDatabaseName { get; set; }
        public DateTime? LoadedAt { get; set; }
        public int TableCount { get; set; }
        public bool IsConnected { get; set; }
        public string? CurrentConnectionString { get; set; }
    }

    /// <summary>
    /// ViewModel para la vista de consultas
    /// </summary>
    public class QueryViewModel
    {
        public string NaturalLanguageQuery { get; set; } = string.Empty;
        public QueryResponse? LastResponse { get; set; }
        public List<QueryHistory> History { get; set; } = new();
        public bool HasActiveConnection { get; set; }
        public string? DatabaseName { get; set; }
    }
}
