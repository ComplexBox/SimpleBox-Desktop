using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SimpleBox.Utils
{
    internal static class DateTimeUtils
    {
        private const long InitialJavaScriptDateTicks = 621355968000000000;

        internal static long ConvertDateToJsTicks(DateTime dateTime, bool convertToUtc = true)
        {
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
            long ret;
            if (dateTime.Kind == DateTimeKind.Utc || dateTime == DateTime.MaxValue || dateTime == DateTime.MinValue)
                ret = dateTime.Ticks;
            else
            {
                long num = dateTime.Ticks - offset.Ticks;
                if (num > 3155378975999999999L)
                    ret = 3155378975999999999;
                else
                    ret = num < 0L ? 0L : num;
            }

            return ((convertToUtc
                ? dateTime.Kind == DateTimeKind.Utc
                    ? dateTime.Ticks
                    : ret
                : dateTime.Ticks) - InitialJavaScriptDateTicks) / 10000L;
        }

        internal static bool TryGetDateFromJson(
            JsonReader reader,
            out DateTime dateTime)
        {
            dateTime = new DateTime();

            if (!reader.Read() || reader.TokenType != JsonToken.Integer || reader.Value == null)
                return false;

            dateTime = new DateTime((long) reader.Value * 10000L + InitialJavaScriptDateTicks, DateTimeKind.Utc);

            return true;
        }
    }

    public sealed class MallowDateTimeConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            long javaScriptTicks = value switch
            {
                DateTime dateTime => DateTimeUtils.ConvertDateToJsTicks(dateTime.ToUniversalTime()),
                DateTimeOffset dateTimeOffset => DateTimeUtils.ConvertDateToJsTicks(dateTimeOffset
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
            if (reader.TokenType == JsonToken.Null || !DateTimeUtils.TryGetDateFromJson(reader, out DateTime dateTime))
                return null;
            return Nullable.GetUnderlyingType(objectType) == typeof(DateTimeOffset)
                ? new DateTimeOffset(dateTime)
                : (object) dateTime;
        }
    }
}
