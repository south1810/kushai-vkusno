using KushaiVkusno.Data;
using KushaiVkusno.Models;
using MySqlConnector;

namespace KushaiVkusno.Services;

internal static class DishService
{
    public static List<Dish> GetAll()
    {
        const string sql = """
            SELECT d.id_dish, d.name, d.price, d.description, d.image_path,
                   c.id_category, c.name
            FROM dish d
                     JOIN category c ON c.id_category = d.id_category
            ORDER BY d.id_dish
            """;

        var result = new List<Dish>();
        using var connection = Database.Open();
        using var command = new MySqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Dish(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(5),
                reader.GetString(6)));
        }

        return result;
    }

    public static List<Category> GetCategories()
    {
        var result = new List<Category>();
        using var connection = Database.Open();
        using var command = new MySqlCommand("SELECT id_category, name FROM category ORDER BY name", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Category(reader.GetInt32(0), reader.GetString(1)));
        }

        return result;
    }

    public static int CountDishes()
    {
        using var connection = Database.Open();
        using var command = new MySqlCommand("SELECT COUNT(*) FROM dish", connection);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    /// <summary>Блюдо присутствует хотя бы в одном заказе — удаление запрещено.</summary>
    public static bool IsUsedInOrders(int dishId)
    {
        using var connection = Database.Open();
        using var command = new MySqlCommand("SELECT COUNT(*) FROM order_item WHERE id_dish = @id", connection);
        command.Parameters.AddWithValue("@id", dishId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public static void Insert(string name, decimal price, string? description, string? imagePath, int categoryId)
    {
        const string sql = """
            INSERT INTO dish (name, price, description, image_path, id_category)
            VALUES (@name, @price, @description, @image, @category)
            """;

        using var connection = Database.Open();
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@price", price);
        command.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("@image", (object?)imagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@category", categoryId);
        command.ExecuteNonQuery();
    }

    public static void Update(int id, string name, decimal price, string? description, string? imagePath, int categoryId)
    {
        const string sql = """
            UPDATE dish
            SET name = @name, price = @price, description = @description,
                image_path = @image, id_category = @category
            WHERE id_dish = @id
            """;

        using var connection = Database.Open();
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@price", price);
        command.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("@image", (object?)imagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@category", categoryId);
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    public static void Delete(int id)
    {
        using var connection = Database.Open();
        using var command = new MySqlCommand("DELETE FROM dish WHERE id_dish = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }
}
