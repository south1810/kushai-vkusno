using KushaiVkusno.Data;
using KushaiVkusno.Services;
using KushaiVkusno.UI;

namespace KushaiVkusno.Forms;

/// <summary>Окно входа — первый экран приложения.</summary>
internal sealed class LoginForm : Form
{
    private readonly TextBox _login = Theme.Input(488);
    private readonly TextBox _password = Theme.Input(488);
    private readonly Label _error = new();
    private readonly Button _submit = Theme.PrimaryButton("Войти");

    public LoginForm()
    {
        Theme.ApplyChrome(this, "КушайВкусно — вход в систему");
        ClientSize = new Size(900, 660);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Theme.Background,
            Padding = new Padding(0, 18, 0, 12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (var i = 0; i < 6; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        var logo = new PictureBox
        {
            Size = new Size(260, 118),
            SizeMode = PictureBoxSizeMode.Zoom,
            Anchor = AnchorStyles.None,
            BackColor = Color.Transparent
        };
        var logoImage = Theme.LoadLogo();
        if (logoImage is not null)
        {
            logo.Image = logoImage;
        }

        var title = new Label
        {
            Text = "КушайВкусно",
            Font = Theme.Display,
            ForeColor = Theme.Accent,
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 6, 0, 0)
        };

        var subtitle = new Label
        {
            Text = "Информационная система для сотрудников зала",
            Font = Theme.Body,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 4, 0, 18)
        };

        var card = BuildCard();

        _error.AutoSize = false;
        _error.Size = new Size(560, 30);
        _error.Font = Theme.BodyBold;
        _error.ForeColor = Color.FromArgb(198, 60, 60);
        _error.TextAlign = ContentAlignment.MiddleCenter;
        _error.Anchor = AnchorStyles.None;
        _error.Margin = new Padding(0, 10, 0, 0);

        var footer = new Label
        {
            Text = "ООО «КушайВкусно» · служба поддержки: support@kushaivkusno.ru",
            Font = Theme.Small,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 16, 0, 0)
        };

        layout.Controls.Add(logo, 0, 0);
        layout.Controls.Add(title, 0, 1);
        layout.Controls.Add(subtitle, 0, 2);
        layout.Controls.Add(card, 0, 3);
        layout.Controls.Add(_error, 0, 4);
        layout.Controls.Add(footer, 0, 5);
        Controls.Add(layout);

        AcceptButton = _submit;
        _submit.Click += (_, _) => Submit();
        Shown += (_, _) => _login.Focus();
    }

    private Panel BuildCard()
    {
        var card = new Panel
        {
            Size = new Size(520, 236),
            BackColor = Theme.Secondary,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 4, 0, 0)
        };

        var loginCaption = Theme.Caption("Логин (адрес электронной почты)");
        loginCaption.Location = new Point(24, 20);

        _login.Location = new Point(24, 44);
        _login.TabIndex = 0;

        var passwordCaption = Theme.Caption("Пароль");
        passwordCaption.Location = new Point(24, 88);

        _password.Location = new Point(24, 112);
        _password.UseSystemPasswordChar = true;
        _password.TabIndex = 1;

        _submit.Location = new Point(24, 168);
        _submit.Size = new Size(472, 44);
        _submit.TabIndex = 2;

        card.Controls.Add(loginCaption);
        card.Controls.Add(_login);
        card.Controls.Add(passwordCaption);
        card.Controls.Add(_password);
        card.Controls.Add(_submit);
        return card;
    }

    /// <summary>Служебный метод: заполнение полей для иллюстраций руководства.</summary>
    internal void SetCredentials(string login, string password)
    {
        _login.Text = login;
        _password.Text = password;
        _error.Text = string.Empty;
    }

    private void Submit()
    {
        var login = _login.Text.Trim();
        var password = _password.Text;

        if (login.Length == 0 || password.Length == 0)
        {
            _error.Text = "Введите логин и пароль.";
            return;
        }

        _error.Text = string.Empty;
        _submit.Enabled = false;
        UseWaitCursor = true;
        try
        {
            var employee = AuthService.Authenticate(login, password);
            if (employee is null)
            {
                _error.Text = "Неверный логин или пароль.";
                _password.SelectAll();
                _password.Focus();
                return;
            }

            Session.SignIn(employee);
            Hide();

            using var main = new MainForm();
            main.ShowDialog();

            var logout = main.LogoutRequested;
            Session.SignOut();
            _password.Clear();

            if (logout)
            {
                Show();
                Activate();
                _login.Focus();
                _login.SelectAll();
            }
            else
            {
                Close();
            }
        }
        catch (Exception exception)
        {
            _error.Text = "Не удалось подключиться к базе данных.";
            MessageBox.Show(this,
                "Не удалось подключиться к базе данных.\n\n" + exception.Message,
                "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _submit.Enabled = true;
        }
    }
}
