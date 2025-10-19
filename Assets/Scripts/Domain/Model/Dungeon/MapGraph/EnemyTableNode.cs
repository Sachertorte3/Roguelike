using Domain.Model.Character;
using Utilities.Table;
using XNode;

namespace Domain.Model.Dungeon
{
    [CreateNodeMenu("Enemy"), NodeWidth(260)]
    class EnemyTableNode : Node
    {
        public Table<EnemyData> Enemies;
        [Output] public Table<EnemyData> Output;
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "Output")
                return Enemies;
            else
                return null;
        }
    }
}