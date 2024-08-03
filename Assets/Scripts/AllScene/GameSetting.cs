public enum GameMode { OfflineMulti = 0, AI, OnlineMulti, } // 추후 AI 기능을 구현하면 AI대전모드 용(서버를 붙이면 온라인 대전도 되게)

[System.Serializable]
public struct GameSetting
{
    public GameMode gameMode;       // 게임 모드 설정

    public int pieceCount;          // 피스 개수
    public int pieceCount_AI;       // AI모드일때는 AI피스 개수를 따로 설정

    public int maxTurn;           // 턴 수
    public float waitingTime;         // 대기 시간    
}
