using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Security;
using System.Xml;

namespace DynamicFormBuilder.API.Data.Helpers
{
    public class SqlHelper
    {
        private string _connectionString;//Veritabanı bağlantısını tutmak için bi field tanımladık.

        public SqlHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection","Appsettings.json içerisinde bağlantı dizeciği bulunamadı!");//Eğer DefaultConnectionum boşsa veya yoksa hata fırlattık.    
        }
        //Sql injection için regex ve boş identifier kontrolleri.
        private string SanitizeIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException("identifier","Bu tanımlayıcı null veya boş.");
            }
            if (!Regex.IsMatch(identifier,"^[a-zA-Z0-9_]+$"))
            {
                throw new SecurityException("Bu tanımlayıcı kabul edilemez.");
            }
            return $"[{identifier}]";
        }
        //Tablo döndürmeyen sorgularımızı çalıştırmak için bu metodu kullanıyoruz ve etkilenen row sayısını return ediyoruz.
        public async Task<int> ExecuteNonQueryAsync(string query,params SqlParameter[] parameters) {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("query","Boş query gönderildi.");
            }
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query,connection);
            if (parameters!=null && parameters.Length != 0)
            {
                command.Parameters.AddRange(parameters);
            }
            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }
        //Viewden veri okumak gibi tablo döndüren sorgularımızı çalıştırmak için bu metodu kullanıyoruz ve tabloyu return ediyoruz.
        public async Task<DataTable> ExecuteDataTableAsync(string query,params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("query","Boş query gönderildi");
            }
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query,connection);
            if (parameters!=null && parameters.Length != 0)
            {
                command.Parameters.AddRange(parameters);
            }
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            DataTable dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }
        //Dinamik olarak gelen json verisine göre veritabanındaki tabloya veri yazacak olan metodumuz.
        public async Task<int> InsertRecordFromJson(string tableName,Dictionary<String,object> formData)
        {
            if (formData==null || formData.Count == 0)
            {
                throw new ArgumentNullException("formdata","Form verisi boş.");
            }
            string checkedTableName = SanitizeIdentifier(tableName); //Regex ve boş kontrolünden geçirmek ve çakışma önlemek için([]) SanitizeIdentifierimizi kullandık.
            List<String> columnNames = [];//Tablodaki attribute namelerimizi (user_name...) tutmak için bi List oluşturuyoruz.
            List<String> parameterNames = [];//Parametre isimlerimizi (@user_name) tutmak için bir List oluşturuyoruz.
            List<SqlParameter> parameters = [];//ExecuteNonQueryAsync metodumuz Sql parametreleri aldığı için onları da bu listede tutuyoruz.
            formData.ToList().ForEach((item) =>
            {
                string checkedColumnName = SanitizeIdentifier(item.Key);//Aynı şekilde keylerimizi de regex kontrolünden geçiriyoruz.
                columnNames.Add(checkedColumnName);
                string parameterName = "@"+item.Key;
                parameterNames.Add(parameterName);
                object parameterValue = item.Value ?? DBNull.Value;
                parameters.Add(new SqlParameter(parameterName,parameterValue));
            });
            string columnString = string.Join(",",columnNames);//Aralarda , olacak şekilde attribute namelerimizi joinliyoruz.
            string parametersString = string.Join(",",parameterNames);//Aynı şekilde parametrelerimizi de joinliyoruz.
            string query = $"INSERT INTO {checkedTableName} ({columnString}) VALUES ({parametersString})";//Joinlediğimiz stringlerle query oluşturuyoruz
            return await ExecuteNonQueryAsync(query,parameters.ToArray());//params bizden array istediği için arraye dönüştürdük.
        }
        public async Task<int> UpdateRecordFromJson(string tableName,UniqueId pk_id,string targetPK,Dictionary<String,object> formData)
        {
            if (formData==null || formData.Count == 0)
            {
                throw new ArgumentNullException("formdata","Form verisi boş.");
            }
            string checkedTableName = SanitizeIdentifier(tableName);
            string checkedTargetPK = SanitizeIdentifier(targetPK); //Regex ve boş kontrolünden geçirmek ve çakışma önlemek için([]) SanitizeIdentifierimizi kullandık.
            List<String> setClauses = [];
            List<SqlParameter> parameters = [];//ExecuteNonQueryAsync metodumuz Sql parametreleri aldığı için onları da bu listede tutuyoruz.
            formData.ToList().ForEach((item) =>
            {
                string checkedColumnName = SanitizeIdentifier(item.Key);//Aynı şekilde keylerimizi de regex kontrolünden geçiriyoruz.
                string parameterName = $"@{item.Key}";
                setClauses.Add($"{checkedColumnName}={parameterName}");
                object parameterValue = item.Value ?? DBNull.Value;
                parameters.Add(new SqlParameter(parameterName,parameterValue));
            });
            string setClausesString = string.Join(",",setClauses);
            string targetPrimaryKey=$"@{targetPK}";
            string query = $"UPDATE {checkedTableName} SET {setClausesString} WHERE {targetPK} = {pk_id} ";//Joinlediğimiz stringlerle query oluşturuyoruz
            return await ExecuteNonQueryAsync(query,parameters.ToArray());//params bizden array istediği için arraye dönüştürdük.
        }
    }
}