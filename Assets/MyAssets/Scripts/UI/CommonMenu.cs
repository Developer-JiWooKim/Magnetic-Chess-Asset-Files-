using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 두 씬이 똑같이 쓰는 메뉴 묶음(막대 · 도움말 · 옵션 · 종료 · 이어하기).
    /// 프리팹 하나로 만들어 씬마다 하나씩 배치한다.
    ///
    /// 예전에는 이 묶음이 DontDestroyOnLoad 캔버스에 하나만 있으면서 두 씬을 겸했다.
    /// 그래서 "지금 어느 씬인가"를 수동으로 추적해 크기·간격·이어하기 버튼 표시를 분기해야 했다.
    /// 씬마다 인스턴스가 따로 있으면 그 분기는 인스펙터 값의 차이가 된다.
    ///
    /// 이어하기만은 어느 씬이냐가 아니라 "대전 중인가"의 문제라 이벤트로 밖에 넘긴다.
    /// CommonMenu는 대전 쪽 타입을 알지 않는다.
    /// </summary>
    public sealed class CommonMenu : MonoBehaviour
    {
        [SerializeField] private MenuBar _menuBar;
        [SerializeField] private MenuList _menuList;

        [Header("Buttons")]
        [SerializeField] private Button _helpButton;
        [SerializeField] private Button _optionButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _resumeButton;

        [Header("Panels")]
        [SerializeField] private HelpPanel _helpPanel;
        [SerializeField] private OptionPanel _optionPanel;
        [SerializeField] private ExitPanel _exitPanel;

        /// <summary>이어하기 버튼이 눌렸다. 대전 씬의 컨트롤러가 구독한다.</summary>
        public event Action ResumeRequested;

        /// <summary>결과 화면의 종료 버튼도 같은 대화상자를 쓴다.</summary>
        public ExitPanel ExitPanel => _exitPanel;

        private void Awake()
        {
            UIBinder.Bind(_helpButton, OnClickHelpButton, this, nameof(_helpButton));
            UIBinder.Bind(_optionButton, OnClickOptionButton, this, nameof(_optionButton));
            UIBinder.Bind(_exitButton, OnClickExitButton, this, nameof(_exitButton));
            UIBinder.Bind(_resumeButton, OnClickResumeButton, this, nameof(_resumeButton));
        }

        private void OnDestroy()
        {
            UIBinder.Unbind(_helpButton);
            UIBinder.Unbind(_optionButton);
            UIBinder.Unbind(_exitButton);
            UIBinder.Unbind(_resumeButton);

            ResumeRequested = null;
        }

        private void Start() => Setup();

        private void Setup()
        {
            _helpPanel.Hide();
            _optionPanel.Hide();
            _exitPanel.Hide();
        }

        /// <summary>
        /// 이어하기 버튼을 이 씬에서 쓰는지 정한다. 타이틀은 false, 대전 씬은 true.
        /// 씬의 컨트롤러가 Start에서 한 번 부른다.
        /// </summary>
        public void SetResumeAvailable(bool available)
        {
            if (_resumeButton != null)
            {
                _menuList.SetAvailable(_resumeButton.gameObject, available);
            }
        }

        private void OnClickHelpButton()
        {
            _helpPanel.Show();
        }

        private void OnClickOptionButton()
        {
            _optionPanel.Show();
            _menuBar.Collapse();
        }

        private void OnClickExitButton()
        {
            _exitPanel.Show();
            _menuBar.Collapse();
        }

        private void OnClickResumeButton()
        {
            _menuBar.Collapse();
            ResumeRequested?.Invoke();
        }
    }
}
