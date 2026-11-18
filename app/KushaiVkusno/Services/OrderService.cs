using KushaiVkusno.Data;
using KushaiVkusno.Models;
using MySqlConnector;

namespace KushaiVkusno.Services;

internal static class OrderService
{
    public static List<OrderStatus> GetStatuses()
    {
        var result = new List<OrderStatus>();
        using var connection = Database.Open();
        using var command = new MySqlCommand("SELECT id_status, name FROM order_status ORDER BY id_status", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new OrderStatus(reader.GetInt32(0), reader.GetString(1)));
        }

        return result;
    }

    public static List<OrderSummary> GetAll()
    {
        const string sql = """
            SELECT id_order, order_date, order_time, table_number, employee_name,
                   status_name, position_count, items_count, total_cost, comment
            FROM v_order_summary
            ORDER BY id_order
            """;

        var result = new List<OrderSummary>();
        using var connection = Database.Open();
        using var command = new MySqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var time = reader.GetTimeSpan(2);
            result.Add(new OrderSummary(
                reader.GetInt32(0),
                reader.GetDateTime(1),
                time,
                reader.GetInt32(3),
                reader.GetString(4),
                reader.GetString(5),
                Convert.ToInt32(reader.GetValue(6)),
                Convert.ToInt32(reader.GetValue(7)),
                reader.GetDecimal(8),
                reader.IsDBNull(9) ? null : reader.GetString(9)));
        }

        return result;
    }

    public static List<OrderLine> GetLines(int orderId)
    {
        const string sql = """
            SELECT id_order, dish_name, quantity, price, line_cost
            FROM v_order_item_detail
            WHERE id_order = @id
            ORDER BY id_order_item
            """;

        var result = new List<OrderLine>();
        using var connection = Database.Open();
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", orderId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new OrderLine(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetDecimal(3),
                reader.GetDecimal(4)));
        }

        return result;
    }

    public static int GetDefaultStatusId()
    {
        using var connection = Database.Open();
        using var command = new MySqlCommand("SELECT id_status FROM order_status WHERE name = 'Новый' LIMIT 1", connection);
        var value = command.ExecuteScalar();
        if (value is null)
        {
            throw new InvalidOperationException("В справочнике статусов отсутствует статус «Новый».");
        }

        return Convert.ToInt32(value);
    }

    /// <summary>
    /// Оформление заказа: номер формируется автоматически (+1 к максимальному в БД),
    /// дата и время подставляются в момент сохранения.
    /// </summary>
    public static int Create(int employeeId, int tableNumber, string? comment,
        IEnumerable<(int DishId, int Quantity)> items)
    {
        var positions = items.Where(i => i.Quantity > 0).ToList();
        if (positions.Count == 0)
        {
            throw new InvalidOperationException("В заказе должна быть хотя бы одна позиция с количеством больше нуля.");
        }

        using var connection = Database.Open();
        using var transaction = connection.BeginTransaction();

        var orderId = NextOrderId(connection, transaction);
        var statusId = GetDefaultStatusId(connection, transaction);
        var now = DateTime.Now;

        using (var command = new MySqlCommand("""
                   INSERT INTO orders (id_order, id_employee, order_date, order_time, table_number, comment, id_status)
                   VALUES (@id, @employee, @date, @time, @table, @comment, @status)
                   """, connection, transaction))
        {
            command.Parameters.AddWithValue("@id", orderId);
            command.Parameters.AddWithValue("@employee", employeeId);
            command.Parameters.AddWithValue("@date", now.Date);
            command.Parameters.AddWithValue("@time", now.TimeOfDay);
            command.Parameters.AddWithValue("@table", tableNumber);
            command.Parameters.AddWithValue("@comment", (object?)comment ?? DBNull.Value);
            command.Parameters.AddWithValue("@status", statusId);
            command.ExecuteNonQuery();
        }

        foreach (var (dishId, quantity) in positions)
        {
            using var command = new MySqlCommand(
                "INSERT INTO order_item (id_order, id_dish, quantity) VALUES (@order, @dish, @qty)",
                connection, transaction);
            command.Parameters.AddWithValue("@order", orderId);
            command.Parameters.AddWithValue("@dish", dishId);
            command.Parameters.AddWithValue("@qty", quantity);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
        return orderId;
    }

    private static int NextOrderId(MySqlConnection connection, MySqlTransaction transaction)
    {
        using var command = new MySqlCommand("SELECT COALESCE(MAX(id_order), 0) + 1 FROM orders FOR UPDATE",
            connection, transaction);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static int GetDefaultStatusId(MySqlConnection connection, MySqlTransaction transaction)
    {
        using var command = new MySqlCommand(
            "SELECT id_status FROM order_status WHERE name = 'Новый' LIMIT 1", connection, transaction);
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
