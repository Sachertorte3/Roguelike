#nullable enable
using R3;

namespace Domain.Model.Setting
{
    public record CheckBox : IOptionInput
    {
        public readonly string Name;
        public readonly ReadOnlyReactiveProperty<bool> IsEnabled;

        public CheckBox(string name, bool defaultValue, ReactiveProperty<bool>? isEnabled = null)
        {
            Name = name;
            Value = new ReactiveProperty<bool>(defaultValue);
            IsEnabled = isEnabled ?? new ReactiveProperty<bool>(true);
        }

        public readonly ReactiveProperty<bool> Value;
        public bool CurrentValue => Value.CurrentValue;
    }
}