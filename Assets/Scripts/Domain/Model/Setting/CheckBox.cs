using R3;

namespace Domain.Model.Setting
{
    public record CheckBox : IOptionInput
    {
        public readonly string Name;

        public CheckBox(string name, bool defaultValue)
        {
            Name = name;
            _value = new ReactiveProperty<bool>(defaultValue);
        }

        private readonly ReactiveProperty<bool> _value;
        public ReadOnlyReactiveProperty<bool> Value => _value;
        public bool CurrentValue => Value.CurrentValue;

        public void SetValue(bool value)
        {
            _value.Value = value;
        }
    }
}