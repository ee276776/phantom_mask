using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PhantomMaskETL.Converters
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string _format = "yyyy-MM-dd HH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
                return DateTime.MinValue;

            if (DateTime.TryParseExact(stringValue, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            // 如果第一個格式失敗，嘗試其他常見格式
            if (DateTime.TryParse(stringValue, out result))
            {
                return result;
            }

            throw new JsonException($"無法解析日期時間格式：{stringValue}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }

    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private readonly string _format = "yyyy-MM-dd HH:mm:ss";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            if (DateTime.TryParseExact(stringValue, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            if (DateTime.TryParse(stringValue, out result))
            {
                return result;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(_format));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
