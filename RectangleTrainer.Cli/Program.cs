using RectangleTrainer.Core.Settings;
using RectangleTrainer.Core;
using RectangleTrainer.Core.Enums;

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
        engine.QuestionReady += q =>
        {
            Console.WriteLine($"\n{q.Prompt}");
            if (settings.TrainMode == TrainerMode.Learning)
            {
                Console.WriteLine($"Ответ: {q.CorrectAnswer:F2}");
                Console.WriteLine($"Формула: {(q.Type == CalcType.Perimeter ? "P = 2*(A+B)" : "S = A*B")}");
                Console.WriteLine($"Расчёт: {(q.Type == CalcType.Perimeter ? $"2*({q.Rect.A}+{q.Rect.B})" : $"{q.Rect.A}*{q.Rect.B}")} = {q.CorrectAnswer:F2}");
            }
        };

        engine.AnswerResult += (ok, _, _, msg) => Console.WriteLine(msg);
        engine.InfoMessage += m => Console.WriteLine($"{m}");

        engine.GenerateQuestion();
        PrintPrompt(settings.TrainMode);

        string? input;
        while ((input = Console.ReadLine()?.Trim().ToLower()) != "exit")
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                PrintPrompt(settings.TrainMode);
                continue;
            }

            if (input == "next")
            {
                engine.GenerateQuestion();
            }
            else if (input == "help")
            {
                PrintHelp();
            }
            else if (settings.TrainMode == TrainerMode.Testing && double.TryParse(input.Replace(',', '.'), out double ans))
            {
                engine.CheckAnswer(ans);
            }
            else if (settings.TrainMode == TrainerMode.Learning)
            {
                Console.WriteLine("В режиме обучения вводить ответ не нужно. Используйте 'next' для нового задания.");
            }
            else
            {
                Console.WriteLine("Введите число, 'next' или 'help'.");
            }

            PrintPrompt(settings.TrainMode);
        }
    }
    static void PrintPrompt(TrainerMode mode)
    {
        if (mode == TrainerMode.Learning)
            Console.Write("\nКоманда (next/exit/help): ");
        else
            Console.Write("\nОтвет (или next/exit/help): ");
    }

    static void ParseArgs(string[] args, AppSettings s)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--mode" && i + 1 < args.Length)
                s.TrainMode = args[++i] == "learning" ? TrainerMode.Learning : TrainerMode.Testing;
            else if (args[i] == "--measure" && i + 1 < args.Length)
                s.MeasureMode = args[++i] == "points" ? MeasurementMode.ByPoints : MeasurementMode.BySides;
            else if (args[i] == "--difficulty" && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out int parsedDifficulty))
                    s.Difficulty = parsedDifficulty;
            }
            else if (args[i] == "--help") { PrintHelp(); Environment.Exit(0); }
        }
    }

    static void PrintHelp() => Console.WriteLine(
        "RectangleTrainer CLI\n" +
        "Параметры: --mode learning|testing  --measure sides|points  --difficulty 1|2|3  --help\n" +
        "Команды: next (пропустить), exit (выход), help (справка)\n" +
        "В режиме обучения ответ показывается сразу. В режиме проверки введите число.");
}