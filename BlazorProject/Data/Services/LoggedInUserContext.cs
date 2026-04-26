using BlazorProject.Data.Models;

namespace BlazorProject.Services;

public static class LoggedInUserContext
{
    private static readonly object SyncRoot = new();
    private static Utilizador? _currentUser;

    public static Utilizador? CurrentUser
    {
        get
        {
            lock (SyncRoot)
            {
                return _currentUser;
            }
        }
    }

    public static event Action? CurrentUserChanged;

    public static void SetCurrentUser(Utilizador user)
    {
        lock (SyncRoot)
        {
            // Keep a detached copy so this state is independent from EF context lifetime.
            _currentUser = user;
        }

        CurrentUserChanged?.Invoke();
    }

    public static void Clear()
    {
        lock (SyncRoot)
        {
            _currentUser = null;
        }

        CurrentUserChanged?.Invoke();
    }
}
