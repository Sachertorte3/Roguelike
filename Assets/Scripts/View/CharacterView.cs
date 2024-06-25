using System;
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
        private SpriteRenderer _groupMarker;
        private SpriteHpBar _hpBar;
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;

        public Direction8 GetDirection()
        {
            return Direction.CurrentValue;
        }

        public void Construct(string characterTypeName, bool isEnemy, bool isAlly)
        {
            var animation = Addressables
                .LoadAssetAsync<RuntimeAnimatorController>($"Assets/Animations/{characterTypeName}.controller")
                .WaitForCompletion();
            GetComponent<Animator>().runtimeAnimatorController = Instantiate(animation);

            var groupMarker = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/GroupMarker.prefab")
                .WaitForCompletion();
            _groupMarker = Instantiate(groupMarker, transform).GetComponent<SpriteRenderer>();
            UpdateGroupMarker(isEnemy, isAlly);

            var hpBar = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/HpBar.prefab").WaitForCompletion();
            _hpBar = Instantiate(hpBar, transform).GetComponent<SpriteHpBar>();
        }

        public void SetScale(float value)
        {
            transform.localScale = new(value, value, 1);
        }

        public void Turn(Direction8 direction)
        {
            _direction.OnNext(direction);
        }

        public void UpdateGroupMarker(bool isEnemy, bool isAlly)
        {
            var color = (isEnemy, isAlly) switch
            {
                (true, false) => new Color(1, 0, 0, 0.5f),
                (false, true) => new Color(0, 1, 0, 0.5f),
                (true, true) => throw new InvalidOperationException("A character cannot be both an enemy and an ally."),
                _ => Color.clear
            };
            _groupMarker.color = color;
        }

        public void UpdateHpBar(float maxHp, float hp)
        {
            _hpBar.SetValue(maxHp, hp);
        }
    }
}