/*Kayıt tabloları duruma göre oluşturulup sayısı artacağı için eğer onları EF Core DbContextiyle yönetmeye çalışırsak her yeni tabloda bir daha build
almak zorunda kalırız bundan kaçınmak için bir SqlHelper metodu yazdık bu tablo ismine ve istenen sütun isimlerine göre 
(Form şemasındaki json parselanarak elde edilecek) sorgu üreten bir SqlHelper yazdık*/

using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Security;

namespace DynamicFormBuilder.API.Data.Helpers
{
    public class SqlHelper
    {
        private readonly string _connectionString;//Veritabanı bağlantısını tutmak için bi field tanımladık.

        public SqlHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection","Appsettings.json içerisinde bağlantı dizeciği bulunamadı!");//Eğer DefaultConnectionum boşsa veya yoksa hata fırlattık.    
        }
        //Sql injectiondan korunmak için regex ve boş identifier kontrolleri.
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
            if (string.IsNullOrWhiteSpace(query))
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
            if (string.IsNullOrWhiteSpace(query))
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
        //Update yaparken formdaki ilgili alanlara verileri getirmemiz gerekecek.
        //Bu yüzden tek bir kayıt döndüren sorguları çalıştırması için yardımcı metod yazıyoruz bu metod bize kaydı bir dictionary olarak döndürecek.
        public async Task<Dictionary<string,object>?> ExecuteSingleRowAsync(string query,params SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(query))
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
            var dictionary = new Dictionary<string,object>();
            if (await reader.ReadAsync()){
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    object value = reader.GetValue(i);
                    dictionary[columnName] = value == DBNull.Value ? null : value;
                }
                return dictionary;
            }
            return null;
        }
        //Dinamik olarak gelen json verisine göre veritabanındaki tabloya veri yazacak olan metodumuz.
        public async Task<int> InsertRecordFromJson(string tableName,Dictionary<String,object> formData)
        {
            if (formData==null || formData.Count == 0)
            {
                throw new ArgumentNullException("formData","Form verisi boş.");
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
        //Kullanıcı recordlarda değişiklik yapmak istediğinde ilgili form sayfasına yönlendirilecek.Gerekli update işlemleri için metod yazıyoruz.
        //tableName ve targetPkName ilgili form şemasıyla birlikte databasedeki form tablosunda bulunacak ve bu parametreler ordan alınacak.
        //targetPKName kayıtların tutulacağı tablonun Primary Key kolonunun ismini tutar.
        public async Task<int> UpdateRecordFromJson(string tableName,string targetPKName,Guid id,Dictionary<String,object> formData)
        {
            if (formData==null || formData.Count == 0)
            {
                throw new ArgumentNullException("formdata","Form verisi boş.");
            }
            string checkedTableName = SanitizeIdentifier(tableName);
            string checkedPKName = SanitizeIdentifier(targetPKName);//Primary key ismini de regex kontrollerinden geçiriyoruz.
            List<String> setClauses = [];
            List<SqlParameter> parameters = [];
            formData.ToList().ForEach((item) =>
            {
                if (item.Key == targetPKName)
                {
                    return;
                }
                string checkedColumnName = SanitizeIdentifier(item.Key);
                string parameterName = $"@{item.Key}";
                setClauses.Add($"{checkedColumnName}={parameterName}");
                object parameterValue = item.Value ?? DBNull.Value;
                parameters.Add(new SqlParameter(parameterName,parameterValue));
            });
            string setClausesString = string.Join(",",setClauses);
            string pkParameterName=$"@{targetPKName}";
            parameters.Add(new SqlParameter(pkParameterName,id));
            string query = $"UPDATE {checkedTableName} SET {setClausesString} WHERE {checkedPKName} = {pkParameterName} AND is_deleted = 0";//Update için dinamik query.
            return await ExecuteNonQueryAsync(query,parameters.ToArray());
        }
        //Idye göre ilgili kaydı getiren metodumuz(güncelleme yapılırken formu doldurmak için kullanılacak).
        public async Task<Dictionary<string,object>?> GetRecordByIdAsync(string tableName,string targetPkName ,Guid id)
        {
            string checkedTableName = SanitizeIdentifier(tableName);
            string checkedPkName = SanitizeIdentifier(targetPkName);
            string pkparameterName =$"@{targetPkName}";
            SqlParameter parameter = new SqlParameter(pkparameterName,id);
            string query = $"SELECT * FROM {checkedTableName} WHERE {checkedPkName} = {pkparameterName} AND is_deleted = 0";//Tek kayıt döndüren querymiz.
            return await ExecuteSingleRowAsync(query,parameter);
        }
        //Bütün kayıtları listeleyecek olan metodumuz.
        public async Task<DataTable> GetAllRecordsAsync(string viewName)
        {
            string checkedTableName = SanitizeIdentifier(viewName);
            string query = $"SELECT * FROM {checkedTableName}";
            return await ExecuteDataTableAsync(query);
        }
    }
}