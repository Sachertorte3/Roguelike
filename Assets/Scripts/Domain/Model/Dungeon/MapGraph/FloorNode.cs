using XNode;

namespace Domain.Model.Dungeon
{
    [CreateNodeMenu("Floor")]
    class FloorNode : Node
    {
        public FloorData Data;
        [Output] public FloorData Output;
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "Output")
                return Data;
            else
                return null;
        }
    }
}