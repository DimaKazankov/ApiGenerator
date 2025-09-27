
#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ShopApi.Users.Model;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ShopApi.Users.Test")]

namespace ShopApi.Users.Client
{
    public static partial class ClientUtils
    {

        public delegate void EventHandler<T>(object sender, T e) where T : EventArgs;

        public static bool TryDeserialize<T>(string json, JsonSerializerOptions options, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(json, options);
                return result != null;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public static bool TryDeserialize<T>(ref Utf8JsonReader reader, JsonSerializerOptions options, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(ref reader, options);
                return result != null;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public static string? ParameterToString(object? obj, string? format = ISO8601_DATETIME_FORMAT)
        {
            if (obj is DateTime dateTime)
                // Return a formatted date string - Can be customized with Configuration.DateTimeFormat
                // Defaults to an ISO 8601, using the known as a Round-trip date/time pattern ("o")
                // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Anchor_8
                // For example: 2009-06-15T13:45:30.0000000
                return dateTime.ToString(format);
            if (obj is DateTimeOffset dateTimeOffset)
                // Return a formatted date string - Can be customized with Configuration.DateTimeFormat
                // Defaults to an ISO 8601, using the known as a Round-trip date/time pattern ("o")
                // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Anchor_8
                // For example: 2009-06-15T13:45:30.0000000
                return dateTimeOffset.ToString(format);
            if (obj is DateOnly dateOnly)
                return dateOnly.ToString(format);
            if (obj is bool boolean)
                return boolean
                    ? "true"
                    : "false";
            if (obj is Order.StatusEnum orderStatusEnum)
                return Order.StatusEnumToJsonValue(orderStatusEnum);
            if (obj is ICollection collection)
            {
                List<string?> entries = new();
                foreach (var entry in collection)
                    entries.Add(ParameterToString(entry));
                return string.Join(",", entries);
            }

            return Convert.ToString(obj, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string UrlEncode(string input)
        {
            const int maxLength = 32766;

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length <= maxLength)
            {
                return Uri.EscapeDataString(input);
            }

            StringBuilder sb = new StringBuilder(input.Length * 2);
            int index = 0;

            while (index < input.Length)
            {
                int length = Math.Min(input.Length - index, maxLength);
                string subString = input.Substring(index, length);

                sb.Append(Uri.EscapeDataString(subString));
                index += subString.Length;
            }

            return sb.ToString();
        }

        public static string Base64Encode(string text)
        {
            return Convert.ToBase64String(global::System.Text.Encoding.UTF8.GetBytes(text));
        }

        public static byte[] ReadAsBytes(Stream inputStream)
        {
            using (var ms = new MemoryStream())
            {
                inputStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string? SelectHeaderContentType(string[] contentTypes)
        {
            if (contentTypes.Length == 0)
                return null;

            foreach (var contentType in contentTypes)
            {
                if (IsJsonMime(contentType))
                    return contentType;
            }

            return contentTypes[0]; // use the first content type specified in 'consumes'
        }

        public static string? SelectHeaderAccept(string[] accepts)
        {
            if (accepts.Length == 0)
                return null;

            if (accepts.Contains("application/json", StringComparer.OrdinalIgnoreCase))
                return "application/json";

            return string.Join(",", accepts);
        }

        [GeneratedRegex("(?i)^(application/json|[^;/ \t]+/[^;/ \t]+[+]json)[ \t]*(;.*)?$")]
        private static partial Regex JsonRegex();

        public static bool IsJsonMime(string mime)
        {
            if (string.IsNullOrWhiteSpace(mime)) return false;

            return JsonRegex().IsMatch(mime) || mime.Equals("application/json-patch+json");
        }

        public static string? GetDiscriminator(Utf8JsonReader utf8JsonReader, string discriminator)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            while (utf8JsonReader.Read())
            {
                if (startingTokenType == JsonTokenType.StartObject && utf8JsonReader.TokenType == JsonTokenType.EndObject && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (startingTokenType == JsonTokenType.StartArray && utf8JsonReader.TokenType == JsonTokenType.EndArray && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (utf8JsonReader.TokenType == JsonTokenType.PropertyName && currentDepth == utf8JsonReader.CurrentDepth - 1)
                {
                    string? localVarJsonPropertyName = utf8JsonReader.GetString();
                    utf8JsonReader.Read();

                    if (localVarJsonPropertyName != null && localVarJsonPropertyName.Equals(discriminator))
                        return utf8JsonReader.GetString();
                }
            }

            throw new JsonException("The specified discriminator was not found.");
        }

        public const string BASE_ADDRESS = "http://localhost";

        public const string SCHEME = "http";

        public const string CONTEXT_PATH = "";

        public const string HOST = "localhost";

        public const string ISO8601_DATETIME_FORMAT = "o";
    }
}