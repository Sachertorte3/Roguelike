using RandomDungeonWithBluePrint;
using Scripts.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Provider
{
    internal class Container: LifetimeScope
    {
        [SerializeField] private FieldBluePrint _bluePrint;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputReceiver>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<TileViewContriller>();
            builder.RegisterComponent(_bluePrint);
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterEntryPoint<Presenter>(Lifetime.Scoped);
        }
    }
}
