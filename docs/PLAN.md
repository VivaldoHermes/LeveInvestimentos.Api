Plano — Desafio LEVE Investimentos (ASP.NET Core MVC, .NET 10)

> Atualizacao (reconciliacao): a arquitetura oficial e de TRES projetos + testes
> (`LeveInvestimentos.Core`, `LeveInvestimentos.Infrastructure`, `LeveInvestimentos.Web`,
> `LeveInvestimentos.Tests`), conforme `ARCHITECTURE.md` e o repositorio real. A descricao
> de "projeto unico" mais abaixo foi superada por esta secao. Nomes tecnicos seguem ingles
> (`User`, `TaskAssignment`, `Address`, `PhoneNumber`, `TaskStatus`) e a experiencia funcional
> e em portugues, conforme decisao de nomenclatura registrada em `TASKS.md`.
>
> Decisoes acordadas adicionais: (1) senha do novo usuario e gerada aleatoriamente, enviada por
> e-mail via Outbox, com troca obrigatoria no primeiro login (`MustChangePassword`); (2) status
> persistido da tarefa e `Pending -> Started -> Completed` + `Canceled`, e "Atrasada" e computado
> (`IsOverdue`), nunca persistido; (3) ha `docker-compose` (app + SQL Server); (4) soft delete e
> auditoria ficam FORA DE ESCOPO, documentados como premissa consciente.

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
│ Arquitetura │ MVC em camadas com três projetos (Core, Infrastructure, Web) + projeto de testes │ Separação real de responsabilidades inspirada no eShopOnWeb, sem copiar a complexidade completa. Core sem dependência de EF/MVC/Identity; Web como composition root. │
├─────────────┼───────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ Identidade │ ASP.NET Core Identity + Cookie Auth com User (IdentityUser<Guid> estendido) e roles Gestor/Subordinado │ Hash de senha (PBKDF2), lockout, claims e roles prontos. Customização via User cobre 100% do schema pedido. │
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
  LeveInvestimentos.Core/                ← regras de negócio e contratos (sem EF/MVC/Identity)
    Domain/
      Entities/
        User.cs                          (IdentityUser estendido — propriedades extras + MustChangePassword)
        TaskAssignment.cs                (aggregate root — nunca chamada apenas "Task")
      ValueObjects/
        Address.cs
        PhoneNumber.cs
      Enums/
        TaskStatus.cs                    (Pending, Started, Completed, Canceled — "Overdue" é computado)
        UserRole.cs                      (Manager, Subordinate)
      Events/
        TaskAssignmentCreatedEvent.cs
        TaskAssignmentCompletedEvent.cs
    Application/
      Common/
        Result.cs / Result<T>.cs / Error.cs
      Users/
        Commands/CreateUserCommand.cs    (sem senha — gerada pelo sistema)
        Services/UserService.cs
        Validators/CreateUserValidator.cs   (FluentValidation — e-mail válido, BirthDate não futura)
        DTOs/
      Tasks/
        Commands/{Create,Start,Complete,Cancel}TaskAssignmentCommand.cs
        Services/TaskAssignmentService.cs
        Validators/
        DTOs/
      Notifications/
        Handlers/                        ← reage a Domain Events e grava no Outbox
          TaskAssignmentCreatedEmailHandler.cs
          TaskAssignmentCompletedEmailHandler.cs
    Abstractions/
      IUserRepository.cs / ITaskAssignmentRepository.cs / IUnitOfWork.cs
      IDomainEventDispatcher.cs / IEmailSender.cs / IFileStorage.cs / IPasswordGenerator.cs

  LeveInvestimentos.Infrastructure/      ← detalhes técnicos
    Persistence/
      ApplicationDbContext.cs            (IdentityDbContext<User, IdentityRole<Guid>, Guid>)
      Configurations/                    (IEntityTypeConfiguration<T> por entidade)
      Repositories/                      (UserRepository, TaskAssignmentRepository, UnitOfWork)
      Seed/DatabaseSeeder.cs             (roles + gestor ti@… com MustChangePassword=false)
    Identity/
    Email/SmtpEmailSender.cs (MailKit) + EmailOptions.cs + Templates/
    Files/LocalFileStorage.cs           (salva foto em wwwroot/uploads, valida magic bytes)
    Outbox/OutboxMessage.cs
    BackgroundServices/OutboxDispatcherService.cs   (usa IServiceScopeFactory)
    DependencyInjection.cs               (AddInfrastructure)

  LeveInvestimentos.Web/                 ← composition root + MVC
    Controllers/                         (AccountController, UsersController [Authorize(Roles="Manager")], TaskAssignmentsController, HomeController)
    ViewModels/                          ← nunca expor entidades para a View
    Views/                               (Account/, Users/, Tasks/, Shared/)
    Filters/ , Middleware/ExceptionHandlingMiddleware.cs
    wwwroot/ (lib/uikit/, uploads/)
    Program.cs                           (HTTPS/HSTS, anti-forgery global, Serilog, DI, OutboxDispatcher)
    appsettings.json / appsettings.Development.json

tests/
  LeveInvestimentos.Tests/
    Domain/                              (TaskAssignment.Start/Complete/Cancel/IsOverdue, value objects)
    Application/                         (services com repos fakes — inclui anti-IDOR e e-mail único)
    Integration/                         (WebApplicationFactory — login, authz, fluxo de tarefa, Outbox)

docs/
  README.md / ARCHITECTURE.md / TASKS.md / CAUTIONS.md / PLAN.md
  database/01_schema.sql / 02_seed.sql
Dockerfile / docker-compose.yml         (app + SQL Server)
ER.png

---

Modelo de domínio

User : IdentityUser<Guid> — propriedades extras: FullName, BirthDate, Address (Value Object owned), LandlinePhone, MobilePhone, ProfilePhotoPath, ManagerId? (auto-referência:
subordinado → gestor), MustChangePassword (true para usuário criado por gestor, força troca no 1º login). BirthDate validada como não futura.

TaskAssignment — aggregate root: Id, Description, DueDate, Status (enum TaskStatus), ManagerId (gestor criador), SubordinateId (subordinado responsável), CreatedAt, CompletedAt?.
Métodos de domínio: Start() (Pending→Started), Complete(), Cancel(), IsOverdue() (computado: now>DueDate e não Completed/Canceled — não altera status persistido). Emite TaskAssignmentCreatedEvent
na factory Create e TaskAssignmentCompletedEvent em Complete(). Setters privados — mutação sempre via método (encapsulamento real). Datas em DateTimeOffset/UTC.

Senha do novo usuário: gerada aleatoriamente via IPasswordGenerator, enviada por e-mail (Outbox); usuário troca obrigatoriamente no 1º login (MustChangePassword).

Address (Value Object owned): Logradouro, Numero, Complemento, Bairro, Cidade, Estado, Cep. Validado no construtor.

OutboxMessage: Id, Type, PayloadJson, OccurredAt, ProcessedAt?, Attempts, LastError?.

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
- Factory — TaskAssignment.Create(manager, subordinate, description, dueDate) valida invariantes antes de instanciar.
- Decorator (via DI) — LoggingRepositoryDecorator opcional para logging sem poluir o repositório real (auditoria de dados fica fora de escopo).
- Middleware — tratamento global de exceções padronizando ProblemDetails.

SOLID — onde cada princípio aparece

- S — UserService faz cadastro; IEmailSender envia; IFileStorage armazena foto. Nenhuma classe acumula responsabilidades.
- O — IEmailSender/IFileStorage permitem nova implementação (SendGrid, S3) sem editar consumidores.
- L — Repositórios não burlam o contrato base (sem throw NotSupportedException).
- I — interfaces pequenas e específicas (IUserRepository, ITaskAssignmentRepository) em vez de um IGenericRepository<T> inchado.
- D — Controllers dependem de IUserService/ITaskAssignmentService, nunca de DbContext direto. Composition root em Program.cs.

---

Etapas de implementação (sugestão de PRs / commits)

1.  Scaffold — solution `LeveInvestimentos.sln` + três projetos (Core, Infrastructure, Web) e Tests; .editorconfig; nullable + warnings as errors; .gitignore; UIkit via libman; Dockerfile + docker-compose (app + SQL Server).
2.  Core — Domain — User, TaskAssignment, value objects (Address, PhoneNumber), enums (TaskStatus, UserRole), eventos, abstractions. Sem dependências externas. Testes unitários do domínio nesta etapa.
3.  Infrastructure — Persistence — ApplicationDbContext + Identity + configurations + migration inicial. Gerar 01_schema.sql.
4.  Infrastructure — Seed — DatabaseSeeder roda no startup (app.Services.CreateScope() em Program.cs): cria roles Gestor/Subordinado e o usuário ti@leveinvestimentos.com.br (MustChangePassword=false) se ainda não
    existir. Gerar 02_seed.sql documentado no README.
5.  Application — Users — UserService, validator (FluentValidation), Result pattern. Gera senha aleatória (IPasswordGenerator), marca MustChangePassword e enfileira e-mail de credenciais. Reutiliza UserManager<User>.
6.  Web — Auth + Users — AccountController (Login/Logout/AccessDenied/ChangePassword), fluxo de troca obrigatória no 1º login, UsersController com [Authorize(Roles="Manager")], ViewModels, Views em UIkit, upload de foto via IFileStorage.
7.  Application + Web — Tasks — TaskAssignmentService (Create/Start/Complete/Cancel/List + validação anti-IDOR), TaskAssignmentsController, Views (lista do gestor com filtro por status; lista do subordinado; badge "Atrasada").
8.  Domain events + Outbox — dispatcher de eventos integrado no SaveChangesAsync; tabela Outbox; OutboxDispatcherService (BackgroundService) com backoff e scope via IServiceScopeFactory.
9.  E-mail — SmtpEmailSender (MailKit), templates simples, integração ao consumidor da Outbox. Configurável em appsettings.json (host, porta, credenciais via User Secrets em dev).
10. Testes — unit (Domain + Application) e integration (WebApplicationFactory cobrindo: login funciona, gestor cadastra usuário, subordinado não cadastra, tarefa finaliza e produz Outbox).
11. Docs + entrega — README.md com passo a passo (pré-requisitos, dotnet ef database update, dotnet run, credenciais), ARCHITECTURE.md, scripts SQL em docs/database/, repositório público no
    GitHub.

---

Arquivos críticos a criar (referência rápida)

- src/LeveInvestimentos.Web/Program.cs — composition root: AddIdentity, AddDbContext, AddInfrastructure(), AddApplication(), AddHostedService<OutboxDispatcherService>, HTTPS/HSTS, anti-forgery global, Serilog, pipeline.
- src/LeveInvestimentos.Core/Domain/Entities/TaskAssignment.cs — agregado com setters privados, factory Create, métodos Start/Complete/Cancel, IsOverdue, eventos.
- src/LeveInvestimentos.Infrastructure/Persistence/ApplicationDbContext.cs — herda IdentityDbContext<User, IdentityRole<Guid>, Guid>, intercepta SaveChangesAsync para gravar OutboxMessages
  a partir de Domain Events.
- src/LeveInvestimentos.Infrastructure/Persistence/Seed/DatabaseSeeder.cs — semeadura idempotente.
- src/LeveInvestimentos.Infrastructure/BackgroundServices/OutboxDispatcherService.cs — loop de processamento com scope por ciclo.
- src/LeveInvestimentos.Web/Controllers/UsersController.cs — [Authorize(Roles="Manager")] em nível de classe.
- src/LeveInvestimentos.Web/Controllers/TaskAssignmentsController.cs — ações Index (lista por papel), Criar (gestor), Iniciar/Finalizar (subordinado dono), Cancelar.

---

Verificação (como provar que está pronto)

1.  dotnet build — zero warnings (warnings as errors ativado).
2.  dotnet test — unit + integration verdes.
3.  dotnet ef database update cria schema; ao subir, seeder cria gestor padrão.
4.  Smoke manual:

- Subir via `docker compose up` (app + SQL Server) → site abre.
- Login com ti@leveinvestimentos.com.br / teste123 → sucesso.
- Cadastrar novo subordinado com foto → registro persiste, foto em wwwroot/uploads/, e-mail de credenciais na Outbox/SMTP local.
- Login com subordinado → redirecionado para trocar a senha no 1º login; depois não enxerga Users/Criar (403).
- Gestor cria tarefa para subordinado → OutboxMessage gerada; após ~1s BackgroundService envia e-mail (usar MailHog/Papercut em dev para inspecionar).
- Gestor tenta criar tarefa para subordinado de outro gestor → negado (anti-IDOR).
- Subordinado inicia e finaliza tarefa → mensagens na Outbox, e-mail para o gestor.

5.  dotnet ef migrations script -o docs/database/01_schema.sql — confere que o script publicado bate com o estado atual.
6.  README executado em máquina limpa segue do clone ao login em ≤10 minutos.

---

Entregáveis finais

- Repositório público no GitHub (link a confirmar).
- README.md com: pré-requisitos (.NET 10 SDK, SQL Server LocalDB ou container, opcionalmente Papercut para SMTP), passos de setup, credenciais do gestor padrão, comandos úteis.
- docs/database/01_schema.sql + 02_seed.sql.
- docs/ARCHITECTURE.md com diagrama de camadas e justificativa de patterns.
- Suite de testes verde.
