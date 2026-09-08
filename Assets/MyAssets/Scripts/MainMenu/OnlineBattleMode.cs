using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class OnlineBattleMode : ModeBase
    {
        [SerializeField]
        private GameObject _preparing;

        public override void Setup() => isPreparing = true;

        public override void PreparingMode()
        {
            if (IsPreparing == true)
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
        public void OnClickOnlineBattleButton()
        {
            GameManager.Instance.SetGameMode(GameMode.OnlineMulti);
        }
    }
}
