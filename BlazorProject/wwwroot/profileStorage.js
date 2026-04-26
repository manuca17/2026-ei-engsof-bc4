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
