using System.Runtime.InteropServices;
using KushaiVkusno.Data;
using KushaiVkusno.Forms;
using KushaiVkusno.Models;
using KushaiVkusno.Services;

namespace KushaiVkusno.Tools;

/// <summary>
/// Служебный режим формирования иллюстраций для руководства пользователя.
/// Запускается ключом --screenshots &lt;папка&gt; и не влияет на обычную работу приложения.
/// </summary>
internal static class ScreenshotRunner
{
    private const string WaiterLogin = "m4ic8j5qgstw@gmail.com";
    private const string WaiterPassword = "2L6KZG";
    private const string AdminLogin = "fvkbcamhlj52@gmail.com";
    private const string AdminPassword = "RSbvHv";

    private const uint PwRenderFullContent = 0x00000002;
    private const uint SwShow = 5;

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, uint command);

    public static void Run(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var login = new LoginForm();
        Capture(login, Path.Combine(outputDirectory, "01-login.png"));

        var filled = new LoginForm();
        filled.SetCredentials(WaiterLogin, WaiterPassword);
        Capture(filled, Path.Combine(outputDirectory, "02-login-filled.png"));

        var waiter = AuthService.Authenticate(WaiterLogin, WaiterPassword);
        var admin = AuthService.Authenticate(AdminLogin, AdminPassword);
        if (waiter is null || admin is null)
        {
            throw new InvalidOperationException("Не удалось авторизовать демонстрационных пользователей.");
        }

        Session.SignIn(waiter);
        var waiterForm = new MainForm();
        Capture(waiterForm, Path.Combine(outputDirectory, "03-main-waiter.png"));

        Session.SignIn(admin);
        var adminForm = new MainForm();
        Capture(adminForm, Path.Combine(outputDirectory, "04-main-admin.png"));

        var filtered = new MainForm();
        filtered.DemoApply("уха", "Первые блюда", 1);
        Capture(filtered, Path.Combine(outputDirectory, "05-main-search.png"));

        var dishes = DishService.GetAll();
        var orderDishes = dishes.Where(d => d.Id is 2 or 3 or 5).ToList();
        Session.SignIn(waiter);
        var orderForm = new OrderForm(orderDishes);
        orderForm.DemoFill(2, 4, "Без лука, подать счёт сразу");
        Capture(orderForm, Path.Combine(outputDirectory, "06-order-form.png"));

        var editForm = new DishEditForm(dishes.First(d => d.Id == 2));
        Capture(editForm, Path.Combine(outputDirectory, "07-dish-edit.png"));

        var ordersForm = new OrdersForm();
        Capture(ordersForm, Path.Combine(outputDirectory, "08-orders.png"));

        var emptyForm = new DishEditForm(null);
        Capture(emptyForm, Path.Combine(outputDirectory, "09-dish-add.png"));

        Console.WriteLine("Скриншоты сформированы: " + outputDirectory);
        Application.Exit();
    }

    private static void Capture(Form form, string path)
    {
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(60, 60);
        form.Show();
        form.BringToFront();
        form.Activate();
        Application.DoEvents();
        ShowWindow(form.Handle, SwShow);
        Thread.Sleep(350);
        Application.DoEvents();

        using var full = new Bitmap(form.Width, form.Height);
        using (var graphics = Graphics.FromImage(full))
        {
            var hdc = graphics.GetHdc();
            try
            {
                PrintWindow(form.Handle, hdc, PwRenderFullContent);
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
            }
        }

        var client = form.RectangleToScreen(form.ClientRectangle);
        var crop = new Rectangle(client.X - form.Left, client.Y - form.Top, client.Width, client.Height);
        crop.Intersect(new Rectangle(Point.Empty, form.Size));

        using (var bitmap = crop.Width > 0 && crop.Height > 0
                   ? full.Clone(crop, full.PixelFormat)
                   : (Bitmap)full.Clone())
        {
            bitmap.Save(path);
        }

        form.Hide();
        form.Dispose();
    }
}
