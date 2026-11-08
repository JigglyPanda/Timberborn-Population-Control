using Timberborn.QuickNotificationSystem;
using Timberborn.SingletonSystem;

namespace PopulationControl
{
    /// <summary>Shows a short on-screen message when a manual migration moved fewer citizens than asked.</summary>
    public class MigrationNotifier : ILoadableSingleton, IUnloadableSingleton
    {
        private static MigrationNotifier _instance;
        private readonly QuickNotificationService _quickNotificationService;

        public MigrationNotifier(QuickNotificationService quickNotificationService)
        {
            _quickNotificationService = quickNotificationService;
        }

        public void Load() => _instance = this;

        public void Unload()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public static void Notify(PopulationGroup group, int moved, int requested, bool filtersActive)
        {
            if (_instance == null)
            {
                return;
            }
            string what = group == PopulationGroup.Adults ? "adults" : group == PopulationGroup.Children ? "children" : "bots";
            string reason = filtersActive ? "match this district's migration filters" : "aren't pinned";
            string text = moved == 0
                ? $"No {what} {reason}. Nobody was sent."
                : $"Sent {moved} of {requested} {what}: only {moved} {reason}.";
            _instance._quickNotificationService.SendWarningNotification(text);
        }
    }
}
