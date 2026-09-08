using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class OfflineMultiMode : ModeBase
    {
        protected override GameMode Mode => GameMode.OfflineMulti;

        protected override bool PreparingByDefault => false;
    }
}
