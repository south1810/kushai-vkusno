using KushaiVkusno.Models;
using KushaiVkusno.Services;
using KushaiVkusno.UI;

namespace KushaiVkusno.Forms;

/// <summary>Просмотр оформленных заказов (роль «Администратор»).</summary>
internal sealed class OrdersForm : Form
{
    private readonly DataGridView _grid = new();
    private readonly FlowLayoutPanel _lines = new();
    private readonly Label _caption = new();

    private List<OrderSummary> _orders = new();

    public OrdersForm()
    {
        Theme.ApplyChrome(this, "КушайВкусно — заказы");
        ClientSize = new Size(1200, 900);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.Background
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 404F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildGrid(), 0, 1);
        root.Controls.Add(BuildDetails(), 0, 2);
        Controls.Add(root);

        LoadOrders();
    }

    private Control BuildHeader()
    {
        var right = new Panel { Dock = DockStyle.Right, Width = 330, BackColor = Color.Transparent };

        var refresh = Theme.HeaderButton("Обновить список");
        refresh.Size = new Size(180, 40);
        refresh.Location = new Point(10, 19);
        refresh.Click += (_, _) => LoadOrders();

        var close = Theme.HeaderButton("Закрыть");
        close.Size = new Size(110, 40);
        close.Location = new Point(200, 19);
        close.Click += (_, _) => Close();

        right.Controls.Add(refresh);
        right.Controls.Add(close);
        return Theme.Header("Заказы", right);
    }

    private Control BuildGrid()
    {
        var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 6, 16, 6) };

        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.FixedSingle;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.Font = Theme.Body;
        _grid.ColumnHeadersDefaultCellStyle.Font = Theme.BodyBold;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Secondary;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Ink;
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Secondary;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeight = 42;
        _grid.RowTemplate.Height = 30;
        _grid.ScrollBars = ScrollBars.Vertical;
        _grid.SelectionChanged += (_, _) => ShowLines();

        AddColumn("№", 45);
        AddColumn("Дата", 100);
        AddColumn("Время", 80);
        AddColumn("Стол", 60);
        AddColumn("Сотрудник", 280);
        AddColumn("Статус", 110);
        AddColumn("Позиций", 85);
        AddColumn("Блюд", 70);
        AddColumn("Сумма, ₽", 110);
        AddColumn("Дополнительно", 240);

        host.Controls.Add(_grid);
        return host;
    }

    private void AddColumn(string header, int weight)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            Name = header,
            FillWeight = weight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private Control BuildDetails()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };

        var header = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Theme.Background };
        _caption.Text = "Состав заказа";
        _caption.Font = Theme.Subtitle;
        _caption.ForeColor = Theme.Ink;
        _caption.AutoSize = false;
        _caption.Dock = DockStyle.Fill;
        _caption.TextAlign = ContentAlignment.MiddleLeft;
        _caption.Padding = new Padding(20, 0, 0, 0);
        header.Controls.Add(_caption);

        _lines.Dock = DockStyle.Fill;
        _lines.FlowDirection = FlowDirection.TopDown;
        _lines.WrapContents = false;
        _lines.AutoScroll = true;
        _lines.BackColor = Color.FromArgb(246, 250, 251);
        _lines.Padding = new Padding(16, 10, 16, 8);

        panel.Controls.Add(_lines);
        panel.Controls.Add(header);
        return panel;
    }

    private void LoadOrders()
    {
        try
        {
            _orders = OrderService.GetAll();
            _grid.Rows.Clear();
            foreach (var order in _orders)
            {
                _grid.Rows.Add(
                    order.Id,
                    order.Date.ToString("dd.MM.yyyy"),
                    order.Time.ToString(@"hh\:mm"),
                    order.TableNumber,
                    order.EmployeeName,
                    order.StatusName,
                    order.PositionCount,
                    order.ItemsCount,
                    order.TotalCost.ToString("N2"),
                    order.Comment ?? "—");
            }

            if (_grid.Rows.Count > 0)
            {
                _grid.Rows[0].Selected = true;
                _grid.CurrentCell = _grid.Rows[0].Cells[0];
            }

            ShowLines();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, "Не удалось загрузить заказы.\n\n" + exception.Message,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowLines()
    {
        _lines.SuspendLayout();
        foreach (Control control in _lines.Controls.Cast<Control>().ToList())
        {
            control.Dispose();
        }

        _lines.Controls.Clear();

        if (_grid.CurrentRow is null || _grid.CurrentRow.Index < 0 ||
            _grid.CurrentRow.Index >= _orders.Count)
        {
            _caption.Text = "Состав заказа";
            _lines.ResumeLayout();
            return;
        }

        var order = _orders[_grid.CurrentRow.Index];
        _caption.Text =
            $"Состав заказа №{order.Id} от {order.Date:dd.MM.yyyy} {order.Time:hh\\:mm} · стол {order.TableNumber}";

        try
        {
            var lines = OrderService.GetLines(order.Id);
            foreach (var line in lines)
            {
                _lines.Controls.Add(new Label
                {
                    Text = $"{line.DishName} — {line.Quantity} × {line.Price:N2} ₽ = {line.LineTotal:N2} ₽",
                    Font = Theme.Body,
                    ForeColor = Theme.Ink,
                    AutoSize = true,
                    Margin = new Padding(2, 2, 2, 2)
                });
            }

            _lines.Controls.Add(new Label
            {
                Text = $"Итого: {order.TotalCost:N2} ₽ · статус: {order.StatusName}" +
                       (string.IsNullOrWhiteSpace(order.Comment) ? string.Empty : $" · комментарий: {order.Comment}"),
                Font = Theme.BodyBold,
                ForeColor = Theme.Accent,
                AutoSize = true,
                Margin = new Padding(2, 8, 2, 2)
            });
        }
        catch (Exception exception)
        {
            _lines.Controls.Add(new Label
            {
                Text = "Не удалось загрузить состав заказа: " + exception.Message,
                Font = Theme.Body,
                ForeColor = Color.FromArgb(198, 60, 60),
                AutoSize = true
            });
        }

        _lines.ResumeLayout();
    }
}
