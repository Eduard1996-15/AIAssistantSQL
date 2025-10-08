namespace AIAssistantSQL.Models
{
    /// <summary>
    /// Modelo que representa la estructura de una tabla
    /// </summary>
    public class TableSchema
    {
        public string TableName { get; set; } = string.Empty;
        public List<ColumnSchema> Columns { get; set; } = new();
        public List<string> PrimaryKeys { get; set; } = new();
        public List<ForeignKeySchema> ForeignKeys { get; set; } = new();
    }

    /// <summary>
    /// Modelo que representa una columna de una tabla
    /// </summary>
    public class ColumnSchema
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public bool IsIdentity { get; set; }
    }

    /// <summary>
    /// Modelo que representa una clave foránea
    /// </summary>
    public class ForeignKeySchema
    {
        public string ColumnName { get; set; } = string.Empty;
        public string ReferencedTable { get; set; } = string.Empty;
        public string ReferencedColumn { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo que representa el esquema completo de una base de datos
    /// </summary>
    public class DatabaseSchema
    {
        public string DatabaseName { get; set; } = string.Empty;
        public DatabaseType DatabaseType { get; set; }
        public List<TableSchema> Tables { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.Now;
    }
}
