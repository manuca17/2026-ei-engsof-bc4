window.themeManager = {
    getTheme: function () {
        return localStorage.getItem('theme') || 'light';
    },
    setTheme: function (theme) {
        localStorage.setItem('theme', theme);
        document.documentElement.setAttribute('data-theme', theme);
    },
    initTheme: function () {
        var theme = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-theme', theme);
        return theme;
    }
};

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

window.downloadCsv = function (content, fileName) {
  try {
    const blob = new Blob(['\uFEFF' + content], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  } catch (error) {
    console.error('Erro ao exportar CSV:', error);
    alert('Erro ao exportar CSV. Tente novamente.');
  }
};

window.printToPdf = function (htmlContent, fileName) {
  try {
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      alert('Por favor, permitir pop-ups para gerar o PDF.');
      return;
    }

    printWindow.document.write(htmlContent);
    printWindow.document.close();
    
    printWindow.onload = function () {
      printWindow.print();
      setTimeout(() => {
        printWindow.close();
      }, 250);
    };
  } catch (error) {
    console.error('Erro ao gerar PDF:', error);
    alert('Erro ao gerar o PDF. Tente novamente.');
  }
};
