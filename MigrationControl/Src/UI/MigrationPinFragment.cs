using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using UnityEngine.UIElements;

namespace MigrationControl.UI
{
    /// <summary>"Never migrate" checkbox shown in a beaver's or bot's panel.</summary>
    internal class MigrationPinFragment : IEntityPanelFragment
    {
        private readonly VisualElementLoader _visualElementLoader;
        private readonly VisualElementInitializer _visualElementInitializer;

        private VisualElement _root;
        private Toggle _toggle;
        private MigrationPin _pin;

        public MigrationPinFragment(VisualElementLoader visualElementLoader, VisualElementInitializer visualElementInitializer)
        {
            _visualElementLoader = visualElementLoader;
            _visualElementInitializer = visualElementInitializer;
        }

        public VisualElement InitializeFragment()
        {
            // Small green sub-panel with the game's stylesheets attached.
            _root = _visualElementLoader.LoadVisualElement("Game/EntityPanel/AdulthoodFragment");
            _root.name = "MigrationPinFragment";
            _root.Clear();
            _root.style.flexDirection = FlexDirection.Column;
            _root.style.alignItems = Align.FlexStart;

            _toggle = UiKit.Toggle("Never migrate", value =>
            {
                if (_pin != null)
                {
                    _pin.Pinned = value;
                }
            });
            _root.Add(_toggle);
            _root.Add(UiKit.Text("Pinned citizens are skipped by manual and automatic migration.", "entity-panel__sub-text"));

            _visualElementInitializer.InitializeVisualElement(_root);
            UiKit.Show(_root, false);
            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            _pin = entity.GetComponent<MigrationPin>();
            if (_pin != null)
            {
                _toggle.SetValueWithoutNotify(_pin.Pinned);
                UiKit.Show(_root, true);
            }
        }

        public void ClearFragment()
        {
            _pin = null;
            UiKit.Show(_root, false);
        }

        public void UpdateFragment()
        {
            if (_pin != null)
            {
                _toggle.SetValueWithoutNotify(_pin.Pinned);
            }
        }
    }
}
