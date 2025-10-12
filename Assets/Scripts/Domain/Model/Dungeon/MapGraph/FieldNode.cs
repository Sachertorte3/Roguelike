#nullable enable
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using Utilities.Table;
using XNode;

namespace Domain.Model.Dungeon
{
    [CreateNodeMenu("Field"), NodeWidth(300)]
    class FieldNode : Node
    {
        [Required] public Table<FieldBluePrint> Fields;
        [Output] public Table<FieldBluePrint> Output;
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "Output")
                return Fields;
            else
                return null;
        }
    }
}