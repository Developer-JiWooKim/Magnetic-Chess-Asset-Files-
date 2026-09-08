using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Core
{
    public sealed class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameSetting _gameSetting;
        [SerializeField] private GameObject _loadingWindow;
        [SerializeField] private GameObject _percent;

        public GameSetting CurrentSetting => _gameSetting;

        public void SetGameMode(GameMode mode) => _gameSetting.gameMode = mode;
        public void SetPieceCount(int count) => _gameSetting.pieceCount = count;
        public void SetPieceCountAI(int count) => _gameSetting.pieceCountAI = count;
        public void SetWaitingTime(float seconds) => _gameSetting.waitingTime = seconds;
        public void SetMaxTurn(int turn) => _gameSetting.maxTurn = turn;

        public event Action ChangeSceneAction;

        private void Start() => Setup();
        private void Setup()
        {
            Application.targetFrameRate = 60;

            if (DataManager.Instance.data.isFirstRun == 0)
            {
                DefaultGameOption();
            }

            DefaultGameSetting();

            ChangeSceneAction += DontDestroyMenu.Instance.ChangeGameScene;
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

        public void AsyncLoadGameScene()
        {
            SoundManager.Instance.StopBGM();

            SoundManager.Instance.PlayBGM(SoundManager.BgmName.SceneChange);

            StartCoroutine(AsyncLoadScene());
        }

        private IEnumerator AsyncLoadScene()
        {
            _loadingWindow.SetActive(true);
            _percent.SetActive(true);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
            asyncLoad.allowSceneActivation = false;

            TextMeshProUGUI percentText = _percent.GetComponentInChildren<TextMeshProUGUI>();

            int progressPercentage = 0;
            float time = 0;
            float progress;

            percentText.text = "Loading...\n" + "0%";

            while (!asyncLoad.isDone)
            {
                progress = asyncLoad.progress;
                progressPercentage = Mathf.RoundToInt(progress * 100f);
                percentText.text = "Loading...\n" + progressPercentage.ToString() + "%";
                time += Time.deltaTime;
                if (time > 4f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }

            percentText.text = "Loading...\n" + "100%";

            yield return new WaitForSeconds(0.7f);

            ChangeSceneAction();

            _percent.SetActive(false);
            StartCoroutine(FadeEffectUI.FadeOutCanvasGroup(_loadingWindow.GetComponent<CanvasGroup>(), 1f,
                () =>
                {
                    SoundManager.Instance.PlayBGM(SoundManager.BgmName.Game);
                    _loadingWindow.SetActive(false);
                }));
        }
    }
}
