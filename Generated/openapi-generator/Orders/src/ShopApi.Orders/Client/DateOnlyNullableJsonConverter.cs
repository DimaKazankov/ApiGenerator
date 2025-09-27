
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShopApi.Orders.Client
{
    public class DateOnlyNullableJsonConverter : JsonConverter<DateOnly?>
    {
        public static string[] Formats { get; } = {
            "yyyy'-'MM'-'dd",
            "yyyyMMdd"

        };

        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            string value = reader.GetString()!;

            foreach(string format in Formats)
                if (DateOnly.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateOnly result))
                    return result;

            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? dateOnlyValue, JsonSerializerOptions options)
        {
            if (dateOnlyValue == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(dateOnlyValue.Value.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture));
        }
    }
}