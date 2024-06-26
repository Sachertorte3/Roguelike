using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Sentry : ICharacterType
    {
        public SentryType Type;

        public Sentry(SentryType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Sentry";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}