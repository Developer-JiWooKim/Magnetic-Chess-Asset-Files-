using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class GameSettingMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _pieceCountDropdown;

        [SerializeField] private TMP_Dropdown _pieceCountDropdownAI;

        [SerializeField] private TMP_Dropdown _waitingTimeDropdown;

        [SerializeField] private TMP_Dropdown _maxTurnDropdown;

        private void OnEnable()
        {
            SetDefaultSetting();
        }
        public void SetPieceCount()
        {
            string option = _pieceCountDropdown.options[_pieceCountDropdown.value].text;
            int optionValue = int.Parse(option);
            GameManager.Instance.SetPieceCount(optionValue);
        }
        public void SetPieceCountAI()
        {
            string option = _pieceCountDropdownAI.options[_pieceCountDropdownAI.value].text;
            int optionValue = int.Parse(option);
            GameManager.Instance.SetPieceCountAI(optionValue);
        }
        public void SetWaitingTime()
        {
            string option = _waitingTimeDropdown.options[_waitingTimeDropdown.value].text;
            string optionDigits = Regex.Replace(option, @"[^0-9]", "");
            int optionValue = int.Parse(optionDigits);
            GameManager.Instance.SetWaitingTime(optionValue);
        }
        public void SetMaxTurn()
        {
            string option = _maxTurnDropdown.options[_maxTurnDropdown.value].text;
            int optionValue;
            if (option == "Infinity")
            {
                optionValue = 999;
            }
            else
            {
                optionValue = int.Parse(option);
            }


            GameManager.Instance.SetMaxTurn(optionValue);
        }
        public void SetDefaultSetting()
        {
            GameManager.Instance.SetPieceCount(15);
            GameManager.Instance.SetPieceCountAI(15);
            GameManager.Instance.SetWaitingTime(1);
            GameManager.Instance.SetMaxTurn(20);
        }
    }
}
