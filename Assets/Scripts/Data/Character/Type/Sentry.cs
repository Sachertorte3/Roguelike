using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Sentry : ICharacterType
    {
        public SentryType Type; public string TypeName() => "Sentry";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Sentry(SentryType type) { Type = type; }
    }
}