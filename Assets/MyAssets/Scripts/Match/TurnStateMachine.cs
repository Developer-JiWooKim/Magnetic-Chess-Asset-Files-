namespace Assets.MyAssets.Scripts.Match
{
    public enum E_GameState
    {
        None,
        Player_1,
        Player_2,
        End,
    }

    /// <summary>
    /// 한 판의 턴 진행 상태(누구 차례인지 · 몇 턴째인지 · 누가 이겼는지)를 담는다.
    /// MonoBehaviour와 Unity API에 의존하지 않는 순수 C# 클래스라 단독으로 테스트할 수 있다.
    /// </summary>
    public sealed class TurnStateMachine
    {
        private const int PLAYER_1_INDEX = 0;
        private const int PLAYER_2_INDEX = 1;

        public E_GameState Current { get; private set; }
        public E_GameState WinPlayer { get; private set; }
        public int TurnCount { get; private set; }

        /// <summary>지금이 플레이어의 턴인가(시작 전·종료 후가 아닌가).</summary>
        public bool IsPlayerTurn => Current == E_GameState.Player_1 || Current == E_GameState.Player_2;

        /// <summary>현재 턴 플레이어의 playerList 인덱스.</summary>
        public int CurrentPlayerIndex => Current == E_GameState.Player_1 ? PLAYER_1_INDEX : PLAYER_2_INDEX;

        public void Reset()
        {
            Current = E_GameState.None;
            WinPlayer = E_GameState.None;
            TurnCount = 0;
        }

        public void BeginFirstTurn()
        {
            Current = E_GameState.Player_1;
        }

        public void ChangeTurn()
        {
            Current = Current == E_GameState.Player_1 ? E_GameState.Player_2 : E_GameState.Player_1;
        }

        public void IncreaseTurnCount()
        {
            TurnCount++;
        }

        /// <summary>
        /// 승자를 확정하고 종료 상태로 넘어간다. 무승부일 때는 E_GameState.None을 넘긴다.
        /// </summary>
        public void FinishWith(E_GameState winner)
        {
            WinPlayer = winner;
            Current = E_GameState.End;
        }

        /// <summary>진행을 멈추고 시작 전 상태로 되돌린다(승자·턴 수는 유지).</summary>
        public void Stop()
        {
            Current = E_GameState.None;
        }

        /// <summary>설정한 최대 턴에 도달했는가. 2번 플레이어까지 두어야 한 턴이 끝난 것으로 본다.</summary>
        public bool IsMaxTurnReached(int maxTurn)
        {
            return maxTurn == TurnCount && Current == E_GameState.Player_2;
        }
    }
}
