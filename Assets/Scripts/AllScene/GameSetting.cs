public enum GameMode { OfflineMulti = 0, AI, OnlineMulti, } // ���� AI ����� �����ϸ� AI������� ��(������ ���̸� �¶��� ������ �ǰ�)

[System.Serializable]
public struct GameSetting
{
    public GameMode gameMode;       // ���� ��� ����

    public int pieceCount;          // �ǽ� ����
    public int pieceCount_AI;       // AI����϶��� AI�ǽ� ������ ���� ����

    public int maxTurn;           // �� ��
    public float waitingTime;         // ��� �ð�    
}
