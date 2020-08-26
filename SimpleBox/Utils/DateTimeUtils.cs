using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SimpleBox.Utils
{
    internal static class DateTimeUtils
    {
        private const long InitialJavaScriptDateTicks = 621355968000000000;

        private static TimeSpan GetUtcOffset(this DateTime d) => TimeZoneInfo.Local.GetUtcOffset(d);

        private static long ToUniversalTicks(DateTime dateTime) =>
            dateTime.Kind == DateTimeKind.Utc
                ? dateTime.Ticks
                : ToUniversalTicks(dateTime, dateTime.GetUtcOffset());

        private static long ToUniversalTicks(DateTime dateTime, TimeSpan offset)
        {
            if (dateTime.Kind == DateTimeKind.Utc || dateTime == DateTime.MaxValue || dateTime == DateTime.MinValue)
                return dateTime.Ticks;
            long num = dateTime.Ticks - offset.Ticks;
            if (num > 3155378975999999999L)
                return 3155378975999999999;
            return num < 0L ? 0L : num;
        }

        internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, bool convertToUtc = true)
        {
            return UniversalTicksToJavaScriptTicks(convertToUtc ? ToUniversalTicks(dateTime) : dateTime.Ticks);
        }

        private static long UniversalTicksToJavaScriptTicks(long universalTicks)
        {
            return (universalTicks - InitialJavaScriptDateTicks) / 10000L;
        }

        private static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
        {
            return new DateTime(javaScriptTicks * 10000L + InitialJavaScriptDateTicks, DateTimeKind.Utc);
        }

        public static bool TryGetDateFromConstructorJson(
            JsonReader reader,
            out DateTime dateTime)
        {
            dateTime = new DateTime();
            if (!TryGetDateConstructorValue(reader, out var integer1) || !integer1.HasValue)
                return false;

            if (!TryGetDateConstructorValue(reader, out var integer2))
                return false;
            if (integer2.HasValue)
            {
                var longList = new List<long>
                {
                    integer1.Value,
                    integer2.Value
                };
                while (TryGetDateConstructorValue(reader, out var integer3))
                {
                    if (integer3.HasValue)
                        longList.Add(integer3.Value);
                    else
                    {
                        if (longList.Count > 7)
                            return false;

                        while (longList.Count < 7)
                            longList.Add(0L);
                        dateTime = new DateTime((int) longList[0], (int) longList[1] + 1,
                            longList[2] == 0L ? 1 : (int) longList[2], (int) longList[3], (int) longList[4],
                            (int) longList[5], (int) longList[6]);
                        goto label_16;
                    }
                }

                return false;
            }

            dateTime = ConvertJavaScriptTicksToDateTime(integer1.Value);
            label_16:
            return true;
        }

        private static bool TryGetDateConstructorValue(
            JsonReader reader,
            out long? integer)
        {
            integer = new long?();
            if (!reader.Read())
                return false;
            if (reader.TokenType == JsonToken.EndConstructor)
                return true;
            if (reader.TokenType != JsonToken.Integer)
                return false;
            if (reader.Value != null) integer = (long) reader.Value;
            return true;
        }
    }

    public sealed class MallowDateTimeConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            long javaScriptTicks = value switch
            {
                DateTime dateTime => DateTimeUtils.ConvertDateTimeToJavaScriptTicks(dateTime.ToUniversalTime()),
                DateTimeOffset dateTimeOffset => DateTimeUtils.ConvertDateTimeToJavaScriptTicks(dateTimeOffset
                    .ToUniversalTime()
                    .UtcDateTime),
                _ => throw new JsonSerializationException("Expected date object value.")
            };
            writer.WriteValue(javaScriptTicks);
        }

        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            if (!DateTimeUtils.TryGetDateFromConstructorJson(reader, out DateTime dateTime))
                return null;
            return Nullable.GetUnderlyingType(objectType) == typeof(DateTimeOffset)
                ? new DateTimeOffset(dateTime)
                : (object) dateTime;
        }
    }
}
