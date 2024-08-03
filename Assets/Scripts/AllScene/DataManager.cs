using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    /// <summary>
    /// Singleton Pattern
    /// </summary>
    private static DataManager instance = null;
    public static DataManager Instance
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

    // 게임 옵션 데이터 파일 이름
    private const string GameOptionFileName = "GameOptionData.json";

    public OptionData data;

    private void Awake()
    {
        SingletonSetup();
        LoadGameOptionData();
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
    public void LoadGameOptionData()
    {
        string filePath = Application.persistentDataPath + "/" + GameOptionFileName;

        if (File.Exists(filePath))
        {
            string FromJsonData = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<OptionData>(FromJsonData);
            Debug.Log("파일 불러오기 완료");
        }
        else
        {
            Debug.Log("파일 불러오기 실패");
        }
    }
    public void SaveGameOptionData()
    {
        // 클래스를 Json 형식으로 전환(true -> 가독성 좋게 작성)
        string ToJsonData = JsonUtility.ToJson(data, true);
        string filePath = Application.persistentDataPath + "/" + GameOptionFileName;

        // 이미 저장된 파일이 있다면 덮어씀, 없으면 새로 만듦
        File.WriteAllText(filePath, ToJsonData);
    }
}
