namespace KushaiVkusno.Models;

/// <summary>Сотрудник, вошедший в систему.</summary>
internal sealed record Employee(int Id, string FullName, string Login, int RoleId, string RoleName)
{
    public bool IsAdministrator => string.Equals(RoleName, "Администратор", StringComparison.OrdinalIgnoreCase);
}

internal sealed record Category(int Id, string Name);

internal sealed record Dish(
    int Id,
    string Name,
    decimal Price,
    string? Description,
    string? ImagePath,
    int CategoryId,
    string CategoryName)
{
    /// <summary>Строка для поиска по всем доступным полям.</summary>
    public string SearchText =>
        $"{Name} {Description} {CategoryName} {Price:0.00} {Id}".ToLowerInvariant();
}

internal sealed record OrderStatus(int Id, string Name);

internal sealed record OrderSummary(
    int Id,
    DateTime Date,
    TimeSpan Time,
    int TableNumber,
    string EmployeeName,
    string StatusName,
    int PositionCount,
    int ItemsCount,
    decimal TotalCost,
    string? Comment);

internal sealed record OrderLine(int OrderId, string DishName, int Quantity, decimal Price, decimal LineCost)
{
    public decimal LineTotal => Quantity * Price;
}
