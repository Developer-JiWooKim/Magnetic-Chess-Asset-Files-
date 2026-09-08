using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.MyAssets.Scripts.Core
{
    /// <summary>
    /// 타이틀로 돌아갈 때 어느 화면부터 보여줄지.
    /// 예전에는 DontDestroyMenu._currentScene이라는 수동 추적 값이 이 역할을 겸했는데,
    /// 씬을 새로 로드하는 지금은 "다음 타이틀의 시작 지점" 하나만 넘기면 된다.
    /// </summary>
    public enum TitleEntry
    {
        Start,
        ModeSelect,
    }

    public sealed class GameManager : Singleton<GameManager>
    {
        // consts
        private const string TITLE_SCENE = "TitleScene";
        private const string MATCH_SCENE = "GameScene";

        /// <summary>로딩이 눈에 띄지 않게 지나가지 않도록 붙잡아 두는 최소 시간.</summary>
        private const float MINIMUM_LOAD_SECONDS = 4f;

        /// <summary>100%를 보여준 뒤 실제로 넘어가기까지의 여유.</summary>
        private const float HOLD_AT_FULL_SECONDS = 0.7f;

        /// <summary>AsyncOperation.progress는 activation을 막아두면 0.9에서 멈춘다.</summary>
        private const float PROGRESS_CEILING = 0.9f;

        [SerializeField] private GameSetting _gameSetting;

        public GameSetting CurrentSetting => _gameSetting;

        /// <summary>
        /// TitleScene이 로드됐을 때 어느 패널부터 보여줄지. TitleUIController가 읽는다.
        /// GameManager는 영구 객체라 씬을 넘어 살아남으므로 이 값도 함께 넘어간다.
        /// </summary>
        public TitleEntry NextTitleEntry { get; private set; } = TitleEntry.Start;

        /// <summary>0~1. 로딩 화면이 구독한다. GameManager는 로딩 UI를 모른다.</summary>
        public event Action<float> LoadProgressChanged;

        private Coroutine _loadRoutine;

        public void SetGameMode(GameMode mode) => _gameSetting.gameMode = mode;
        public void SetPieceCount(int count) => _gameSetting.pieceCount = count;
        public void SetPieceCountAI(int count) => _gameSetting.pieceCountAI = count;
        public void SetWaitingTime(float seconds) => _gameSetting.waitingTime = seconds;
        public void SetMaxTurn(int turn) => _gameSetting.maxTurn = turn;

        private void Start() => Setup();

        private void Setup()
        {
            Application.targetFrameRate = 60;

            if (DataManager.Instance.data.isFirstRun == 0)
            {
                DefaultGameOption();
            }

            DefaultGameSetting();
        }

        private void DefaultGameOption()
        {
            OptionData data = new OptionData();

            SoundManager.Instance.SetDefaultVolume();

            data.volumeSfx = .5f;
            data.volumeBgm = .5f;

            data.isFirstRun = 1;

            DataManager.Instance.data = data;
        }

        private void OnApplicationQuit()
        {
            DataManager.Instance.SaveGameOptionData();
        }

        private void DefaultGameSetting()
        {
            _gameSetting = new GameSetting
            {
                gameMode = GameMode.OfflineMulti,
                pieceCount = 20,
                maxTurn = 20,
                waitingTime = 1f,
            };
        }

        /// <summary>대전 씬으로 간다.</summary>
        public void LoadMatch() => LoadScene(MATCH_SCENE);

        /// <summary>
        /// 판을 다시 시작한다. 예전에는 GameDirector.Setup()을 다시 불러 상태를 손으로 되돌렸는데,
        /// 씬을 통째로 다시 읽으면 되돌릴 것이 남지 않는다.
        /// </summary>
        public void ReloadMatch() => LoadScene(MATCH_SCENE);

        /// <summary>타이틀로 돌아간다. entry는 돌아가서 처음 보여줄 화면.</summary>
        public void LoadTitle(TitleEntry entry)
        {
            NextTitleEntry = entry;
            LoadScene(TITLE_SCENE);
        }

        private void LoadScene(string sceneName)
        {
            if (_loadRoutine != null)
            {
                // 로딩 중에 버튼이 한 번 더 눌린 경우. 두 번 로드하지 않는다.
                return;
            }

            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlayBGM(SoundManager.BgmName.SceneChange);

            _loadRoutine = StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            float elapsed = 0f;

            LoadProgressChanged?.Invoke(0f);

            // 실제 진행도와 최소 시간 중 더 뒤처진 쪽을 보여준다.
            // 예전에는 progress를 그대로 %로 썼는데, activation을 막아두면 0.9에서 멈추므로
            // 화면이 90%에서 멎었다가 갑자기 넘어갔다.
            while (asyncLoad.progress < PROGRESS_CEILING || elapsed < MINIMUM_LOAD_SECONDS)
            {
                elapsed += Time.deltaTime;

                float byProgress = asyncLoad.progress / PROGRESS_CEILING;
                float byTime = elapsed / MINIMUM_LOAD_SECONDS;

                LoadProgressChanged?.Invoke(Mathf.Clamp01(Mathf.Min(byProgress, byTime)));
                yield return null;
            }

            LoadProgressChanged?.Invoke(1f);

            yield return new WaitForSeconds(HOLD_AT_FULL_SECONDS);

            _loadRoutine = null;
            asyncLoad.allowSceneActivation = true;
        }
    }
}
