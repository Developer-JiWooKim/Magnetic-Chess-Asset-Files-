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

    // ���� �ɼ� ������ ���� �̸�
    private const string GameOptionFileName = "GameOptionData.json";

    public OptionData data;

    private void Awake()
    {
        SingletonSetup();
        LoadGameOptionData();
    }
    /// <summary>
    /// �̱��� ó���ϴ� �Լ�
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
            Debug.Log("���� �ҷ����� �Ϸ�");
        }
        else
        {
            Debug.Log("���� �ҷ����� ����");
        }
    }
    public void SaveGameOptionData()
    {
        // Ŭ������ Json �������� ��ȯ(true -> ������ ���� �ۼ�)
        string ToJsonData = JsonUtility.ToJson(data, true);
        string filePath = Application.persistentDataPath + "/" + GameOptionFileName;

        // �̹� ����� ������ �ִٸ� ���, ������ ���� ����
        File.WriteAllText(filePath, ToJsonData);
    }
}
