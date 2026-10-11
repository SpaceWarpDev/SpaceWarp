using System;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceWarp2.UI.Backend.UI.Appbar;

public interface IAppbarBackend
{
    public static IAppbarBackend Instance;

    public void AddButton(string buttonText, Sprite buttonIcon, string buttonId, Action<bool> function);
    public void AddOABButton(string buttonText, Sprite buttonIcon, string buttonId, Action<bool> function);
    public void AddKSCButton(string buttonText, Sprite buttonIcon, string buttonId, Action function);

    /// <summary>
    /// Removes the button with an id from whichever app bar holds it.
    /// </summary>
    /// <param name="buttonId">The button's id.</param>
    public void RemoveButton(string buttonId);

    public UnityEvent AppBarOABSubscriber { get; }
    public UnityEvent AppBarKSCSubscriber { get; }
    public UnityEvent AppBarInFlightSubscriber { get; }
}