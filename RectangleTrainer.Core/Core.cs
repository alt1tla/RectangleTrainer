using RectangleTrainer.Core.Enums;
using RectangleTrainer.Core.Models;
using RectangleTrainer.Core.Settings;

namespace RectangleTrainer.Core;

public record Question(Rectangle Rect, CalcType Type, string Prompt, double CorrectAnswer);

public interface IApp
{
    event Action<Question>? QuestionReady;
    event Action<bool, double, double, string>? AnswerResult;
    event Action<string>? InfoMessage;
    void GenerateQuestion();
    void CheckAnswer(double userAnswer);
    AppSettings Settings { get; }
    void UpdateSettings(AppSettings settings);
}

public class App : IApp
{
    public event Action<Question>? QuestionReady;
    public event Action<bool, double, double, string>? AnswerResult;
    public event Action<string>? InfoMessage;
    public AppSettings Settings { get; private set; }

    private readonly Random _rnd = new();
    private Question? _current;

    public App(AppSettings settings) => Settings = settings;

    public void UpdateSettings(AppSettings settings)
    {
        Settings = settings;
        GenerateQuestion();
    }

    public void GenerateQuestion()
    {
        int max = Settings.Difficulty switch { 1 => 10, 2 => 50, 3 => 100, _ => 20 };
        double a = _rnd.Next(1, max), b = _rnd.Next(1, max);

        Rectangle rect = Settings.MeasureMode == MeasurementMode.ByPoints
            ? Rectangle.FromPoints(_rnd.Next(-10, 10), _rnd.Next(-10, 10), _rnd.Next(1, 20), _rnd.Next(1, 20))
            : Rectangle.FromSides(a, b);

        bool calcPerimeter = Settings.DefaultCalc == CalcType.Perimeter ? _rnd.Next(0, 2) == 0 : true;
        CalcType type = calcPerimeter ? CalcType.Perimeter : CalcType.Area;
        double correct = type == CalcType.Perimeter ? rect.Perimeter : rect.Area;

        string prompt = Settings.MeasureMode == MeasurementMode.BySides
            ? $"Стороны: A = {rect.A}, B = {rect.B}"
            : $"Точки: ({rect.X1}, {rect.Y1}) → ({rect.X2}, {rect.Y2})";
        prompt += $"\nНайдите: {type}";

        _current = new Question(rect, type, prompt, correct);
        QuestionReady?.Invoke(_current);
    }

    public void CheckAnswer(double userAnswer)
    {
        if (_current == null) { InfoMessage?.Invoke("Сначала сгенерируйте задание."); return; }

        bool isCorrect = Math.Abs(userAnswer - _current.CorrectAnswer) < 0.01;
        string feedback = isCorrect ? "✅ Верно!" : $"❌ Неверно. Правильный ответ: {_current.CorrectAnswer:F2}";

        if (Settings.TrainMode == TrainerMode.Learning)
            feedback += $"\n📖 Формула: {(_current.Type == CalcType.Perimeter ? "P = 2×(A+B)" : "S = A×B")}";

        AnswerResult?.Invoke(isCorrect, userAnswer, _current.CorrectAnswer, feedback);
        if (isCorrect || Settings.TrainMode == TrainerMode.Testing) GenerateQuestion();
    }
}
