using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.GameDistricts;
using UnityEngine;
using UnityEngine.UIElements;

namespace PopulationControl.UI
{
    /// <summary>"Migration filters" section shown in every District Center's panel.</summary>
    internal class MigrationFiltersFragment : IEntityPanelFragment
    {
        private readonly VisualElementLoader _visualElementLoader;
        private readonly VisualElementInitializer _visualElementInitializer;
        private readonly DistrictCenterRegistry _districtCenterRegistry;

        private VisualElement _root;
        private VisualElement _details;
        private Button _expandButton;
        private Label _summary;
        private Toggle _manualToggle;
        private Toggle _automaticToggle;
        private readonly Dictionary<PopulationGroup, Button> _tabs = new Dictionary<PopulationGroup, Button>();
        private Button _orderButton;
        private Label _minAgeValue;
        private Label _maxAgeValue;
        private VisualElement _jobsRow;
        private Button _jobsButton;
        private Toggle _homelessToggle;
        private Label _eligible;
        private Label _copyResult;

        private DistrictCenter _district;
        private MigrationFilters _filters;
        private PopulationGroup _group = PopulationGroup.Adults;
        private static bool _expanded;
        private float _nextRefresh;

        public MigrationFiltersFragment(VisualElementLoader visualElementLoader,
                                        VisualElementInitializer visualElementInitializer,
                                        DistrictCenterRegistry districtCenterRegistry)
        {
            _visualElementLoader = visualElementLoader;
            _visualElementInitializer = visualElementInitializer;
            _districtCenterRegistry = districtCenterRegistry;
        }

        private GroupFilter Current => _filters.Get(_group);

        public VisualElement InitializeFragment()
        {
            // Borrow the District Center fragment's layout: it carries the game's stylesheets and a green sub-panel.
            _root = _visualElementLoader.LoadVisualElement("Game/EntityPanel/DistrictCenterFragment");
            _root.name = "PopulationControlFragment";
            VisualElement box = _root.Q<VisualElement>("Migration");
            _root.Q<VisualElement>("WorkerType")?.RemoveFromHierarchy();
            box.Clear();
            box.style.flexDirection = FlexDirection.Column;
            box.style.alignItems = Align.Stretch;

            // Header
            VisualElement header = UiKit.Row();
            header.style.flexWrap = Wrap.NoWrap;
            Label title = UiKit.Heading("Migration filters");
            title.style.flexGrow = 1;
            header.Add(title);
            _expandButton = UiKit.Button("Show", ToggleExpanded, narrow: true);
            header.Add(_expandButton);
            box.Add(header);

            _summary = UiKit.Text("", "entity-panel__sub-text");
            box.Add(_summary);

            // Details
            _details = new VisualElement();
            box.Add(_details);
            _details.Add(UiKit.Spacer());

            _manualToggle = UiKit.Toggle("Use for manual migration (1 / 10 / all buttons)", v => Change(() => _filters.ApplyToManual = v));
            _automaticToggle = UiKit.Toggle("Use for automatic migration", v => Change(() => _filters.ApplyToAutomatic = v));
            _details.Add(_manualToggle);
            _details.Add(_automaticToggle);
            _details.Add(UiKit.Spacer());

            VisualElement tabs = UiKit.Row();
            AddTab(tabs, PopulationGroup.Adults, "Adults");
            AddTab(tabs, PopulationGroup.Children, "Children");
            AddTab(tabs, PopulationGroup.Bots, "Bots");
            _details.Add(tabs);

            VisualElement orderRow = UiKit.Row();
            orderRow.Add(UiKit.Text("Send first:"));
            _orderButton = UiKit.Button("", CycleOrder, narrow: true);
            orderRow.Add(_orderButton);
            _details.Add(orderRow);

            VisualElement ageRow = UiKit.Row();
            ageRow.Add(UiKit.Text("Min age:"));
            ageRow.Add(UiKit.StepButton(false, s => Change(() => Current.MinAge = Clamp(Current.MinAge + s))));
            _minAgeValue = UiKit.Text("", "text--yellow");
            _minAgeValue.style.minWidth = 34;
            _minAgeValue.style.unityTextAlign = TextAnchor.MiddleCenter;
            ageRow.Add(_minAgeValue);
            ageRow.Add(UiKit.StepButton(true, s => Change(() => Current.MinAge = Clamp(Current.MinAge + s))));
            Label maxLabel = UiKit.Text("Max age:");
            maxLabel.style.marginLeft = 8;
            ageRow.Add(maxLabel);
            ageRow.Add(UiKit.StepButton(false, s => Change(() => Current.MaxAge = Clamp(Current.MaxAge + s))));
            _maxAgeValue = UiKit.Text("", "text--yellow");
            _maxAgeValue.style.minWidth = 34;
            _maxAgeValue.style.unityTextAlign = TextAnchor.MiddleCenter;
            ageRow.Add(_maxAgeValue);
            ageRow.Add(UiKit.StepButton(true, s => Change(() => Current.MaxAge = Clamp(Current.MaxAge + s))));
            _details.Add(ageRow);

            _jobsRow = UiKit.Row();
            _jobsRow.Add(UiKit.Text("Workers:"));
            _jobsButton = UiKit.Button("", CycleWorkers, narrow: true);
            _jobsRow.Add(_jobsButton);
            _details.Add(_jobsRow);

            _homelessToggle = UiKit.Toggle("Send homeless first", v => Change(() => Current.PreferHomeless = v));
            _details.Add(_homelessToggle);

            _eligible = UiKit.Text("", "text--yellow");
            _eligible.style.marginTop = 4;
            _details.Add(_eligible);

            VisualElement actions = UiKit.Row();
            actions.Add(UiKit.Button("Reset this tab", () => Change(() => Current.Reset()), narrow: true));
            actions.Add(UiKit.Button("Reset all", () => Change(() => _filters.ResetAll()), narrow: true));
            actions.Add(UiKit.Button("Copy to all districts", CopyToAllDistricts, narrow: true));
            _details.Add(actions);

            _copyResult = UiKit.Text("", "entity-panel__sub-text");
            _details.Add(_copyResult);

            _details.Add(UiKit.Text("Ages are in days (0 = no limit). Shift-click + / − to step by 10. "
                                    + "Pin single beavers or bots with \"Never migrate\" in their own panel.",
                                    "entity-panel__sub-text"));

            _visualElementInitializer.InitializeVisualElement(_root);
            UiKit.Show(_root, false);
            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            _district = entity.GetComponent<DistrictCenter>();
            _filters = _district != null ? _district.GetComponent<MigrationFilters>() : null;
            _copyResult.text = "";
            if (_filters != null)
            {
                Refresh();
            }
        }

        public void ClearFragment()
        {
            _district = null;
            _filters = null;
            UiKit.Show(_root, false);
        }

        public void UpdateFragment()
        {
            bool visible = _filters != null && _district != null && _district.Enabled;
            UiKit.Show(_root, visible);
            if (visible && Time.unscaledTime >= _nextRefresh)
            {
                Refresh();
            }
        }

        // ---------------- actions ----------------

        private void AddTab(VisualElement parent, PopulationGroup group, string text)
        {
            Button tab = UiKit.Button(text, () =>
            {
                _group = group;
                Refresh();
            }, narrow: true);
            _tabs[group] = tab;
            parent.Add(tab);
        }

        private void ToggleExpanded()
        {
            _expanded = !_expanded;
            Refresh();
        }

        private void CycleOrder() =>
            Change(() => Current.Order = Current.Order == AgeOrder.YoungestFirst ? AgeOrder.OldestFirst : AgeOrder.YoungestFirst);

        private void CycleWorkers() =>
            Change(() => Current.Workers = (WorkerRule)(((int)Current.Workers + 1) % 3));

        private void CopyToAllDistricts()
        {
            if (_filters == null)
            {
                return;
            }
            int copied = 0;
            foreach (DistrictCenter other in _districtCenterRegistry.FinishedDistrictCenters)
            {
                if (other == _district)
                {
                    continue;
                }
                MigrationFilters otherFilters = other.GetComponent<MigrationFilters>();
                if (otherFilters != null)
                {
                    otherFilters.CopyFrom(_filters);
                    copied++;
                }
            }
            _copyResult.text = copied == 0 ? "No other districts to copy to." : $"Copied to {copied} other district(s).";
        }

        private void Change(System.Action change)
        {
            if (_filters == null)
            {
                return;
            }
            change();
            Refresh();
        }

        private static int Clamp(int value) => Mathf.Clamp(value, 0, 999);

        // ---------------- view ----------------

        private void Refresh()
        {
            _nextRefresh = Time.unscaledTime + 0.5f;
            if (_filters == null || _district == null)
            {
                return;
            }

            _expandButton.text = _expanded ? "Hide" : "Show";
            UiKit.Show(_details, _expanded);
            _summary.text = Summary();

            _manualToggle.SetValueWithoutNotify(_filters.ApplyToManual);
            _automaticToggle.SetValueWithoutNotify(_filters.ApplyToAutomatic);
            foreach (KeyValuePair<PopulationGroup, Button> tab in _tabs)
            {
                UiKit.SetSelected(tab.Value, tab.Key == _group);
            }

            GroupFilter f = Current;
            _orderButton.text = f.Order == AgeOrder.OldestFirst ? "Oldest" : "Youngest (game default)";
            _minAgeValue.text = f.MinAge == 0 ? "off" : f.MinAge.ToString();
            _maxAgeValue.text = f.MaxAge == 0 ? "off" : f.MaxAge.ToString();

            UiKit.Show(_jobsRow, _group != PopulationGroup.Children);
            _jobsButton.text = WorkerText(f.Workers);

            UiKit.Show(_homelessToggle, _group != PopulationGroup.Bots);
            _homelessToggle.SetValueWithoutNotify(f.PreferHomeless);

            if (_expanded)
            {
                _eligible.text = EligibleText();
            }
        }

        private string EligibleText()
        {
            string noun = GroupNoun(_group);
            int total = 0;
            foreach (BaseComponent _ in MigrationSelector.Pool(_district, _group))
            {
                total++;
            }
            GroupFilter manualFilter = MigrationSelector.ActiveFilter(_district, _group, automatic: false);
            List<BaseComponent> candidates = MigrationSelector.Candidates(_district, _group, manualFilter);
            int pinned = MigrationSelector.CountPinned(_district, _group);
            string text = $"Can leave now: {candidates.Count} of {total} healthy {noun}";
            if (pinned > 0)
            {
                text += $" ({pinned} pinned)";
            }
            if (candidates.Count > 0)
            {
                text += $". Next to go: age {MigrationSelector.Age(candidates[0])}";
            }
            if (!_filters.ApplyToManual && !Current.IsDefault)
            {
                text += ". (Manual migration is not using these filters.)";
            }
            return text;
        }

        private string Summary()
        {
            if (_filters.Adults.IsDefault && _filters.Children.IsDefault && _filters.Bots.IsDefault)
            {
                return "Off: using the game's normal rules (youngest first).";
            }
            var parts = new List<string>();
            AddGroupSummary(parts, "Adults", _filters.Adults, PopulationGroup.Adults);
            AddGroupSummary(parts, "Children", _filters.Children, PopulationGroup.Children);
            AddGroupSummary(parts, "Bots", _filters.Bots, PopulationGroup.Bots);
            string usedFor = _filters.ApplyToManual && _filters.ApplyToAutomatic ? "manual + automatic"
                : _filters.ApplyToManual ? "manual only"
                : _filters.ApplyToAutomatic ? "automatic only"
                : "nothing (both switches are off)";
            return string.Join("; ", parts) + ". Used for: " + usedFor + ".";
        }

        private static void AddGroupSummary(List<string> parts, string name, GroupFilter f, PopulationGroup group)
        {
            if (f.IsDefault)
            {
                return;
            }
            var bits = new List<string> { f.Order == AgeOrder.OldestFirst ? "oldest first" : "youngest first" };
            if (f.MinAge > 0 && f.MaxAge > 0)
            {
                bits.Add($"age {f.MinAge}-{f.MaxAge}");
            }
            else if (f.MinAge > 0)
            {
                bits.Add($"age {f.MinAge}+");
            }
            else if (f.MaxAge > 0)
            {
                bits.Add($"age up to {f.MaxAge}");
            }
            if (group != PopulationGroup.Children && f.Workers == WorkerRule.UnemployedOnly)
            {
                bits.Add("never employed ones");
            }
            else if (group != PopulationGroup.Children && f.Workers == WorkerRule.IgnoreJobs)
            {
                bits.Add("jobs ignored");
            }
            if (group != PopulationGroup.Bots && !f.PreferHomeless)
            {
                bits.Add("housing ignored");
            }
            parts.Add(name + ": " + string.Join(", ", bits));
        }

        private static string WorkerText(WorkerRule rule)
        {
            switch (rule)
            {
                case WorkerRule.IgnoreJobs: return "Ignore jobs";
                case WorkerRule.UnemployedOnly: return "Never send employed";
                default: return "Unemployed first (game default)";
            }
        }

        private static string GroupNoun(PopulationGroup group) =>
            group == PopulationGroup.Adults ? "adults" : group == PopulationGroup.Children ? "children" : "bots";
    }
}
