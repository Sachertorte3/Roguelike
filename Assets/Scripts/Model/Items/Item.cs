using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Model.Items
{
    public class Item
    {
        public Skill Skill;
        public int UsableTimes;
        public Item(Skill skill, int usableTimes)
        {
            Skill = skill;
            UsableTimes = usableTimes;
        }
        public async UniTask Use(IActor actor, Direction8 direction)
        {
            await Skill.Use(actor, direction);
            UsableTimes--;
        }
    }
}
