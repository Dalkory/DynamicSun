using System.ComponentModel.DataAnnotations;

namespace DynamicSun.Models
{
    public class WeatherData
    {
        [Key]
        public int Id { get; set; }

        // День
        [Required(ErrorMessage = "Поле 'Day' обязательно для заполнения.")]
        public int Day { get; set; }

        // Месяц
        [Required(ErrorMessage = "Поле 'Month' обязательно для заполнения.")]
        public int Month { get; set; }

        // Год
        [Required(ErrorMessage = "Поле 'Year' обязательно для заполнения.")]
        public int Year { get; set; }

        // Время
        [Required(ErrorMessage = "Поле 'Time' обязательно для заполнения.")]
        public string ?Time { get; set; }

        // Температура воздуха
        [Required(ErrorMessage = "Поле 'Temperature' обязательно для заполнения.")]
        public double Temperature { get; set; }

        // Относительная влажность
        [Required(ErrorMessage = "Поле 'RelativeHumidity' обязательно для заполнения.")]
        public double RelativeHumidity { get; set; }

        // Точка росы
        [Required(ErrorMessage = "Поле 'DewPoint' обязательно для заполнения.")]
        public double TemperatureDew { get; set; }

        // Атмосферное давление
        [Required(ErrorMessage = "Поле 'AtmosphericPressure' обязательно для заполнения.")]
        public int AtmosphericPressure { get; set; }

        // Направление ветра
        public string? WindDirection { get; set; }

        // Скорость ветра
        public int? WindSpeed { get; set; }

        // Облачность
        public int? CloudCover { get; set; }

        // Нижняя граница облачности
        public int? CloudBase { get; set; }

        // Горизонтальная видимость
        public int? Visibility { get; set; }

        // Погодные явления
        public string? WeatherPhenomena { get; set; }
    }
}
