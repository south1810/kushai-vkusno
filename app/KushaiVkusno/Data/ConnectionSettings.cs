using System.Text.Json;

namespace KushaiVkusno.Data;

/// <summary>
/// Параметры подключения к базе данных. Читаются из файла connection.json,
/// который лежит рядом с исполняемым файлом и не хранится в репозитории
/// (пример настроек — connection.sample.json).
/// </summary>
internal sealed class ConnectionSettings
{
    public string Server { get; init; } = "localhost";
    public uint Port { get; init; } = 3306;
    public string Database { get; init; } = "isp-45_dolgih_ka_dolgih_schema2";
    public string User { get; init; } = "root";
    public string Password { get; init; } = string.Empty;

    public const string FileName = "connection.json";

    public static ConnectionSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Не найден файл настроек подключения «{FileName}». " +
                "Скопируйте connection.sample.json в connection.json и укажите параметры сервера БД.",
                path);
        }

        var json = File.ReadAllText(path);
        var settings = JsonSerializer.Deserialize<ConnectionSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (settings is null || string.IsNullOrWhiteSpace(settings.Server))
        {
            throw new InvalidOperationException($"Файл «{FileName}» заполнен некорректно.");
        }

        return settings;
    }
}
