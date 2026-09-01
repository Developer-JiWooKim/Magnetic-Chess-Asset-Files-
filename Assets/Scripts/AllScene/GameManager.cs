using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton Pattern
    /// </summary>
    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public GameSetting gameSetting;

    public event Action ChangeSceneAction;

    [SerializeField]
    private GameObject loadingWindow;
    [SerializeField]
    private GameObject percent;

    private void Awake()
    {
        SingletonSetup();
    }
    private void Start()
    {
        Setup();
    }
    private void SingletonSetup()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
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
        Debug.Log("������ ó�� ������");

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
    public GameSetting DefaultGameSetting()
    {
        {
            gameSetting = new GameSetting();

            gameSetting.gameMode = GameMode.OfflineMulti;
            gameSetting.pieceCount = 20;
            gameSetting.maxTurn = 20;
            gameSetting.waitingTime = 1f;
            return gameSetting;
        }
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
