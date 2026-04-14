using System;
using System.Collections.Generic;
using BlazorProject.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.Data;

public partial class EiEngsofContext : DbContext
{
    public EiEngsofContext()
    {
    }

    public EiEngsofContext(DbContextOptions<EiEngsofContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Anotacao> Anotacaos { get; set; }

    public virtual DbSet<CodigoPostal> CodigoPostals { get; set; }

    public virtual DbSet<Consulta> Consulta { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<ExameMedico> ExameMedicos { get; set; }

    public virtual DbSet<Fatura> Faturas { get; set; }

    public virtual DbSet<Paciente> Pacientes { get; set; }

    public virtual DbSet<TipoConsulta> TipoConsulta { get; set; }

    public virtual DbSet<Utilizador> Utilizadores { get; set; }

    public virtual DbSet<UtilizadorConsulta> UtilizadorConsulta { get; set; }
    
    public virtual DbSet<Anotacao> Anotacoes { get; set; }
    
    public virtual DbSet<ExameMedicoConsulta> ExameMedicoConsulta { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=dpg-d7f632741pts73cb1iog-a.frankfurt-postgres.render.com;Database=ei_engsof;Username=admin;Password=XyBqk2fjseXaSz9VXVDGALfy9UeeHId6;SSL Mode=Require;Trust Server Certificate=true");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Anotacao>(entity =>
        {
            entity.HasKey(e => e.IdAnotacao).HasName("pk_anotacao");

            entity.ToTable("anotacao");

            entity.HasIndex(e => e.IdConsulta, "idx_anotacao_consulta");

            entity.HasIndex(e => e.IdUtilizador, "idx_anotacao_utilizador");

            entity.Property(e => e.IdAnotacao).HasColumnName("id_anotacao");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.DhCriacao)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dh_criacao");
            entity.Property(e => e.DhUltimaEdicao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dh_ultima_edicao");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");

            entity.HasOne(d => d.IdConsultaNavigation).WithMany(p => p.Anotacaos)
                .HasForeignKey(d => d.IdConsulta)
                .HasConstraintName("fk_anot_consulta");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.Anotacaos)
                .HasForeignKey(d => d.IdUtilizador)
                .HasConstraintName("fk_anot_utilizador");
        });

        modelBuilder.Entity<CodigoPostal>(entity =>
        {
            entity.HasKey(e => e.CodPostal).HasName("pk_codigo_postal");

            entity.ToTable("codigo_postal");

            entity.Property(e => e.CodPostal)
                .HasMaxLength(10)
                .HasColumnName("cod_postal");
            entity.Property(e => e.Localidade)
                .HasMaxLength(100)
                .HasColumnName("localidade");
        });

        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.HasKey(e => e.IdConsulta).HasName("pk_consulta");

            entity.ToTable("consulta");

            entity.HasIndex(e => e.IdFatura, "idx_consulta_fatura");
            entity.HasIndex(e => e.IdPaciente, "idx_consulta_paciente");
            entity.HasIndex(e => e.IdTipoConsulta, "idx_consulta_tipo");

            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.DhFim)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dh_fim");
            entity.Property(e => e.DhInicio)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dh_inicio");
            entity.Property(e => e.IdFatura).HasColumnName("id_fatura");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.IdTipoConsulta).HasColumnName("id_tipo_consulta");
            entity.Property(e => e.ValorHora)
                .HasPrecision(10, 2)
                .HasColumnName("valor_hora");
            entity.Property(e => e.ValorTotal)
                .HasPrecision(10, 2)
                .HasColumnName("valor_total");

            entity.HasOne(d => d.IdFaturaNavigation).WithMany(p => p.Consulta)
                .HasForeignKey(d => d.IdFatura)
                .HasConstraintName("fk_con_fatura");

            entity.HasOne(d => d.IdPacienteNavigation).WithMany(p => p.Consulta)
                .HasForeignKey(d => d.IdPaciente)
                .HasConstraintName("fk_con_paciente");

            entity.HasOne(d => d.IdTipoConsultaNavigation).WithMany(p => p.Consulta)
                .HasForeignKey(d => d.IdTipoConsulta)
                .HasConstraintName("fk_con_tipo");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("pk_estado");

            entity.ToTable("estado");

            entity.HasIndex(e => e.IdConsulta, "idx_estado_consulta");

            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.Comentario).HasColumnName("comentario");
            entity.Property(e => e.DhRegisto)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dh_registo");
            entity.Property(e => e.EstadoTo)
                .HasMaxLength(100)
                .HasColumnName("estado_to");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");

            entity.HasOne(d => d.IdConsultaNavigation).WithMany(p => p.Estados)
                .HasForeignKey(d => d.IdConsulta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_est_consulta");
        });

        modelBuilder.Entity<ExameMedico>(entity =>
        {
            entity.HasKey(e => e.IdExameMedico).HasName("pk_exame_medico");

            entity.ToTable("exame_medico");

            entity.HasIndex(e => e.IdUtilizador, "idx_exame_medico_util");

            entity.Property(e => e.IdExameMedico).HasColumnName("id_exame_medico");
            entity.Property(e => e.DhExame)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dh_exame");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.Resultado).HasColumnName("resultado");
            entity.Property(e => e.Tipo)
                .HasMaxLength(100)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.ExameMedicos)
                .HasForeignKey(d => d.IdUtilizador)
                .HasConstraintName("fk_em_utilizador");
        });

        modelBuilder.Entity<Fatura>(entity =>
        {
            entity.HasKey(e => e.IdFatura).HasName("pk_fatura");

            entity.ToTable("fatura");

            entity.Property(e => e.IdFatura).HasColumnName("id_fatura");
            entity.Property(e => e.DhPagamento)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dh_pagamento");
            entity.Property(e => e.Metodo)
                .HasMaxLength(50)
                .HasColumnName("metodo");
            entity.Property(e => e.ValorPago)
                .HasPrecision(10, 2)
                .HasColumnName("valor_pago");
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.IdPaciente).HasName("pk_paciente");

            entity.ToTable("paciente");

            entity.HasIndex(e => e.CodPostal, "idx_paciente_codpostal");

            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.CodPostal)
                .HasMaxLength(10)
                .HasColumnName("cod_postal");
            entity.Property(e => e.DtNasc).HasColumnName("dt_nasc");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Nif)
                .HasMaxLength(20)
                .HasColumnName("nif");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.NumPorta)
                .HasMaxLength(10)
                .HasColumnName("num_porta");
            entity.Property(e => e.Rua)
                .HasMaxLength(200)
                .HasColumnName("rua");
            entity.Property(e => e.Telefone)
                .HasMaxLength(20)
                .HasColumnName("telefone");

            entity.HasOne(d => d.CodPostalNavigation).WithMany(p => p.Pacientes)
                .HasForeignKey(d => d.CodPostal)
                .HasConstraintName("fk_pac_codpostal");
            
            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.Pacientes)
                .HasForeignKey(d => d.IdUtilizador)
                .HasConstraintName("fk_pac_utilizador");
        });

        modelBuilder.Entity<TipoConsulta>(entity =>
        {
            entity.HasKey(e => e.IdTipoConsulta).HasName("pk_tipo_consulta");

            entity.ToTable("tipo_consulta");

            entity.HasIndex(e => e.IdUtilizador, "idx_tipo_consulta_util");

            entity.Property(e => e.IdTipoConsulta).HasColumnName("id_tipo_consulta");
            entity.Property(e => e.Descricao)
                .HasMaxLength(200)
                .HasColumnName("descricao");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.PrecoFixo)
                .HasPrecision(10, 2)
                .HasColumnName("preco_fixo");
            entity.Property(e => e.PrecoHora)
                .HasPrecision(10, 2)
                .HasColumnName("preco_hora");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.TipoConsulta)
                .HasForeignKey(d => d.IdUtilizador)
                .HasConstraintName("fk_tc_utilizador");
        });

        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(e => e.IdUtilizador).HasName("pk_utilizador");

            entity.ToTable("utilizador");

            entity.HasIndex(e => e.CodPostal, "idx_utilizador_codpostal");

            entity.HasIndex(e => e.Username, "utilizador_username_key").IsUnique();

            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.CodPostal)
                .HasMaxLength(10)
                .HasColumnName("cod_postal");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Especialidade)
                .HasMaxLength(100)
                .HasColumnName("especialidade");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.IsManager).HasColumnName("is_manager");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.NumCarteira)
                .HasMaxLength(50)
                .HasColumnName("num_carteira");
            entity.Property(e => e.NumPorta)
                .HasMaxLength(10)
                .HasColumnName("num_porta");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Rua)
                .HasMaxLength(200)
                .HasColumnName("rua");
            entity.Property(e => e.Telefone)
                .HasMaxLength(20)
                .HasColumnName("telefone");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.CodPostalNavigation).WithMany(p => p.Utilizadores)
                .HasForeignKey(d => d.CodPostal)
                .HasConstraintName("fk_util_codpostal");
        });

        modelBuilder.Entity<ExameMedicoConsulta>(entity =>
        {
            entity.HasKey(e => new { e.IdExameMedico, e.IdConsulta }).HasName("pk_exame_medico_consulta");

            entity.ToTable("exame_medico_consulta");

            entity.Property(e => e.IdExameMedico).HasColumnName("id_exame_medico");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.IdUtilzador).HasColumnName("id_utilizador");
            entity.Property(e => e.dhRegisto)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dh_registo");

            entity.HasOne(e => e.IdExameMedicoNavigation)
                .WithMany(p => p.ExameMedicoConsultas)
                .HasForeignKey(e => e.IdExameMedico)
                .HasConstraintName("fk_emc_exame");

            entity.HasOne(e => e.IdConsultaNavigation)
                .WithMany(p => p.ExamesDaConsulta)
                .HasForeignKey(e => e.IdConsulta)
                .HasConstraintName("fk_emc_consulta");
        });
        
        modelBuilder.Entity<UtilizadorConsulta>(entity =>
        {
            entity.HasKey(e => new { e.IdUtilizador, e.IdConsulta }).HasName("pk_utilizador_consulta");

            entity.ToTable("utilizador_consulta");

            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.ConviteAceite).HasColumnName("convite_aceite");
            entity.Property(e => e.IsCriador).HasColumnName("is_criador");

            entity.HasOne(d => d.IdConsultaNavigation).WithMany(p => p.UtilizadorConsulta)
                .HasForeignKey(d => d.IdConsulta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_uc_consulta");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.UtilizadorConsulta)
                .HasForeignKey(d => d.IdUtilizador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_uc_utilizador");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
