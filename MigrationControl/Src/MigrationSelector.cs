using System.Collections.Generic;
using System.Linq;
using Timberborn.BaseComponentSystem;
using Timberborn.BeaverContaminationSystem;
using Timberborn.Characters;
using Timberborn.DwellingSystem;
using Timberborn.GameDistricts;
using Timberborn.WorkSystem;

namespace MigrationControl
{
    /// <summary>
    /// Decides which citizens leave a district. With default settings the ordering is identical to
    /// the base game (willing workers, then unemployed, then homeless, then youngest).
    /// </summary>
    public static class MigrationSelector
    {
        /// <summary>True while the game's automatic migration is moving someone (set by a Harmony patch).</summary>
        public static int AutomaticDepth;

        public static bool IsAutomatic => AutomaticDepth > 0;

        // ---------- population pools (same pools the base game uses) ----------

        public static IEnumerable<BaseComponent> Pool(DistrictCenter district, PopulationGroup group)
        {
            DistrictPopulation population = district.DistrictPopulation;
            switch (group)
            {
                case PopulationGroup.Children:
                    return population.Children.Where(IsNotContaminated).Cast<BaseComponent>();
                case PopulationGroup.Bots:
                    return population.Bots.Cast<BaseComponent>();
                default:
                    return population.Adults.Where(IsNotContaminated).Cast<BaseComponent>();
            }
        }

        /// <summary>
        /// Ordered list of citizens allowed to leave. <paramref name="filter"/> == null means "base game rules"
        /// (pinned citizens are still skipped).
        /// </summary>
        public static List<BaseComponent> Candidates(DistrictCenter district, PopulationGroup group, GroupFilter filter)
        {
            IEnumerable<BaseComponent> pool = Pool(district, group).Where(c => !MigrationPin.IsPinned(c));
            GroupFilter rules = filter ?? Vanilla;

            if (filter != null)
            {
                pool = pool.Where(c => rules.PassesAge(Age(c)));
                if (group != PopulationGroup.Children && rules.Workers == WorkerRule.UnemployedOnly)
                {
                    pool = pool.Where(c => !IsEmployed(c));
                }
            }

            // Build the sort chain. A constant first key keeps the remaining ThenBy calls uniform.
            IOrderedEnumerable<BaseComponent> ordered = pool.OrderBy(_ => 0);
            if (group != PopulationGroup.Children && rules.Workers != WorkerRule.IgnoreJobs)
            {
                ordered = ordered.ThenBy(RefusesWork).ThenBy(IsEmployed);
            }
            if (group != PopulationGroup.Bots && rules.PreferHomeless)
            {
                ordered = ordered.ThenBy(HasHome);
            }
            ordered = rules.Order == AgeOrder.OldestFirst
                ? ordered.ThenBy(DayOfBirth)            // born earliest = oldest
                : ordered.ThenByDescending(DayOfBirth); // base game: youngest first
            return ordered.ToList();
        }

        private static readonly GroupFilter Vanilla = new GroupFilter();

        public static bool AnyPinned(DistrictCenter district, PopulationGroup group) =>
            Pool(district, group).Any(MigrationPin.IsPinned);

        public static int CountPinned(DistrictCenter district, PopulationGroup group) =>
            Pool(district, group).Count(MigrationPin.IsPinned);

        /// <summary>Filter that applies right now for this district/group, or null for base game rules.</summary>
        public static GroupFilter ActiveFilter(DistrictCenter district, PopulationGroup group, bool automatic)
        {
            MigrationFilters filters = district.GetComponent<MigrationFilters>();
            if (filters == null || !filters.IsActiveFor(group, automatic))
            {
                return null;
            }
            return filters.Get(group);
        }

        /// <summary>
        /// Harmony prefix body for the three distributor templates.
        /// Returns true to let the base game run unchanged, false if we handled the migration.
        /// </summary>
        public static bool HandleMigrateTo(BaseComponent template, PopulationGroup group, DistrictCenter target, int amount)
        {
            DistrictCenter source = template.GetComponent<DistrictCenter>();
            if (source == null || target == null || amount <= 0)
            {
                return true;
            }

            bool automatic = IsAutomatic;
            GroupFilter filter = ActiveFilter(source, group, automatic);
            if (filter == null && !AnyPinned(source, group))
            {
                return true; // nothing for us to do: pure base game behaviour
            }

            List<BaseComponent> candidates = Candidates(source, group, filter);
            int moved = 0;
            foreach (BaseComponent citizen in candidates)
            {
                if (moved >= amount)
                {
                    break;
                }
                citizen.GetComponent<Citizen>().AssignDistrict(target);
                moved++;
            }

            if (!automatic && moved < amount)
            {
                MigrationNotifier.Notify(group, moved, amount, filter != null);
            }
            return false;
        }

        // ---------- small helpers (mirror Timberborn.GameDistrictsMigration.MigrationService) ----------

        public static int Age(BaseComponent c) => c.GetComponent<Character>().Age;

        private static int DayOfBirth(BaseComponent c) => c.GetComponent<Character>().DayOfBirth;

        private static bool RefusesWork(BaseComponent c)
        {
            WorkRefuser refuser = c.GetComponent<WorkRefuser>();
            return refuser != null && refuser.RefusesWork;
        }

        public static bool IsEmployed(BaseComponent c)
        {
            Worker worker = c.GetComponent<Worker>();
            return worker != null && worker.Employed;
        }

        private static bool HasHome(BaseComponent c)
        {
            Dweller dweller = c.GetComponent<Dweller>();
            return dweller != null && dweller.HasHome;
        }

        private static bool IsNotContaminated(BaseComponent c)
        {
            Contaminable contaminable = c.GetComponent<Contaminable>();
            return contaminable == null || !contaminable.IsContaminated;
        }
    }
}
