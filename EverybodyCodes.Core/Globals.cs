using System.Text.Json;
using System.Text.Json.Serialization;

namespace EverybodyCodes.Core
{
    public static class Globals
    {
        public static bool IsTest { get; set; }
        public static IArgs? Args { get; set; }

        public interface IArgs
        {
            public abstract object Value { get; }

            public T Get<T>()
            where T : notnull
            => ((IArgs<T>)this).Value;

            public IArgs this[string key]
            => ((Dictionary)this).Value[key];

            public IArgs this[int index]
            => ((Array)this).Value[index];

            public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General)
            {
                AllowTrailingCommas = true,
                Converters =
                {
                    new ArgsConverter(),
                }
            };

            private sealed class ArgsConverter : JsonConverter<IArgs>
            {
                public override IArgs? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.StartObject:
                            return new Dictionary { Value = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IArgs>>(ref reader, options)! };
                        case JsonTokenType.StartArray:
                            return new Array { Value = JsonSerializer.Deserialize<IReadOnlyList<IArgs>>(ref reader, options)! };
                        case JsonTokenType.String:
                            return new String { Value = JsonSerializer.Deserialize<string>(ref reader, options)! };
                        case JsonTokenType.Number:
                            if (reader.TryGetInt32(out var i))
                                return new Int { Value = i };
                            if (reader.TryGetInt64(out var l))
                                return new Long { Value = l };
                            return new Decimal { Value = reader.GetDecimal() };
                        case JsonTokenType.Null:
                            return null;
                    }
                    throw new NotImplementedException();
                }

                public override void Write(Utf8JsonWriter writer, IArgs value, JsonSerializerOptions options)
                { }
            }
        }

        public interface IArgs<T> : IArgs
        where T : notnull
        {
            object IArgs.Value => Value;
            public new T Value { get; }
        }

        private sealed class String : IArgs<string>
        {
            public required string Value { get; init; }
        }

        private sealed class Int : IArgs<int>
        {
            public required int Value { get; init; }
        }

        private sealed class Long : IArgs<long>
        {
            public required long Value { get; init; }
        }

        private sealed class Decimal : IArgs<decimal>
        {
            public required decimal Value { get; init; }
        }

        private sealed class Array : IArgs<IReadOnlyList<IArgs>>
        {
            public required IReadOnlyList<IArgs> Value { get; init; }
        }

        private sealed class Dictionary : IArgs<IReadOnlyDictionary<string, IArgs>>
        {
            public required IReadOnlyDictionary<string, IArgs> Value { get; init; }
        }
    }
}
