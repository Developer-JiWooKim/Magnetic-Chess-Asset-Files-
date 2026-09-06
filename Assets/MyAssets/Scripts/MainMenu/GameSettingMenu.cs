using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class GameSettingMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown pieceCountDropdown;

        [SerializeField] private TMP_Dropdown pieceCountDropdown_AI;

        [SerializeField] private TMP_Dropdown waitingTimeDropdown;

        [SerializeField] private TMP_Dropdown maxTurnDropdown;

        private void OnEnable()
        {
            SetDefaultSetting();
        }
        public void SetPieceCount()
        {
            string option = pieceCountDropdown.options[pieceCountDropdown.value].text;
            int option_value = int.Parse(option);
            GameManager.Instance.SetPieceCount(option_value);
        }
        public void SetPieceCount_AI()
        {
            string option = pieceCountDropdown_AI.options[pieceCountDropdown_AI.value].text;
            int option_value = int.Parse(option);
            GameManager.Instance.SetPieceCount_AI(option_value);
        }
        public void SetWaitingTime()
        {
            string option = waitingTimeDropdown.options[waitingTimeDropdown.value].text;
            string option_int = Regex.Replace(option, @"[^0-9]", "");
            int option_value = int.Parse(option_int);
            GameManager.Instance.SetWaitingTime(option_value);
        }
        public void SetMaxTurn()
        {
            string option = maxTurnDropdown.options[maxTurnDropdown.value].text;
            int option_value;
            if (option == "Infinity")
            {
                option_value = 999;
            }
            else
            {
                option_value = int.Parse(option);
            }


            GameManager.Instance.SetMaxTurn(option_value);
        }
        public void SetDefaultSetting()
        {
            GameManager.Instance.SetPieceCount(15);
            GameManager.Instance.SetPieceCount_AI(15);
            GameManager.Instance.SetWaitingTime(1);
            GameManager.Instance.SetMaxTurn(20);
        }
    }
}
