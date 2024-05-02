using RandomDungeonWithBluePrint;
using Scripts.Model.Setting;
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
using Scripts.View.UI;

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
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterEntryPoint<Presenter>(Lifetime.Scoped);
            builder.RegisterEntryPoint<SettingPresenter>(Lifetime.Scoped);
        }
    }
}
