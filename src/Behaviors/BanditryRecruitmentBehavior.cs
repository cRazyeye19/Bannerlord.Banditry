using System;
using System.Collections.Generic;
using System.Linq;
using Banditry.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Banditry.Behaviors
{
    /// <summary>
    /// Lets the player recruit bandits from infested hideouts and looters from friendly town gang leaders.
    /// Each location keeps its own pool of recruits that refills daily.
    /// </summary>
    internal sealed class BanditryRecruitmentBehavior : CampaignBehaviorBase
    {
        private const string LooterId = "looter";

        private Dictionary<string, RecruitPool> _hideoutPools = new Dictionary<string, RecruitPool>();
        private Dictionary<string, RecruitPool> _townPools = new Dictionary<string, RecruitPool>();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("Banditry_hideoutPools", ref _hideoutPools);
            dataStore.SyncData("Banditry_townPools", ref _townPools);
            _hideoutPools = _hideoutPools ?? new Dictionary<string, RecruitPool>();
            _townPools = _townPools ?? new Dictionary<string, RecruitPool>();
        }

        // ---------------------------------------------------------------- menus

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddGameMenuOption("hideout_place", "banditry_hideout_recruit", "Recruit bandits",
                HideoutRecruitCondition, args => OpenRecruitment(_hideoutPools, Settlement.CurrentSettlement, "Bandits willing to join you"),
                false, 1, false);

            starter.AddGameMenuOption("town", "banditry_town_recruit", "Recruit from gang leaders",
                TownRecruitCondition, args => OpenRecruitment(_townPools, Settlement.CurrentSettlement, "Recruits from the local gang leaders"),
                false, 6, false);
        }

        private bool HideoutRecruitCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            Settlement hideout = Settlement.CurrentSettlement;
            if (hideout?.Hideout == null || !hideout.Hideout.IsInfested)
                return false;

            return SetAvailability(args, GetHideoutPool(hideout));
        }

        private bool TownRecruitCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            Settlement town = Settlement.CurrentSettlement;
            if (town == null || !town.IsTown || TownTarget(town) <= 0)
                return false;

            return SetAvailability(args, GetTownPool(town));
        }

        private static bool SetAvailability(MenuCallbackArgs args, RecruitPool pool)
        {
            if (pool.Count <= 0)
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("No one is willing to join you right now. Check back in a few days.");
            }
            return true;
        }

        private static void OpenRecruitment(Dictionary<string, RecruitPool> pools, Settlement settlement, string title)
        {
            if (settlement != null && pools.TryGetValue(settlement.StringId, out RecruitPool pool))
                RecruitmentDialog.Show(pool, title);
        }

        // ---------------------------------------------------------------- daily refill

        private void OnDailyTick()
        {
            // Hideouts: drop pools for hideouts that were cleared, refill the rest by one recruit per day.
            foreach (string id in _hideoutPools.Keys.ToList())
            {
                Settlement hideout = Settlement.Find(id);
                if (hideout?.Hideout == null || !hideout.Hideout.IsInfested)
                {
                    _hideoutPools.Remove(id);
                    continue;
                }

                RecruitPool pool = _hideoutPools[id];
                if (pool.Count < HideoutTarget(hideout))
                {
                    CharacterObject troop = RandomBanditTroop(hideout);
                    if (troop != null)
                        pool.Troops.AddToCounts(troop, 1);
                }
            }

            // Towns: friendly gang leaders supply one looter each per day until the pool reaches its target.
            CharacterObject looter = MBObjectManager.Instance.GetObject<CharacterObject>(LooterId);
            foreach (string id in _townPools.Keys.ToList())
            {
                Settlement town = Settlement.Find(id);
                if (town == null || looter == null)
                    continue;

                RecruitPool pool = _townPools[id];
                int leaders = FriendlyGangLeaders(town).Count;
                int missing = TownTarget(town) - pool.Count;
                if (missing > 0 && leaders > 0)
                    pool.Troops.AddToCounts(looter, Math.Min(leaders, missing));
            }
        }

        // ---------------------------------------------------------------- hideouts

        private RecruitPool GetHideoutPool(Settlement hideout)
        {
            if (_hideoutPools.TryGetValue(hideout.StringId, out RecruitPool pool))
                return pool;

            pool = new RecruitPool(hideout.StringId);
            List<CharacterObject> bandits = BanditTroopsIn(hideout);
            int target = HideoutTarget(bandits.Count);
            for (int i = 0; i < target && bandits.Count > 0; i++)
                pool.Troops.AddToCounts(bandits[MBRandom.RandomInt(bandits.Count)], 1);

            _hideoutPools[hideout.StringId] = pool;
            return pool;
        }

        private static List<CharacterObject> BanditTroopsIn(Settlement hideout)
        {
            var result = new List<CharacterObject>();
            foreach (MobileParty party in hideout.Parties)
            {
                if (party.IsMainParty || !party.IsBandit)
                    continue;

                foreach (TroopRosterElement element in party.MemberRoster.GetTroopRoster())
                {
                    if (element.Character.IsHero)
                        continue;
                    for (int i = 0; i < element.Number; i++)
                        result.Add(element.Character);
                }
            }
            return result;
        }

        private static CharacterObject RandomBanditTroop(Settlement hideout)
        {
            List<CharacterObject> bandits = BanditTroopsIn(hideout);
            return bandits.Count == 0 ? null : bandits[MBRandom.RandomInt(bandits.Count)];
        }

        private static int HideoutTarget(Settlement hideout) => HideoutTarget(BanditTroopsIn(hideout).Count);

        private static int HideoutTarget(int banditCount)
        {
            float multiplier = BanditrySettings.Instance?.HideoutRecruitableMultiplier ?? 2f;
            return (int)(banditCount * multiplier / 10.0);
        }

        // ---------------------------------------------------------------- towns

        private RecruitPool GetTownPool(Settlement town)
        {
            if (_townPools.TryGetValue(town.StringId, out RecruitPool pool))
                return pool;

            pool = new RecruitPool(town.StringId);
            CharacterObject looter = MBObjectManager.Instance.GetObject<CharacterObject>(LooterId);
            if (looter != null)
                pool.Troops.AddToCounts(looter, TownTarget(town));

            _townPools[town.StringId] = pool;
            return pool;
        }

        private static List<Hero> FriendlyGangLeaders(Settlement town)
        {
            return town.Notables
                .Where(h => h.IsGangLeader && h.GetRelation(Hero.MainHero) >= 0)
                .ToList();
        }

        private static int TownTarget(Settlement town)
        {
            float multiplier = BanditrySettings.Instance?.TownRecruitableMultiplier ?? 2f;
            int total = 0;
            foreach (Hero leader in FriendlyGangLeaders(town))
                total += Math.Max(1, (int)(Math.Sqrt(leader.GetRelation(Hero.MainHero) / 10.0) * multiplier));
            return total;
        }
    }
}
