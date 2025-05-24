using System;
using ReduxLib.GameInterfaces;

namespace SpaceWarp.UI.Backend;

public struct LocalizedString
{
    public string Term;

    public static implicit operator LocalizedString(string x) => new()
    {
        Term = x
    };

    public static implicit operator string(LocalizedString x) => x.ToString();
    
    public override string ToString()
    {
        return ILocalizer.Instance.GetTranslation(Term) ?? Term;
    }
}