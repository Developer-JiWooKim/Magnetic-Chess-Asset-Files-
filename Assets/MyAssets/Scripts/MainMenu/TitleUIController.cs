using System;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    /// <summary>타이틀에서 보여줄 수 있는 화면.</summary>
    public enum TitleScreen
    {
        Start,
        ModeSelect,
        GameSetting,
        Loading,
    }

    /// <summary>
    /// TitleScene UI의 유일한 진입점.
    ///
    /// 패널은 서로를 모른다. 각 패널은 자기 버튼을 배선하고 "무슨 일이 있었다"만 알리고,
    /// 그 다음에 무엇을 켜고 끌지는 전부 여기 안에 갇힌다.
    /// 밖으로 나가는 것은 ShowModeSelect / ShowGameSetting / StartMatch 처럼
    /// 무엇을 하려는지만 담은 메소드다.
    ///
    /// 예전에는 이 역할을 MenuManager가 했는데, 패널을 panelName(enum)으로 리스트에서 찾았고
    /// 두 씬에 하나씩 존재했으며 태블릿 복제 UI를 손으로 맞춰 주기까지 했다.
    /// </summary>
    public sealed class TitleUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private StartPanel _startPanel;
        [SerializeField] private ModeSelectPanel _modeSelectPanel;
        [SerializeField] private GameSettingPanel _gameSettingPanel;

        [Header("Shared")]
        [SerializeField] private CommonMenu _commonMenu;
        [SerializeField] private LoadingScreen _loadingScreen;

        [Header("Camera")]
        [SerializeField] private Animator _cameraAnimator;

        // consts
        private const string TRIGGER_MOVE_START = "MoveStart";
        private const string TRIGGER_PLAY_START = "PlayStart";

        /// <summary>
        /// 화면이 바뀔 때마다 알린다.
        ///
        /// 지금은 아무도 구독하지 않는다. 나중에 태블릿 화면을 다시 넣을 때
        /// 그 뷰가 이것만 구독하면 따라오게 하려고 남겨 둔다.
        /// 예전처럼 화면 전환마다 태블릿 표시를 손으로 맞추는 코드가 다시 생기지 않도록.
        /// </summary>
        public event Action<TitleScreen> ScreenChanged;

        public TitleScreen CurrentScreen { get; private set; } = TitleScreen.Start;

        private void Awake() => Subscribe();

        private void OnDestroy() => Unsubscribe();

        private void Start() => Setup();

        private void Subscribe()
        {
            _startPanel.StartRequested += OnStartRequested;
            _modeSelectPanel.ModeSelected += OnModeSelected;
            _gameSettingPanel.PlayRequested += StartMatch;
            _gameSettingPanel.BackRequested += ShowModeSelect;
        }

        private void Unsubscribe()
        {
            if (_startPanel != null)
            {
                _startPanel.StartRequested -= OnStartRequested;
            }
            if (_modeSelectPanel != null)
            {
                _modeSelectPanel.ModeSelected -= OnModeSelected;
            }
            if (_gameSettingPanel != null)
            {
                _gameSettingPanel.PlayRequested -= StartMatch;
                _gameSettingPanel.BackRequested -= ShowModeSelect;
            }

            ScreenChanged = null;
        }

        private void Setup()
        {
            // 이어하기는 대전 중일 때만 뜻이 있다.
            _commonMenu.SetResumeAvailable(false);

            SoundManager.Instance.PlayBGM(SoundManager.BgmName.Title);

            _modeSelectPanel.Hide();
            _gameSettingPanel.Hide();

            if (GameManager.Instance.NextTitleEntry == TitleEntry.ModeSelect)
            {
                // 대전에서 모드 선택으로 돌아온 경우. 시작 화면을 거치지 않고
                // 카메라만 메뉴 위치로 보낸다. 도착하면 애니메이션 이벤트가 모드 선택을 켠다.
                _startPanel.HideImmediate();
                _cameraAnimator.SetTrigger(TRIGGER_MOVE_START);
                SetScreen(TitleScreen.Loading);
            }
            else
            {
                _startPanel.Show();
                SetScreen(TitleScreen.Start);
            }
        }

        /// <summary>
        /// 카메라가 메뉴 위치에 도착했을 때 CameraAnimationEvent가 부른다.
        /// 시작 버튼을 눌렀을 때와 대전에서 돌아왔을 때가 같은 길을 탄다.
        /// </summary>
        public void OnCameraArrivedAtMenu() => ShowModeSelect();

        public void ShowModeSelect()
        {
            _startPanel.HideImmediate();
            _gameSettingPanel.Hide();
            _modeSelectPanel.Show();

            SetScreen(TitleScreen.ModeSelect);
        }

        public void ShowGameSetting()
        {
            _modeSelectPanel.Hide();
            _gameSettingPanel.Show();

            SetScreen(TitleScreen.GameSetting);
        }

        /// <summary>화면을 덮고, 다 덮이면 대전 씬을 읽는다.</summary>
        public void StartMatch()
        {
            _cameraAnimator.SetTrigger(TRIGGER_PLAY_START);

            _modeSelectPanel.Hide();
            _gameSettingPanel.Hide();

            SetScreen(TitleScreen.Loading);

            _loadingScreen.Cover(() => GameManager.Instance.LoadMatch());
        }

        private void OnStartRequested()
        {
            _startPanel.HideAnimated(() => _cameraAnimator.SetTrigger(TRIGGER_MOVE_START));
        }

        private void OnModeSelected(GameMode mode)
        {
            GameManager.Instance.SetGameMode(mode);
            ShowGameSetting();
        }

        private void SetScreen(TitleScreen screen)
        {
            CurrentScreen = screen;
            ScreenChanged?.Invoke(screen);
        }
    }
}
