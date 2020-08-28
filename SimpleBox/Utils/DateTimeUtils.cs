using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace SimpleBox.Utils
{
    internal static class DateTimeUtils
    {
        public const long InitialJavaScriptDateTicks = 621355968000000000;

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
    }

    public sealed class MallowDateTimeConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) return;
            long javaScriptTicks = DateTimeUtils.ConvertDateToJsTicks(((DateTime) value).ToUniversalTime());
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
            return new DateTime((long)JToken.Load(reader) * 10000L + DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc).ToLocalTime();
        }
    }
}
