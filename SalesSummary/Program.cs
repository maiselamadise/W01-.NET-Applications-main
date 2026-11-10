using System.Globalization;
using System.Text;
using System.Text.Json;

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

var currentDirectory = Directory.GetCurrentDirectory();
var storesDirectory = Path.Combine(currentDirectory, "stores");
var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");
Directory.CreateDirectory(salesTotalDir);

var salesFiles = FindFiles(storesDirectory);
var salesTotal = CalculateSalesTotal(salesFiles);

File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesTotal}{Environment.NewLine}");
GenerateSalesSummary(salesFiles, storesDirectory, Path.Combine(salesTotalDir, "salesSummary.txt"));

IEnumerable<string> FindFiles(string folderName)
{
    List<string> salesFiles = new();
    foreach (var file in Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories))
    {
        if (Path.GetExtension(file) == ".json")
            salesFiles.Add(file);
    }
    return salesFiles;
}

double CalculateSalesTotal(IEnumerable<string> salesFiles)
{
    double salesTotal = 0;
    foreach (var file in salesFiles)
    {
        string salesJson = File.ReadAllText(file);
        SalesData? data = JsonSerializer.Deserialize<SalesData?>(salesJson, jsonOptions);
        salesTotal += data?.Total ?? 0;
    }
    return salesTotal;
}

void GenerateSalesSummary(IEnumerable<string> salesFiles, string storesDirectory, string reportPath)
{
    var usCulture = CultureInfo.GetCultureInfo("en-US"); // forces "$" formatting
    var details = new List<(string FileName, double Total)>();

    foreach (var file in salesFiles)
    {
        string salesJson = File.ReadAllText(file);
        SalesData? data = JsonSerializer.Deserialize<SalesData?>(salesJson, jsonOptions);
        details.Add((Path.GetRelativePath(storesDirectory, file), data?.Total ?? 0));
    }

    var report = new StringBuilder();
    report.AppendLine("Sales Summary");
    report.AppendLine("----------------------------");
    report.AppendLine($" Total Sales: {details.Sum(d => d.Total).ToString("C", usCulture)}");
    report.AppendLine();
    report.AppendLine(" Details:");

    foreach (var (fileName, total) in details)
    {
        report.AppendLine($"  {fileName}: {total.ToString("C", usCulture)}");
    }

    File.WriteAllText(reportPath, report.ToString());
}

record SalesData(double Total);