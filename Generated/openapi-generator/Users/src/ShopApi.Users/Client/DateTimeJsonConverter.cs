
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShopApi.Users.Client
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public static string[] Formats { get; } = {
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffK",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffK",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffK",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffK",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fK",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ssK",
            "yyyyMMddTHHmmss.fffffffK",
            "yyyyMMddTHHmmss.ffffffK",
            "yyyyMMddTHHmmss.fffffK",
            "yyyyMMddTHHmmss.ffffK",
            "yyyyMMddTHHmmss.fffK",
            "yyyyMMddTHHmmss.ffK",
            "yyyyMMddTHHmmss.fK",
            "yyyyMMddTHHmmssK",

        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null)
                throw new NotSupportedException();

            string value = reader.GetString()!;

            foreach(string format in Formats)
                if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime result))
                    return result;

            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, DateTime dateTimeValue, JsonSerializerOptions options) =>
            writer.WriteStringValue(dateTimeValue.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK", CultureInfo.InvariantCulture));
    }
}