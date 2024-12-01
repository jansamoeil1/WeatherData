using EFCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public static class WeatherService
{
    public static double GetAverageTemperatureForDate(List<WeatherData> weatherData, DateTime date, string place)
    {
        // Filtrera väderdata baserat på datum och plats
        var filteredData = weatherData
            .Where(w => w.Date.Date == date.Date && w.Place.Equals(place, StringComparison.OrdinalIgnoreCase) && w.Temperature.HasValue) // Säkerställ att vi bara använder data med giltig temperatur
            .ToList();

        // Om det finns väderdata för det angivna datumet och platsen med giltig temperatur
        if (filteredData.Any())
        {
            // Beräkna medeltemperaturen för de väderdata som har giltig temperatur
            double averageTemperature = filteredData.Average(w => w.Temperature.Value);
            return averageTemperature;
        }
        else
        {
            Console.WriteLine("Ingen väderdata för det angivna datumet och platsen.");
            return double.NaN; // Returnera NaN om det inte finns någon data
        }
    }




    // 2. Sortera dagar från varmaste till kallaste

    public static void SortDaysByTemperature(List<WeatherData> data)
    {
        var validData = data.Where(d => d.Temperature.HasValue).ToList();

        if (validData.Count == 0)
        {
            Console.WriteLine("Inga väderdata med giltig temperatur.");
            return;
        }

        // Filtrera för både "Outside" och "Inside"
        var filteredData = validData.Where(d => d.Place == "Outside" || d.Place == "Inside").ToList();

        // Gruppera efter både datum och plats
        var dailyAverageTemps = filteredData
            .GroupBy(d => new { d.Date.Date, d.Place }) // Grupp av både datum och plats
            .Select(group => new
            {
                Date = group.Key.Date,
                Place = group.Key.Place,
                AverageTemperature = group.Average(d => d.Temperature.Value)
            })
            .ToList();

        // Hitta den varmaste och kallaste dagen för varje plats
        var hottestDay = dailyAverageTemps.OrderByDescending(d => d.AverageTemperature).FirstOrDefault();
        var coldestDay = dailyAverageTemps.OrderBy(d => d.AverageTemperature).FirstOrDefault();

        if (hottestDay != null)
        {
            Console.WriteLine($"Varmaste dagen: {hottestDay.Date.ToShortDateString()} ({hottestDay.Place}) - Temp: {hottestDay.AverageTemperature}°C");
        }
        else
        {
            Console.WriteLine("Ingen varm dag att visa.");
        }

        if (coldestDay != null)
        {
            Console.WriteLine($"Kallaste dagen: {coldestDay.Date.ToShortDateString()} ({coldestDay.Place}) - Temp: {coldestDay.AverageTemperature}°C");
        }
        else
        {
            Console.WriteLine("Ingen kall dag att visa.");
        }
    }

    // 3. Sortera dagar från torraste till fuktigaste
    public static void SortDaysByHumidity(List<WeatherData> weatherData)
    {
        var sortedByHumidity = weatherData.OrderBy(w => w.Humidity).ToList();

        Console.WriteLine("Dagar sorterade från torraste till fuktigaste:");
        foreach (var data in sortedByHumidity)
        {
            Console.WriteLine($"Datum: {data.Date}, Plats: {data.Place}, Fuktighet: {data.Humidity}%");
        }
    }


    // 4. Sortera dagar från minst till störst risk för mögel
    public static void SortDaysByMoldRisk(List<WeatherData> weatherData)
    {
        var sortedByMoldRisk = weatherData.OrderBy(w => w.MoldRisk).ToList();

        Console.WriteLine("Dagar sorterade från minst till störst risk för mögel:");
        foreach (var data in sortedByMoldRisk)
        {
            Console.WriteLine($"Datum: {data.Date}, Plats: {data.Place}, Risk för mögel: {data.MoldRisk}%");
        }
    }
}