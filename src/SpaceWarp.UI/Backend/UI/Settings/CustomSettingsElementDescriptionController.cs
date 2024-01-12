using KSP.Game;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace SpaceWarp.Backend.UI.Settings;

/// <summary>
/// This class is used to display a description for a custom setting element.
/// </summary>
public class CustomSettingsElementDescriptionController :
    KerbalMonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    /// <summary>
    /// The description to display when the element is hovered.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string description;

    /// <summary>
    /// Whether or not this element is an input setting element.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool isInputSettingElement;

    /// <summary>
    /// The tween animations to play when the element is hovered.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public List<DOTweenAnimation> tweenAnimations = new();

    /// <summary>
    /// Called when the object is created.
    /// </summary>
    public void Start() => tweenAnimations.AddRange(GetComponents<DOTweenAnimation>());

    /// <summary>
    /// Called when the pointer enters the object.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData) => OnHover(true);

    /// <summary>
    /// Called when the pointer exits the object.
    /// </summary>
    /// <param name="eventData"></param>
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
        {
            animationComponent.tween.Restart();
        }
    }
}
