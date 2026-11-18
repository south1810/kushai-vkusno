using KushaiVkusno.Data;
using KushaiVkusno.Models;
using MySqlConnector;

namespace KushaiVkusno.Services;

internal static class AuthService
{
    /// <summary>Проверка логина и пароля. Возвращает сотрудника либо null.</summary>
    public static Employee? Authenticate(string login, string password)
    {
        const string sql = """
            SELECT e.id_employee, e.full_name, e.login, r.id_role, r.name
            FROM employee e
                     JOIN role r ON r.id_role = e.id_role
            WHERE e.login = @login AND e.password = @password
            LIMIT 1
            """;

        using var connection = Database.Open();
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@login", login.Trim());
        command.Parameters.AddWithValue("@password", password);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Employee(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetString(4));
    }
}
