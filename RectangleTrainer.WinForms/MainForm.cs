using RectangleTrainer.Core;
using RectangleTrainer.Core.Settings;

using ModelRectangle = RectangleTrainer.Core.Models.Rectangle;

namespace RectangleTrainer.WinForms;

public class MainForm : Form
{
    private readonly AppSettings _settings;
    private readonly App _app;

    private readonly Label _lblGoal;
    private readonly Label _lblStatus;

    private readonly Panel _field;

    private readonly Button _btnCheck;
    private readonly Button _btnSkip;
    private readonly Button _btnSettings;

    private Question? _question;

    private int _gridSize;
    private const int CellSize = 25;
    private readonly Panel _centerPanel;

    private readonly HelpProvider _helpProvider = new();

    private int _left = -1;
    private int _right = 1;
    private int _top = 1;
    private int _bottom = -1;

    private DragCorner? _dragCorner;

    private enum DragCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public MainForm()
    {
        _settings = AppSettings.Load("winforms_settings.json");
        _app = new App(_settings);

        Text = "Rectangle Trainer";

        Width = 1000;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;

        _lblGoal = new Label
        {
            Dock = DockStyle.Top,
            Height = 60,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _lblStatus = new Label
        {
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 12),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _field = new Panel
        {
            Width = 800,
            Height = 800,
            BackColor = Color.White
        };

        _btnSettings = new Button
        {
            Text = "Настройки",
            Dock = DockStyle.Bottom,
            Height = 55
        };

        _btnCheck = new Button
        {
            Text = "Проверить",
            Dock = DockStyle.Bottom,
            Height = 55
        };


        _btnSkip = new Button
        {
            Text = "Пропустить",
            Dock = DockStyle.Bottom,
            Height = 55
        };

        _centerPanel = new Panel
        {
            Dock = DockStyle.Fill,
        };

        Controls.Add(_centerPanel);

        _centerPanel.Controls.Add(_field);

        Controls.Add(_lblStatus);
        Controls.Add(_lblGoal);

        Controls.Add(_btnCheck);
        Controls.Add(_btnSkip);
        Controls.Add(_btnSettings);

        _field.Paint += Field_Paint;

        _field.MouseDown += Field_MouseDown;
        _field.MouseMove += Field_MouseMove;
        _field.MouseUp += Field_MouseUp;

        _btnCheck.Click += BtnCheck_Click;

        _btnSkip.Click += (_, _) =>
        {
            ResetRectangle();
            _app.GenerateVisualQuestion();
        };

        _btnSettings.Click += (_, _) =>
        {
            var settings = new SettingsForm(_settings, _app);
            settings.ShowDialog();

            ApplyTheme();
            UpdateGridSize();

            ResetRectangle();

            _app.GenerateVisualQuestion();

            _field.Invalidate();
        };

        _helpProvider.SetShowHelp(this, true);

        _helpProvider.SetHelpString(
        this,
        """
            Управление:

            • Перетаскивайте углы прямоугольника мышкой
            • Постройте фигуру с нужной площадью или периметром
            • Проверка — кнопка "Проверить"
            • Пропуск задания — кнопка "Пропустить"

            Обозначения:
            Ширина: расстояние между вертикальными сторонами
            Высота: расстояние между горизонтальными сторонами
            Периметр: сумма всех сторон
            Площадь: ширина × высота
         """);

        _app.QuestionReady += q =>
        {
            _question = q;

            _lblGoal.Text = q.Prompt;

            ResetRectangle();

            UpdateStatus();

            _field.Invalidate();
        };

        Resize += (_, _) =>
        {
            UpdateFieldLayout();

            _field.Invalidate();
        };

        ApplyTheme();

        UpdateGridSize();

        UpdateFieldLayout();

        ResetRectangle();

        _app.GenerateVisualQuestion();
    }

    private void ApplyTheme()
    {
        bool dark = _settings.Theme == "Dark";

        Color back = dark
            ? Color.FromArgb(35, 35, 35)
            : Color.White;

        Color fore = dark
            ? Color.White
            : Color.Black;

        BackColor = back;
        ForeColor = fore;

        foreach (Control c in Controls)
        {
            c.BackColor = back;
            c.ForeColor = fore;
        }

        _field.BackColor = dark
            ? Color.FromArgb(45, 45, 45)
            : Color.White;
    }

    private void UpdateGridSize()
    {
        _gridSize = _settings.Difficulty switch
        {
            1 => 5,
            2 => 10,
            3 => 15,
            _ => 5
        };
    }

    private void ResetRectangle()
    {
        _left = -1;
        _right = 1;
        _top = 1;
        _bottom = -1;

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        int width = _right - _left;
        int height = _top - _bottom;

        var rect = ModelRectangle.FromSides(width, height);

        string type = _question?.Type == Core.Enums.CalcType.Perimeter
            ? "Периметр"
            : "Площадь";

        double value = _question?.Type == Core.Enums.CalcType.Perimeter
            ? rect.Perimeter
            : rect.Area;

            _lblStatus.Text =
            $"Ширина: {width} | Высота: {height}";

        if (_settings.TrainMode == Core.Enums.TrainerMode.Learning)
        {
            _lblStatus.Text += $" | {type}: {value}";
        }

    }

    private void Field_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        Graphics g = e.Graphics;

        int centerX = _field.Width / 2;
        int centerY = _field.Height / 2;

        DrawGrid(g, centerX, centerY);

        DrawAxes(g, centerX, centerY);

        DrawRectangle(g, centerX, centerY);
    }

    private void DrawGrid(Graphics g, int centerX, int centerY)
    {
        using Pen pen = new(Color.LightGray);

        for (int x = -_gridSize; x <= _gridSize; x++)
        {
            int px = centerX + x * CellSize;

            g.DrawLine(
                pen,
                px,
                centerY - _gridSize * CellSize,
                px,
                centerY + _gridSize * CellSize);
        }

        for (int y = -_gridSize; y <= _gridSize; y++)
        {
            int py = centerY - y * CellSize;

            g.DrawLine(
                pen,
                centerX - _gridSize * CellSize,
                py,
                centerX + _gridSize * CellSize,
                py);
        }
    }

    private void DrawAxes(Graphics g, int centerX, int centerY)
    {
        using Pen axisPen = new(Color.Black, 2);

        g.DrawLine(
            axisPen,
            centerX,
            centerY - _gridSize * CellSize,
            centerX,
            centerY + _gridSize * CellSize);

        g.DrawLine(
            axisPen,
            centerX - _gridSize * CellSize,
            centerY,
            centerX + _gridSize * CellSize,
            centerY);
    }

    private void DrawRectangle(Graphics g, int centerX, int centerY)
    {
        int x1 = centerX + _left * CellSize;
        int x2 = centerX + _right * CellSize;

        int y1 = centerY - _top * CellSize;
        int y2 = centerY - _bottom * CellSize;

        int width = x2 - x1;
        int height = y2 - y1;

        using Brush brush = new SolidBrush(Color.FromArgb(120, Color.Green));

        g.FillRectangle(brush, x1, y1, width, height);

        g.DrawRectangle(
            Pens.DarkGreen,
            x1,
            y1,
            width,
            height);

        DrawCorner(g, x1, y1);
        DrawCorner(g, x2, y1);
        DrawCorner(g, x1, y2);
        DrawCorner(g, x2, y2);
    }

    private void DrawCorner(Graphics g, int x, int y)
    {
        g.FillEllipse(
            Brushes.DarkBlue,
            x - 5,
            y - 5,
            10,
            10);
    }

    private void Field_MouseDown(object? sender, MouseEventArgs e)
    {
        int centerX = _field.Width / 2;
        int centerY = _field.Height / 2;

        Point tl = GridToScreen(_left, _top, centerX, centerY);
        Point tr = GridToScreen(_right, _top, centerX, centerY);
        Point bl = GridToScreen(_left, _bottom, centerX, centerY);
        Point br = GridToScreen(_right, _bottom, centerX, centerY);

        if (Near(e.Location, tl))
            _dragCorner = DragCorner.TopLeft;
        else if (Near(e.Location, tr))
            _dragCorner = DragCorner.TopRight;
        else if (Near(e.Location, bl))
            _dragCorner = DragCorner.BottomLeft;
        else if (Near(e.Location, br))
            _dragCorner = DragCorner.BottomRight;
    }

    private void Field_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_dragCorner == null)
            return;

        int centerX = _field.Width / 2;
        int centerY = _field.Height / 2;

        int gx = (int)Math.Round((e.X - centerX) / (double)CellSize);
        int gy = (int)Math.Round((centerY - e.Y) / (double)CellSize);

        gx = Math.Clamp(gx, -_gridSize, _gridSize);
        gy = Math.Clamp(gy, -_gridSize, _gridSize);

        switch (_dragCorner)
        {
            case DragCorner.TopLeft:
                _left = Math.Min(gx, _right - 1);
                _top = Math.Max(gy, _bottom + 1);
                break;

            case DragCorner.TopRight:
                _right = Math.Max(gx, _left + 1);
                _top = Math.Max(gy, _bottom + 1);
                break;

            case DragCorner.BottomLeft:
                _left = Math.Min(gx, _right - 1);
                _bottom = Math.Min(gy, _top - 1);
                break;

            case DragCorner.BottomRight:
                _right = Math.Max(gx, _left + 1);
                _bottom = Math.Min(gy, _top - 1);
                break;
        }

        UpdateStatus();

        _field.Invalidate();
    }

    private void Field_MouseUp(object? sender, MouseEventArgs e)
    {
        _dragCorner = null;
    }

    private Point GridToScreen(int gx, int gy, int centerX, int centerY)
    {
        return new Point(
            centerX + gx * CellSize,
            centerY - gy * CellSize);
    }

    private bool Near(Point a, Point b)
    {
        return Math.Abs(a.X - b.X) < 10 &&
               Math.Abs(a.Y - b.Y) < 10;
    }

    private void BtnCheck_Click(object? sender, EventArgs e)
    {
        int width = _right - _left;
        int height = _top - _bottom;

        var rect = ModelRectangle.FromSides(width, height);

        bool ok = _app.CheckRectangleAnswer(rect);

        if (ok)
        {
            MessageBox.Show(
                "Верно!",
                "Успех",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ResetRectangle();

            _app.GenerateVisualQuestion();
        }
        else
        {
            MessageBox.Show(
                "Неверный ответ.",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void UpdateFieldLayout()
    {
        int size = Math.Min(
            _centerPanel.ClientSize.Width - 40,
            _centerPanel.ClientSize.Height - 40);

        size = Math.Max(size, 300);

        _field.Width = size;
        _field.Height = size;

        int centerX = _centerPanel.ClientSize.Width / 2;
        int centerY = _centerPanel.ClientSize.Height / 2;

        _field.Left = centerX - (_field.Width / 2);
        _field.Top = centerY - (_field.Height / 2);
    }
}
