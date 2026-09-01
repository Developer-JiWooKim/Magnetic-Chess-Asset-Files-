namespace Assets.Scripts.Match
{
    public enum PlayerName { Player_1, Player_2, Player_AI }

    public sealed class Player
    {
        public PlayerName playerName;

        private int pieceCount;

        public int PieceCount
        {
            get
            {
                return pieceCount;
            }
            set
            {
                pieceCount = value;
            }
        }

        public Player(PlayerName _playerName) => playerName = _playerName;
    }
}
