using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Sentry : ICharacterType
    {
        public SentryType Type; public string Name() => "Sentry";
        public string TypeName() => $"{Name()}{Type}"; public Sentry(SentryType type) { Type = type; }
    }
}