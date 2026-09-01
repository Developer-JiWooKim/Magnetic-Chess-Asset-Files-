using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core;

namespace Assets.Scripts.MainMenu
{
    public sealed class AIBattleMode : ModeBase
    {
        [SerializeField]
        private GameObject preparing;

        public override void Setup()
        {
            isPreparing = false;
        }
        public override void PreparingMode()
        {
            if (IsPreparing == true)
            {
                preparing.SetActive(true);
                gameObject.GetComponent<Button>().interactable = false;
            }
            else
            {
                preparing.SetActive(false);
                gameObject.GetComponent<Button>().interactable = true;
            }
        }
        public void OnClickAIBattleButton()
        {
            GameManager.Instance.SetGameMode(GameMode.AI);
        }
    }
}
