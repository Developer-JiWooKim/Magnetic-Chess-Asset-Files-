using UnityEngine;


public class DontDestroy_Menu : MonoBehaviour
{
    // title, game 씬에서 사용되는 메뉴를 파괴되지 않는 오브젝트로 설정
    private static DontDestroy_Menu instance = null;
    public static DontDestroy_Menu Instance { get { return instance;  } }
    public enum SceneName
    {
        Title,
        Game,
    }
    private SceneName currentScene = SceneName.Title;
    public SceneName CurrentScene
    {
        get
        {
            return currentScene;
        }
        private set
        {
            currentScene = value;
        }
    }

    private void Awake()
    {
        SingletonSetup();
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

    /// <summary>
    /// currentScene -> Game
    /// </summary>
    public void ChangeGameScene()
    {
        currentScene = SceneName.Game;
    }

    /// <summary>
    /// currentScene -> Title
    /// </summary>
    public void ChangeTitleScene()
    {
        currentScene = SceneName.Title;
    }
}
