using R3;
using Scripts.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.View
{
    [RequireComponent(typeof(EntityView), typeof(Animator))]
    public class CharacterView : MonoBehaviour, IDirectional
    {
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        private ReactiveProperty<Direction8> _direction = new();
        public Direction8 GetDirection() => Direction.CurrentValue;
        public void Construct(string characterTypeName)
        {
            RuntimeAnimatorController animation = Addressables
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