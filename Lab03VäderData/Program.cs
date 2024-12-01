using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Models;
using CsvHelper;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting the program...");

        // Kontrollera om väderdata finns i databasen och importera vid behov
        CheckAndImportWeatherData();

        List<WeatherData> weatherData = GetWeatherDataFromDatabase();  // Hämtar väderdata från databasen

        // Beräkna startdatum för meteorologisk höst
        DateTime autumnStart = GetMeteorologicalAutumnStart(weatherData);
        if (autumnStart != DateTime.MinValue)
        {
            Console.WriteLine($"Meteorologisk höst startar den: {autumnStart.ToShortDateString()}");
        }
        else
        {
            Console.WriteLine("Ingen meteorologisk höst hittades.");
        }

        // Beräkna startdatum för meteorologisk vinter
        DateTime winterStart = GetMeteorologicalWinterStart(weatherData);
        if (winterStart != DateTime.MinValue)
        {
            Console.WriteLine($"Meteorologisk vinter startar den: {winterStart.ToShortDateString()}");
        }
        else
        {
            Console.WriteLine("Ingen meteorologisk vinter hittades.");
        }

        // Menyn för användaren
        while (true)
        {
            Console.WriteLine("Välj ett alternativ:");
            Console.WriteLine("1. Hämta medeltemperatur för valt datum och plats");
            Console.WriteLine("2. Sortera dagar från varmaste till kallaste");
            Console.WriteLine("3. Sortera dagar från torraste till fuktigaste");
            Console.WriteLine("4. Sortera dagar från minst till störst risk för mögel");
            Console.WriteLine("5. Avsluta");

            string choice = Console.ReadLine();
            Console.WriteLine($"Du valde: '{choice}'");

            if (string.IsNullOrEmpty(choice))
            {
                Console.WriteLine("Inget val angavs. Försök igen.");
                return; // Avbryt om val saknas
            }

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Ange datum (yyyy-mm-dd):");
                    DateTime date = DateTime.Parse(Console.ReadLine());
                    Console.WriteLine("Ange plats (Inside/Outside):");
                    string place = Console.ReadLine();

                    double avgTemp = WeatherService.GetAverageTemperatureForDate(weatherData, date, place);

                    if (!double.IsNaN(avgTemp))
                    {
                        Console.WriteLine($"Medeltemperatur för {place} den {date.ToShortDateString()}: {avgTemp}°C");
                    }
                    break;

                case "2":
                    WeatherService.SortDaysByTemperature(weatherData);
                    break;

                case "3":
                    WeatherService.SortDaysByHumidity(weatherData);
                    break;

                case "4":
                    WeatherService.SortDaysByMoldRisk(weatherData);
                    break;

                case "5":
                    Console.WriteLine("Avslutar programmet...");
                    return;

                default:
                    Console.WriteLine("Ogiltigt val, vänligen försök igen.");
                    break;
            }
        }
    }

    // Metod för att beräkna meteorologisk höst
    static DateTime GetMeteorologicalAutumnStart(List<WeatherData> weatherData)
    {
        var autumnStart = weatherData
            .Where(w => w.Temperature.HasValue) // Ta bort poster med null-värden i temperatur
            .OrderBy(w => w.Date) // Ordna efter datum
            .FirstOrDefault(w => w.Temperature.Value <= 10); // Om temperaturen går under 10°C

        return autumnStart?.Date ?? DateTime.MinValue; // Om vi inte hittar något, returnera DateTime.MinValue
    }

    // Metod för att beräkna meteorologisk vinter
    static DateTime GetMeteorologicalWinterStart(List<WeatherData> weatherData)
    {
        var winterStart = weatherData
            .Where(w => w.Temperature.HasValue) // Ta bort poster med null-värden i temperatur
            .OrderBy(w => w.Date) // Ordna efter datum
            .FirstOrDefault(w => w.Temperature.Value <= 0); // Om temperaturen går under 0°C

        return winterStart?.Date ?? DateTime.MinValue; // Om vi inte hittar något, returnera DateTime.MinValue
    }

    static List<WeatherData> GetWeatherDataFromDatabase()
    {
        using (var db = new EFContext())
        {
            var weatherData = db.WeatherData.ToList();  // Hämtar alla poster från WeatherData-tabellen
            return weatherData;
        }
    }

    // Kontrollera om väderdata redan finns i databasen
    public static void CheckAndImportWeatherData()
    {
        // Hämta väderdata från databasen
        List<WeatherData> weatherData = GetWeatherDataFromDatabase();  // Hämta väderdata från databasen

        // Visa väderdata direkt när metoden körs
        Console.WriteLine("Väderdata i databasen:");
        if (weatherData.Count == 0)
        {
            Console.WriteLine("Ingen väderdata hittades. Kontrollera att data har importerats.");
        }
        else
        {
            foreach (var data in weatherData)
            {
                Console.WriteLine($"Datum: {data.Date.ToShortDateString()}, Plats: {data.Place}, Temperatur: {data.Temperature}°C, Fuktighet: {data.Humidity}%, Mögelrisk: {data.MoldRisk}%");
            }
        }
    }

    // Övriga metoder (import, CSV-hantering, osv.) behålls som tidigare
    static void ImportWeatherData()
    {
        string filePath = @"TempFuktData02.csv";  // Relativt till appens arbetskatalog

        // Kolla om filen finns
        if (!File.Exists(filePath))
        {
            Console.WriteLine("CSV file not found.");
            return;
        }

        // Läs CSV-filen och importera till databasen
        using (var db = new EFContext()) // Använd din DbContext för att interagera med databasen
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // För att ange att filen har en rubrikrad
                Delimiter = ";" // Ange att semikolon används som separator
            }))
            {
                // Läs headern och registrera mappningen
                csv.Read();
                csv.ReadHeader();
                csv.Context.RegisterClassMap<WeatherDataMap>();

                var records = new List<WeatherData>();

                // Läs varje rad i CSV-filen
                while (csv.Read())
                {
                    var record = new WeatherData
                    {
                        Date = csv.GetField<DateTime>("Date"),
                        Place = csv.GetField<string>("Place"),
                        Temperature = HandleInvalidValue(csv.GetField<string>("Temperature")),
                        Humidity = HandleInvalidValue(csv.GetField<string>("Humidity")),
                        MoldRisk = HandleInvalidValue(csv.GetField<string>("Mold Risk (%)")),
                        Location = csv.GetField<string>("Location")
                    };

                    records.Add(record);
                }

                // Lägg till alla poster i databasen och spara ändringar
                db.AddRange(records);
                db.SaveChanges();
                Console.WriteLine($"Imported {records.Count} records to the database.");
            }
        }
    }

    // Metod för att hantera ogiltiga värden (t.ex. tomma eller ogiltiga data)
    private static double? HandleInvalidValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null; // Returnera null om värdet är tomt eller saknas
        }

        // Rensa bort procenttecken om det finns
        value = value.Replace("%", "").Trim();

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result; // Returnera det konverterade värdet
        }

        return null; // Returnera null om värdet inte går att konvertera
    }

    // Mappning av CSV-data till WeatherData-objekt
    public sealed class WeatherDataMap : ClassMap<WeatherData>
    {
        public WeatherDataMap()
        {
            Map(m => m.Date).Name("Date");
            Map(m => m.Place).Name("Place");
            Map(m => m.Temperature).Name("Temperature");
            Map(m => m.Humidity).Name("Humidity");
            Map(m => m.MoldRisk).Name("Mold Risk (%)");
            Map(m => m.Location).Name("Location");
        }
    }


    // Läs och visa väderdata
    static void ReadWeatherData()
    {
        using (var db = new EFContext())
        {
            Console.WriteLine("Reading Weather Data...");

            var data = db.WeatherData.ToList();

            if (data.Count > 0)
            {
                foreach (var record in data)
                {
                    Console.WriteLine($"Id: {record.Id}, Date: {record.Date}, Place: {record.Place}, Temperature: {record.Temperature}, Humidity: {record.Humidity}, MoldRisk: {record.MoldRisk}, Location: {record.Location}");
                }
            }
            else
            {
                Console.WriteLine("No records found.");
            }
        }
    }
}


