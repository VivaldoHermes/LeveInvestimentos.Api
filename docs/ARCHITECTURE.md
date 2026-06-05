# Arquitetura LEVE Investimentos

## Modelo adotado

Estrutura hibrida em tres projetos, inspirada no eShopOnWeb, mas sem copiar a complexidade completa da referencia.

```text
src/
  LeveInvestimentos.Core/
    Domain/
    Application/
    Abstractions/
  LeveInvestimentos.Infrastructure/
    Persistence/
    Identity/
    Email/
    Files/
    Outbox/
    BackgroundServices/
  LeveInvestimentos.Web/
    Controllers/
    ViewModels/
    Views/
    Filters/
    Middleware/
    wwwroot/
tests/
  LeveInvestimentos.Tests/
    Domain/
    Application/
    Integration/
docs/
  database/
```

## Intencao das camadas

- `Core`: regras de negocio e contratos, sem dependencia de MVC, EF Core ou Identity.
- `Infrastructure`: detalhes tecnicos, persistencia, identidade, arquivos, e-mail e processamento em background.
- `Web`: composition root, MVC, Razor, autorizacao e entrada HTTP.
- `Tests`: suites organizadas por camada e fluxo.

## Decisoes iniciais

- MVC Razor sera a interface principal.
- EF Core, Identity e SQL Server pertencem a `Infrastructure`.
- Controllers e Views pertencem somente a `Web`.
- Regras de tarefa e usuario devem nascer em `Core`.
- eShopOnWeb e referencia arquitetural, nao blueprint literal.
- Autenticacao Web usa Application Service + Gateway: `AccountController` coordena HTTP, `IAccountService` orquestra o fluxo e `IAccountIdentityGateway` adapta ASP.NET Core Identity na infraestrutura.
