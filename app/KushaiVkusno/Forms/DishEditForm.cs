using KushaiVkusno.Models;
using KushaiVkusno.Services;
using KushaiVkusno.UI;
using MySqlConnector;

namespace KushaiVkusno.Forms;

/// <summary>Добавление и редактирование блюда (роль «Администратор»).</summary>
internal sealed class DishEditForm : Form
{
    private readonly Dish? _dish;
    private readonly TextBox _name = Theme.Input(350);
    private readonly NumericUpDown _price = new();
    private readonly ComboBox _category = new();
    private readonly TextBox _description = new();
    private readonly PictureBox _preview = new();

    private string? _imagePath;

    public DishEditForm(Dish? dish)
    {
        _dish = dish;
        Theme.ApplyChrome(this, dish is null ? "Добавление блюда" : "Редактирование блюда");
        ClientSize = new Size(680, 640);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var header = Theme.Header(dish is null ? "Добавление блюда" : "Редактирование блюда");
        Controls.Add(header);

        BuildFields();

        LoadCategories();

        if (dish is not null)
        {
            _name.Text = dish.Name;
            _price.Value = Math.Clamp(dish.Price, 0, 1_000_000);
            _category.SelectedValue = dish.CategoryId;
            _description.Text = dish.Description ?? string.Empty;
            _imagePath = dish.ImagePath;
            UpdatePreview();
        }
        else
        {
            _price.Value = 0;
            UpdatePreview();
        }

        AcceptButton = _save;
    }

    private readonly Button _save = Theme.PrimaryButton("Сохранить");

    private void BuildFields()
    {
        var nameCaption = Theme.Caption("Наименование блюда");
        nameCaption.Location = new Point(26, 96);
        _name.Location = new Point(26, 120);
        _name.TabIndex = 0;

        var priceCaption = Theme.Caption("Стоимость, ₽");
        priceCaption.Location = new Point(26, 170);

        _price.Font = Theme.Body;
        _price.Location = new Point(26, 194);
        _price.Size = new Size(150, 28);
        _price.DecimalPlaces = 2;
        _price.Minimum = 0;
        _price.Maximum = 1_000_000;
        _price.Increment = 5;
        _price.ThousandsSeparator = true;
        _price.TabIndex = 1;

        var categoryCaption = Theme.Caption("Категория");
        categoryCaption.Location = new Point(196, 170);

        _category.Font = Theme.Body;
        _category.DropDownStyle = ComboBoxStyle.DropDownList;
        _category.Location = new Point(196, 194);
        _category.Size = new Size(180, 28);
        _category.TabIndex = 2;

        var descriptionCaption = Theme.Caption("Состав (описание)");
        descriptionCaption.Location = new Point(26, 244);

        _description.Font = Theme.Body;
        _description.Multiline = true;
        _description.ScrollBars = ScrollBars.Vertical;
        _description.BorderStyle = BorderStyle.FixedSingle;
        _description.Location = new Point(26, 268);
        _description.Size = new Size(350, 170);
        _description.TabIndex = 3;

        var photoCaption = Theme.Caption("Изображение блюда");
        photoCaption.Location = new Point(404, 96);

        _preview.Location = new Point(404, 120);
        _preview.Size = new Size(210, 210);
        _preview.BorderStyle = BorderStyle.FixedSingle;
        _preview.SizeMode = PictureBoxSizeMode.Zoom;
        _preview.BackColor = Color.White;

        var choose = Theme.SecondaryButton("Выбрать файл…");
        choose.Location = new Point(404, 342);
        choose.Size = new Size(210, 36);
        choose.Click += (_, _) => ChooseImage();

        var clear = Theme.SecondaryButton("Убрать изображение");
        clear.Location = new Point(404, 384);
        clear.Size = new Size(210, 36);
        clear.Click += (_, _) =>
        {
            _imagePath = null;
            UpdatePreview();
        };

        var hint = new Label
        {
            Text = "Если изображение не выбрано,\nв списке будет показана картинка-заглушка.",
            Font = Theme.Small,
            ForeColor = Theme.Muted,
            AutoSize = false,
            Size = new Size(210, 48),
            Location = new Point(404, 428)
        };

        var cancel = Theme.SecondaryButton("Отмена");
        cancel.Location = new Point(300, 560);
        cancel.Size = new Size(150, 44);
        cancel.DialogResult = DialogResult.Cancel;

        _save.Location = new Point(464, 560);
        _save.Size = new Size(190, 44);
        _save.Click += (_, _) => Save();

        CancelButton = cancel;

        Controls.Add(nameCaption);
        Controls.Add(_name);
        Controls.Add(priceCaption);
        Controls.Add(_price);
        Controls.Add(categoryCaption);
        Controls.Add(_category);
        Controls.Add(descriptionCaption);
        Controls.Add(_description);
        Controls.Add(photoCaption);
        Controls.Add(_preview);
        Controls.Add(choose);
        Controls.Add(clear);
        Controls.Add(hint);
        Controls.Add(cancel);
        Controls.Add(_save);
    }

    private void LoadCategories()
    {
        try
        {
            var categories = DishService.GetCategories();
            _category.DataSource = categories;
            _category.DisplayMember = nameof(Category.Name);
            _category.ValueMember = nameof(Category.Id);
            if (_dish is null && _category.Items.Count > 0)
            {
                _category.SelectedIndex = 0;
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, "Не удалось загрузить категории.\n\n" + exception.Message,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ChooseImage()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Выбор изображения блюда",
            Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(Theme.ImagesFolder);
            var fileName = Path.GetFileName(dialog.FileName);
            var destination = Path.Combine(Theme.ImagesFolder, fileName);
            if (!string.Equals(Path.GetFullPath(dialog.FileName), Path.GetFullPath(destination),
                    StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(dialog.FileName, destination, true);
            }

            _imagePath = fileName;
            UpdatePreview();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, "Не удалось скопировать изображение.\n\n" + exception.Message,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdatePreview()
    {
        var image = Theme.LoadDishImage(_imagePath);
        _preview.Image = image;
    }

    private void Save()
    {
        var name = _name.Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show(this, "Укажите наименование блюда.", "Проверка данных",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            _name.Focus();
            return;
        }

        if (_category.SelectedValue is not int categoryId)
        {
            MessageBox.Show(this, "Выберите категорию блюда.", "Проверка данных",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var description = _description.Text.Trim();
        if (description.Length == 0)
        {
            description = string.Empty;
        }

        try
        {
            if (_dish is null)
            {
                DishService.Insert(name, _price.Value, description, _imagePath, categoryId);
            }
            else
            {
                DishService.Update(_dish.Id, name, _price.Value, description, _imagePath, categoryId);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            MessageBox.Show(this, "Блюдо с таким наименованием уже существует.",
                "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, "Не удалось сохранить блюдо.\n\n" + exception.Message,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
