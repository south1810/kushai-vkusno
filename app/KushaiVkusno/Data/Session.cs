namespace KushaiVkusno.Data;

/// <summary>Данные активной сессии: кто вошёл в систему.</summary>
internal static class Session
{
    public static Models.Employee? Current { get; private set; }

    public static void SignIn(Models.Employee employee) => Current = employee;

    public static void SignOut() => Current = null;
}
