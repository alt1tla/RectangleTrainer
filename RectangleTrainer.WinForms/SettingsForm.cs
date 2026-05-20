using RectangleTrainer.Core;
using RectangleTrainer.Core.Enums;
using RectangleTrainer.Core.Settings;

namespace RectangleTrainer.WinForms;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly App _engine;

    public SettingsForm(AppSettings settings, App engine)
    {
        _settings = settings;
        _engine = engine;
        Text = "Настройки";
        Size = new System.Drawing.Size(300, 320);

        var cmbMode = new ComboBox { Location = new System.Drawing.Point(20, 20), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbMode.Items.AddRange(new[] { "Обучение (сразу ответ)", "Проверка знаний" });
        cmbMode.SelectedIndex = _settings.TrainMode == TrainerMode.Learning ? 0 : 1;

        var cmbDiff = new ComboBox { Location = new System.Drawing.Point(20, 60), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbDiff.Items.AddRange(new[] { "Лёгкий", "Средний", "Сложный" });
        cmbDiff.SelectedIndex = _settings.Difficulty - 1;

        var cmbTheme = new ComboBox { Location = new System.Drawing.Point(20, 100), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbTheme.Items.AddRange(new[] { "Light", "Dark" });
        cmbTheme.SelectedItem = _settings.Theme;

        var btnSave = new Button { Text = "Сохранить", Location = new System.Drawing.Point(20, 190), Width = 240, Height = 50 };
        btnSave.Click += (_, _) =>
        {
            _settings.TrainMode = cmbMode.SelectedIndex == 0 ? TrainerMode.Learning : TrainerMode.Testing;
            _settings.MeasureMode = MeasurementMode.BySides;
            _settings.Difficulty = cmbDiff.SelectedIndex + 1;
            _settings.Theme = cmbTheme.SelectedItem?.ToString() ?? "Light";
            _settings.Save("winforms_settings.json");
            _engine.UpdateSettings(_settings);
            MessageBox.Show("Настройки применены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        };

        Controls.AddRange(new Control[] { cmbMode, cmbDiff, cmbTheme, btnSave });
    }
}