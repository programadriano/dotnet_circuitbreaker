using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace API.Services
{
    public class TestClientService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;


        public TestClientService(HttpClient httpClient, AsyncCircuitBreakerPolicy circuitBreaker)
        {
            _httpClient = httpClient;
            _circuitBreaker = circuitBreaker;

        }

        public async Task<List<WeatherForecast>> GetWeather()
        {
            var result = new List<WeatherForecast>();


            if (_circuitBreaker.CircuitState == CircuitState.Closed || _circuitBreaker.CircuitState == CircuitState.HalfOpen)
            {
                try
                {
                    result = await _circuitBreaker.ExecuteAsync(() =>
                    {
                       return IsClosed();
                    });
                }
                catch (Exception)
                {

                    return IsOpen();
                }
            }

            if (_circuitBreaker.CircuitState == CircuitState.Open)
            {
                return IsOpen();
            }

            return result;
        }


        public async Task<List<WeatherForecast>> IsClosed()
        {
            return await _httpClient.GetFromJsonAsync<List<WeatherForecast>>("WeatherForecast");
        }

        public List<WeatherForecast> IsOpen()
        {
            var result = new List<WeatherForecast>();

            var rng = new Random();
            var wetherForecast = new WeatherForecast
            {
                Date = DateTime.Now,
                Summary = "Freezing - Cache",
                TemperatureC = rng.Next(-20, 55)
            };

            result.Add(wetherForecast);

            return result;
        }

    }
}
