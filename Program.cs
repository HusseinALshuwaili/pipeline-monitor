using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

string jsonContent = File.ReadAllText("sensor_data.json");
var readings = JsonConvert.DeserializeObject<SensorBatch>(jsonContent);

string connectionString = "Server=localhost,1433;Database=PipelineMonitor;User Id=sa;Password=Passw0rd123;TrustServerCertificate=True;";

using SqlConnection conn = new SqlConnection(connectionString);
conn.Open();
Console.WriteLine("✅ Connected to database");

foreach (var reading in readings.Readings)
{
    bool isAlert = false;
    string alertReason = null;

    if (reading.PressurePSI > readings.MaxPressurePSI)
    {
        isAlert = true;
        alertReason = $"Pressure {reading.PressurePSI} PSI exceeds limit of {readings.MaxPressurePSI} PSI";
    }
    else if (reading.TemperatureCelsius > readings.MaxTempCelsius)
    {
        isAlert = true;
        alertReason = $"Temperature {reading.TemperatureCelsius}°C exceeds limit of {readings.MaxTempCelsius}°C";
    }

    string sql = @"INSERT INTO SensorReadings 
                   (PipelineID, PressurePSI, TemperatureCelsius, FlowRateLPM, IsAlert, AlertReason)
                   VALUES (@PipelineID, @Pressure, @Temp, @Flow, @IsAlert, @AlertReason)";

    using SqlCommand cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@PipelineID", readings.PipelineID);
    cmd.Parameters.AddWithValue("@Pressure", reading.PressurePSI);
    cmd.Parameters.AddWithValue("@Temp", reading.TemperatureCelsius);
    cmd.Parameters.AddWithValue("@Flow", reading.FlowRateLPM);
    cmd.Parameters.AddWithValue("@IsAlert", isAlert);
    cmd.Parameters.AddWithValue("@AlertReason", (object)alertReason ?? DBNull.Value);
    cmd.ExecuteNonQuery();

    string status = isAlert ? "⚠️  ALERT" : "✅ OK";
    Console.WriteLine($"{status} | Pressure: {reading.PressurePSI} PSI | Temp: {reading.TemperatureCelsius}°C | Flow: {reading.FlowRateLPM} L/min");
}

Console.WriteLine("\n========== PIPELINE SUMMARY REPORT ==========");

string reportSql = @"
    SELECT p.PipelineName, p.Location,
        COUNT(s.ReadingID) AS TotalReadings,
        SUM(CASE WHEN s.IsAlert = 1 THEN 1 ELSE 0 END) AS TotalAlerts,
        MAX(s.PressurePSI) AS HighestPressure,
        MAX(s.TemperatureCelsius) AS HighestTemperature,
        MIN(s.FlowRateLPM) AS LowestFlowRate
    FROM Pipelines p
    JOIN SensorReadings s ON p.PipelineID = s.PipelineID
    GROUP BY p.PipelineName, p.Location";

using SqlCommand reportCmd = new SqlCommand(reportSql, conn);
using SqlDataReader reader = reportCmd.ExecuteReader();

while (reader.Read())
{
    Console.WriteLine($"Pipeline:     {reader["PipelineName"]} ({reader["Location"]})");
    Console.WriteLine($"Readings:     {reader["TotalReadings"]}");
    Console.WriteLine($"Alerts:       {reader["TotalAlerts"]}");
    Console.WriteLine($"Max Pressure: {reader["HighestPressure"]} PSI");
    Console.WriteLine($"Max Temp:     {reader["HighestTemperature"]}°C");
    Console.WriteLine($"Min Flow:     {reader["LowestFlowRate"]} L/min");
}
Console.WriteLine("==============================================");

public class SensorBatch
{
    public int PipelineID { get; set; }
    public decimal MaxPressurePSI { get; set; }
    public decimal MaxTempCelsius { get; set; }
    public List<SensorReading> Readings { get; set; }
}

public class SensorReading
{
    public decimal PressurePSI { get; set; }
    public decimal TemperatureCelsius { get; set; }
    public decimal FlowRateLPM { get; set; }
}
