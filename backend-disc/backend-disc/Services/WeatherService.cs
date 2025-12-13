using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
namespace backend_disc.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WeatherService> _logger;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public WeatherService(HttpClient httpClient, IMemoryCache cache, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CurrentWeatherData?> GetWeatherAsync(double latitude, double longitude)
        {
            // Create cache key based on rounded coordinates (to 2 decimal places)
            string cacheKey = $"weather_{Math.Round(latitude, 2)}_{Math.Round(longitude, 2)}";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out CurrentWeatherData? cachedWeather))
            {
                _logger.LogInformation("Weather data retrieved from cache for coordinates ({Latitude}, {Longitude})", latitude, longitude);
                return cachedWeather;
            }
            
            string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&hourly=temperature_2m,weathercode&forecast_days=1&timezone=auto";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();

            var weatherData = JsonSerializer.Deserialize<WeatherData>(jsonResponse);
            int currentHour = DateTime.Now.Hour;
            if (weatherData == null || weatherData.Hourly == null
                || weatherData.Hourly.temperature_2m == null || weatherData.Hourly.weathercode == null
                || currentHour >= weatherData.Hourly.temperature_2m.Length
                )
            {
                return null;
            }
            var currentTemp = weatherData.Hourly.temperature_2m[currentHour];
            var currentWeatherCode = weatherData.Hourly.weathercode[currentHour];
            var currentWeather = new CurrentWeatherData
            {
                Temperature = currentTemp,
                WeatherCode = currentWeatherCode
            };

            // Store in cache
            _cache.Set(cacheKey, currentWeather, CacheDuration);
            _logger.LogInformation("Weather data cached for {Duration} minutes", CacheDuration.TotalMinutes);
            
            return currentWeather;
        }
    }

    public class WeatherData
    {
        [JsonPropertyName("hourly")]
        public HourlyData? Hourly { get; set; } 
    }

    public class HourlyData
    {
        [JsonPropertyName("time")]
        public string[]? time { get; set; }
        [JsonPropertyName("temperature_2m")]
        public double[]? temperature_2m { get; set; }
        [JsonPropertyName("weathercode")]
        public int[]? weathercode { get; set; }
    }

    public class CurrentWeatherData
    {
        public double Temperature { get; set; }
        public int WeatherCode { get; set; }
    }
}