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
        [SerializeField] private GameSetting gameSetting;
        [SerializeField] private GameObject loadingWindow;
        [SerializeField] private GameObject percent;

        public GameSetting CurrentSetting => gameSetting;

        public void SetGameMode(GameMode mode) => gameSetting.gameMode = mode;
        public void SetPieceCount(int count) => gameSetting.pieceCount = count;
        public void SetPieceCount_AI(int count) => gameSetting.pieceCount_AI = count;
        public void SetWaitingTime(float seconds) => gameSetting.waitingTime = seconds;
        public void SetMaxTurn(int turn) => gameSetting.maxTurn = turn;

        public event Action ChangeSceneAction;

        private void Start() => Setup();
        private void Setup()
        {
            Application.targetFrameRate = 60;

            if (DataManager.Instance.data.isFirst == 0)
            {
                DefaultGameOption();
            }

            DefaultGameSetting();

            ChangeSceneAction += DontDestroy_Menu.Instance.ChangeGameScene;
        }

        private void DefaultGameOption()
        {
            OptionData data = new OptionData();

            SoundManager.Instance.SetDefaultVolume();

            data.volume_value_SFX = .5f;
            data.volume_value_BGM = .5f;

            data.isFirst = 1;

            DataManager.Instance.data = data;
        }
        private void OnApplicationQuit()
        {
            DataManager.Instance.SaveGameOptionData();
        }
        private void DefaultGameSetting()
        {
            gameSetting = new GameSetting
            {
                gameMode = GameMode.OfflineMulti,
                pieceCount = 20,
                maxTurn = 20,
                waitingTime = 1f,
            };
        }

        public void AsyncLoadGameScene()
        {
            SoundManager.Instance.Stop_BGM();

            SoundManager.Instance.Play_BGM(SoundManager.E_BGM_Name.SCENE_CHANGE);

            StartCoroutine(AsyncLoadScene());
        }

        private IEnumerator AsyncLoadScene()
        {
            loadingWindow.SetActive(true);
            percent.SetActive(true);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
            asyncLoad.allowSceneActivation = false;

            TextMeshProUGUI percentText = percent.GetComponentInChildren<TextMeshProUGUI>();

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

            percent.SetActive(false);
            StartCoroutine(FadeEffect_UI.FadeOut_CanvasGroup(loadingWindow.GetComponent<CanvasGroup>(), 1f,
                () =>
                {
                    SoundManager.Instance.Play_BGM(SoundManager.E_BGM_Name.GAME);
                    loadingWindow.SetActive(false);
                }));
        }
    }
}
