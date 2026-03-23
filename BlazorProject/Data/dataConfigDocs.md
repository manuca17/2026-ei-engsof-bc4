# Comandos para que o modulo de acesso á base de dados possa ser trabalhado
```` 
git clone oNossoRepositorioFixe
cd oNossoRepositorioFixe
dotnet restore
dotnet ef database update
dotnet run
````

# Criação dos modelos
Os modelos que representam cada entidade foram gerados automaticamente utilzando o seguinte comando:
````shell
dotnet ef dbcontext scaffold "Host=localhost;Port=5432;Database=ei-engsof;Username=admin;Password=admin123" Npgsql.EntityFrameworkCore.PostgreSQL -o Data/Models             
````
Que faz algo chamado de "reverse scaffolding" para gerar os modelos de acordo com o que está presente na base de dados pgsql.
# Detalhes da ligação
O comando contem uma ligação ao postgres que pode gerar erros se nao for feita corretamente:
* O host será sempre localhost(durante o desenvolvimento)
* A Port é 5433 por opção minha. SE OCORRER UM ERRO, RETIRA ``Port=5433``, isto vai fazer com que use a port default (5432)
* Datbase é o nome que-lhe atribui, seria ideal que não fosse mudado.
* Username e Password deve ser um utilizador (NÃO POSTGRES) criado por ti prorio, idealmente seria o mesmo para todos para não haver conflitos no merge.

# Criação de migrações
Migrações são ficheiros criados pelo Blazor para aplicar modificações á base de dados a partir do projeto.
Para registar uma nova modificação fazemos a modificação aos modelos e executamos o comando:
````shell
dotnet ef migrations add ACTONTABLE
dotnet ef database update
````


