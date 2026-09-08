using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class OfflineMultiMode : ModeBase
    {
        [SerializeField] private GameObject _preparing;

        public override void Setup() => isPreparing = false;

        public override void PreparingMode()
        {
            if (IsPreparing)
            {
                _preparing.SetActive(true);
                gameObject.GetComponent<Button>().interactable = false;
            }
            else
            {
                _preparing.SetActive(false);
                gameObject.GetComponent<Button>().interactable = true;
            }
        }
        public void OnClickOfflineMultiButton()
        {
            GameManager.Instance.SetGameMode(GameMode.OfflineMulti);
        }
    }
}
