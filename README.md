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

Isso cria um container `leve-investimentos-sqlserver` na porta `1433`, com banco persistido no volume `leve-investimentos-sqlserver-data`.

A connection string de desenvolvimento fica em `src/LeveInvestimentos.Web/appsettings.Development.json`:

```text
Server=localhost,1433;Database=LeveInvestimentos;User Id=sa;Password=Leve@123456;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true
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

Comandos uteis:

```powershell
docker compose ps
docker compose logs sqlserver
docker compose down
```

Se estiver usando `docker-compose`, mantenha o hifen nesses comandos: `docker-compose ps`, `docker-compose logs sqlserver` e `docker-compose down`.

## Configuracao SMTP local

As chaves publicas de configuracao ficam em `src/LeveInvestimentos.Web/appsettings.json`, na secao `Email`. Nao grave usuario ou senha SMTP reais nesse arquivo.

Para desenvolvimento, use User Secrets no projeto Web:

```powershell
dotnet user-secrets init --project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
dotnet user-secrets set "Email:Host" "localhost" --project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
dotnet user-secrets set "Email:Port" "25" --project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
dotnet user-secrets set "Email:UserName" "seu-usuario-smtp" --project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
dotnet user-secrets set "Email:Password" "sua-senha-smtp" --project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
dotnet user-secrets set "Email:FromEmail" "nao-responda@leveinvestimentos.com.br" --project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
dotnet user-secrets set "Email:UseSsl" "false" --project src\LeveInvestimentos.Web\LeveInvestimentos.Web.csproj
```

Para smoke tests locais, Papercut, MailHog ou outro servidor SMTP local pode escutar em `localhost:25` sem autenticacao. Em SMTP externo, configure `Email:UseSsl=true` e informe usuario/senha pelos User Secrets.
