using Timberborn.BaseComponentSystem;
using Timberborn.Persistence;
using Timberborn.WorldPersistence;

namespace PopulationControl
{
    /// <summary>
    /// Per-district migration rules, attached to every District Center and saved with the game.
    /// The filters describe who is allowed to LEAVE this district, and in what order.
    /// </summary>
    public class MigrationFilters : BaseComponent, IPersistentEntity
    {
        private static readonly ComponentKey FiltersKey = new ComponentKey("PopulationControl.Filters");
        private static readonly PropertyKey<bool> ApplyToManualKey = new PropertyKey<bool>("ApplyToManual");
        private static readonly PropertyKey<bool> ApplyToAutomaticKey = new PropertyKey<bool>("ApplyToAutomatic");

        /// <summary>Use the filters for the 1 / 10 / all buttons in the migration panel.</summary>
        public bool ApplyToManual = true;

        /// <summary>Use the filters for the game's automatic migration. Off by default so vanilla auto-migration is untouched.</summary>
        public bool ApplyToAutomatic;

        public GroupFilter Adults { get; } = new GroupFilter();
        public GroupFilter Children { get; } = new GroupFilter();
        public GroupFilter Bots { get; } = new GroupFilter();

        public GroupFilter Get(PopulationGroup group)
        {
            switch (group)
            {
                case PopulationGroup.Children: return Children;
                case PopulationGroup.Bots: return Bots;
                default: return Adults;
            }
        }

        public bool AppliesTo(bool automatic) => automatic ? ApplyToAutomatic : ApplyToManual;

        public bool IsActiveFor(PopulationGroup group, bool automatic) => AppliesTo(automatic) && !Get(group).IsDefault;

        public bool IsEverythingDefault =>
            Adults.IsDefault && Children.IsDefault && Bots.IsDefault && ApplyToManual && !ApplyToAutomatic;

        public void ResetAll()
        {
            ApplyToManual = true;
            ApplyToAutomatic = false;
            Adults.Reset();
            Children.Reset();
            Bots.Reset();
        }

        public void CopyFrom(MigrationFilters other)
        {
            ApplyToManual = other.ApplyToManual;
            ApplyToAutomatic = other.ApplyToAutomatic;
            Adults.CopyFrom(other.Adults);
            Children.CopyFrom(other.Children);
            Bots.CopyFrom(other.Bots);
        }

        public void Save(IEntitySaver entitySaver)
        {
            if (IsEverythingDefault)
            {
                // Nothing to remember: keeps untouched saves completely clean.
                return;
            }
            IObjectSaver saver = entitySaver.GetComponent(FiltersKey);
            saver.Set(ApplyToManualKey, ApplyToManual);
            saver.Set(ApplyToAutomaticKey, ApplyToAutomatic);
            Adults.Save(saver, "Adults");
            Children.Save(saver, "Children");
            Bots.Save(saver, "Bots");
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.TryGetComponent(FiltersKey, out IObjectLoader loader))
            {
                return;
            }
            if (loader.Has(ApplyToManualKey))
            {
                ApplyToManual = loader.Get(ApplyToManualKey);
            }
            if (loader.Has(ApplyToAutomaticKey))
            {
                ApplyToAutomatic = loader.Get(ApplyToAutomaticKey);
            }
            Adults.Load(loader, "Adults");
            Children.Load(loader, "Children");
            Bots.Load(loader, "Bots");
        }
    }
}
