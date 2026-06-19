using MoonSharp.Interpreter;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// The Lua <c>Console</c> global, through which a mod exposes values and functions to the in-game console.
/// </summary>
/// <remarks>
/// Whatever a mod registers becomes a global on the shared console environment, so a console user can call a
/// mod-provided function or read a mod-provided value by name. A registered function keeps the mod environment
/// it was defined in, so it still sees that mod's globals when the console calls it.
/// </remarks>
[MoonSharpUserData]
public sealed class ConsoleLibrary
{
    static ConsoleLibrary()
    {
        UserData.RegisterType<ConsoleLibrary>();
    }

    /// <summary>
    /// Registers a value or function under <paramref name="name" /> as a global on the console environment.
    /// </summary>
    /// <param name="name">The global name console scripts use to reach the value.</param>
    /// <param name="value">The value or function to expose.</param>
    public void Register(string name, DynValue value)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ScriptRuntimeException("Console.Register expects a non-empty name.");
        }

        ModScriptRuntime.RegisterConsoleGlobal(name, value);
    }
}
