namespace BlazorProject.Data.Models;

public class ExameMedicoConsulta
{
    public int IdExameMedico { get; set; }
    
    public int IdConsulta { get; set; }
    
    public int IdUtilzador { get; set; } // id do utilizador que atribuio o exame á consulta
    
    public DateTime? dhRegisto { get; set; }
    
    public virtual Utilizador? IdUtilizadorNavigation { get; set; }
    
    public virtual Consulta? IdConsultaNavigation { get; set; }
    
    public virtual ExameMedico? IdExameMedicoNavigation { get; set; }
}