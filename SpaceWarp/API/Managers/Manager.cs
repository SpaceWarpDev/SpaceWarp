using KSP.Game;

namespace SpaceWarp.API.Managers;

/// <summary>
/// Class to register itself on the ManagerLocator class for ease and performant access.
/// </summary>
public class Manager : KerbalMonoBehaviour
{
    protected virtual void Start()
    {
        ManagerLocator.Add(this); 
    }

    private void OnDestroy()
    {
        ManagerLocator.Remove(this);
    }
}