using AIAssistantSQL.Models;

namespace AIAssistantSQL.Interfaces
{
    /// <summary>
    /// Interfaz para cargar esquemas de base de datos
    /// </summary>
    public interface ISchemaLoaderService
    {
        /// <summary>
        /// Carga el esquema desde un archivo JSON
        /// </summary>
        Task<DatabaseSchema> LoadSchemaFromFileAsync(string filePath);

        /// <summary>
        /// Carga el esquema desde una conexión a base de datos
        /// </summary>
        Task<DatabaseSchema> LoadSchemaFromConnectionStringAsync(string connectionString, DatabaseType databaseType);

        /// <summary>
        /// Guarda el esquema en un archivo JSON
        /// </summary>
        Task SaveSchemaToFileAsync(DatabaseSchema schema, string filePath);

        /// <summary>
        /// Obtiene el esquema actualmente cargado
        /// </summary>
        DatabaseSchema? GetCurrentSchema();

        /// <summary>
        /// Establece el esquema actual
        /// </summary>
        void SetCurrentSchema(DatabaseSchema schema);
    }
}
