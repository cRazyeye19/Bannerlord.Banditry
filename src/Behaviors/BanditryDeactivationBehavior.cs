using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Banditry.Behaviors
{
    /// <summary>
    /// Active only when "Remove Modded Data from Save" is ticked: removes every Banditry troop
    /// from every party so the mod can be disabled without leaving orphaned troops in the save.
    /// </summary>
    internal sealed class BanditryDeactivationBehavior : CampaignBehaviorBase
    {
        private static readonly string[] ModdedTroopIds = { "ransacker", "pillager", "scrimmager", "manslayer", "vandal" };

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            foreach (MobileParty party in MobileParty.All.ToList())
            {
                foreach (string id in ModdedTroopIds)
                {
                    CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(id);
                    if (troop == null)
                        continue;

                    int members = party.MemberRoster.GetTroopCount(troop);
                    if (members > 0)
                        party.MemberRoster.AddToCounts(troop, -members);

                    int prisoners = party.PrisonRoster.GetTroopCount(troop);
                    if (prisoners > 0)
                        party.PrisonRoster.AddToCounts(troop, -prisoners);
                }
            }

            InformationManager.DisplayMessage(new InformationMessage(
                "Banditry: modded troops removed from this save. Save the game, then you can quit and disable the mod.",
                Color.FromUint(0xFF00FF13)));
        }
    }
}
