using Timberborn.BaseComponentSystem;
using Timberborn.Characters;
using Timberborn.Persistence;
using Timberborn.WorldPersistence;

namespace PopulationControl
{
    /// <summary>
    /// "Never migrate" flag on an individual beaver or bot. Pinned citizens are skipped by
    /// every kind of migration (manual and automatic). Survives a child growing into an adult.
    /// </summary>
    public class MigrationPin : BaseComponent, IPersistentEntity, IChildhoodInfluenced
    {
        private static readonly ComponentKey PinKey = new ComponentKey("PopulationControl.Pin");
        private static readonly PropertyKey<bool> PinnedKey = new PropertyKey<bool>("Pinned");

        public bool Pinned { get; set; }

        public void Save(IEntitySaver entitySaver)
        {
            if (Pinned)
            {
                entitySaver.GetComponent(PinKey).Set(PinnedKey, true);
            }
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (entityLoader.TryGetComponent(PinKey, out IObjectLoader loader) && loader.Has(PinnedKey))
            {
                Pinned = loader.Get(PinnedKey);
            }
        }

        public void InfluenceByChildhood(Character child)
        {
            MigrationPin childPin = child.GetComponent<MigrationPin>();
            if (childPin != null && childPin.Pinned)
            {
                Pinned = true;
            }
        }

        public static bool IsPinned(BaseComponent citizen)
        {
            MigrationPin pin = citizen.GetComponent<MigrationPin>();
            return pin != null && pin.Pinned;
        }
    }
}
