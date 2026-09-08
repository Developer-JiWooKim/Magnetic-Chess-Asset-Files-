using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class ExitPanel : UIPanel
    {
        [SerializeField] private GameObject _exitPanel;

        [Header("Buttons")]
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;

        protected override void Bind()
        {
            UIBinder.Bind(_yesButton, OnClickYesButton, this, nameof(_yesButton));
            UIBinder.Bind(_noButton, Hide, this, nameof(_noButton));
        }

        protected override void Unbind()
        {
            UIBinder.Unbind(_yesButton);
            UIBinder.Unbind(_noButton);
        }

        public override void Show()
        {
            EnsureBound();
            _exitPanel.SetActive(true);
        }

        public override void Hide() => _exitPanel.SetActive(false);

        private void OnClickYesButton()
        {
            Application.Quit();
        }
    }
}
