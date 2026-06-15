# Pipeline Sensor Monitor

A C# / .NET application that simulates real-time pipeline sensor monitoring for Oil & Gas infrastructure.

## What it does

- Reads sensor data (pressure, temperature, flow rate) from a JSON input file
- Connects to a SQL Server database and stores every reading
- Automatically detects dangerous readings and flags them as alerts
- Generates a summary report showing total readings, alert count, and worst values

## Tech Stack

- C# / .NET 10
- Microsoft SQL Server
- T-SQL (joins, aggregations, CASE statements)
- JSON data ingestion (Newtonsoft.Json)

## Real-world application

This pattern is used in Oil & Gas pipeline monitoring, power plant turbine health systems, and infrastructure sensor networks anywhere sensor data needs to be ingested, validated, and stored automatically.
