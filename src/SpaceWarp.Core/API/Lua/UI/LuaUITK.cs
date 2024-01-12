using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp.API.Assets;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace SpaceWarp.API.Lua.UI;

/// <summary>
/// Lua API for UITK
/// </summary>
[SpaceWarpLuaAPI("UI")]
[PublicAPI]
// ReSharper disable once InconsistentNaming
public static class LuaUITK
{
    #region Creation

    /// <summary>
    /// Creates a new window from a UXML file.
    /// </summary>
    /// <param name="mod">Mod to create the window for</param>
    /// <param name="id">ID of the window</param>
    /// <param name="documentPath">Path to the UXML file</param>
    /// <returns>Created window</returns>
    public static UIDocument Window(LuaMod mod, string id, string documentPath)
    {
        return Window(mod, id, AssetManager.GetAsset<VisualTreeAsset>(documentPath));
    }

    /// <summary>
    /// Creates a new window from a VisualTreeAsset.
    /// </summary>
    /// <param name="mod">Mod to create the window for</param>
    /// <param name="id">ID of the window</param>
    /// <param name="uxml">VisualTreeAsset to create the window from</param>
    /// <returns>Created window</returns>
    public static UIDocument Window(LuaMod mod, string id, VisualTreeAsset uxml)
    {
        var windowOptions = WindowOptions.Default;
        windowOptions.Parent = mod.transform;
        windowOptions.WindowId = id;
        return UitkForKsp2.API.Window.Create(windowOptions, uxml);
    }

    /// <summary>
    /// Creates a new window with an empty root element.
    /// </summary>
    /// <param name="mod">Mod to create the window for</param>
    /// <param name="id">ID of the window</param>
    /// <returns>Created window</returns>
    public static UIDocument Window(LuaMod mod, string id)
    {
        var windowOptions = WindowOptions.Default;
        windowOptions.Parent = mod.transform;
        windowOptions.WindowId = id;
        return UitkForKsp2.API.Window.Create(windowOptions);
    }

    #region Element Creation

    /// <summary>
    /// Creates a new VisualElement.
    /// </summary>
    /// <returns>Created VisualElement</returns>
    public static VisualElement VisualElement()
    {
        return new VisualElement();
    }

    /// <summary>
    /// Creates a new ScrollView.
    /// </summary>
    /// <returns>Created ScrollView</returns>
    public static ScrollView ScrollView()
    {
        return new ScrollView();
    }

    /// <summary>
    /// Creates a new ListView.
    /// </summary>
    /// <returns>Created ListView</returns>
    public static ListView ListView()
    {
        return new ListView();
    }

    /// <summary>
    /// Creates a new Toggle.
    /// </summary>
    /// <param name="text">Text of the Toggle</param>
    /// <returns>Created Toggle</returns>
    public static Toggle Toggle(string text = "")
    {
        return new Toggle
        {
            text = text
        };
    }

    /// <summary>
    /// Creates a new Label.
    /// </summary>
    /// <param name="text">Text of the Label</param>
    /// <returns>Created Label</returns>
    public static Label Label(string text = "")
    {
        return new Label
        {
            text = text
        };
    }

    /// <summary>
    /// Creates a new Button.
    /// </summary>
    /// <param name="text">Text of the Button</param>
    /// <returns>Created Button</returns>
    public static Button Button(string text = "")
    {
        return new Button
        {
            text = text
        };
    }

    /// <summary>
    /// Creates a new Scroller.
    /// </summary>
    /// <returns>Created Scroller</returns>
    public static Scroller Scroller()
    {
        return new Scroller();
    }

    /// <summary>
    /// Creates a new TextField.
    /// </summary>
    /// <param name="text">Text of the TextField</param>
    /// <returns>Created TextField</returns>
    public static TextField TextField(string text = "")
    {
        return new TextField
        {
            text = text
        };
    }

    /// <summary>
    /// Creates a new Foldout.
    /// </summary>
    /// <returns>Created Foldout</returns>
    public static Foldout Foldout()
    {
        return new Foldout();
    }

    /// <summary>
    /// Creates a new Slider.
    /// </summary>
    /// <param name="value">Value of the Slider</param>
    /// <param name="minValue">Minimum value of the Slider</param>
    /// <param name="maxValue">Maximum value of the Slider</param>
    /// <returns>Created Slider</returns>
    public static Slider Slider(float value = 0.0f, float minValue = 0.0f, float maxValue = 1.0f)
    {
        return new Slider
        {
            lowValue = minValue,
            highValue = maxValue,
            value = value
        };
    }

    /// <summary>
    /// Creates a new SliderInt.
    /// </summary>
    /// <param name="value">Value of the SliderInt</param>
    /// <param name="minValue">Minimum value of the SliderInt</param>
    /// <param name="maxValue">Maximum value of the SliderInt</param>
    /// <returns></returns>
    public static SliderInt SliderInt(int value = 0, int minValue = 0, int maxValue = 100)
    {
        return new SliderInt
        {
            lowValue = minValue,
            highValue = maxValue,
            value = value
        };
    }

    /// <summary>
    /// Creates a new MinMaxSlider.
    /// </summary>
    /// <param name="minValue">Minimum value of the MinMaxSlider</param>
    /// <param name="maxValue">Maximum value of the MinMaxSlider</param>
    /// <param name="minLimit">Minimum limit of the MinMaxSlider</param>
    /// <param name="maxLimit">Maximum limit of the MinMaxSlider</param>
    /// <returns></returns>
    public static MinMaxSlider MinMaxSlider(
        float minValue = 0.0f,
        float maxValue = 1.0f,
        float minLimit = 0.0f,
        float maxLimit = 1.0f
    )
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

    /// <summary>
    /// Adds a callback to a button from Lua
    /// </summary>
    /// <param name="button">Button to add the callback to</param>
    /// <param name="callback">Callback to add</param>
    /// <param name="self">Self parameter for the callback</param>
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
    /// Registers a value changed callback from Lua
    /// The Lua functions parameters should be like function(self?,previous,new)
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <typeparam name="T">Type of the value</typeparam>
    public static void RegisterValueChangedCallback<T>(
        INotifyValueChanged<T> element,
        Closure callback,
        [CanBeNull] DynValue self = null
    )
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

    private static void RegisterGenericCallback<T>(
        VisualElement element,
        Closure callback,
        DynValue self,
        bool trickleDown = false
    ) where T : EventBase<T>, new()
    {
        if (self != null)
        {
            element.RegisterCallback<T>(
                evt => callback.Call(self, evt),
                trickleDown ? TrickleDown.TrickleDown : TrickleDown.NoTrickleDown
            );
        }
        else
        {
            element.RegisterCallback<T>(
                evt => callback.Call(evt),
                trickleDown ? TrickleDown.TrickleDown : TrickleDown.NoTrickleDown
            );
        }
    }


    #region Capture Events

    /// <summary>
    /// Registers a mouse capture callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseCaptureCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseCaptureEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse capture out callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseCaptureOutCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseCaptureOutEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a pointer capture callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerCaptureCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerCaptureEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a pointer capture out callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerCaptureOutCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerCaptureOutEvent>(element, callback, self, trickleDown);

    #endregion

    #region Change Events

    /// <summary>
    /// Registers a boolean value change event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterChangeBoolCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<ChangeEvent<bool>>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers an integer value change event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterChangeIntCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<ChangeEvent<int>>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a float value change event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterChangeFloatCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<ChangeEvent<float>>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a string value change event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterChangeStringCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<ChangeEvent<string>>(element, callback, self, trickleDown);

    #endregion

    #region Click Events

    /// <summary>
    /// Registers a click event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterClickCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<ClickEvent>(element, callback, self, trickleDown);

    #endregion

    #region Focus Events

    /// <summary>
    /// Registers a focus out event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterFocusOutCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<FocusOutEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a focus in event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterFocusInCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<FocusInEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a blur event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterBlurCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<BlurEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a focus event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterFocusCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<FocusEvent>(element, callback, self, trickleDown);

    #endregion

    #region Input Events

    /// <summary>
    /// Registers an input event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterInputCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<InputEvent>(element, callback, self, trickleDown);

    #endregion

    #region Layout Events

    /// <summary>
    /// Registers a geometry changed event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterGeometryChangedCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<GeometryChangedEvent>(element, callback, self, trickleDown);

    #endregion

    #region Mouse Events

    /// <summary>
    /// Registers a mouse down event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseDownCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseDownEvent>(element, callback, self, trickleDown);


    /// <summary>
    /// Registers a mouse up event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseUpCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseUpEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse move event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseMoveCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseMoveEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse wheel event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterWheelCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<WheelEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse enter window event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseEnterWindowCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseEnterWindowEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse leave window event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseLeaveWindowCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseLeaveWindowEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse enter event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseEnterCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseEnterEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse leave event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseLeaveCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseLeaveEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse over event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseOverCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseOverEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a mouse out event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterMouseOutCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<MouseOutEvent>(element, callback, self, trickleDown);

    #endregion

    #region Pointer Events

    /// <summary>
    /// Registers a pointer down event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerDownCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerDownEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a pointer up event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerUpCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerUpEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a pointer move event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerMoveCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerMoveEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a pointer enter event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerEnterCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerEnterEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a pointer leave event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerLeaveCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerLeaveEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a pointer over event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerOverCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerOverEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a pointer out event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterPointerOutCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<PointerOutEvent>(element, callback, self, trickleDown);

    #endregion

    #region Panel Events

    /// <summary>
    /// Registers an attach to panel event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterAttachToPanelCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<AttachToPanelEvent>(element, callback, self, trickleDown);

    /// <summary>
    /// Registers a detach from panel event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterDetachFromPanelCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<DetachFromPanelEvent>(element, callback, self, trickleDown);

    #endregion

    #region Tooltip Events

    /// <summary>
    /// Registers a tooltip event callback from Lua
    /// </summary>
    /// <param name="element">Element to register the callback for</param>
    /// <param name="callback">Callback to register</param>
    /// <param name="self">Self parameter for the callback</param>
    /// <param name="trickleDown">Whether the event should trickle down</param>
    public static void RegisterTooltipCallback(
        VisualElement element,
        Closure callback,
        [CanBeNull] DynValue self = null,
        bool trickleDown = false
    ) => RegisterGenericCallback<TooltipEvent>(element, callback, self, trickleDown);

    #endregion

    #endregion
}