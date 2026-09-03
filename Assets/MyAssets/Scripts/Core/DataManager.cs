using System.IO;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Core
{
    public sealed class DataManager : Singleton<DataManager>
    {
        private const string GameOptionFileName = "GameOptionData.json";

        public OptionData data;

        protected override void Awake()
        {
            base.Awake();
            LoadGameOptionData();
        }

        public void LoadGameOptionData()
        {
            string filePath = Application.persistentDataPath + "/" + GameOptionFileName;

            if (File.Exists(filePath))
            {
                string FromJsonData = File.ReadAllText(filePath);
                data = JsonUtility.FromJson<OptionData>(FromJsonData);
            }
        }
        public void SaveGameOptionData()
        {

            string ToJsonData = JsonUtility.ToJson(data, true);
            string filePath = Application.persistentDataPath + "/" + GameOptionFileName;


            File.WriteAllText(filePath, ToJsonData);
        }
    }
}
