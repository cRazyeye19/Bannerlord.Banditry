using System;
using System.Collections.Generic;
using Banditry.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Library;

namespace Banditry.Behaviors
{
    /// <summary>
    /// Two-step pop-up recruitment: pick a troop type, then pick how many.
    /// Uses only the stock inquiry pop-ups, so it doesn't depend on the party screen internals.
    /// </summary>
    internal static class RecruitmentDialog
    {
        public static void Show(RecruitPool pool, string title)
        {
            var elements = new List<InquiryElement>();
            foreach (TroopRosterElement element in pool.Troops.GetTroopRoster())
            {
                if (element.Number <= 0)
                    continue;

                CharacterObject troop = element.Character;
                int cost = CostOf(troop);
                string reason = DisabledReason(cost);
                elements.Add(new InquiryElement(
                    troop,
                    $"{troop.Name} x{element.Number}  ({cost} gold each)",
                    new CharacterImageIdentifier(CharacterCode.CreateFrom(troop)),
                    reason == null,
                    reason));
            }

            if (elements.Count == 0)
            {
                InformationManager.DisplayMessage(new InformationMessage("There is no one left to recruit here."));
                return;
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                title,
                $"Gold: {Hero.MainHero.Gold}   Party: {MobileParty.MainParty.MemberRoster.TotalManCount}/{MobileParty.MainParty.Party.PartySizeLimit}",
                elements,
                true, 1, 1, "Recruit", "Done",
                selected => ChooseAmount(pool, title, (CharacterObject)selected[0].Identifier),
                null));
        }

        private static void ChooseAmount(RecruitPool pool, string title, CharacterObject troop)
        {
            int cost = CostOf(troop);
            int max = MaxAffordable(pool, troop, cost);
            if (max <= 0)
            {
                Show(pool, title);
                return;
            }

            var amounts = new SortedSet<int> { 1, 5, 10, max };
            var elements = new List<InquiryElement>();
            foreach (int n in amounts)
            {
                if (n > max)
                    continue;
                string label = n == max && n > 1 ? $"Recruit all {n}  ({n * cost} gold)" : $"Recruit {n}  ({n * cost} gold)";
                elements.Add(new InquiryElement(n, label, new CharacterImageIdentifier(CharacterCode.CreateFrom(troop))));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                $"Recruit {troop.Name}",
                $"Up to {max} can be recruited right now.",
                elements,
                true, 1, 1, "Recruit", "Back",
                selected => Recruit(pool, title, troop, cost, (int)selected[0].Identifier),
                chosen => Show(pool, title)));
        }

        private static void Recruit(RecruitPool pool, string title, CharacterObject troop, int cost, int amount)
        {
            amount = Math.Min(amount, MaxAffordable(pool, troop, cost));
            if (amount > 0)
            {
                int total = amount * cost;
                pool.Troops.AddToCounts(troop, -amount);
                MobileParty.MainParty.MemberRoster.AddToCounts(troop, amount);
                GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, total, true);
                InformationManager.DisplayMessage(new InformationMessage(
                    $"Recruited {amount} {troop.Name} for {total} gold.", Color.FromUint(0xFF00FF13)));
            }
            Show(pool, title);
        }

        private static int CostOf(CharacterObject troop)
        {
            return Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troop, Hero.MainHero).RoundedResultNumber;
        }

        private static int FreePartySpace()
        {
            return MobileParty.MainParty.Party.PartySizeLimit - MobileParty.MainParty.MemberRoster.TotalManCount;
        }

        private static string DisabledReason(int cost)
        {
            if (FreePartySpace() <= 0)
                return "Your party is full.";
            if (Hero.MainHero.Gold < cost)
                return "You can't afford this troop.";
            return null;
        }

        private static int MaxAffordable(RecruitPool pool, CharacterObject troop, int cost)
        {
            int available = pool.Troops.GetTroopCount(troop);
            int byGold = cost > 0 ? Hero.MainHero.Gold / cost : available;
            return Math.Max(0, Math.Min(available, Math.Min(byGold, FreePartySpace())));
        }
    }
}
