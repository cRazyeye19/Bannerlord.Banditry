using Banditry.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Banditry
{
    public class Main : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (!(game.GameType is Campaign) || !(gameStarter is CampaignGameStarter starter))
                return;

            if (BanditrySettings.Instance != null && BanditrySettings.Instance.RemoveModdedData)
            {
                // Uninstall mode: strip modded troops and don't write any mod data into the save.
                starter.AddBehavior(new BanditryDeactivationBehavior());
            }
            else
            {
                starter.AddBehavior(new BanditryRecruitmentBehavior());
            }
        }
    }
}
