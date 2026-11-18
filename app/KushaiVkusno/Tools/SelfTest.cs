using KushaiVkusno.Data;
using KushaiVkusno.Models;
using KushaiVkusno.Services;
using MySqlConnector;

namespace KushaiVkusno.Tools;

/// <summary>
/// Служебная проверка сквозного сценария «оформление заказа»:
/// создаёт заказ, читает его обратно, проверяет расчёты и удаляет тестовую запись.
/// Запуск: KushaiVkusno.exe --selftest &lt;файл-отчёта&gt;
/// </summary>
internal static class SelfTest
{
    public static void Run(string reportPath)
    {
        var report = new List<string>();
        void Log(string message)
        {
            report.Add(message);
            Console.WriteLine(message);
        }

        var waiter = AuthService.Authenticate("m4ic8j5qgstw@gmail.com", "2L6KZG");
        if (waiter is null)
        {
            Log("ОШИБКА: авторизация официанта не выполнена.");
            return;
        }

        Log($"Авторизация: {waiter.FullName}, роль {waiter.RoleName}");

        var dishes = DishService.GetAll();
        var before = OrderService.GetAll();
        Log($"Заказов до теста: {before.Count}");

        var selection = new List<(int DishId, int Quantity)>
        {
            (dishes.First(d => d.Id == 3).Id, 2),
            (dishes.First(d => d.Id == 5).Id, 1)
        };

        var expected = selection.Sum(s => dishes.First(d => d.Id == s.DishId).Price * s.Quantity);
        var orderId = OrderService.Create(waiter.Id, 9, "Автотест: оформление заказа", selection);
        Log($"Создан заказ №{orderId}, ожидаемая сумма {expected:N2}");

        var created = OrderService.GetAll().FirstOrDefault(o => o.Id == orderId);
        if (created is null)
        {
            Log("ОШИБКА: созданный заказ не найден в представлении v_order_summary.");
            return;
        }

        Log($"Прочитано: дата {created.Date:dd.MM.yyyy}, время {created.Time:hh\\:mm}, стол {created.TableNumber}, " +
            $"статус {created.StatusName}, позиций {created.PositionCount}, блюд {created.ItemsCount}, сумма {created.TotalCost:N2}");

        var lines = OrderService.GetLines(orderId);
        foreach (var line in lines)
        {
            Log($"  позиция: {line.DishName} × {line.Quantity} = {line.LineTotal:N2}");
        }

        var checks = new (string Name, bool Ok)[]
        {
            ("Сумма заказа совпадает с расчётной", created.TotalCost == expected),
            ("Количество позиций = 2", created.PositionCount == 2),
            ("Количество блюд = 3", created.ItemsCount == 3),
            ("Статус нового заказа «Новый»", created.StatusName == "Новый"),
            ("Заказ сохранён с номером последним", created.Id == OrderService.GetAll().Max(o => o.Id)),
            ("Блюдо из заказа нельзя удалить",
                DishService.IsUsedInOrders(dishes.First(d => d.Id == 3).Id)),
            ("Блюдо вне заказов удалить можно",
                !DishService.IsUsedInOrders(0))
        };

        foreach (var (name, ok) in checks)
        {
            Log($"{(ok ? "OK  " : "FAIL")} {name}");
        }

        DeleteOrder(orderId);
        Log($"Тестовый заказ №{orderId} удалён. Заказов после теста: {OrderService.GetAll().Count}");

        File.WriteAllLines(reportPath, report);
        Application.Exit();
    }

    private static void DeleteOrder(int orderId)
    {
        using var connection = Database.Open();
        using var command = new MySqlCommand("DELETE FROM orders WHERE id_order = @id", connection);
        command.Parameters.AddWithValue("@id", orderId);
        command.ExecuteNonQuery();
    }
}
