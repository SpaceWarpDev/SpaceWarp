using System.Collections.Generic;
using KSP.Game;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace SpaceWarp.Backend.UI.Settings;

public class CustomSettingsElementDescriptionController :
    KerbalMonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{

    public string description;
    public bool isInputSettingElement;

    public List<DOTweenAnimation> tweenAnimations = new();
    public void Start() => tweenAnimations.AddRange(GetComponents<DOTweenAnimation>());

    public void OnPointerEnter(PointerEventData eventData) => OnHover(true);

    public void OnPointerExit(PointerEventData eventData) => OnHover(false);


    private void OnHover(bool isHovered)
    {
        if (isHovered)
        {
            Game.SettingsMenuManager._settingDescription.SetValue(description);
            Game.SettingsMenuManager._isKeybindInstructionVisible.SetValue(isInputSettingElement);
            HandleAnimation("Hovered");
        }
        else
        {
            Game.SettingsMenuManager.UpdateSettingsDescription("");
            HandleAnimation("Normal");
        }
    }
    private void HandleAnimation(string triggerType)
    {
        var all = tweenAnimations.FindAll(da => da.id.Equals(triggerType));
        foreach (var animationComponent in all)
            animationComponent.tween.Restart();
    }
}
