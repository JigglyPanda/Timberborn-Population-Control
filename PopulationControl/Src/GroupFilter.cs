using Timberborn.Persistence;

namespace PopulationControl
{
    public enum PopulationGroup
    {
        Adults = 0,
        Children = 1,
        Bots = 2,
    }

    public enum AgeOrder
    {
        /// <summary>Base game behaviour.</summary>
        YoungestFirst = 0,
        OldestFirst = 1,
    }

    public enum WorkerRule
    {
        /// <summary>Base game behaviour: unemployed go first, but employed can still be sent.</summary>
        PreferUnemployed = 0,
        /// <summary>Jobs are ignored completely; only age (and housing) decide.</summary>
        IgnoreJobs = 1,
        /// <summary>Employed citizens are never sent.</summary>
        UnemployedOnly = 2,
    }

    /// <summary>
    /// Rules for one population group (adults, children or bots) of one district.
    /// The default values reproduce the base game's ordering exactly.
    /// </summary>
    public class GroupFilter
    {
        public AgeOrder Order = AgeOrder.YoungestFirst;
        /// <summary>0 = no limit.</summary>
        public int MinAge;
        /// <summary>0 = no limit.</summary>
        public int MaxAge;
        public WorkerRule Workers = WorkerRule.PreferUnemployed;
        public bool PreferHomeless = true;

        public bool IsDefault =>
            Order == AgeOrder.YoungestFirst
            && MinAge == 0
            && MaxAge == 0
            && Workers == WorkerRule.PreferUnemployed
            && PreferHomeless;

        public void Reset()
        {
            Order = AgeOrder.YoungestFirst;
            MinAge = 0;
            MaxAge = 0;
            Workers = WorkerRule.PreferUnemployed;
            PreferHomeless = true;
        }

        public void CopyFrom(GroupFilter other)
        {
            Order = other.Order;
            MinAge = other.MinAge;
            MaxAge = other.MaxAge;
            Workers = other.Workers;
            PreferHomeless = other.PreferHomeless;
        }

        public bool PassesAge(int age)
        {
            if (MinAge > 0 && age < MinAge)
            {
                return false;
            }
            if (MaxAge > 0 && age > MaxAge)
            {
                return false;
            }
            return true;
        }

        // ---- persistence (stored as plain ints/bools so old/new versions stay compatible) ----

        private static PropertyKey<int> OrderKey(string p) => new PropertyKey<int>(p + "Order");
        private static PropertyKey<int> MinAgeKey(string p) => new PropertyKey<int>(p + "MinAge");
        private static PropertyKey<int> MaxAgeKey(string p) => new PropertyKey<int>(p + "MaxAge");
        private static PropertyKey<int> WorkersKey(string p) => new PropertyKey<int>(p + "Workers");
        private static PropertyKey<bool> HomelessKey(string p) => new PropertyKey<bool>(p + "PreferHomeless");

        public void Save(IObjectSaver saver, string prefix)
        {
            saver.Set(OrderKey(prefix), (int)Order);
            saver.Set(MinAgeKey(prefix), MinAge);
            saver.Set(MaxAgeKey(prefix), MaxAge);
            saver.Set(WorkersKey(prefix), (int)Workers);
            saver.Set(HomelessKey(prefix), PreferHomeless);
        }

        public void Load(IObjectLoader loader, string prefix)
        {
            Reset();
            if (loader.Has(OrderKey(prefix)))
            {
                Order = (AgeOrder)loader.Get(OrderKey(prefix));
            }
            if (loader.Has(MinAgeKey(prefix)))
            {
                MinAge = loader.Get(MinAgeKey(prefix));
            }
            if (loader.Has(MaxAgeKey(prefix)))
            {
                MaxAge = loader.Get(MaxAgeKey(prefix));
            }
            if (loader.Has(WorkersKey(prefix)))
            {
                Workers = (WorkerRule)loader.Get(WorkersKey(prefix));
            }
            if (loader.Has(HomelessKey(prefix)))
            {
                PreferHomeless = loader.Get(HomelessKey(prefix));
            }
        }
    }
}
