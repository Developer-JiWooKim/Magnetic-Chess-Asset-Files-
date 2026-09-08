using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    /// <summary>
    /// GameScene UI의 유일한 진입점.
    ///
    /// 예전에는 이 자리에 MenuManager(두 번째 인스턴스) + AddResumeAction이 있었고,
    /// 판을 다시 시작하려면 GameDirector.Setup()을 불러 상태를 손으로 되돌렸다.
    /// 지금은 씬을 다시 읽으므로 되돌릴 것이 남지 않는다.
    ///
    /// 그래서 대전 씬에서 모드 선택 · 설정 패널을 다시 띄울 이유도 없어졌다.
    /// 모드를 바꾸려면 타이틀로 돌아간다.
    /// </summary>
    public sealed class MatchUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private ResultPanel _resultPanel;
        [SerializeField] private ResumePanel _resumePanel;
        [SerializeField] private InGameUIManager _inGameUIManager;

        [Header("Shared")]
        [SerializeField] private CommonMenu _commonMenu;
        [SerializeField] private LoadingScreen _loadingScreen;

        [Header("Match")]
        [SerializeField] private GameDirector _gameDirector;
        [SerializeField] private Button _startButton;
        [SerializeField] private GameObject _background;

        private void Awake() => Subscribe();

        private void OnDestroy() => Unsubscribe();

        private void Start() => Setup();

        private void Subscribe()
        {
            _resumePanel.OnReplay += OnReplayRequested;
            _resumePanel.OnSelect += OnSelectModeRequested;

            _resultPanel.ReplayRequested += OnReplayRequested;
            _resultPanel.SelectModeRequested += OnSelectModeRequested;
            _resultPanel.QuitRequested += OnQuitRequested;

            _commonMenu.ResumeRequested += OnResumeMenuRequested;

            UIBinder.Bind(_startButton, OnClickStartButton, this, nameof(_startButton));
        }

        private void Unsubscribe()
        {
            if (_resumePanel != null)
            {
                _resumePanel.OnReplay -= OnReplayRequested;
                _resumePanel.OnSelect -= OnSelectModeRequested;
            }
            if (_resultPanel != null)
            {
                _resultPanel.ReplayRequested -= OnReplayRequested;
                _resultPanel.SelectModeRequested -= OnSelectModeRequested;
                _resultPanel.QuitRequested -= OnQuitRequested;
            }
            if (_commonMenu != null)
            {
                _commonMenu.ResumeRequested -= OnResumeMenuRequested;
            }

            UIBinder.Unbind(_startButton);
        }

        private void Setup()
        {
            _commonMenu.SetResumeAvailable(true);

            SoundManager.Instance.PlayBGM(SoundManager.BgmName.Game);

            _resumePanel.Hide();
            _resultPanel.Hide();

            // 판은 아직 시작하지 않았다. 배경을 깔고 시작 버튼만 띄운다.
            // GameDirector.Start()가 이미 Setup()으로 판을 초기화해 둔 상태다.
            ShowStartButton(true);
        }

        private void ShowStartButton(bool show)
        {
            _startButton.gameObject.SetActive(show);

            if (_background != null)
            {
                _background.SetActive(show);
            }
        }

        private void OnClickStartButton()
        {
            ShowStartButton(false);
            _gameDirector.GamePlay();
        }

        /// <summary>메뉴의 이어하기 버튼. 판을 멈추고 대화상자를 띄운다.</summary>
        private void OnResumeMenuRequested()
        {
            _resumePanel.Show();
        }

        /// <summary>다시하기. 씬을 다시 읽는 것이 곧 초기화다.</summary>
        private void OnReplayRequested()
        {
            _gameDirector.StopGame();
            _inGameUIManager.HideAllPanel();

            _loadingScreen.Cover(() => GameManager.Instance.ReloadMatch());
        }

        /// <summary>모드 선택으로. 타이틀로 돌아가되 모드 선택 화면부터 보여준다.</summary>
        private void OnSelectModeRequested()
        {
            _gameDirector.StopGame();
            _inGameUIManager.HideAllPanel();

            _loadingScreen.Cover(() => GameManager.Instance.LoadTitle(TitleEntry.ModeSelect));
        }

        private void OnQuitRequested()
        {
            _commonMenu.ExitPanel.Show();
        }
    }
}
