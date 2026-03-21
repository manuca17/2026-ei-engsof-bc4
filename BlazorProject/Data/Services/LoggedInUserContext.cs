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

    public static void SetCurrentUser(Utilizador user)
    {
        lock (SyncRoot)
        {
            // Keep a detached copy so this state is independent from EF context lifetime.
            _currentUser = new Utilizador
            {
                IdUtilizador = user.IdUtilizador,
                IsManager = user.IsManager,
                IsAdmin = user.IsAdmin,
                Nome = user.Nome,
                Username = user.Username,
                Password = user.Password,
                Telefone = user.Telefone,
                Email = user.Email,
                NumPorta = user.NumPorta,
                Rua = user.Rua,
                CodPostal = user.CodPostal,
                NumCarteira = user.NumCarteira,
                Especialidade = user.Especialidade
            };
        }
    }

    public static void Clear()
    {
        lock (SyncRoot)
        {
            _currentUser = null;
        }
    }
}
