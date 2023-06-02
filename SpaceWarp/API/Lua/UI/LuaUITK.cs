using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp.API.Assets;
using UnityEngine.UIElements;
using UitkForKsp2.API;

// ReSharper disable MemberCanBePrivate.Global

namespace SpaceWarp.API.Lua.UI;

// ReSharper disable UnusedMember.Global
[SpaceWarpLuaAPI("UI")]
// ReSharper disable once UnusedType.Global
public static class LuaUITK
{
    #region Creation
    public static UIDocument Window(LuaMod mod, string id, string documentPath)
    {
        return Window(mod, id, AssetManager.GetAsset<VisualTreeAsset>(documentPath));
    }

    public static UIDocument Window(LuaMod mod, string id, VisualTreeAsset uxml)
    {
        var parent = mod.transform;
        return UitkForKsp2.API.Window.CreateFromUxml(uxml, id, parent, true);
    }

    public static UIDocument Window(LuaMod mod, string id)
    {
        var parent = mod.transform;
        return UitkForKsp2.API.Window.Create(out _, id, parent, true);
    }
    
    #region Element Creation

    public static VisualElement VisualElement()
    {
        return new VisualElement();
    }

    public static ScrollView ScrollView()
    {
        return new ScrollView();
    }
    
    public static ListView ListView()
    {
        return new ListView();
    }

    public static Toggle Toggle(string text = "")
    {
        return new Toggle
        {
            text=text
        };
}

    public static Label Label(string text = "")
    {
        return new Label
        {
            text = text
        };
    }

    public static Button Button(string text = "")
    {
        return new Button
        {
            text = text
        };
    }

    public static Scroller Scroller()
    {
        return new Scroller();
    }

    public static TextField TextField(string text = "")
    {
        return new TextField
        {
            text = text
        };
    }

    public static Foldout Foldout()
    {
        return new Foldout();
    }

    public static Slider Slider(float value = 0.0f, float minValue = 0.0f, float maxValue = 1.0f)
    {
        return new Slider
        {
            lowValue = minValue,
            highValue = maxValue,
            value = value
        };
    }

    public static SliderInt SliderInt(int value = 0, int minValue = 0, int maxValue = 100)
    {
        return new SliderInt
        {
            lowValue = minValue,
            highValue = maxValue,
            value = value
        };
    }

    public static MinMaxSlider MinMaxSlider(float minValue = 0.0f, float maxValue = 1.0f, float minLimit = 0.0f, float maxLimit = 1.0f)
    {
        return new MinMaxSlider
        {
            minValue = minValue,
            maxValue = maxValue,
            lowLimit = minLimit,
            highLimit = maxLimit
        };
    }
    
    #endregion
    
    #endregion

    #region Callbacks
    public static void AddCallback(Button button, Closure callback, [CanBeNull] DynValue self = null)
    {
        if (self != null)
        {
            button.clicked += () => callback.Call(self);
        }
        else
        {
            button.clicked += () => callback.Call();
        }
    }


    /// <summary>
    /// Registers a value changed callback from lua
    /// The lua functions parameters should be like function(self?,previous,new)
    /// </summary>
    /// <param name="element"></param>
    /// <param name="callback"></param>
    /// <param name="self"></param>
    /// <typeparam name="T"></typeparam>
    public static void RegisterValueChangedCallback<T>(INotifyValueChanged<T> element, Closure callback,
        [CanBeNull] DynValue self = null)
    {
        if (self != null)
        {
            element.RegisterValueChangedCallback(evt => callback.Call(self, evt.previousValue, evt.newValue));
        }
        else
        {
            element.RegisterValueChangedCallback(evt => callback.Call(evt.previousValue, evt.newValue));
        }
    }

    private static void RegisterGenericCallback<T>(VisualElement element, Closure callback, DynValue self,
        bool trickleDown = false) where T : EventBase<T>, new()
    {
        if (self != null)
        {
            element.RegisterCallback<T>(evt => callback.Call(self, evt),
                trickleDown ? TrickleDown.TrickleDown : TrickleDown.NoTrickleDown);
        }
        else
        {
            element.RegisterCallback<T>(evt => callback.Call(evt),
                trickleDown ? TrickleDown.TrickleDown : TrickleDown.NoTrickleDown);
        }
    }


    #region Capture Events

    public static void RegisterMouseCaptureCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<MouseCaptureEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseCaptureOutCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) =>
        RegisterGenericCallback<MouseCaptureOutEvent>(element, callback, self, trickleDown);

    public static void RegisterPointerCaptureCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<PointerCaptureEvent>(element, callback, self, trickleDown);

    public static void RegisterPointerCaptureOutCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) =>
        RegisterGenericCallback<PointerCaptureOutEvent>(element, callback, self, trickleDown);

    #endregion

    #region Change Events

    public static void RegisterChangeBoolCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<ChangeEvent<bool>>(element, callback, self, trickleDown);

    public static void RegisterChangeIntCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<ChangeEvent<int>>(element, callback, self, trickleDown);

    public static void RegisterChangeFloatCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<ChangeEvent<float>>(element, callback, self, trickleDown);

    public static void RegisterChangeStringCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<ChangeEvent<string>>(element, callback, self, trickleDown);

    #endregion

    #region Click Events

    public static void RegisterClickCallback(VisualElement element, Closure callback, [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<ClickEvent>(element, callback, self, trickleDown);

    #endregion

    #region Focus Events

    public static void RegisterFocusOutCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<FocusOutEvent>(element, callback, self, trickleDown);

    public static void RegisterFocusInCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<FocusInEvent>(element, callback, self, trickleDown);

    public static void RegisterBlurCallback(VisualElement element, Closure callback, [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<BlurEvent>(element, callback, self, trickleDown);

    public static void RegisterFocusCallback(VisualElement element, Closure callback, [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<FocusEvent>(element, callback, self, trickleDown);

    #endregion

    #region Input Events

    public static void RegisterInputCallback(VisualElement element, Closure callback, [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<InputEvent>(element, callback, self, trickleDown);

    #endregion

    #region Layout Events

    public static void RegisterGeometryChangedCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) =>
        RegisterGenericCallback<GeometryChangedEvent>(element, callback, self, trickleDown);

    #endregion

    #region Mouse Events

    public static void RegisterMouseDownCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<MouseDownEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseUpCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<MouseUpEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseMoveCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<MouseMoveEvent>(element, callback, self, trickleDown);

    public static void RegisterWheelCallback(VisualElement element, Closure callback, [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<WheelEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseEnterWindowCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) =>
        RegisterGenericCallback<MouseEnterWindowEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseLeaveWindowCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) =>
        RegisterGenericCallback<MouseLeaveWindowEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseEnterCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<MouseEnterEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseLeaveCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<MouseLeaveEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseOverCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<MouseOverEvent>(element, callback, self, trickleDown);

    public static void RegisterMouseOutCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<MouseOutEvent>(element, callback, self, trickleDown);

    #endregion
    
    #region Pointer Events
    
    public static void RegisterPointerDownCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<PointerDownEvent>(element, callback, self, trickleDown);

    public static void RegisterPointerUpCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<PointerUpEvent>(element, callback, self, trickleDown);

    public static void RegisterPointerMoveCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<PointerMoveEvent>(element, callback, self, trickleDown);
    
    public static void RegisterPointerEnterCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<PointerEnterEvent>(element, callback, self, trickleDown);

    public static void RegisterPointerLeaveCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<PointerLeaveEvent>(element, callback, self, trickleDown);

    public static void RegisterPointerOverCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<PointerOverEvent>(element, callback, self, trickleDown);

    public static void RegisterPointerOutCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<PointerOutEvent>(element, callback, self, trickleDown);
    #endregion
    
    #region Panel Events
    
    public static void RegisterAttachToPanelCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<AttachToPanelEvent>(element, callback, self, trickleDown);
    public static void RegisterDetachFromPanelCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<DetachFromPanelEvent>(element, callback, self, trickleDown);
    #endregion

    #region Tooltip Events
    public static void RegisterTooltipCallback(VisualElement element, Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false) => RegisterGenericCallback<TooltipEvent>(element, callback, self, trickleDown);
    #endregion
    #endregion
}