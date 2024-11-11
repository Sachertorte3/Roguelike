using System;

namespace Stats
{
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