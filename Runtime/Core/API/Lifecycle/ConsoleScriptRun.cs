using System;
using MoonSharp.Interpreter;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// A console Lua run in progress: a coroutine on the console environment that the console resumes each frame.
/// </summary>
public sealed class ConsoleScriptRun
{
    private readonly DynValue _coroutine;

    internal ConsoleScriptRun(DynValue coroutine)
    {
        _coroutine = coroutine;
    }

    /// <summary>
    /// Whether the run has finished, either by completing or by erroring.
    /// </summary>
    public bool IsFinished { get; private set; }

    /// <summary>
    /// Whether the run ended with an error.
    /// </summary>
    public bool IsErrored { get; private set; }

    /// <summary>
    /// The run's result text: the return value when it completed, or the error message when it failed.
    /// </summary>
    public string Result { get; private set; } = string.Empty;

    /// <summary>
    /// Resumes the run once, advancing the coroutine to its next yield or to completion.
    /// </summary>
    public void Resume()
    {
        if (IsFinished)
        {
            return;
        }

        try
        {
            var value = _coroutine.Coroutine.Resume();
            if (_coroutine.Coroutine.State == CoroutineState.Dead)
            {
                IsFinished = true;
                Result = value.IsNil() ? string.Empty : value.ToPrintString();
            }
        }
        catch (InterpreterException e)
        {
            IsFinished = true;
            IsErrored = true;
            Result = e.DecoratedMessage;
        }
        catch (Exception e)
        {
            IsFinished = true;
            IsErrored = true;
            Result = e.Message;
        }
    }
}
