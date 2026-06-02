Plano — Desafio LEVE Investimentos (ASP.NET Core MVC, .NET 10)

Context

A LEVE Investimentos é um novo cliente do ramo financeiro. Esta entrega inicial servirá de referência arquitetural para os próximos projetos da equipe — o padrão definido aqui será
reutilizado. O objetivo, portanto, não é apenas "fazer funcionar", e sim entregar um código coeso, extensível, reutilizável e alinhado às melhores práticas (SOLID + Design Patterns
adequados).

Escopo funcional (extraído do PDF):

1.  Autenticação por e-mail e senha.
2.  Usuário gestor padrão semeado: ti@leveinvestimentos.com.br / teste123.
3.  Cadastro de usuários (somente gestores podem cadastrar) com: Nome completo, Data de nascimento, Telefone fixo, Telefone celular, E-mail, Endereço, Foto.
4.  Distinção de perfil Gestor vs Subordinado.
5.  Agendamento de tarefas: gestor cria tarefa (mensagem + data limite) para subordinado e acompanha andamento.
6.  (Desejável) Notificação por e-mail ao subordinado na atribuição e ao gestor na finalização.

Restrições técnicas (do PDF):

- Front-end em Razor; estilização com UIkit; banco SQL Server; repositório público no GitHub; README com passo a passo e scripts de banco.

Restrições adicionais (do usuário):

- ASP.NET Core Full .NET 10.
- Foco em back-end (Razor/UIkit existirá, mas sem polimento visual extenso).
- Padrão MVC, SOLID, design patterns adequados.
- "Senior não significa mais código, e sim o melhor código" — preferir simplicidade certa sobre cerimônia desnecessária.

---

Decisões arquiteturais (acordadas)

┌─────────────┬───────────────────────────────────────────────────────────┬───────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ Eixo │ Decisão │ Por quê │
├─────────────┼───────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ Arquitetura │ MVC clássico em camadas leves (um projeto Web + um │ Velocidade de entrega sem perder separação de responsabilidades por pastas (Domain/, Application/, │
│ │ projeto de testes) │ Infrastructure/, Controllers/, Views/). Evita explosão de .csproj para o tamanho real do problema. │
├─────────────┼───────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ Identidade │ ASP.NET Core Identity + Cookie Auth com ApplicationUser │ Hash de senha (PBKDF2), lockout, claims e roles prontos. Customização via ApplicationUser cobre 100% do │
│ │ estendido e roles Gestor/Subordinado │ schema pedido. │
├─────────────┼───────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ ORM │ EF Core 10 + Migrations + Repository/UnitOfWork abstrato │ Migrations geram os scripts pedidos pelo desafio. Repository abstrai EF de Controllers e libera testes com │
│ │ │ fakes. │
├─────────────┼───────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ E-mails │ MailKit + Outbox Pattern via BackgroundService │ Não bloqueia request, sobrevive a falha de SMTP, mantém transacionalidade entre "tarefa salva" e "e-mail │
│ │ │ enfileirado". │
└─────────────┴───────────────────────────────────────────────────────────┴───────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

---

Estrutura de pastas (projeto único, organização semântica)

src/
Leve.Web/ ← único projeto ASP.NET Core MVC
Domain/ ← regras de negócio puras (sem dependências externas)
Entities/
ApplicationUser.cs (IdentityUser estendido)
Tarefa.cs (aggregate root)
ValueObjects/
Endereco.cs (owned entity EF)
Telefone.cs
Enums/
StatusTarefa.cs (Pendente, EmAndamento, Concluida, Cancelada)
PerfilUsuario.cs (Gestor, Subordinado)
Events/
TarefaAtribuidaEvent.cs
TarefaFinalizadaEvent.cs
Abstractions/ ← interfaces que o Domain expõe
IUserRepository.cs
ITarefaRepository.cs
IUnitOfWork.cs
IDomainEventDispatcher.cs

     Application/                         ← casos de uso (Service Layer)
       Common/
         Result.cs                        (Result Pattern — sem exceptions para fluxo)
         Error.cs
       Users/
         Commands/CriarUsuarioCommand.cs
         Services/UserService.cs          (implementa IUserService)
         Validators/CriarUsuarioValidator.cs    (FluentValidation)
         DTOs/
       Tarefas/
         Commands/{Criar,Finalizar,Cancelar}TarefaCommand.cs
         Services/TarefaService.cs
         Validators/
         DTOs/
       Notifications/
         Handlers/                        ← reage a Domain Events e grava no Outbox
           TarefaAtribuidaEmailHandler.cs
           TarefaFinalizadaEmailHandler.cs
         Abstractions/IEmailSender.cs

     Infrastructure/
       Persistence/
         ApplicationDbContext.cs          (IdentityDbContext<ApplicationUser>)
         Configurations/                  ← IEntityTypeConfiguration<T> para cada entidade
           ApplicationUserConfiguration.cs
           TarefaConfiguration.cs
           OutboxMessageConfiguration.cs
         Repositories/
           UserRepository.cs
           TarefaRepository.cs
           UnitOfWork.cs
         Seed/
           DatabaseSeeder.cs              (cria role Gestor + usuário ti@…)
         Outbox/
           OutboxMessage.cs
       Email/
         SmtpEmailSender.cs               (MailKit)
         EmailOptions.cs
         Templates/                       (Razor templates renderizados via RazorLight ou string interpolation)
       Files/
         LocalFileStorage.cs              (salva foto em wwwroot/uploads — abstração IFileStorage)
       BackgroundServices/
         OutboxDispatcherService.cs       (varre Outbox e envia)
       DependencyInjection.cs             (extension method AddInfrastructure)

     Web/
       Controllers/
         AccountController.cs             (Login, Logout, AccessDenied)
         UsuariosController.cs            ([Authorize(Roles="Gestor")])
         TarefasController.cs
         HomeController.cs
       ViewModels/                        ← nunca expor entidades para a View
       Views/
         Account/, Usuarios/, Tarefas/, Shared/
       wwwroot/
         lib/uikit/                       (UIkit via libman ou CDN)
         uploads/
       Filters/
         GestorOnlyAttribute.cs
       Middleware/
         ExceptionHandlingMiddleware.cs   (ProblemDetails)

     Program.cs                           (composition root)
     appsettings.json / appsettings.Development.json

tests/
Leve.Web.UnitTests/ (xUnit + FluentAssertions + NSubstitute)
Domain/ (regras puras — Tarefa.Finalizar(), etc.)
Application/ (services com repos fakes)
Leve.Web.IntegrationTests/ (WebApplicationFactory + SQL Server em container ou LocalDb)
AuthFlow, UsuariosCrud, TarefasFlow

docs/
README.md (passo a passo de execução)
ARCHITECTURE.md (diagrama de camadas + decisões)
database/
01_schema.sql (gerado por `dotnet ef migrations script`)
02_seed.sql (gestor padrão + roles)
ER.png

---

Modelo de domínio

ApplicationUser : IdentityUser<Guid> — propriedades extras: NomeCompleto, DataNascimento, Endereco (Value Object owned), TelefoneFixo, TelefoneCelular, FotoPath, GestorId? (auto-referência:
subordinado → gestor).

Tarefa — aggregate root: Id, Mensagem, DataLimite, Status (enum), CriadaPorId (gestor), AtribuidaParaId (subordinado), DataCriacao, DataFinalizacao?. Métodos de domínio: Finalizar(),
Cancelar(), EstaAtrasada(). Emite eventos TarefaAtribuidaEvent no construtor e TarefaFinalizadaEvent em Finalizar(). Setters privados — mutação sempre via método (encapsulamento real).

Endereco (Value Object owned): Logradouro, Numero, Complemento, Bairro, Cidade, Estado, Cep. Validado no construtor.

OutboxMessage: Id, Tipo, PayloadJson, OcorridoEm, ProcessadoEm?, Tentativas, UltimoErro?.

---

Padrões aplicados (e por que cada um)

- MVC — exigência do desafio; separa Controller (orquestração HTTP), View (Razor) e ViewModel (forma de dados para a View).
- Repository + Unit of Work — isola EF Core do Application; testes unitários sem precisar de SQL Server.
- Result Pattern (Result<T>) — fluxos previsíveis (validação, regra de negócio) não usam exceptions; exceptions ficam para falhas genuínas. Melhora legibilidade dos Controllers.
- Options Pattern (EmailOptions, SeedOptions) — config tipada em vez de IConfiguration["chave"] espalhado.
- Strategy — IFileStorage (local hoje, S3/Azure Blob amanhã sem tocar no Controller).
- Observer / Domain Events + Outbox — desacopla "criar tarefa" de "mandar e-mail". O caso de uso não conhece e-mail; só publica evento. Handler escreve no Outbox dentro da mesma transação do
  SaveChanges. OutboxDispatcherService (BackgroundService) entrega via MailKit com retry.
- Specification / Query Object (opcional, só onde valer) — filtros de tarefa por status/atribuído.
- Factory — Tarefa.Criar(gestor, subordinado, msg, prazo) valida invariantes antes de instanciar.
- Decorator (via DI) — LoggingRepositoryDecorator opcional para auditoria sem poluir o repositório real.
- Middleware — tratamento global de exceções padronizando ProblemDetails.

SOLID — onde cada princípio aparece

- S — UserService faz cadastro; IEmailSender envia; IFileStorage armazena foto. Nenhuma classe acumula responsabilidades.
- O — IEmailSender/IFileStorage permitem nova implementação (SendGrid, S3) sem editar consumidores.
- L — Repositórios não burlam o contrato base (sem throw NotSupportedException).
- I — interfaces pequenas e específicas (IUserRepository, ITarefaRepository) em vez de um IGenericRepository<T> inchado.
- D — Controllers dependem de IUserService/ITarefaService, nunca de DbContext direto. Composition root em Program.cs.

---

Etapas de implementação (sugestão de PRs / commits)

1.  Scaffold — dotnet new mvc -n Leve.Web --framework net10.0; .editorconfig; nullable + warnings as errors; .gitignore; estrutura de pastas; UIkit via libman.
2.  Domain layer — ApplicationUser, Tarefa, value objects, enums, eventos, abstractions. Sem dependências externas. Testes unitários do domínio nesta etapa.
3.  Infrastructure — Persistence — ApplicationDbContext + Identity + configurations + migration inicial. Gerar 01_schema.sql.
4.  Infrastructure — Seed — DatabaseSeeder roda no startup (app.Services.CreateScope() em Program.cs): cria roles Gestor/Subordinado e o usuário ti@leveinvestimentos.com.br se ainda não
    existir. Gerar 02_seed.sql documentado no README.
5.  Application — Users — UserService, validator (FluentValidation), Result pattern. Reutiliza UserManager<ApplicationUser> do Identity.
6.  Web — Auth + Users — AccountController (Login/Logout/AccessDenied), UsuariosController com [Authorize(Roles="Gestor")], ViewModels, Views em UIkit, upload de foto via IFileStorage.
7.  Application + Web — Tarefas — TarefaService (Criar/Finalizar/Cancelar/Listar), TarefasController, Views (lista do gestor com filtro por status; lista do subordinado).
8.  Domain events + Outbox — dispatcher de eventos integrado no SaveChangesAsync; tabela Outbox; OutboxDispatcherService (BackgroundService) com backoff exponencial.
9.  E-mail — SmtpEmailSender (MailKit), templates simples, integração ao consumidor da Outbox. Configurável em appsettings.json (host, porta, credenciais via User Secrets em dev).
10. Testes — unit (Domain + Application) e integration (WebApplicationFactory cobrindo: login funciona, gestor cadastra usuário, subordinado não cadastra, tarefa finaliza e produz Outbox).
11. Docs + entrega — README.md com passo a passo (pré-requisitos, dotnet ef database update, dotnet run, credenciais), ARCHITECTURE.md, scripts SQL em docs/database/, repositório público no
    GitHub.

---

Arquivos críticos a criar (referência rápida)

- src/Leve.Web/Program.cs — composition root: AddIdentity, AddDbContext, AddInfrastructure(), AddApplication(), AddHostedService<OutboxDispatcherService>, pipeline.
- src/Leve.Web/Domain/Entities/Tarefa.cs — agregado com setters privados, factory Criar, métodos Finalizar/Cancelar, eventos.
- src/Leve.Web/Infrastructure/Persistence/ApplicationDbContext.cs — herda IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, intercepta SaveChangesAsync para gravar OutboxMessages
  a partir de Domain Events.
- src/Leve.Web/Infrastructure/Seed/DatabaseSeeder.cs — semeadura idempotente.
- src/Leve.Web/Infrastructure/BackgroundServices/OutboxDispatcherService.cs — loop de processamento.
- src/Leve.Web/Web/Controllers/UsuariosController.cs — [Authorize(Roles="Gestor")] em nível de classe.
- src/Leve.Web/Web/Controllers/TarefasController.cs — ações Index (lista por papel), Criar (gestor), Finalizar (subordinado dono).

---

Verificação (como provar que está pronto)

1.  dotnet build — zero warnings (warnings as errors ativado).
2.  dotnet test — unit + integration verdes.
3.  dotnet ef database update cria schema; ao subir, seeder cria gestor padrão.
4.  Smoke manual:

- Login com ti@leveinvestimentos.com.br / teste123 → sucesso.
- Cadastrar novo subordinado com foto → registro persiste, foto em wwwroot/uploads/.
- Login com subordinado → não enxerga Usuarios/Criar (403).
- Gestor cria tarefa para subordinado → OutboxMessage gerada; após ~1s BackgroundService envia e-mail (usar MailHog/Papercut em dev para inspecionar).
- Subordinado finaliza tarefa → segunda mensagem na Outbox, e-mail para o gestor.

5.  dotnet ef migrations script -o docs/database/01_schema.sql — confere que o script publicado bate com o estado atual.
6.  README executado em máquina limpa segue do clone ao login em ≤10 minutos.

---

Entregáveis finais

- Repositório público no GitHub (link a confirmar).
- README.md com: pré-requisitos (.NET 10 SDK, SQL Server LocalDB ou container, opcionalmente Papercut para SMTP), passos de setup, credenciais do gestor padrão, comandos úteis.
- docs/database/01_schema.sql + 02_seed.sql.
- docs/ARCHITECTURE.md com diagrama de camadas e justificativa de patterns.
- Suite de testes verde.
