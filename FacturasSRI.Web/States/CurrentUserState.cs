using System;

namespace FacturasSRI.Web.States
{
    public class CurrentUserState
    {
        public bool IsLoggedIn { get; private set; }
        public string? Role { get; private set; }
        public Guid UserId { get; private set; }

        public event Action? OnChange;

        public void Login(string role)
        {
            IsLoggedIn = true;
            Role = role;
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}