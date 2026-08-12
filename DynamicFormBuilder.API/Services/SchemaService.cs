using System.Text.Json;
using Microsoft.Data.SqlClient;
using DynamicFormBuilder.API.Exceptions;

namespace DynamicFormBuilder.API.Services
{
    public record ColumnDetails(string ColumnName, string DataType, short MaxLength, bool IsNullable);
    public class FormSchemaValidator : ISchemaService
    {
        private readonly string _connectionString;
        private static readonly Dictionary<string, HashSet<string>> FormIoToAllowedSqlTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            // ===== BASIC =====
            { "textfield",    new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar", "char", "nchar", "text", "ntext" } },
            { "textarea",     new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar", "text", "ntext" } },
            { "number",       new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "int", "bigint", "smallint", "tinyint", "float", "real", "decimal", "numeric", "money", "smallmoney" } },
            { "password",     new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "checkbox",     new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bit" } },
            { "selectboxes",  new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "select",       new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar", "char", "nchar", "int", "bigint", "smallint", "tinyint" } },
            { "radio",        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar", "char", "nchar", "int", "bigint", "smallint", "tinyint" } },

            // ===== ADVANCED =====
            { "email",        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "url",          new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "phoneNumber",  new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar", "char" } },
            { "tags",         new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "address",      new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "datetime",     new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "datetime", "datetime2", "date", "smalldatetime", "datetimeoffset", "nvarchar", "varchar" } },
            { "day",          new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar", "date" } },
            { "time",         new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "time", "nvarchar", "varchar" } },
            { "currency",     new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "decimal", "numeric", "money", "smallmoney", "float", "real" } },
            { "survey",       new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "signature",    new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },

            // ===== DATA =====
            { "hidden",       new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar", "char", "nchar", "int", "bigint", "smallint", "tinyint", "uniqueidentifier", "bit", "float", "real", "decimal", "numeric" } },
            { "container",    new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "datamap",      new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },

            // ===== PREMIUM =====
            { "file",         new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar" } },
            { "custom",       new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nvarchar", "varchar", "int", "bigint", "bit", "float", "decimal", "numeric" } }
        };
        private static readonly HashSet<string> BannedComponentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "datagrid",
            "editgrid",
            "form"
        };
        private static readonly HashSet<string> LayoutComponentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "button", "htmlelement", "content", "columns", "fieldset", "panel", "table", "tabs", "well"
        };
        private static readonly HashSet<string> StringSqlTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "nvarchar", "varchar", "char", "nchar"
        };
        private static readonly HashSet<string> DoubleByteSqlTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "nvarchar", "nchar"
        };

        public FormSchemaValidator(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("DefaultConnection", "Appsettings.json içerisinde bağlantı dizeciği bulunamadı!");
        }

        // ==================== PUBLIC METOTLAR ====================

        // Publish sırasında tüm kontrolleri sırayla çalıştıran orkestratör metot.
        public async Task<bool> ValidatePublishRequirementsAsync(string tableName, string primaryKey, string? viewName, string formSchema)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(primaryKey))
                throw new BadRequestException("Tablo adı ve Primary Key boş olamaz.");

            if (string.IsNullOrWhiteSpace(formSchema))
                throw new SchemaValidationException("Form şeması (JSON) boş olamaz.");

            await ValidateTableAndPrimaryKeyAsync(tableName, primaryKey);

            if (!string.IsNullOrWhiteSpace(viewName))
                await ValidateViewExistsAsync(viewName);

            await ValidateSchemaCompatibilityAsync(tableName, formSchema);

            return true;
        }

        // Tablo ve Primary Key varlık kontrolü. Bağımsız olarak çağrılabilir.
        public async Task ValidateTableAndPrimaryKeyAsync(string tableName, string primaryKey)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(primaryKey))
                throw new BadRequestException("Tablo adı ve Primary Key boş olamaz.");

            string query = @"
                SELECT 1 
                FROM sys.columns c
                INNER JOIN sys.tables t ON c.object_id = t.object_id
                WHERE t.name = @TableName AND c.name = @PrimaryKey";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@TableName", tableName));
            command.Parameters.Add(new SqlParameter("@PrimaryKey", primaryKey));

            if (await command.ExecuteScalarAsync() == null)
                throw new SchemaValidationException($"Validasyon Hatası: '{tableName}' tablosu veya içindeki '{primaryKey}' kolonu veritabanında bulunamadı.");
        }

        // View varlık kontrolü. ViewName güncellendiğinde bağımsız olarak çağrılabilir.
        public async Task ValidateViewExistsAsync(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                throw new BadRequestException("View adı boş olamaz.");

            string query = "SELECT 1 FROM sys.views WHERE name = @ViewName";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@ViewName", viewName));

            if (await command.ExecuteScalarAsync() == null)
                throw new SchemaValidationException($"Validasyon Hatası: '{viewName}' isimli View veritabanında bulunamadı.");
        }

        // Form şemasını veritabanı tablosuyla karşılaştıran ana doğrulama metodu.
        public async Task ValidateSchemaCompatibilityAsync(string tableName, string formSchema)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new BadRequestException("Tablo adı boş olamaz.");

            if (string.IsNullOrWhiteSpace(formSchema))
                throw new SchemaValidationException("Form şeması (JSON) boş olamaz.");

            // JSON Parse
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(formSchema);
            }
            catch (JsonException ex)
            {
                throw new SchemaValidationException($"Form şeması geçerli bir JSON değil: {ex.Message}");
            }

            using (document)
            {
                var root = document.RootElement;

                if (!root.TryGetProperty("components", out var components) || components.ValueKind != JsonValueKind.Array)
                {
                    throw new SchemaValidationException("Form JSON formatı geçersiz: 'components' dizisi bulunamadı.");
                }

                var allInputComponents = new List<JsonElement>();
                ExtractInputComponents(components, allInputComponents);

                // Veritabanı şemasını çek
                var dbColumns = await GetTableSchemaAsync(tableName);

                // Her bir form bileşenini veritabanı kolonlarıyla karşılaştır
                foreach (var component in allInputComponents)
                {
                    string? key = component.TryGetProperty("key", out var keyProp) ? keyProp.GetString() : null;
                    if (string.IsNullOrWhiteSpace(key))
                        throw new SchemaValidationException("Form içinde 'key' değeri boş veya tanımsız olan bir alan tespit edildi.");

                    string? type = component.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
                    if (string.IsNullOrWhiteSpace(type))
                        throw new SchemaValidationException($"'{key}' alanı için 'type' değeri boş veya tanımsız.");

                    ValidateColumnExists(key, tableName, dbColumns);
                    var dbCol = dbColumns[key];

                    ValidateTypeCompatibility(key, type, dbCol);
                    ValidateNullability(key, component, dbCol);
                    ValidateMaxLength(key, component, dbCol);
                }
            }
        }

        // ==================== PRİVATE YARDIMCI METOTLAR ====================

        // Belirtilen tablonun tüm kolon bilgilerini veritabanından çeker.
        private async Task<Dictionary<string, ColumnDetails>> GetTableSchemaAsync(string tableName)
        {
            string query = @"
                SELECT 
                    c.name AS ColumnName,
                    ty.name AS DataType,
                    c.max_length AS MaxLength,
                    c.is_nullable AS IsNullable
                FROM sys.columns c
                INNER JOIN sys.tables t ON c.object_id = t.object_id
                INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
                WHERE t.name = @TableName";

            var dbColumns = new Dictionary<string, ColumnDetails>(StringComparer.OrdinalIgnoreCase);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@TableName", tableName));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var colName = reader.GetString(0);
                dbColumns[colName] = new ColumnDetails(
                    colName,
                    reader.GetString(1),
                    reader.GetInt16(2),
                    reader.GetBoolean(3)
                );
            }

            return dbColumns;
        }

        // Formdaki alanın veritabanı tablosunda karşılığı var mı kontrol eder.
        private void ValidateColumnExists(string key, string tableName, Dictionary<string, ColumnDetails> dbColumns)
        {
            if (!dbColumns.ContainsKey(key))
                throw new SchemaValidationException($"Eşleşme Hatası: Formdaki '{key}' alanı '{tableName}' tablosunda bulunamadı.");
        }

        // Form.io bileşen tipi ile SQL veri tipinin uyumluluğunu kontrol eder.
        private void ValidateTypeCompatibility(string key, string type, ColumnDetails dbCol)
        {
            if (!FormIoToAllowedSqlTypes.TryGetValue(type, out var allowedSqlTypes))
            {
                throw new SchemaValidationException(
                    $"Desteklenmeyen bileşen tipi: '{type}' (alan: '{key}'). Bu bileşen tipi için SQL veri tipi eşlemesi tanımlı değil, publish işlemi güvenlik gereği durduruldu.");
            }

            if (!allowedSqlTypes.Contains(dbCol.DataType))
                throw new SchemaValidationException($"Tip Uyuşmazlığı: '{key}' alanı formda '{type}' tipinde ancak veritabanında '{dbCol.DataType}' tanımlı. Kabul edilen SQL tipleri: {string.Join(", ", allowedSqlTypes)}");
        }

        // Veritabanında NOT NULL olan kolonun formda Required yapılıp yapılmadığını kontrol eder.
        private void ValidateNullability(string key, JsonElement component, ColumnDetails dbCol)
        {
            if (dbCol.IsNullable) return;

            bool isRequiredInForm = false;
            if (component.TryGetProperty("validate", out var validateProp) &&
                validateProp.TryGetProperty("required", out var reqProp) &&
                reqProp.ValueKind == JsonValueKind.True)
            {
                isRequiredInForm = true;
            }

            if (!isRequiredInForm)
                throw new SchemaValidationException($"Kısıtlama Hatası: '{key}' alanı veritabanında zorunlu (NOT NULL) ancak form üzerinde Required yapılmamış.");
        }

        // String tipindeki kolonlarda MaxLength uyumluluğunu kontrol eder.
        private void ValidateMaxLength(string key, JsonElement component, ColumnDetails dbCol)
        {
            if (!StringSqlTypes.Contains(dbCol.DataType) || dbCol.MaxLength == -1) return;

            int actualDbMaxLength = DoubleByteSqlTypes.Contains(dbCol.DataType) ? dbCol.MaxLength / 2 : dbCol.MaxLength;

            if (component.TryGetProperty("validate", out var validateProp) &&
                validateProp.TryGetProperty("maxLength", out var maxLenProp) &&
                maxLenProp.TryGetInt32(out int formMaxLength))
            {
                if (formMaxLength > actualDbMaxLength)
                    throw new SchemaValidationException($"Boyut Hatası: '{key}' form alanı için max_length ({formMaxLength}), veritabanı sınırını ({actualDbMaxLength}) aşıyor.");
            }
            else
            {
                throw new SchemaValidationException($"Güvenlik Riski: '{key}' alanı veritabanında maksimum {actualDbMaxLength} karakterle sınırlı, form tasarımında da 'maxLength' sınırı belirtilmek zorunda.");
            }
        }

        // Form.io JSON ağacını recursive olarak dolaşır ve input olan bileşenleri düz bir listeye toplar.
        private void ExtractInputComponents(JsonElement components, List<JsonElement> inputList)
        {
            foreach (var component in components.EnumerateArray())
            {
                string type = component.TryGetProperty("type", out var typeProp) ? (typeProp.GetString() ?? "") : "";

                if (BannedComponentTypes.Contains(type))
                {
                    throw new SchemaValidationException($"Yasaklı Bileşen: Formda '{type}' tipinde bir bileşen tespit edildi. DataGrid, EditGrid ve Nested Form kullanılamaz.");
                }

                bool isLayout = LayoutComponentTypes.Contains(type);

                if (!isLayout &&
                    component.TryGetProperty("input", out var inputProp) &&
                    inputProp.ValueKind == JsonValueKind.True &&
                    component.TryGetProperty("key", out _))
                {
                    inputList.Add(component);
                }

                if (!type.Equals("container", StringComparison.OrdinalIgnoreCase) &&
                    component.TryGetProperty("components", out var childComponents) &&
                    childComponents.ValueKind == JsonValueKind.Array)
                {
                    ExtractInputComponents(childComponents, inputList);
                }

                if (type.Equals("columns", StringComparison.OrdinalIgnoreCase) &&
                    component.TryGetProperty("columns", out var cols) &&
                    cols.ValueKind == JsonValueKind.Array)
                {
                    foreach (var col in cols.EnumerateArray())
                    {
                        if (col.TryGetProperty("components", out var colChildComponents) && colChildComponents.ValueKind == JsonValueKind.Array)
                        {
                            ExtractInputComponents(colChildComponents, inputList);
                        }
                    }
                }

                if (type.Equals("table", StringComparison.OrdinalIgnoreCase) &&
                    component.TryGetProperty("rows", out var rows) &&
                    rows.ValueKind == JsonValueKind.Array)
                {
                    foreach (var row in rows.EnumerateArray())
                    {
                        if (row.ValueKind != JsonValueKind.Array) continue;

                        foreach (var cell in row.EnumerateArray())
                        {
                            if (cell.TryGetProperty("components", out var cellComponents) && cellComponents.ValueKind == JsonValueKind.Array)
                            {
                                ExtractInputComponents(cellComponents, inputList);
                            }
                        }
                    }
                }
            }
        }
    }
}