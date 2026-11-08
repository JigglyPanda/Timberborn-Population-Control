using System;
using Timberborn.CoreUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace MigrationControl.UI
{
    /// <summary>
    /// Builds UI elements in code using the game's own USS classes, so the panel looks native.
    /// The containers are loaded from game UXML files because those carry the game's stylesheets.
    /// </summary>
    internal static class UiKit
    {
        // Timberborn.CoreUI.NineSliceButton is internal; create it through reflection so we get the game's button art.
        private static readonly Type NineSliceButtonType =
            typeof(NineSliceVisualElement).Assembly.GetType("Timberborn.CoreUI.NineSliceButton");

        public static Label Text(string text, string extraClass = null)
        {
            var label = new Label(text);
            label.AddToClassList("entity-panel__text");
            if (extraClass != null)
            {
                label.AddToClassList(extraClass);
            }
            return label;
        }

        public static Label Heading(string text)
        {
            var label = new Label(text);
            label.AddToClassList("entity-panel__heading-text");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            return label;
        }

        public static Button Button(string text, Action onClick, bool narrow = false)
        {
            Button button = NineSliceButtonType != null
                ? (Button)Activator.CreateInstance(NineSliceButtonType)
                : new Button();
            button.text = text;
            button.AddToClassList("entity-fragment__button");
            button.AddToClassList("entity-fragment__button--green");
            button.AddToClassList("entity-panel__text");
            button.style.minWidth = narrow ? 0 : new StyleLength(StyleKeyword.Null);
            button.style.marginLeft = 2;
            button.style.marginRight = 2;
            button.style.marginTop = 2;
            button.style.marginBottom = 2;
            button.style.paddingLeft = 6;
            button.style.paddingRight = 6;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.RegisterCallback<ClickEvent>(_ => onClick());
            return button;
        }

        /// <summary>Small square +/- button. Calls back with the step (1, or 10 with Shift held).</summary>
        public static Button StepButton(bool plus, Action<int> onStep)
        {
            var button = new Button();
            button.AddToClassList(plus ? "button-plus" : "button-minus");
            button.style.width = 20;
            button.style.height = 20;
            button.style.flexShrink = 0;
            button.style.marginLeft = 3;
            button.style.marginRight = 3;
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.backgroundColor = Color.clear;
            button.RegisterCallback<ClickEvent>(e =>
            {
                int step = e.shiftKey ? 10 : 1;
                onStep(plus ? step : -step);
            });
            return button;
        }

        public static Toggle Toggle(string text, Action<bool> onChanged)
        {
            var toggle = new Toggle { text = text };
            toggle.AddToClassList("game-toggle");
            toggle.AddToClassList("entity-panel__text");
            toggle.AddToClassList("entity-panel__toggle");
            toggle.style.marginTop = 2;
            toggle.style.marginBottom = 2;
            toggle.RegisterValueChangedCallback(e => onChanged(e.newValue));
            return toggle;
        }

        public static VisualElement Row()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.flexWrap = Wrap.Wrap;
            row.style.marginTop = 1;
            row.style.marginBottom = 1;
            return row;
        }

        public static VisualElement Spacer(float height = 4)
        {
            var spacer = new VisualElement();
            spacer.style.height = height;
            return spacer;
        }

        public static void SetSelected(Button button, bool selected)
        {
            button.EnableInClassList("entity-fragment__button--green", !selected);
            button.EnableInClassList("entity-fragment__button--red", selected);
        }

        public static void Show(VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
