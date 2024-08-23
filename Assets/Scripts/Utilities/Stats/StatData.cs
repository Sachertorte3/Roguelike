using System;

namespace Stats
{
    [Serializable]
    public class StatData
    {
        public float BaseValue;
        public float AdditiveValue;
        public float MultiplicativeValue;
        public StatData(float baseValue, float additiveValue=0, float multiplicativeValue=1)
        {
            BaseValue = baseValue;
            AdditiveValue = additiveValue;
            MultiplicativeValue = multiplicativeValue;
        }
    }
    [Serializable]
    public class ResourceData
    {
        public StatData Max;
        public float Value;
        public ResourceData(StatData max, float value)
        {
            Max = max;
            Value = value;
        }
    }
}