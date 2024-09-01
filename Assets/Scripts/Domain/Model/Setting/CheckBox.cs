using R3;

namespace Domain.Model.Setting
{
    public record CheckBox : IOptionInput
    {
        public readonly string Name;

        public CheckBox(string name, bool defaultValue)
        {
            Name = name;
            OnValueChanged = new ReactiveProperty<bool>(defaultValue);
        }

        public bool Value => OnValueChanged.Value;
        public ReactiveProperty<bool> OnValueChanged { get; }

        public void SetValue(bool value)
        {
            OnValueChanged.Value = value;
        }
    }
}