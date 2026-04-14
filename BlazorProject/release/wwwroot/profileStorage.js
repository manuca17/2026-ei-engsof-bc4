window.profileStorage = {
  getCurrentUser: function () {
    try {
      const raw = localStorage.getItem('currentUser');
      if (!raw) return null;
      return JSON.parse(raw);
    } catch {
      return null;
    }
  },

  updateUser: function (id, updates) {
    try {
      const raw = localStorage.getItem('currentUser');
      if (!raw) return null;

      const user = JSON.parse(raw);
      if (!user || user.id !== id) return null;

      const updated = { ...user, ...updates };
      localStorage.setItem('currentUser', JSON.stringify(updated));
      return updated;
    } catch {
      return null;
    }
  }
};
