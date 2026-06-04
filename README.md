# LEVE Investimentos

Aplicacao ASP.NET Core MVC em .NET 10 para cadastro de usuarios, atribuicao de tarefas e notificacoes por e-mail via Outbox.

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
