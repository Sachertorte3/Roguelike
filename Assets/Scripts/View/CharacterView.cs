using Assets.Scripts.View;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace View
{
    [RequireComponent(typeof(EntityView), typeof(Animator), typeof(ParticleController))]
    public class CharacterView : MonoBehaviour, IDirectional
    {
        private readonly ReactiveProperty<Direction8> _direction = new();
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;

        public Direction8 GetDirection()
        {
            return Direction.CurrentValue;
        }

        public void Construct(string characterTypeName)
        {
            var animation = Addressables
                .LoadAssetAsync<RuntimeAnimatorController>($"Assets/Animations/{characterTypeName}.controller")
                .WaitForCompletion();
            GetComponent<Animator>().runtimeAnimatorController = Instantiate(animation);
        }

        public void Turn(Direction8 direction)
        {
            _direction.OnNext(direction);
        }
    }
}