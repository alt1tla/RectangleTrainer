using RectangleTrainer.Core.Settings;
using RectangleTrainer.Core;

namespace RectangleTrainer.Cli;

class Program
{
    static void Main(string[] args)
    {
        var settingsPath = "cli_settings.json";
        var settings = AppSettings.Load(settingsPath);
        ParseArgs(args, settings);
        settings.Save(settingsPath);

        var engine = new App(settings);
        engine.QuestionReady += q => Console.WriteLine($"\n{q.Prompt}");
        engine.AnswerResult += (ok, user, correct, msg) =>
        {
            Console.WriteLine(msg);
            if (!ok) Console.WriteLine($"Ваш ответ: {user:F2}");
            Console.Write("\nОтвет (или 'skip', 'exit', 'help'): ");
        };
        engine.InfoMessage += m => Console.WriteLine(m);

        engine.GenerateQuestion();
        Console.Write("Ответ: ");

        string? input;
        while ((input = Console.ReadLine()?.Trim().ToLower()) != "exit")
        {
            if (input == "skip") { engine.GenerateQuestion(); }
            else if (input == "help") { PrintHelp(); }
            else if (double.TryParse(input, out double ans)) { engine.CheckAnswer(ans); }
            else { Console.WriteLine("Введите число или команду."); }
            Console.Write("Ответ: ");
        }
    }

    static void ParseArgs(string[] args, AppSettings s)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--mode" && i + 1 < args.Length)
                s.TrainMode = args[++i] == "learning" ? Core.Enums.TrainerMode.Learning : Core.Enums.TrainerMode.Testing;
            else if (args[i] == "--measure" && i + 1 < args.Length)
                s.MeasureMode = args[++i] == "points" ? Core.Enums.MeasurementMode.ByPoints : Core.Enums.MeasurementMode.BySides;
            else if (args[i] == "--difficulty" && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out int parsedDifficulty))
                    s.Difficulty = parsedDifficulty;
            }
            else if (args[i] == "--help") { PrintHelp(); Environment.Exit(0); }
        }
    }

    static void PrintHelp() => Console.WriteLine(
        "Использование: RectangleTrainer.Cli.exe [--mode learning|testing] [--measure sides|points] [--difficulty 1|2|3] [--help]\n" +
        "Команды в консоли: exit (выход), skip (пропустить), help (справка)");
}