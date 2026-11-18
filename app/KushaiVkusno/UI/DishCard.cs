using KushaiVkusno.Models;

namespace KushaiVkusno.UI;

/// <summary>Карточка блюда в списке (фото, наименование, категория, состав, цена).</summary>
internal sealed class DishCard : Panel
{
    private readonly PictureBox _photo;
    private readonly Label _name;
    private readonly Label _category;
    private readonly Label _description;
    private readonly Label _price;

    public Dish Dish { get; }

    public CheckBox OrderCheck { get; }

    public DishCard(Dish dish)
    {
        Dish = dish;
        Height = 158;
        Width = 900;
        BackColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
        Margin = new Padding(4, 4, 4, 4);
        Cursor = Cursors.Hand;

        _photo = new PictureBox
        {
            Left = 12,
            Top = 12,
            Size = new Size(132, 132),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        var image = Theme.LoadDishImage(dish.ImagePath);
        if (image is not null)
        {
            _photo.Image = image;
        }

        _name = new Label
        {
            Text = dish.Name,
            Font = Theme.Subtitle,
            ForeColor = Theme.Ink,
            Left = 158,
            Top = 12,
            Height = 26,
            AutoSize = false,
            AutoEllipsis = true
        };

        OrderCheck = new CheckBox
        {
            Text = "В заказ",
            Font = Theme.Body,
            AutoSize = false,
            Size = new Size(104, 26),
            Top = 10,
            Left = Width - 122,
            Cursor = Cursors.Hand,
            BackColor = Color.Transparent
        };

        _category = new Label
        {
            Text = dish.CategoryName,
            Font = Theme.Body,
            ForeColor = Theme.Accent,
            Left = 158,
            Top = 40,
            AutoSize = true
        };

        _description = new Label
        {
            Text = dish.Description ?? "Состав не указан",
            Font = Theme.Small,
            ForeColor = Theme.Muted,
            Left = 158,
            Top = 68,
            Height = 52,
            AutoSize = false
        };

        _price = new Label
        {
            Text = $"{dish.Price:N2} ₽",
            Font = new Font(Theme.BodyBold.FontFamily, 13F, FontStyle.Bold),
            ForeColor = Theme.Accent,
            Left = 158,
            Top = 122,
            AutoSize = true
        };

        Controls.Add(_photo);
        Controls.Add(_name);
        Controls.Add(OrderCheck);
        Controls.Add(_category);
        Controls.Add(_description);
        Controls.Add(_price);

        SetCardWidth(Width);
    }

    public void SetSelected(bool selected)
    {
        BackColor = selected ? Theme.Secondary : Color.White;
        foreach (Control child in Controls)
        {
            if (child is Label or Panel)
            {
                child.BackColor = selected ? Theme.Secondary : Color.Transparent;
            }
        }
    }

    public void SetCardWidth(int width)
    {
        Width = Math.Max(520, width);
        _name.Width = Width - 158 - 136;
        _description.Width = Width - 158 - 20;
        OrderCheck.Left = Width - 122;
    }
}
