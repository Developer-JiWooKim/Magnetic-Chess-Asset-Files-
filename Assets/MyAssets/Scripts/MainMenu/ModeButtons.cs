using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class ModeButtons : MonoBehaviour
    {
        [SerializeField] private List<ModeBase> _buttons;

        private void Start() => Setup();

        private void Setup()
        {
            _buttons = GetComponentsInChildren<ModeBase>().ToList();

            if (_buttons == null)
            {
                Debug.Log("ModeButtons.cs - Setup() : _buttons is null!!");
                return;
            }

            _buttons.ForEach(modeButton =>
            {
                modeButton.Setup();
                modeButton.PreparingMode();
            });
        }
    }
}
