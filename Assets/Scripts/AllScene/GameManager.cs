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
    /// <summary>
    /// 싱글톤 처리하는 함수
    /// </summary>
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
        // 60프레임 설정
        Application.targetFrameRate = 60;

        // isFirst가 0이면 게임을 처음실행 -> 디폴트로 초기화
        if (DataManager.Instance.data.isFirst == 0)
        {
            DefaultGameOption();
        }

        // 게임 세팅을 디폴트 값으로 설정 -> 유저가 변경 가능
        DefaultGameSetting();

        ChangeSceneAction += DontDestroy_Menu.Instance.ChangeGameScene;
    }
    /// <summary>
    /// 게임 처음 실행하면 호출될 함수
    /// </summary>
    private void DefaultGameOption()
    {
        Debug.Log("게임을 처음 실행함");

        OptionData data = new OptionData();

        // 사운드 디폴트(.5f)
        SoundManager.Instance.SetDefaultVolume();

        data.volume_value_SFX = .5f;
        data.volume_value_BGM = .5f;

        data.isFirst = 1; // 이후에는 디폴트값 넣지 않음

        DataManager.Instance.data = data;
    }
    /// <summary>
    /// 게임 종료 시 자동으로 옵션 값 저장(Json)
    /// </summary>
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
    /// <summary>
    /// 비동기 Scene 전환하는 함수
    /// </summary>
    public void AsyncLoadGameScene()
    {
        // 화면 전환 시 배경음 종료
        SoundManager.Instance.Stop_BGM();

        // 화면 전환 브금 재생
        SoundManager.Instance.Play_BGM(SoundManager.E_BGM_Name.SCENE_CHANGE);

        StartCoroutine(AsyncLoadScene());
    }
    /// <summary>
    /// 비동기 Scene 전환 코루틴 함수(비동기 Scene 전환 시 로딩화면도 표기해주는 코루틴 함수)
    /// </summary>
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

        ChangeSceneAction(); // 씬 전환 후 실행되어야 할 함수들을 등록한 액션 이벤트 실행

        percent.SetActive(false);
        StartCoroutine(FadeEffect_UI.FadeOut_CanvasGroup(loadingWindow.GetComponent<CanvasGroup>(), 1f, 
            () => {
                SoundManager.Instance.Play_BGM(SoundManager.E_BGM_Name.GAME);
                loadingWindow.SetActive(false);
            }));
    }
}
