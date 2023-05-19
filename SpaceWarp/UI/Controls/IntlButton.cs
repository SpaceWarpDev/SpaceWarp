using I2.Loc;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.Controls;

public class IntlButton: Button
{
    public new class UxmlFactory : UxmlFactory<IntlButton, UxmlTraits>
    {
    }

    public new class UxmlTraits : Button.UxmlTraits
    {
        private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var button = (IntlButton) ve;
            button._localizedText = m_Text.GetValueFromBag(bag, cc);
            button.text = button._localizedText;
        }
    }

    private LocalizedString _localizedText;

    public override string text
    {
        get => _localizedText;
        set => _localizedText = value;
    }
}