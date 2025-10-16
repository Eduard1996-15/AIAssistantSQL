namespace AIAssistantSQL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de configuraci�n de base de datos
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
        public string? DatabaseType { get; set; }
        public List<string> TableNames { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para el selector de modelos de IA
    /// </summary>
    public class ModelSelectorViewModel
    {
        public string CurrentModel { get; set; } = string.Empty;
        public List<OllamaModel> AvailableModels { get; set; } = new();
        public bool IsOllamaAvailable { get; set; }
        public Dictionary<string, int> ModelUsageStats { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// ViewModel para configuración de IA
    /// </summary>
    public class AIConfigurationViewModel
    {
        public string CurrentModel { get; set; } = string.Empty;
        public List<OllamaModel> AvailableModels { get; set; } = new();
        public bool IsOllamaAvailable { get; set; }
        public string OllamaUrl { get; set; } = "http://localhost:11434";
        public int TimeoutSeconds { get; set; } = 60;
        public float Temperature { get; set; } = 0.3f;
        public int MaxTokens { get; set; } = 2048;
        public bool UseCache { get; set; } = true;
        public int CacheExpirationMinutes { get; set; } = 10;
    }
}
