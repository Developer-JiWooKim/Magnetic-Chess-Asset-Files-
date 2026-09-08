namespace Assets.MyAssets.Scripts.Match
{
    public enum PlayerName { Player1, Player2, PlayerAI }

    public sealed class Player
    {
        public PlayerName playerName;

        private int _pieceCount;

        public int PieceCount
        {
            get
            {
                return _pieceCount;
            }
            set
            {
                _pieceCount = value;
            }
        }

        public Player(PlayerName playerName) => this.playerName = playerName;
    }
}
