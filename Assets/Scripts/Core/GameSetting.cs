namespace Assets.Scripts.Core
{
    public enum GameMode { OfflineMulti = 0, AI, OnlineMulti, }

    [System.Serializable]
    public struct GameSetting
    {
        public GameMode gameMode;

        public int pieceCount;
        public int pieceCount_AI;

        public int maxTurn;
        public float waitingTime;
    }
}
