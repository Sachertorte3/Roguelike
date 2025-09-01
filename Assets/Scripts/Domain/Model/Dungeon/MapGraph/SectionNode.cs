using XNode;

namespace Domain.Model.Dungeon
{
    [CreateNodeMenu("Section")]
    class SectionNode : Node
    {
        public SectionData Data;
        [Output] public SectionData Output;
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "Output")
                return Data;
            else
                return null;
        }
    }
}