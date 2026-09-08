using System.IO;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Core
{
    public sealed class DataManager : Singleton<DataManager>
    {
        // const
        private const string GAME_OPTION_FILE_NAME = "GameOptionData.json";

        public OptionData data;

        protected override void Awake()
        {
            base.Awake();
            LoadGameOptionData();
        }

        public void LoadGameOptionData()
        {
            string filePath = Application.persistentDataPath + "/" + GAME_OPTION_FILE_NAME;

            if (File.Exists(filePath))
            {
                string fromJsonData = File.ReadAllText(filePath);
                data = JsonUtility.FromJson<OptionData>(fromJsonData);
            }
        }
        public void SaveGameOptionData()
        {
            string toJsonData = JsonUtility.ToJson(data, true);
            string filePath = Application.persistentDataPath + "/" + GAME_OPTION_FILE_NAME;

            File.WriteAllText(filePath, toJsonData);
        }
    }
}
