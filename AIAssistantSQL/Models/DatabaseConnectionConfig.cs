namespace AIAssistantSQL.Models
{
    /// <summary>
    /// Modelo para configurar la conexi�n a la base de datos
    /// </summary>
    public class DatabaseConnectionConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public DatabaseType DatabaseType { get; set; }
        public string DatabaseName { get; set; } = string.Empty;
    }
}
