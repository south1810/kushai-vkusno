namespace KushaiVkusno.UI;

/// <summary>Единое оформление приложения по «Руководству по стилю».</summary>
internal static class Theme
{
    public static readonly Color Background = Color.FromArgb(255, 255, 255);
    public static readonly Color Secondary = Color.FromArgb(169, 246, 244);
    public static readonly Color Accent = Color.FromArgb(43, 174, 229);
    public static readonly Color Ink = Color.FromArgb(31, 41, 55);
    public static readonly Color Muted = Color.FromArgb(112, 120, 127);
    public static readonly Color Line = Color.FromArgb(214, 226, 230);

    private const string Family = "Comic Sans MS";

    public static readonly Font Body = new(Family, 10F);
    public static readonly Font BodyBold = new(Family, 10F, FontStyle.Bold);
    public static readonly Font Small = new(Family, 8.5F);
    public static readonly Font Subtitle = new(Family, 12F, FontStyle.Bold);
    public static readonly Font Title = new(Family, 16F, FontStyle.Bold);
    public static readonly Font Display = new(Family, 24F, FontStyle.Bold);

    public static string LogoPath => Path.Combine(AppContext.BaseDirectory, "Resources", "logo.png");

    public static string ImagesFolder => Path.Combine(AppContext.BaseDirectory, "Images");

    public static string PlaceholderImage => Path.Combine(ImagesFolder, "picture.png");

    public static Image? LoadLogo()
    {
        try
        {
            return File.Exists(LogoPath) ? Image.FromFile(LogoPath) : null;
        }
        catch
        {
            return null;
        }
    }

    public static Image? LoadDishImage(string? fileName)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var path = Path.Combine(ImagesFolder, fileName);
                if (File.Exists(path))
                {
                    return Image.FromFile(path);
                }
            }

            return File.Exists(PlaceholderImage) ? Image.FromFile(PlaceholderImage) : null;
        }
        catch
        {
            return null;
        }
    }

    public static Button PrimaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            BackColor = Accent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = BodyBold,
            Cursor = Cursors.Hand,
            AutoSize = false,
            Height = 38,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(Accent, 0.15f);
        button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(Accent, 0.05f);
        return button;
    }

    public static Button SecondaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            BackColor = Secondary,
            ForeColor = Ink,
            FlatStyle = FlatStyle.Flat,
            Font = Body,
            Cursor = Cursors.Hand,
            AutoSize = false,
            Height = 38,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(Secondary, 0.25f);
        return button;
    }

    public static Button HeaderButton(string text)
    {
        var button = new Button
        {
            Text = text,
            BackColor = Color.White,
            ForeColor = Accent,
            FlatStyle = FlatStyle.Flat,
            Font = BodyBold,
            Cursor = Cursors.Hand,
            AutoSize = false,
            Height = 38,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 252, 252);
        return button;
    }

    public static Label Caption(string text) => new()
    {
        Text = text,
        Font = Body,
        ForeColor = Muted,
        AutoSize = true
    };

    public static Label Heading(string text, float size = 16F) => new()
    {
        Text = text,
        Font = new Font(Family, size, FontStyle.Bold),
        ForeColor = Ink,
        AutoSize = true
    };

    public static TextBox Input(int width = 240)
    {
        return new TextBox
        {
            Font = Body,
            Width = width,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            ForeColor = Ink
        };
    }

    public static Icon? LoadIcon()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", "icon.ico");
            return File.Exists(path) ? new Icon(path) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Панель-шапка с логотипом и заголовком экрана.</summary>
    public static Panel Header(string title, Control? rightSide = null)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 78,
            BackColor = Secondary
        };

        var logo = new PictureBox
        {
            Left = 16,
            Top = 11,
            Width = 56,
            Height = 56,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        var logoImage = LoadLogo();
        if (logoImage is not null)
        {
            logo.Image = logoImage;
        }

        var label = new Label
        {
            Text = title,
            Font = Title,
            ForeColor = Ink,
            AutoSize = true,
            Left = 84,
            Top = 24,
            BackColor = Color.Transparent
        };

        panel.Controls.Add(logo);
        panel.Controls.Add(label);

        if (rightSide is not null)
        {
            rightSide.Dock = DockStyle.Right;
            rightSide.Width = rightSide.Width;
            rightSide.BackColor = Color.Transparent;
            panel.Controls.Add(rightSide);
        }

        return panel;
    }

    public static void ApplyChrome(Form form, string title, string? headerTitle = null)
    {
        form.Text = title;
        form.BackColor = Background;
        form.Font = Body;
        form.StartPosition = FormStartPosition.CenterScreen;
        var icon = LoadIcon();
        if (icon is not null)
        {
            form.Icon = icon;
        }

        form.AutoScaleMode = AutoScaleMode.Font;
    }
}
