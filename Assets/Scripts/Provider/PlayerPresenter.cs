#nullable enable
using Scripts.Model.Characters;
using Scripts.View;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Scripts.Provider
{
    public class PlayerPresenter
    {
        [Inject]
        public PlayerPresenter(CharacterManager characterManager, SynchronizedCharacterView characters, CameraFollowTarget camera)
        {
            CharacterView playerView = characters.Get(characterManager.Player);

            GameObject arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab").WaitForCompletion();
            GameObject arrow = GameObject.Instantiate(arrowPrefab, playerView.transform);
            arrow.GetComponent<CharacterArrow>().Constract(playerView);

            camera.SetTarget(playerView.gameObject);
        }
    }
}