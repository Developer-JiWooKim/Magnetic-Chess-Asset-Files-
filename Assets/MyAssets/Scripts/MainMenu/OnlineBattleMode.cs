using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class OnlineBattleMode : ModeBase
    {
        protected override GameMode Mode => GameMode.OnlineMulti;

        protected override bool PreparingByDefault => true;
    }
}
