using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace Banditry.Data
{
    internal sealed class BanditryDataSaveDefiner : SaveableTypeDefiner
    {
        // Unique id range for this mod's saveable types.
        private const int SaveBaseId = 358923857;

        public BanditryDataSaveDefiner() : base(SaveBaseId)
        {
        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(RecruitPool), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<string, RecruitPool>));
        }
    }
}
