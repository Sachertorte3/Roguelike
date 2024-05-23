using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;

namespace Model.Entities
{
    public interface IEntity
    {
        public Entity Entity { get; }
        public ReadOnlyReactiveProperty<Vector2Int> Position { get; }
        public Vector2Int CurrentPosition { get; }
        public ReadOnlyReactiveProperty<bool> Visibility { get; }
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove { get; }
        public Observable<Vector2Int> OnTeleport { get; }
        public void SetVisiblity(bool visiblity)
        {
            Entity.SetVisibility(visiblity);
        }
    }
}
