using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace Banditry.Data
{
    /// <summary>The troops currently available to recruit from one hideout or town.</summary>
    internal sealed class RecruitPool
    {
        [SaveableProperty(1)]
        public string SettlementId { get; set; }

        [SaveableProperty(2)]
        public TroopRoster Troops { get; set; }

        public int Count => Troops.TotalManCount;

        public RecruitPool()
        {
            Troops = TroopRoster.CreateDummyTroopRoster();
        }

        public RecruitPool(string settlementId) : this()
        {
            SettlementId = settlementId;
        }
    }
}
