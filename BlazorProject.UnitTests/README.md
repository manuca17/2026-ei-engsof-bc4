# Testes Unitarios

Este projeto contem testes unitarios em NUnit para a logica de negocio implementada nos servicos da aplicacao.

## Projeto

- Projeto de testes: `BlazorProject.UnitTests`
- Framework de testes: `NUnit`
- Base de dados de teste: `EF Core InMemory`

Os testes nao usam a base de dados PostgreSQL real. Cada teste cria uma base de dados em memoria isolada, para garantir execucao rapida e repetivel.

## Como executar

Na raiz do repositorio, correr:

```powershell
dotnet test BlazorProject.UnitTests\BlazorProject.UnitTests.csproj
```

Se quiser correr apenas uma classe de testes:

```powershell
dotnet test BlazorProject.UnitTests\BlazorProject.UnitTests.csproj --filter UtilizadorServiceTests
dotnet test BlazorProject.UnitTests\BlazorProject.UnitTests.csproj --filter PacienteServiceTests
dotnet test BlazorProject.UnitTests\BlazorProject.UnitTests.csproj --filter ConsultasServiceTests
```

## Estrutura

### `Infrastructure/TestDbContextFactory.cs`

Contem a infraestrutura de apoio aos testes.

- Cria instancias de `EiEngsofContext` para testes.
- Usa `EF Core InMemory` em vez da base de dados real.
- Evita o `OnConfiguring` original, que força `Npgsql` e impediria a execucao isolada dos testes.

### `UtilizadorServiceTests.cs`

Testa os comportamentos principais do servico de utilizadores.

- `RegisterAsync_WithUniqueEmailAndUsername_HashesPasswordAndPersistsUser`
  Verifica que um utilizador valido e guardado e que a password fica cifrada com hash.

- `RegisterAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException`
  Verifica que o registo falha quando o email ja existe.

- `LoginAsync_WithMatchingPassword_ReturnsUser`
  Verifica que o login devolve o utilizador quando as credenciais estao corretas.

- `LoginAsync_WithWrongPassword_ReturnsNull`
  Verifica que o login falha com password incorreta.

### `PacienteServiceTests.cs`

Testa operacoes relevantes da gestao de pacientes.

- `SavePacienteAsync_NewPatientWithoutOwner_AssignsCreatorUserId`
  Verifica que, ao criar um paciente novo, o medico criador fica associado ao registo.

- `SavePacienteAsync_WithFutureBirthDate_ThrowsArgumentException`
  Verifica a validacao da data de nascimento, impedindo datas no futuro.

- `GetByDoctorAsync_ReturnsOwnAndAcceptedConsultationPatientsOrderedByName`
  Verifica que o medico ve os seus pacientes e tambem pacientes ligados por consultas aceites, por ordem alfabetica.

### `ConsultasServiceTests.cs`

Testa regras de acesso e projecao de dados das consultas.

- `GetPatientsForDoctorAsync_WithInvalidDoctorId_ReturnsEmptyList`
  Verifica que um identificador invalido nao devolve pacientes.

- `GetPatientsForDoctorAsync_ReturnsDistinctOwnAndAcceptedPatientsOrderedByName`
  Verifica que o medico obtem apenas pacientes validos, sem duplicados e por ordem alfabetica.

- `GetByIdForDoctorAsync_WithoutPermission_ReturnsNull`
  Verifica que um medico sem permissao nao consegue aceder ao resumo de uma consulta.

- `GetByIdForDoctorAsync_WithAcceptedAccess_ReturnsProjectedConsultationSummary`
  Verifica que, com permissao valida, o servico devolve os dados projetados da consulta, incluindo estado, descricao e cobranca.

## Resultado atual

Estado no momento da validacao:

- Total de testes: 11
- Aprovados: 11
- Falhados: 0

## Nota tecnica

Durante a execucao pode surgir um warning de versoes do `Microsoft.EntityFrameworkCore.Relational` (`10.0.4` e `10.0.5`).
Esse warning nao impediu a compilacao nem a execucao dos testes e nao afetou os resultados obtidos.

## Resumo para defesa

Este trabalho cobre tres areas principais da logica da aplicacao:

- autenticacao e registo de utilizadores;
- criacao e validacao de pacientes;
- acesso, filtragem e leitura de consultas por medico.

Os testes foram desenhados para validar casos normais, casos de erro e regras de permissao, sem dependencia da base de dados real.
