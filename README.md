# W01 .NET Applications: Notes

Repository contents: `ContosoPizza/` (ASP.NET Core web API, net8.0) and `SalesSummary/` (console app).

## 1. Evidence: existing pizzas and my additional pizza

The API starts with two seeded pizzas. After creating a third with POST, `GET /pizza` returns:

```json
[
  { "id": 1, "name": "Classic Italian", "isGlutenFree": false },
  { "id": 2, "name": "Veggie", "isGlutenFree": true },
  { "id": 3, "name": "Hawaiian", "isGlutenFree": false }
]
```

<!-- Optional: add a screenshot, e.g. ![GET /pizza](images/get-all.png) -->

## 2. Requests, responses and status codes

### GET (all pizzas)
Request:
```http
GET http://localhost:5122/pizza
Accept: application/json
```
Response: `200 OK`
```json
[{"id":1,"name":"Classic Italian","isGlutenFree":false},{"id":2,"name":"Veggie","isGlutenFree":true}]
```

### POST (create a pizza)
Request:
```http
POST http://localhost:5122/pizza
Content-Type: application/json

{ "name": "Hawaiian", "isGlutenFree": false }
```
Response: `201 Created` (Location: `/Pizza/3`)
```json
{"id":3,"name":"Hawaiian","isGlutenFree":false}
```

### PUT (update pizza 3)
Request:
```http
PUT http://localhost:5122/pizza/3
Content-Type: application/json

{ "id": 3, "name": "Super Hawaiian", "isGlutenFree": true }
```
Response: `204 No Content` (empty body)

### DELETE (delete pizza 3)
Request:
```http
DELETE http://localhost:5122/pizza/3
```
Response: `204 No Content` (empty body)

## 3. Working sales summary code (`SalesSummary/Program.cs`)

```csharp
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

record SalesData(double Total);```

Output (`salesTotalDir/salesSummary.txt`):

```text
Sales Summary
----------------------------
 Total Sales: $249,368.48

 Details:
  sales.json: $22,385.32
  202/sales.json: $75,000.50
  201/sales.json: $151,982.66
```
