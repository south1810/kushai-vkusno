using KushaiVkusno.Data;
using KushaiVkusno.Models;
using KushaiVkusno.Services;
using KushaiVkusno.UI;

namespace KushaiVkusno.Forms;

/// <summary>Главный экран: список блюд по макету с поиском, сортировкой и фильтром.</summary>
internal sealed class MainForm : Form
{
    private const string AllCategories = "Все категории";

    private readonly Employee _employee;

    private readonly TextBox _search = Theme.Input(300);
    private readonly ComboBox _categoryFilter = new();
    private readonly ComboBox _sortBox = new();
    private readonly Label _counter = new();
    private readonly FlowLayoutPanel _list = new();

    private readonly List<Dish> _dishes = new();
    private readonly HashSet<int> _checked = new();
    private List<Dish> _filtered = new();
    private DishCard? _selected;

    public bool LogoutRequested { get; private set; }

    public MainForm()
    {
        _employee = Session.Current
                    ?? throw new InvalidOperationException("Сессия не создана: требуется авторизация.");

        Theme.ApplyChrome(this, "КушайВкусно — список блюд");
        ClientSize = new Size(1280, 820);
        MinimumSize = new Size(1120, 720);
        StartPosition = FormStartPosition.CenterScreen;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Theme.Background
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildFilters(), 0, 1);
        root.Controls.Add(BuildActions(), 0, 2);
        root.Controls.Add(BuildList(), 0, 3);
        Controls.Add(root);

        LoadCategories();
        Reload();

        Shown += (_, _) => _search.Focus();
    }

    // ------------------------------------------------------------------ шапка
    private Control BuildHeader()
    {
        var right = new Panel
        {
            Dock = DockStyle.Right,
            Width = 470,
            BackColor = Color.Transparent
        };

        var user = new Label
        {
            Text = $"{_employee.FullName}\n{_employee.RoleName}",
            Font = Theme.BodyBold,
            ForeColor = Theme.Ink,
            TextAlign = ContentAlignment.MiddleRight,
            AutoSize = false,
            Size = new Size(310, 46),
            Location = new Point(0, 16)
        };

        var logout = Theme.HeaderButton("Выйти");
        logout.Size = new Size(120, 40);
        logout.Location = new Point(326, 19);
        logout.Click += (_, _) => Logout();

        right.Controls.Add(user);
        right.Controls.Add(logout);

        return Theme.Header("КушайВкусно — список блюд", right);
    }

    // ------------------------------------------------------------------ фильтры
    private Control BuildFilters()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };

        var searchCaption = Theme.Caption("Поиск:");
        searchCaption.Location = new Point(18, 21);

        _search.Location = new Point(78, 17);
        _search.PlaceholderText = "поиск по всем полям";
        _search.TextChanged += (_, _) => ApplyFilter();

        var categoryCaption = Theme.Caption("Категория:");
        categoryCaption.Location = new Point(396, 21);

        _categoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _categoryFilter.Font = Theme.Body;
        _categoryFilter.Size = new Size(210, 28);
        _categoryFilter.Location = new Point(486, 17);
        _categoryFilter.SelectedIndexChanged += (_, _) => ApplyFilter();

        var sortCaption = Theme.Caption("Сортировка:");
        sortCaption.Location = new Point(712, 21);

        _sortBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _sortBox.Font = Theme.Body;
        _sortBox.Size = new Size(230, 28);
        _sortBox.Location = new Point(808, 17);
        _sortBox.Items.AddRange(new object[] { "Без сортировки", "По возрастанию цены", "По убыванию цены" });
        _sortBox.SelectedIndex = 0;
        _sortBox.SelectedIndexChanged += (_, _) => ApplyFilter();

        var counterHost = new Panel { Dock = DockStyle.Right, Width = 250, BackColor = Color.Transparent };
        _counter.AutoSize = false;
        _counter.Dock = DockStyle.Fill;
        _counter.Font = Theme.BodyBold;
        _counter.ForeColor = Theme.Ink;
        _counter.TextAlign = ContentAlignment.MiddleRight;
        _counter.Padding = new Padding(0, 0, 18, 0);
        counterHost.Controls.Add(_counter);

        panel.Controls.Add(searchCaption);
        panel.Controls.Add(_search);
        panel.Controls.Add(categoryCaption);
        panel.Controls.Add(_categoryFilter);
        panel.Controls.Add(sortCaption);
        panel.Controls.Add(_sortBox);
        panel.Controls.Add(counterHost);
        return panel;
    }

    // ------------------------------------------------------------------ кнопки
    private Control BuildActions()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };

        var line = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Theme.Line };

        var addOrder = Theme.PrimaryButton("Добавить заказ");
        addOrder.Size = new Size(190, 40);
        addOrder.Location = new Point(18, 9);
        addOrder.Click += (_, _) => AddOrder();

        panel.Controls.Add(addOrder);

        if (_employee.IsAdministrator)
        {
            var addDish = Theme.SecondaryButton("Добавить блюдо");
            addDish.Size = new Size(180, 40);
            addDish.Location = new Point(224, 9);
            addDish.Click += (_, _) => AddDish();

            var editDish = Theme.SecondaryButton("Редактировать");
            editDish.Size = new Size(170, 40);
            editDish.Location = new Point(416, 9);
            editDish.Click += (_, _) => EditDish();

            var deleteDish = Theme.SecondaryButton("Удалить блюдо");
            deleteDish.Size = new Size(160, 40);
            deleteDish.Location = new Point(598, 9);
            deleteDish.Click += (_, _) => DeleteDish();

            var orders = Theme.SecondaryButton("Заказы");
            orders.Size = new Size(140, 40);
            orders.Location = new Point(770, 9);
            orders.Click += (_, _) => OpenOrders();

            panel.Controls.Add(addDish);
            panel.Controls.Add(editDish);
            panel.Controls.Add(deleteDish);
            panel.Controls.Add(orders);
        }

        panel.Controls.Add(line);
        return panel;
    }

    // ------------------------------------------------------------------ список
    private Control BuildList()
    {
        _list.Dock = DockStyle.Fill;
        _list.FlowDirection = FlowDirection.TopDown;
        _list.WrapContents = false;
        _list.AutoScroll = true;
        _list.BackColor = Color.FromArgb(246, 250, 251);
        _list.Padding = new Padding(8, 8, 8, 8);
        _list.ClientSizeChanged += (_, _) => ResizeCards();
        return _list;
    }

    // ------------------------------------------------------------------ данные
    private void LoadCategories()
    {
        try
        {
            var categories = DishService.GetCategories();
            _categoryFilter.Items.Clear();
            _categoryFilter.Items.Add(AllCategories);
            foreach (var category in categories)
            {
                _categoryFilter.Items.Add(category.Name);
            }

            _categoryFilter.SelectedIndex = 0;
        }
        catch (Exception exception)
        {
            ShowDatabaseError(exception);
        }
    }

    private void Reload()
    {
        try
        {
            _dishes.Clear();
            _dishes.AddRange(DishService.GetAll());
            _filtered = new List<Dish>(_dishes);
            _checked.RemoveWhere(id => _dishes.All(d => d.Id != id));
            ApplyFilter();
        }
        catch (Exception exception)
        {
            ShowDatabaseError(exception);
        }
    }

    private void ApplyFilter()
    {
        var query = _search.Text.Trim().ToLowerInvariant();
        var selectedCategory = _categoryFilter.SelectedItem as string ?? AllCategories;

        IEnumerable<Dish> items = _dishes;

        if (!string.Equals(selectedCategory, AllCategories, StringComparison.Ordinal))
        {
            items = items.Where(d => string.Equals(d.CategoryName, selectedCategory, StringComparison.Ordinal));
        }

        if (query.Length > 0)
        {
            items = items.Where(d => d.SearchText.Contains(query, StringComparison.Ordinal));
        }

        var result = items.ToList();
        result = _sortBox.SelectedIndex switch
        {
            1 => result.OrderBy(d => d.Price).ThenBy(d => d.Id).ToList(),
            2 => result.OrderByDescending(d => d.Price).ThenBy(d => d.Id).ToList(),
            _ => result.OrderBy(d => d.Id).ToList()
        };

        _filtered = result;
        RenderCards();
        UpdateCounter();
    }

    private void RenderCards()
    {
        _list.SuspendLayout();
        foreach (Control control in _list.Controls.Cast<Control>().ToList())
        {
            control.Dispose();
        }

        _list.Controls.Clear();
        _selected = null;

        foreach (var dish in _filtered)
        {
            var card = new DishCard(dish);
            card.OrderCheck.Checked = _checked.Contains(dish.Id);
            card.OrderCheck.CheckedChanged += (sender, _) =>
            {
                if (sender is CheckBox box && box.Parent is DishCard owner)
                {
                    if (box.Checked)
                    {
                        _checked.Add(owner.Dish.Id);
                    }
                    else
                    {
                        _checked.Remove(owner.Dish.Id);
                    }
                }
            };

            card.Click += (_, _) => Select(card);
            foreach (var child in card.Controls.Cast<Control>())
            {
                if (child is not CheckBox)
                {
                    child.Click += (_, _) => Select(card);
                    child.DoubleClick += (_, _) => EditDish(card.Dish);
                }
            }

            card.DoubleClick += (_, _) => EditDish(card.Dish);

            _list.Controls.Add(card);
        }

        ResizeCards();
        _list.ResumeLayout();
    }

    private void ResizeCards()
    {
        var cards = _list.Controls.OfType<DishCard>().ToList();
        var neededHeight = cards.Sum(c => c.Height + c.Margin.Vertical) + _list.Padding.Vertical;
        var hasScrollBar = neededHeight > _list.ClientSize.Height;

        var width = _list.ClientSize.Width - 30;
        if (hasScrollBar)
        {
            width -= SystemInformation.VerticalScrollBarWidth;
        }

        foreach (var card in _list.Controls.OfType<DishCard>())
        {
            card.SetCardWidth(width);
        }
    }

    private void UpdateCounter()
    {
        _counter.Text = $"Показано {_filtered.Count} из {_dishes.Count}";
    }

    /// <summary>Служебный метод: подготовка состояния поиска/фильтра для иллюстраций.</summary>
    internal void DemoApply(string search, string? category, int sortIndex)
    {
        if (category is not null && _categoryFilter.Items.Contains(category))
        {
            _categoryFilter.SelectedItem = category;
        }

        if (sortIndex >= 0 && sortIndex < _sortBox.Items.Count)
        {
            _sortBox.SelectedIndex = sortIndex;
        }

        _search.Text = search;
        ApplyFilter();
    }

    private void Select(DishCard card)
    {
        if (_selected is not null && !ReferenceEquals(_selected, card))
        {
            _selected.SetSelected(false);
        }

        _selected = card;
        card.SetSelected(true);
    }

    // ------------------------------------------------------------------ действия
    private void AddOrder()
    {
        var selected = _dishes.Where(d => _checked.Contains(d.Id)).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show(this,
                "Отметьте флажком «В заказ» хотя бы одно блюдо, затем нажмите «Добавить заказ».",
                "Заказ не выбран", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (OrderForm.Open(selected, this))
        {
            _checked.Clear();
            Reload();
        }
    }

    private void AddDish()
    {
        if (!_employee.IsAdministrator)
        {
            return;
        }

        using var form = new DishEditForm(null);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            Reload();
        }
    }

    private void EditDish()
    {
        if (_selected is null)
        {
            MessageBox.Show(this, "Выберите блюдо в списке.", "Блюдо не выбрано",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        EditDish(_selected.Dish);
    }

    private void EditDish(Dish dish)
    {
        if (!_employee.IsAdministrator)
        {
            return;
        }

        using var form = new DishEditForm(dish);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            Reload();
        }
    }

    private void DeleteDish()
    {
        if (!_employee.IsAdministrator)
        {
            return;
        }

        if (_selected is null)
        {
            MessageBox.Show(this, "Выберите блюдо в списке.", "Блюдо не выбрано",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var dish = _selected.Dish;
        try
        {
            if (DishService.IsUsedInOrders(dish.Id))
            {
                MessageBox.Show(this,
                    $"Блюдо «{dish.Name}» присутствует в заказе, поэтому удалить его нельзя.",
                    "Удаление запрещено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(this,
                $"Удалить блюдо «{dish.Name}»?", "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            DishService.Delete(dish.Id);
            Reload();
        }
        catch (Exception exception)
        {
            ShowDatabaseError(exception);
        }
    }

    private void OpenOrders()
    {
        using var form = new OrdersForm();
        form.ShowDialog(this);
    }

    private void Logout()
    {
        LogoutRequested = true;
        Close();
    }

    private void ShowDatabaseError(Exception exception)
    {
        MessageBox.Show(this, "Ошибка работы с базой данных.\n\n" + exception.Message,
            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
