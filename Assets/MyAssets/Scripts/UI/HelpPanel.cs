using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>규칙 / 설명 두 쪽을 넘겨 보는 도움말.</summary>
    public sealed class HelpPanel : UIPanel
    {
        [SerializeField] private GameObject _root;

        [Header("Pages")]
        [SerializeField] private GameObject _rulePage;
        [SerializeField] private GameObject _descriptionPage;

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;

        /// <summary>규칙 쪽에 있는 버튼. 설명 쪽으로 넘긴다.</summary>
        [SerializeField] private Button _toDescriptionButton;

        /// <summary>설명 쪽에 있는 버튼. 규칙 쪽으로 되돌린다.</summary>
        [SerializeField] private Button _toRuleButton;

        protected override void Bind()
        {
            UIBinder.Bind(_closeButton, Hide, this, nameof(_closeButton));
            UIBinder.Bind(_toDescriptionButton, ShowDescriptionPage, this, nameof(_toDescriptionButton));
            UIBinder.Bind(_toRuleButton, ShowRulePage, this, nameof(_toRuleButton));
        }

        protected override void Unbind()
        {
            UIBinder.Unbind(_closeButton);
            UIBinder.Unbind(_toDescriptionButton);
            UIBinder.Unbind(_toRuleButton);
        }

        public override void Show()
        {
            EnsureBound();
            _root.SetActive(true);
            ShowRulePage();
        }

        public override void Hide() => _root.SetActive(false);

        private void ShowRulePage() => SetPage(isRule: true);

        private void ShowDescriptionPage() => SetPage(isRule: false);

        private void SetPage(bool isRule)
        {
            if (_rulePage != null)
            {
                _rulePage.SetActive(isRule);
            }
            if (_descriptionPage != null)
            {
                _descriptionPage.SetActive(isRule == false);
            }
        }
    }
}
