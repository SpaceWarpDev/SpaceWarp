using I2.Loc;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.Controls;

public class IntlLabel : Label
{
    public new class UxmlFactory : UxmlFactory<IntlLabel, UxmlTraits>
    {
    }

    public new class UxmlTraits : Label.UxmlTraits
    {
        private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var label = (IntlLabel) ve;
            label._localizedText = m_Text.GetValueFromBag(bag, cc);
            label.text = label._localizedText;
        }
    }

    private LocalizedString _localizedText;

    public override string text
    {
        get => _localizedText;
        set => _localizedText = value;
    }
}