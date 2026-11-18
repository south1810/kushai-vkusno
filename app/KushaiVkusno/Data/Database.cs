using MySqlConnector;

namespace KushaiVkusno.Data;

/// <summary>
/// Параметры подключения к базе данных «КушайВкусно» и фабрика соединений.
/// Данные доступа берутся из файла connection.json (см. класс ConnectionSettings).
/// </summary>
internal static class Database
{
    private static readonly Lazy<string> LazyConnectionString = new(BuildConnectionString);

    private static string ConnectionString => LazyConnectionString.Value;

    private static string BuildConnectionString()
    {
        var settings = ConnectionSettings.Load();
        return new MySqlConnectionStringBuilder
        {
            Server = settings.Server,
            Port = settings.Port,
            Database = settings.Database,
            UserID = settings.User,
            Password = settings.Password,
            CharacterSet = "utf8mb4",
            // СУБД учебного стенда доступна по внутренней сети; TLS-согласование
            // на некоторых версиях Windows недоступно, поэтому соединение идёт
            // без SSL, а пароль передаётся через RSA-обмен.
            SslMode = MySqlSslMode.None,
            AllowPublicKeyRetrieval = true,
            ConnectionTimeout = 10,
            DefaultCommandTimeout = 30
        }.ConnectionString;
    }

    public static MySqlConnection Open()
    {
        var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    /// <summary>Проверка доступности сервера — используется на форме входа.</summary>
    public static void Test()
    {
        using var connection = Open();
        using var command = new MySqlCommand("SELECT 1", connection);
        command.ExecuteScalar();
    }
}
