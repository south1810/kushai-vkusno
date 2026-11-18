using KushaiVkusno.Data;
using KushaiVkusno.Models;
using KushaiVkusno.Services;
using KushaiVkusno.UI;

namespace KushaiVkusno.Forms;

/// <summary>Оформление нового заказа по выбранным в списке блюдам.</summary>
internal sealed class OrderForm : Form
{
    private static OrderForm? _opened;

    private readonly List<(Dish Dish, NumericUpDown Quantity, Label LineTotal)> _rows = new();
    private readonly Panel _itemsPanel = new();
    private readonly TextBox _comment = new();
    private readonly NumericUpDown _table = new();
    private readonly Label _positionsValue = new();
    private readonly Label _dishesValue = new();
    private readonly Label _totalValue = new();

    internal OrderForm(IReadOnlyList<Dish> dishes)
    {
        Theme.ApplyChrome(this, "КушайВкусно — оформление заказа");
        ClientSize = new Size(940, 800);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var employee = Session.Current!;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.Background
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(Theme.Header("Оформление заказа"), 0, 0);
        root.Controls.Add(BuildInfo(employee), 0, 1);
        root.Controls.Add(BuildBody(dishes), 0, 2);
        Controls.Add(root);

        Recalculate();
    }

    /// <summary>Открывает окно заказа; второе окно одновременно открыть нельзя.</summary>
    public static bool Open(IReadOnlyList<Dish> dishes, IWin32Window owner)
    {
        if (_opened is { IsDisposed: false })
        {
            _opened.Activate();
            MessageBox.Show(owner, "Окно оформления заказа уже открыто.",
                "Заказ уже оформляется", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        using var form = new OrderForm(dishes);
        _opened = form;
        try
        {
            return form.ShowDialog(owner) == DialogResult.OK;
        }
        finally
        {
            _opened = null;
        }
    }

    private Control BuildInfo(Employee employee)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };

        var employeeLabel = new Label
        {
            Text = $"Оформляет заказ: {employee.FullName} ({employee.RoleName})",
            Font = Theme.BodyBold,
            ForeColor = Theme.Ink,
            AutoSize = true,
            Location = new Point(26, 14)
        };

        var dateLabel = new Label
        {
            Text = $"Дата и время оформления: {DateTime.Now:dd.MM.yyyy HH:mm}",
            Font = Theme.Body,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Location = new Point(26, 44)
        };

        var tableCaption = Theme.Caption("Стол №");
        tableCaption.Location = new Point(742, 20);

        _table.Font = Theme.Body;
        _table.Location = new Point(806, 16);
        _table.Size = new Size(76, 28);
        _table.Minimum = 1;
        _table.Maximum = 200;
        _table.Value = 1;

        var note = new Label
        {
            Text = "Номер заказа присваивается автоматически.",
            Font = Theme.Small,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Location = new Point(26, 72)
        };

        panel.Controls.Add(employeeLabel);
        panel.Controls.Add(dateLabel);
        panel.Controls.Add(tableCaption);
        panel.Controls.Add(_table);
        panel.Controls.Add(note);
        return panel;
    }

    private Control BuildBody(IReadOnlyList<Dish> dishes)
    {
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.Background
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 232F));

        _itemsPanel.Dock = DockStyle.Fill;
        _itemsPanel.AutoScroll = true;
        _itemsPanel.BackColor = Color.FromArgb(246, 250, 251);
        _itemsPanel.Padding = new Padding(12, 8, 12, 8);

        var top = 8;
        foreach (var dish in dishes)
        {
            var row = BuildRow(dish, top);
            _itemsPanel.Controls.Add(row);
            top += 96;
        }

        body.Controls.Add(_itemsPanel, 0, 0);
        body.Controls.Add(BuildFooter(), 0, 1);
        return body;
    }

    private Panel BuildRow(Dish dish, int top)
    {
        var row = new Panel
        {
            Left = 8,
            Top = top,
            Width = 870,
            Height = 86,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var photo = new PictureBox
        {
            Left = 10,
            Top = 9,
            Size = new Size(66, 66),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White
        };
        var image = Theme.LoadDishImage(dish.ImagePath);
        if (image is not null)
        {
            photo.Image = image;
        }

        var name = new Label
        {
            Text = dish.Name,
            Font = Theme.BodyBold,
            ForeColor = Theme.Ink,
            AutoSize = false,
            Size = new Size(430, 26),
            Left = 90,
            Top = 12,
            AutoEllipsis = true
        };

        var price = new Label
        {
            Text = $"Цена: {dish.Price:N2} ₽",
            Font = Theme.Body,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Left = 90,
            Top = 46
        };

        var quantityCaption = Theme.Caption("Кол-во:");
        quantityCaption.Location = new Point(530, 32);

        var quantity = new NumericUpDown
        {
            Font = Theme.Body,
            Location = new Point(598, 28),
            Size = new Size(70, 28),
            Minimum = 0,
            Maximum = 99,
            Value = 1
        };

        var lineTotal = new Label
        {
            Text = $"{dish.Price:N2} ₽",
            Font = Theme.BodyBold,
            ForeColor = Theme.Accent,
            AutoSize = false,
            Size = new Size(150, 26),
            Left = 690,
            Top = 30,
            TextAlign = ContentAlignment.MiddleRight
        };

        quantity.ValueChanged += (_, _) =>
        {
            lineTotal.Text = $"{dish.Price * quantity.Value:N2} ₽";
            Recalculate();
        };

        row.Controls.Add(photo);
        row.Controls.Add(name);
        row.Controls.Add(price);
        row.Controls.Add(quantityCaption);
        row.Controls.Add(quantity);
        row.Controls.Add(lineTotal);

        _rows.Add((dish, quantity, lineTotal));
        return row;
    }

    private Control BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
        var line = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Theme.Line };

        var commentCaption = Theme.Caption("Дополнительная информация к заказу");
        commentCaption.Location = new Point(26, 16);

        _comment.Font = Theme.Body;
        _comment.Multiline = true;
        _comment.BorderStyle = BorderStyle.FixedSingle;
        _comment.ScrollBars = ScrollBars.Vertical;
        _comment.Location = new Point(26, 40);
        _comment.Size = new Size(420, 96);

        var positionsCaption = new Label
        {
            Text = "Позиций в заказе:",
            Font = Theme.Body,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Location = new Point(500, 20)
        };

        _positionsValue.Font = Theme.BodyBold;
        _positionsValue.ForeColor = Theme.Ink;
        _positionsValue.AutoSize = true;
        _positionsValue.Location = new Point(700, 20);

        var dishesCaption = new Label
        {
            Text = "Всего блюд:",
            Font = Theme.Body,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Location = new Point(500, 50)
        };

        _dishesValue.Font = Theme.BodyBold;
        _dishesValue.ForeColor = Theme.Ink;
        _dishesValue.AutoSize = true;
        _dishesValue.Location = new Point(700, 50);

        var totalCaption = new Label
        {
            Text = "Общая стоимость:",
            Font = Theme.BodyBold,
            ForeColor = Theme.Ink,
            AutoSize = true,
            Location = new Point(500, 82)
        };

        _totalValue.Font = new Font(Theme.BodyBold.FontFamily, 18F, FontStyle.Bold);
        _totalValue.ForeColor = Theme.Accent;
        _totalValue.AutoSize = true;
        _totalValue.Location = new Point(700, 76);

        var cancel = Theme.SecondaryButton("Отмена");
        cancel.Location = new Point(560, 150);
        cancel.Size = new Size(140, 46);
        cancel.DialogResult = DialogResult.Cancel;

        var save = Theme.PrimaryButton("Оформить заказ");
        save.Location = new Point(716, 150);
        save.Size = new Size(200, 46);
        save.Click += (_, _) => Save();

        AcceptButton = save;
        CancelButton = cancel;

        footer.Controls.Add(line);
        footer.Controls.Add(commentCaption);
        footer.Controls.Add(_comment);
        footer.Controls.Add(positionsCaption);
        footer.Controls.Add(_positionsValue);
        footer.Controls.Add(dishesCaption);
        footer.Controls.Add(_dishesValue);
        footer.Controls.Add(totalCaption);
        footer.Controls.Add(_totalValue);
        footer.Controls.Add(cancel);
        footer.Controls.Add(save);
        return footer;
    }

    /// <summary>Служебный метод: заполнение формы для иллюстраций руководства.</summary>
    internal void DemoFill(int table, int quantity, string comment)
    {
        _table.Value = Math.Clamp(table, (int)_table.Minimum, (int)_table.Maximum);
        _comment.Text = comment;
        foreach (var row in _rows)
        {
            row.Quantity.Value = Math.Clamp(quantity, 0, 99);
        }

        Recalculate();
    }

    private void Recalculate()
    {
        var positions = _rows.Count(r => r.Quantity.Value > 0);
        var dishes = _rows.Sum(r => (int)r.Quantity.Value);
        var total = _rows.Sum(r => r.Dish.Price * r.Quantity.Value);

        _positionsValue.Text = positions.ToString();
        _dishesValue.Text = dishes.ToString();
        _totalValue.Text = $"{total:N2} ₽";
    }

    private void Save()
    {
        var items = _rows
            .Where(r => r.Quantity.Value > 0)
            .Select(r => (r.Dish.Id, Quantity: (int)r.Quantity.Value))
            .ToList();

        if (items.Count == 0)
        {
            MessageBox.Show(this,
                "Укажите количество хотя бы для одного блюда — заказ не может быть пустым.",
                "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            OrderService.Create(Session.Current!.Id, (int)_table.Value, _comment.Text.Trim(), items);
            MessageBox.Show(this, "Заказ успешно оформлен и сохранён в базе данных.",
                "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, "Не удалось сохранить заказ.\n\n" + exception.Message,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
