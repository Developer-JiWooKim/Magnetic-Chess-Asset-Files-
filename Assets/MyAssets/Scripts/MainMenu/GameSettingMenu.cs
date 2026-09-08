using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class GameSettingMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _pieceCountDropdown;

        [SerializeField] private TMP_Dropdown _pieceCountDropdownAI;

        [SerializeField] private TMP_Dropdown _waitingTimeDropdown;

        [SerializeField] private TMP_Dropdown _maxTurnDropdown;

        // consts
        private const int DEFAULT_PIECE_COUNT = 15;
        private const int DEFAULT_WAITING_TIME = 1;
        private const int DEFAULT_MAX_TURN = 20;
        private const int TURN_INFINITY = 999;
        private const string INFINITY_OPTION = "Infinity";

        private void Awake()
        {
            UIBinder.Bind(_pieceCountDropdown, _ => SetPieceCount(), this, nameof(_pieceCountDropdown));
            UIBinder.Bind(_pieceCountDropdownAI, _ => SetPieceCountAI(), this, nameof(_pieceCountDropdownAI));
            UIBinder.Bind(_waitingTimeDropdown, _ => SetWaitingTime(), this, nameof(_waitingTimeDropdown));
            UIBinder.Bind(_maxTurnDropdown, _ => SetMaxTurn(), this, nameof(_maxTurnDropdown));
        }

        private void OnDestroy()
        {
            UIBinder.Unbind(_pieceCountDropdown);
            UIBinder.Unbind(_pieceCountDropdownAI);
            UIBinder.Unbind(_waitingTimeDropdown);
            UIBinder.Unbind(_maxTurnDropdown);
        }

        private void OnEnable()
        {
            SetDefaultSetting();
        }

        private void SetPieceCount()
        {
            string option = _pieceCountDropdown.options[_pieceCountDropdown.value].text;
            GameManager.Instance.SetPieceCount(int.Parse(option));
        }

        private void SetPieceCountAI()
        {
            string option = _pieceCountDropdownAI.options[_pieceCountDropdownAI.value].text;
            GameManager.Instance.SetPieceCountAI(int.Parse(option));
        }

        private void SetWaitingTime()
        {
            string option = _waitingTimeDropdown.options[_waitingTimeDropdown.value].text;
            string optionDigits = Regex.Replace(option, @"[^0-9]", "");
            GameManager.Instance.SetWaitingTime(int.Parse(optionDigits));
        }

        private void SetMaxTurn()
        {
            string option = _maxTurnDropdown.options[_maxTurnDropdown.value].text;
            int optionValue = option == INFINITY_OPTION ? TURN_INFINITY : int.Parse(option);

            GameManager.Instance.SetMaxTurn(optionValue);
        }

        private void SetDefaultSetting()
        {
            GameManager.Instance.SetPieceCount(DEFAULT_PIECE_COUNT);
            GameManager.Instance.SetPieceCountAI(DEFAULT_PIECE_COUNT);
            GameManager.Instance.SetWaitingTime(DEFAULT_WAITING_TIME);
            GameManager.Instance.SetMaxTurn(DEFAULT_MAX_TURN);
        }
    }
}
