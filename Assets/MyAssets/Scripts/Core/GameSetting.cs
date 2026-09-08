namespace Assets.MyAssets.Scripts.Core
{
    public enum GameMode { OfflineMulti = 0, AI, OnlineMulti, }

    [System.Serializable]
    public struct GameSetting
    {
        public GameMode gameMode;

        public int pieceCount;
        public int pieceCountAI;

        public int maxTurn;
        public float waitingTime;
    }
}
