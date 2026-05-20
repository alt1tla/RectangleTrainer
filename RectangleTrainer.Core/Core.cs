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
    }

    public void GenerateQuestion()
    {
        int max = Settings.Difficulty switch { 1 => 10, 2 => 50, 3 => 100, _ => 20 };
        double a = _rnd.Next(1, max), b = _rnd.Next(1, max);

        Rectangle rect = Settings.MeasureMode == MeasurementMode.ByPoints
            ? Rectangle.FromPoints(_rnd.Next(-max, max), _rnd.Next(-max, max), _rnd.Next(1, max), _rnd.Next(1, max))
            : Rectangle.FromSides(a, b);

        bool calcPerimeter = Settings.DefaultCalc == CalcType.Perimeter ? _rnd.Next(0, 2) == 0 : true;
        CalcType type = calcPerimeter ? CalcType.Perimeter : CalcType.Area;
        string readableType = type == CalcType.Perimeter ? "периметр" : "площадь";
        double correct = type == CalcType.Perimeter ? rect.Perimeter : rect.Area;

        string prompt = Settings.MeasureMode == MeasurementMode.BySides
            ? $"Стороны: A = {rect.A}, B = {rect.B}"
            : $"Точки: ({rect.X1}, {rect.Y1}) и ({rect.X2}, {rect.Y2})";
        prompt += $"\nНайдите {readableType}";

        _current = new Question(rect, type, prompt, correct);
        QuestionReady?.Invoke(_current);
    }

    public void GenerateVisualQuestion()
    {
        int max = Settings.Difficulty switch
        {
            1 => 10,
            2 => 20,
            3 => 30,
            _ => 10
        };

        bool perimeter = _rnd.Next(0, 2) == 0;

        int a = _rnd.Next(1, max);
        int b = _rnd.Next(1, max);

        double target = perimeter
            ? 2 * (a + b)
            : a * b;

        string readable = perimeter
            ? "периметром"
            : "площадью";

        _current = new Question(
            Rectangle.FromSides(a, b),
            perimeter ? CalcType.Perimeter : CalcType.Area,
            $"Постройте прямоугольник с {readable} = {target}",
            target
        );

        QuestionReady?.Invoke(_current);
    }

    public void CheckAnswer(double userAnswer)
    {
        if (_current == null) { InfoMessage?.Invoke("Сначала сгенерируйте задание."); return; }

        bool isCorrect = Math.Abs(userAnswer - _current.CorrectAnswer) < 0.01;
        string feedback;

        if (Settings.TrainMode == TrainerMode.Learning)
        {
            feedback = $"\nФормула: {(_current.Type == CalcType.Perimeter ? "P = 2*(A+B)" : "S = A*B")}";
            if (Settings.MeasureMode == MeasurementMode.ByPoints)
                feedback += $"\nПеревод точек в стороны: A = |X2−X1| = {Math.Abs(_current.Rect.X2 - _current.Rect.X1)}, B = |Y2−Y1| = {Math.Abs(_current.Rect.Y2 - _current.Rect.Y1)}";
            feedback += $"\nРасчёт: {(_current.Type == CalcType.Perimeter ? $"2*({_current.Rect.A}+{_current.Rect.B})" : $"{_current.Rect.A}*{_current.Rect.B}")} = {_current.CorrectAnswer:F2}";
            feedback += $"Ответ: {_current.CorrectAnswer:F2}";

            AnswerResult?.Invoke(true, userAnswer, _current.CorrectAnswer, feedback);
            GenerateQuestion(); 
            return;
        }
        feedback = isCorrect
            ? "Верно!"
            : "Неверно. Попробуйте ещё раз.";

        AnswerResult?.Invoke(isCorrect, userAnswer, _current.CorrectAnswer, feedback);
        if (isCorrect) GenerateQuestion();
    }

    public bool CheckRectangleAnswer(Rectangle rect)
    {
        if (_current == null)
            return false;

        double value = _current.Type == CalcType.Perimeter
            ? rect.Perimeter
            : rect.Area;

        return Math.Abs(value - _current.CorrectAnswer) < 0.01;
    }
}