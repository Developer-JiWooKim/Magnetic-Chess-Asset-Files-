using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class AIBattleMode : ModeBase
    {
        protected override GameMode Mode => GameMode.AI;

        protected override bool PreparingByDefault => false;
    }
}
