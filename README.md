# LEVE Investimentos

Aplicacao ASP.NET Core MVC em .NET 10 para cadastro de usuarios, atribuicao de tarefas e notificacoes por e-mail via Outbox.

## Banco de dados local com Docker

O projeto usa SQL Server. Para subir o banco local pelo Docker Desktop no Windows, execute na raiz do repositorio:

```powershell
docker compose up -d
```

Se o seu terminal nao reconhecer `docker compose`, use o comando legado:

```powershell
docker-compose up -d
```

Depois que o container estiver em execucao, aplique as migrations:

```powershell
dotnet tool install dotnet-ef --tool-path .tools --version 10.0.8
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.tools\dotnet-ef database update --project src\LeveInvestimentos.Infrastructure\LeveInvestimentos.Infrastructure.csproj --startup-project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
```

Para rodar a aplicacao:

```powershell
dotnet run --project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
```

## Primeiro Acesso

E-mail: ti@leveinvestimentos.com.br
Senha: teste123

## Arquitetura em Camadas

![Arquitetura em camadas](docs/img1.jpg)

## Pipeline HTTP (toda requisição)

![Pipeline HTTP](docs/img2.jpg)


## Para Rodar os Testes

Os testes usam SQLite em memoria e **nao precisam** do SQL Server ou do Docker em execucao.

Comando recomendado (restaura, compila e executa):

```powershell
dotnet test tests\LeveInvestimentos.Tests\LeveInvestimentos.Tests.csproj
```

Se o codigo ja foi compilado e nada mudou desde a ultima execucao, voce pode pular restore e build:

```powershell
dotnet test tests\LeveInvestimentos.Tests\LeveInvestimentos.Tests.csproj --no-build --no-restore --verbosity minimal
```

Depois de alterar codigo ou adicionar testes, rode novamente sem `--no-build` para evitar executar binarios desatualizados.