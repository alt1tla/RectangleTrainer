using System.IO;
using System.Text.Json;
using RectangleTrainer.Core.Enums;

namespace RectangleTrainer.Core.Settings;

public class AppSettings
{
    public MeasurementMode MeasureMode { get; set; } = MeasurementMode.BySides;
    public TrainerMode TrainMode { get; set; } = TrainerMode.Testing;
    public CalcType DefaultCalc { get; set; } = CalcType.Perimeter;
    public string Theme { get; set; } = "Light";
    public int Difficulty { get; set; } = 1; 

    public void Save(string path) =>
        File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));

    public static AppSettings Load(string path) =>
        File.Exists(path) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path))! : new AppSettings();
}