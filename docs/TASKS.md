# Tasks - LEVE Investimentos

Decisao critica de nomenclatura: todo codigo deve usar ingles, e toda experiencia funcional deve usar portugues. Entidades, classes, enums, DTOs, ViewModels, services, controllers, migrations e nomes tecnicos devem seguir o ecossistema C#/.NET com termos como `User`, `Address`, `UserPhoto`, `Role`, `TaskAssignment`, `TaskStatus`, `EmailNotification`, `Manager`, `Subordinate`, `DueDate`, `Description`, `ProfilePhoto`, `LandlinePhone` e `MobilePhone`. Nao criar entidade, classe ou model chamada apenas `Task`, porque em C# ja existe `System.Threading.Tasks.Task`; a escolha principal para a regra do desafio e `TaskAssignment`, pois representa que um gestor atribui uma tarefa a um subordinado. Telas, labels, mensagens de validacao e README funcional devem aparecer em portugues, com termos como "Usuario", "Gestor", "Subordinado", "Tarefa", "Data limite" e "Cadastro de usuarios". Essa decisao existe porque o desafio esta em portugues, mas o stack tecnico e os padroes de mercado sao C#/.NET, Razor, SQL Server, MVC, Repository, Service, Controller, DTO, ViewModel, Entity e Migration.

Este arquivo transforma o `PLAN.md` em uma lista operacional de acompanhamento.

## Premissas

- A arquitetura real do repositorio usa tres projetos: `Core`, `Infrastructure` e `Web`.
- `Core` contem dominio, casos de uso e contratos.
- `Infrastructure` contem EF Core, Identity, seed, arquivos, e-mail, Outbox e background services.
- `Web` contem MVC, Razor, controllers, view models, views, filtros, middleware e composition root.
- Cada task deve ter uma unica responsabilidade.
- Nao juntar domain, service, controller, view, infra, migration ou teste na mesma task.
- Marcar uma task como concluida apenas quando o criterio de pronto dela estiver validado.

## Legenda

- `[Core]`: dominio, application ou contratos.
- `[Infra]`: persistencia, Identity, seed, arquivos, e-mail, Outbox ou background services.
- `[Web]`: MVC, Razor, controllers, view models, filtros, middleware ou `Program.cs`.
- `[Tests]`: testes unitarios ou de integracao.
- `[Docs]`: documentacao e scripts.
- `[Manual]`: validacao manual.

## 0. Estado Atual do Projeto

- [ ] `[Docs]` Confirmar que `docs/PLAN.md` continua sendo a referencia funcional. Criterio de pronto: nao ha requisito novo fora do plano.
- [ ] `[Docs]` Confirmar que `docs/ARCHITECTURE.md` define a arquitetura em tres projetos. Criterio de pronto: `Core`, `Infrastructure` e `Web` aparecem como camadas oficiais.
- [ ] `[Core]` Confirmar que `src/LeveInvestimentos.Core` existe. Criterio de pronto: projeto esta listado na solution.
- [ ] `[Infra]` Confirmar que `src/LeveInvestimentos.Infrastructure` existe. Criterio de pronto: projeto esta listado na solution.
- [ ] `[Web]` Confirmar que `src/LeveInvestimentos.Web` existe. Criterio de pronto: projeto esta listado na solution.
- [ ] `[Tests]` Confirmar que `tests/LeveInvestimentos.Tests` existe. Criterio de pronto: projeto esta listado na solution.

## 1. Configuracao Base

- [ ] `[Infra]` Criar `LeveInvestimentos.sln` referenciando os quatro projetos. Criterio de pronto: `dotnet sln list` mostra `Core`, `Infrastructure`, `Web` e `Tests`.
- [ ] `[Docs]` Definir cultura pt-BR e UTF-8 como convencao da aplicacao. Criterio de pronto: datas exibidas em `dd/MM/yyyy` e arquivos/respostas em UTF-8 estao documentados como padrao.
- [ ] `[Core]` Conferir `TargetFramework` do projeto `Core`. Criterio de pronto: `net10.0`.
- [ ] `[Infra]` Conferir `TargetFramework` do projeto `Infrastructure`. Criterio de pronto: `net10.0`.
- [ ] `[Web]` Conferir `TargetFramework` do projeto `Web`. Criterio de pronto: `net10.0`.
- [ ] `[Tests]` Conferir `TargetFramework` do projeto de testes. Criterio de pronto: `net10.0`.
- [ ] `[Core]` Conferir `Nullable` no projeto `Core`. Criterio de pronto: `enable`.
- [ ] `[Infra]` Conferir `Nullable` no projeto `Infrastructure`. Criterio de pronto: `enable`.
- [ ] `[Web]` Conferir `Nullable` no projeto `Web`. Criterio de pronto: `enable`.
- [ ] `[Tests]` Conferir `Nullable` no projeto de testes. Criterio de pronto: `enable`.
- [ ] `[Core]` Decidir e aplicar `ImplicitUsings` no projeto `Core`. Criterio de pronto: configuracao documentada no `.csproj`.
- [ ] `[Infra]` Decidir e aplicar `ImplicitUsings` no projeto `Infrastructure`. Criterio de pronto: configuracao documentada no `.csproj`.
- [ ] `[Web]` Decidir e aplicar `ImplicitUsings` no projeto `Web`. Criterio de pronto: configuracao documentada no `.csproj`.
- [ ] `[Tests]` Decidir e aplicar `ImplicitUsings` no projeto de testes. Criterio de pronto: configuracao documentada no `.csproj`.
- [ ] `[Core]` Adicionar referencia do projeto `Core` onde necessario. Criterio de pronto: dependencias respeitam a direcao `Web -> Infrastructure -> Core` e `Web -> Core`.
- [ ] `[Infra]` Adicionar referencia de `Infrastructure` para `Core`. Criterio de pronto: `Infrastructure` compila usando contratos e entidades do `Core`.
- [ ] `[Web]` Adicionar referencia de `Web` para `Core`. Criterio de pronto: `Web` acessa contratos application sem acessar EF diretamente.
- [ ] `[Web]` Adicionar referencia de `Web` para `Infrastructure`. Criterio de pronto: `Program.cs` consegue registrar servicos de infraestrutura.
- [ ] `[Tests]` Adicionar referencias do projeto de testes. Criterio de pronto: testes conseguem acessar `Core`, `Infrastructure` e `Web` quando necessario.
- [ ] `[Docs]` Conferir `.gitignore`. Criterio de pronto: `bin`, `obj`, uploads locais e artefatos temporarios nao entram no Git.
- [ ] `[Docs]` Criar ou revisar `.editorconfig`. Criterio de pronto: convencoes minimas de C# estao definidas.
- [ ] `[Docs]` Decidir uso de warnings como erro. Criterio de pronto: decisao aplicada nos projetos ou documentada como pendente.
- [ ] `[Docs]` Documentar convencoes de codigo criticas (CAUTIONS secao 5). Criterio de pronto: async de ponta a ponta (sem `.Result`/`.Wait()`/`async void`), `CancellationToken` propagado, `AsNoTracking` em leitura, nunca `Html.Raw` com dado do usuario, estao registradas como convencao do time.

## 2. Core - Dominio

- [x] `[Core]` Criar enum `UserRole`. Criterio de pronto: valores `Manager` e `Subordinate` existem.
- [x] `[Core]` Criar enum `TaskStatus`. Criterio de pronto: valores `Pending`, `Started`, `Completed` e `Canceled` existem. `Overdue` nao e status persistido (e calculado por `IsOverdue`).
- [x] `[Core]` Criar value object `PhoneNumber`. Criterio de pronto: objeto representa telefone sem depender de EF ou MVC.
- [x] `[Core]` Validar formato basico no `PhoneNumber`. Criterio de pronto: valores invalidos nao criam instancia valida.
- [x] `[Core]` Criar value object `Address`. Criterio de pronto: propriedades de endereco do plano existem.
- [x] `[Core]` Validar campos obrigatorios no `Address`. Criterio de pronto: endereco incompleto nao cria instancia valida.
- [x] `[Core]` Criar entidade `User`. Criterio de pronto: entidade representa usuario da aplicacao e integra com Identity.
- [x] `[Core]` Adicionar `FullName` em `User`. Criterio de pronto: propriedade existe e aceita persistencia.
- [x] `[Core]` Adicionar `BirthDate` em `User`. Criterio de pronto: propriedade existe e aceita persistencia.
- [x] `[Core]` Adicionar `Address` em `User`. Criterio de pronto: propriedade usa o value object definido.
- [x] `[Core]` Adicionar `LandlinePhone` em `User`. Criterio de pronto: propriedade usa `PhoneNumber` ou formato decidido.
- [x] `[Core]` Adicionar `MobilePhone` em `User`. Criterio de pronto: propriedade usa `PhoneNumber` ou formato decidido.
- [x] `[Core]` Adicionar `ProfilePhotoPath` em `User`. Criterio de pronto: caminho da foto pode ser persistido.
- [x] `[Core]` Adicionar `ManagerId` em `User`. Criterio de pronto: subordinado pode apontar para gestor.
- [x] `[Core]` Adicionar flag `MustChangePassword` em `User`. Criterio de pronto: usuario criado por gestor nasce com `true`; gestor padrao do seed nasce com `false`.
- [x] `[Core]` Criar entidade `TaskAssignment`. Criterio de pronto: entidade existe sem dependencia de EF ou MVC e nenhuma classe de dominio se chama apenas `Task`.
- [x] `[Core]` Adicionar `Id` em `TaskAssignment`. Criterio de pronto: identificador existe.
- [x] `[Core]` Adicionar `Description` em `TaskAssignment`. Criterio de pronto: mensagem descritiva pode ser persistida.
- [x] `[Core]` Adicionar `DueDate` em `TaskAssignment`. Criterio de pronto: data limite pode ser persistida.
- [x] `[Core]` Adicionar `Status` em `TaskAssignment`. Criterio de pronto: usa `TaskStatus`.
- [x] `[Core]` Adicionar `ManagerId` em `TaskAssignment`. Criterio de pronto: gestor criador pode ser identificado.
- [x] `[Core]` Adicionar `SubordinateId` em `TaskAssignment`. Criterio de pronto: subordinado responsavel pode ser identificado.
- [x] `[Core]` Adicionar `CreatedAt` em `TaskAssignment`. Criterio de pronto: data de criacao e definida ao criar.
- [x] `[Core]` Adicionar `CompletedAt` em `TaskAssignment`. Criterio de pronto: data pode ficar nula antes da conclusao.
- [x] `[Core]` Encapsular setters de `TaskAssignment`. Criterio de pronto: mudancas relevantes passam por metodos de dominio.
- [x] `[Core]` Criar factory `TaskAssignment.Create`. Criterio de pronto: factory valida invariantes antes de retornar a atribuicao de tarefa.
- [x] `[Core]` Validar mensagem na factory de tarefa. Criterio de pronto: mensagem vazia e rejeitada.
- [x] `[Core]` Validar data limite na factory de tarefa. Criterio de pronto: prazo invalido e rejeitado.
- [x] `[Core]` Validar gestor na factory de tarefa. Criterio de pronto: criador obrigatorio e validado.
- [x] `[Core]` Validar subordinado na factory de tarefa. Criterio de pronto: responsavel obrigatorio e validado.
- [x] `[Core]` Criar metodo `Start` em `TaskAssignment`. Criterio de pronto: status muda de `Pending` para `Started`.
- [x] `[Core]` Bloquear `Start` em transicao invalida. Criterio de pronto: iniciar tarefa concluida ou cancelada e rejeitado.
- [x] `[Core]` Criar metodo `Complete` em `TaskAssignment`. Criterio de pronto: status muda para `Completed` a partir de `Pending` ou `Started`.
- [x] `[Core]` Definir `CompletedAt` em `Complete`. Criterio de pronto: data e preenchida ao finalizar.
- [x] `[Core]` Bloquear finalizacao de tarefa cancelada. Criterio de pronto: regra impede transicao invalida.
- [x] `[Core]` Bloquear finalizacao duplicada. Criterio de pronto: regra impede finalizar tarefa concluida.
- [x] `[Core]` Criar metodo `Cancel` em `TaskAssignment`. Criterio de pronto: status muda para `Canceled`.
- [x] `[Core]` Bloquear cancelamento de tarefa concluida. Criterio de pronto: regra impede transicao invalida.
- [x] `[Core]` Criar metodo `IsOverdue` em `TaskAssignment`. Criterio de pronto: retorna verdadeiro quando `now > DueDate` e tarefa nao esta `Completed` nem `Canceled`. E regra de leitura: nao altera o status persistido nem exige job.
- [x] `[Core]` Garantir datas em UTC com `DateTimeOffset`. Criterio de pronto: `CreatedAt`, `DueDate` e `CompletedAt` usam `DateTimeOffset`/UTC, nunca `DateTime.Now` local.
- [x] `[Core]` Criar contrato de domain event. Criterio de pronto: eventos de dominio tem tipo comum.
- [x] `[Core]` Criar armazenamento interno de domain events na entidade. Criterio de pronto: entidade consegue acumular eventos gerados.
- [x] `[Core]` Criar metodo para limpar domain events. Criterio de pronto: dispatcher consegue evitar reprocessamento.
- [x] `[Core]` Criar evento `TaskAssignmentCreatedEvent`. Criterio de pronto: evento carrega identificadores necessarios para notificar subordinado.
- [x] `[Core]` Emitir `TaskAssignmentCreatedEvent` na criacao da tarefa. Criterio de pronto: factory adiciona evento.
- [x] `[Core]` Criar evento `TaskAssignmentCompletedEvent`. Criterio de pronto: evento carrega identificadores necessarios para notificar gestor.
- [x] `[Core]` Emitir `TaskAssignmentCompletedEvent` ao finalizar tarefa. Criterio de pronto: metodo `Complete` adiciona evento.

## 3. Core - Contratos e Application

- [x] `[Core]` Criar interface `IUserRepository`. Criterio de pronto: contrato cobre consultas de usuario exigidas pelos services.
- [x] `[Core]` Criar interface `ITaskAssignmentRepository`. Criterio de pronto: contrato cobre consultas e persistencia de tarefas exigidas pelos services.
- [x] `[Core]` Criar interface `IUnitOfWork`. Criterio de pronto: contrato expoe commit assincrono.
- [x] `[Core]` Criar interface `IDomainEventDispatcher`. Criterio de pronto: contrato permite publicar eventos de dominio.
- [x] `[Core]` Criar tipo `Error`. Criterio de pronto: erro possui codigo e mensagem.
- [x] `[Core]` Criar tipo `Result`. Criterio de pronto: representa sucesso ou falha sem exception de fluxo.
- [x] `[Core]` Criar tipo `Result<T>`. Criterio de pronto: representa sucesso com valor ou falha.
- [x] `[Core]` Criar command `CreateUserCommand`. Criterio de pronto: command contem dados do cadastro e NAO contem senha (a senha e gerada pelo sistema).
- [x] `[Core]` Criar validator de `CreateUserCommand`. Criterio de pronto: campos obrigatorios sao validados, e-mail tem formato valido e `BirthDate` nao e futura.
- [x] `[Core]` Criar abstracao `IPasswordGenerator`. Criterio de pronto: contrato gera senha aleatoria forte para o novo usuario sem expor logica no controller.
- [x] `[Core]` Criar DTO de usuario para listagem. Criterio de pronto: DTO nao expoe entidade diretamente.
- [x] `[Core]` Criar DTO de usuario para detalhes. Criterio de pronto: DTO contem dados necessarios para views.
- [x] `[Core]` Criar interface `IUserService`. Criterio de pronto: contrato expoe criacao e consultas usadas pela Web.
- [x] `[Core]` Criar `UserService`. Criterio de pronto: service implementa `IUserService`.
- [x] `[Core]` Implementar criacao de usuario no `UserService`. Criterio de pronto: fluxo usa Result, gera senha aleatoria via `IPasswordGenerator`, marca `MustChangePassword = true` e nao retorna entidade para Web.
- [x] `[Core]` Garantir unicidade de e-mail na criacao de usuario. Criterio de pronto: e-mail duplicado retorna falha via Result e a race condition e tratada (constraint unica + captura de `DbUpdateException`).
- [x] `[Core]` Implementar consulta de subordinados no `UserService`. Criterio de pronto: gestor consegue obter lista para atribuicao.
- [x] `[Core]` Implementar consulta de usuarios para gestor no `UserService`. Criterio de pronto: lista dados de acompanhamento.
- [x] `[Core]` Criar command `CreateTaskAssignmentCommand`. Criterio de pronto: command contem descricao, data limite e subordinado.
- [x] `[Core]` Criar validator de `CreateTaskAssignmentCommand`. Criterio de pronto: descricao, data limite e subordinado sao validados.
- [x] `[Core]` Criar command `StartTaskAssignmentCommand`. Criterio de pronto: command contem identificador da tarefa e usuario atual.
- [x] `[Core]` Criar validator de `StartTaskAssignmentCommand`. Criterio de pronto: identificadores obrigatorios sao validados.
- [x] `[Core]` Criar command `CompleteTaskAssignmentCommand`. Criterio de pronto: command contem identificador da tarefa e usuario atual.
- [x] `[Core]` Criar validator de `CompleteTaskAssignmentCommand`. Criterio de pronto: identificadores obrigatorios sao validados.
- [x] `[Core]` Criar command `CancelTaskAssignmentCommand`. Criterio de pronto: command contem identificador da tarefa e usuario atual.
- [x] `[Core]` Criar validator de `CancelTaskAssignmentCommand`. Criterio de pronto: identificadores obrigatorios sao validados.
- [x] `[Core]` Criar DTO de tarefa para listagem. Criterio de pronto: DTO contem status, prazo e participantes.
- [x] `[Core]` Criar DTO de tarefa para detalhes. Criterio de pronto: DTO contem dados necessarios para view de tarefa.
- [x] `[Core]` Criar interface `ITaskAssignmentService`. Criterio de pronto: contrato expoe criar, iniciar, finalizar, cancelar e listar.
- [x] `[Core]` Criar `TaskAssignmentService`. Criterio de pronto: service implementa `ITaskAssignmentService`.
- [x] `[Core]` Implementar criacao de tarefa no `TaskAssignmentService`. Criterio de pronto: cria aggregate e salva via repository/unit of work.
- [x] `[Core]` Validar vinculo gestor-subordinado na criacao de tarefa (anti-IDOR). Criterio de pronto: criar tarefa para `SubordinateId` que nao e subordinado do gestor autenticado retorna falha.
- [x] `[Core]` Implementar inicio de tarefa no `TaskAssignmentService`. Criterio de pronto: apenas subordinado responsavel consegue iniciar (Pending para Started).
- [x] `[Core]` Implementar finalizacao de tarefa no `TaskAssignmentService`. Criterio de pronto: apenas subordinado responsavel consegue finalizar.
- [x] `[Core]` Implementar cancelamento de tarefa no `TaskAssignmentService`. Criterio de pronto: regra de permissao e transicao e aplicada.
- [x] `[Core]` Implementar listagem de tarefas para gestor. Criterio de pronto: gestor ve tarefas que criou ou gerencia.
- [x] `[Core]` Implementar listagem de tarefas para subordinado. Criterio de pronto: subordinado ve tarefas atribuidas a ele.
- [x] `[Core]` Criar abstracao `IEmailSender`. Criterio de pronto: contrato permite envio assíncrono de e-mail.
- [x] `[Core]` Criar handler de e-mail para tarefa atribuida. Criterio de pronto: handler traduz evento em mensagem para Outbox.
- [x] `[Core]` Criar handler de e-mail para tarefa finalizada. Criterio de pronto: handler traduz evento em mensagem para Outbox.
- [x] `[Core]` Criar mensagem de e-mail de credenciais do novo usuario. Criterio de pronto: criacao de usuario enfileira no Outbox um e-mail com o e-mail de acesso e a senha gerada, instruindo troca no primeiro login.

## 4. Infrastructure - Persistencia

- [x] `[Infra]` Adicionar pacotes EF Core necessarios. Criterio de pronto: projeto restaura pacotes de SQL Server e migrations.
- [x] `[Infra]` Adicionar pacotes Identity necessarios. Criterio de pronto: projeto suporta `IdentityDbContext`.
- [x] `[Infra]` Criar `ApplicationDbContext`. Criterio de pronto: contexto compila.
- [x] `[Infra]` Herdar `ApplicationDbContext` de `IdentityDbContext`. Criterio de pronto: Identity usa `User` e chave `Guid`.
- [x] `[Infra]` Adicionar `DbSet<TaskAssignment>`. Criterio de pronto: tarefa entra no modelo EF.
- [x] `[Infra]` Adicionar `DbSet<OutboxMessage>`. Criterio de pronto: Outbox entra no modelo EF.
- [x] `[Infra]` Criar configuration de `User`. Criterio de pronto: propriedades extras sao configuradas.
- [x] `[Infra]` Configurar `Address` owned em `User`. Criterio de pronto: colunas de endereco aparecem no modelo.
- [x] `[Infra]` Configurar telefones de `User`. Criterio de pronto: telefone fixo e celular persistem corretamente.
- [x] `[Infra]` Configurar relacionamento gestor-subordinado. Criterio de pronto: `ManagerId` e navegacoes funcionam.
- [x] `[Infra]` Criar configuration de `TaskAssignment`. Criterio de pronto: campos, tamanhos e relacoes sao configurados.
- [x] `[Infra]` Configurar relacao tarefa-gestor. Criterio de pronto: `ManagerId` referencia usuario gestor.
- [x] `[Infra]` Configurar relacao tarefa-subordinado. Criterio de pronto: `SubordinateId` referencia usuario subordinado.
- [x] `[Infra]` Criar `UserRepository`. Criterio de pronto: implementa `IUserRepository`.
- [x] `[Infra]` Implementar consulta por id no `UserRepository`. Criterio de pronto: retorna usuario esperado.
- [x] `[Infra]` Implementar consulta de subordinados no `UserRepository`. Criterio de pronto: filtra por gestor.
- [x] `[Infra]` Implementar consulta de usuarios no `UserRepository`. Criterio de pronto: retorna dados para gestor.
- [x] `[Infra]` Criar `TaskAssignmentRepository`. Criterio de pronto: implementa `ITaskAssignmentRepository`.
- [x] `[Infra]` Implementar add de tarefa no `TaskAssignmentRepository`. Criterio de pronto: tarefa nova entra no tracking.
- [x] `[Infra]` Implementar consulta por id no `TaskAssignmentRepository`. Criterio de pronto: retorna tarefa esperada.
- [x] `[Infra]` Implementar listagem de tarefas por gestor no `TaskAssignmentRepository`. Criterio de pronto: consulta filtra corretamente.
- [x] `[Infra]` Implementar listagem de tarefas por subordinado no `TaskAssignmentRepository`. Criterio de pronto: consulta filtra corretamente.
- [x] `[Infra]` Implementar filtro opcional por status no `TaskAssignmentRepository`. Criterio de pronto: status filtra quando informado.
- [x] `[Infra]` Criar `UnitOfWork`. Criterio de pronto: implementa `IUnitOfWork`.
- [x] `[Infra]` Implementar commit assincrono no `UnitOfWork`. Criterio de pronto: chama `SaveChangesAsync`.
- [x] `[Infra]` Integrar domain events ao save. Criterio de pronto: eventos gerados sao coletados antes do commit final.
- [x] `[Infra]` Criar migration inicial. Criterio de pronto: migration compila.
- [x] `[Docs]` Gerar script `docs/database/01_schema.sql`. Criterio de pronto: script reflete migration inicial.

## 5. Infrastructure - Identity e Seed

- [x] `[Infra]` Configurar Identity para usar `User`. Criterio de pronto: Identity usa usuario customizado.
- [x] `[Infra]` Configurar roles de Identity. Criterio de pronto: roles `Gestor` e `Subordinado` sao suportadas.
- [x] `[Infra]` Configurar password policy. Criterio de pronto: senha `teste123` funciona conforme requisito.
- [x] `[Infra]` Configurar cookie auth. Criterio de pronto: login MVC autentica via cookie.
- [x] `[Infra]` Configurar cookies seguros. Criterio de pronto: cookie de auth usa `HttpOnly`, `SecurePolicy` e `SameSite` adequados.
- [x] `[Infra]` Criar `DatabaseSeeder`. Criterio de pronto: classe existe e compila.
- [x] `[Infra]` Implementar seed da role `Gestor`. Criterio de pronto: role e criada se nao existir.
- [x] `[Infra]` Implementar seed da role `Subordinado`. Criterio de pronto: role e criada se nao existir.
- [x] `[Infra]` Implementar seed do usuario gestor padrao. Criterio de pronto: `ti@leveinvestimentos.com.br` e criado se nao existir, com `MustChangePassword = false`.
- [x] `[Infra]` Atribuir role `Gestor` ao usuario padrao. Criterio de pronto: usuario padrao autentica como gestor.
- [x] `[Infra]` Garantir idempotencia do seeder. Criterio de pronto: executar duas vezes nao duplica dados.
- [x] `[Web]` Chamar seeder no startup. Criterio de pronto: aplicacao semeia dados ao iniciar.
- [x] `[Docs]` Gerar ou documentar `docs/database/02_seed.sql`. Criterio de pronto: script ou orientacao cobre usuario e roles.

## 6. Infrastructure - Arquivos

Objetivo da camada: receber o arquivo bruto vindo da Web, validar, salvar no storage configurado e devolver apenas os dados persistiveis do arquivo. O dominio nao faz upload nem conhece `IFormFile`; ele recebe, no maximo, o path publico que sera gravado em `User.ProfilePhotoPath`.

- [x] `[Core]` Criar abstracao central `IFileStorage`. Criterio de pronto: contrato recebe `Stream`, nome original e content type, sem depender de MVC.
- [x] `[Core]` Criar retorno `StoredFile`. Criterio de pronto: retorno contem `PublicUrl`, `StorageKey`, `ContentType` e `SizeBytes`.
- [x] `[Infra]` Criar implementacao central `LocalFileStorage`. Criterio de pronto: implementa `IFileStorage` e orquestra validacao, escrita fisica e retorno do path publico.
- [x] `[Infra]` Definir pasta de upload local. Criterio de pronto: destino e `wwwroot/uploads` ou caminho configurado.
- [x] `[Infra]` Validar extensao de foto dentro do fluxo de storage. Criterio de pronto: extensoes nao permitidas sao rejeitadas antes da escrita.
- [x] `[Infra]` Validar magic bytes da foto dentro do fluxo de storage. Criterio de pronto: arquivo cujo conteudo real nao bate com imagem permitida e rejeitado, mesmo com extensao valida.
- [x] `[Infra]` Validar tamanho de foto dentro do fluxo de storage. Criterio de pronto: arquivo acima do limite e rejeitado sem ser salvo.
- [x] `[Infra]` Gerar nome seguro para foto. Criterio de pronto: nome salvo nao usa nome bruto do usuario.
- [x] `[Infra]` Retornar caminho publico da foto salva. Criterio de pronto: Web/Application conseguem persistir `StoredFile.PublicUrl` em `ProfilePhotoPath`.

## 7. Infrastructure - Outbox

- [x] `[Infra]` Criar entidade `OutboxMessage`. Criterio de pronto: possui `Id`, `Type`, `PayloadJson`, `OccurredAt`, `ProcessedAt`, `Attempts` e `LastError`.
- [x] `[Infra]` Criar configuration de `OutboxMessage`. Criterio de pronto: campos e indices essenciais sao configurados.
- [x] `[Infra]` Criar repositorio ou acesso de escrita para Outbox. Criterio de pronto: handlers conseguem enfileirar mensagens.
- [x] `[Infra]` Serializar payload de evento para Outbox. Criterio de pronto: mensagem contem dados necessarios para envio.
- [x] `[Infra]` Gravar Outbox na mesma transacao da tarefa. Criterio de pronto: tarefa e mensagem sao persistidas juntas.
- [x] `[Infra]` Criar `OutboxDispatcherService`. Criterio de pronto: background service compila.
- [x] `[Infra]` Resolver dependencias por scope no dispatcher. Criterio de pronto: cada ciclo cria um scope via `IServiceScopeFactory` para obter `DbContext`/`IEmailSender` (evita captive dependency em singleton).
- [x] `[Infra]` Implementar busca de mensagens pendentes no dispatcher. Criterio de pronto: apenas mensagens sem `ProcessadoEm` sao processadas.
- [x] `[Infra]` Implementar marcacao de mensagem processada. Criterio de pronto: `ProcessedAt` e preenchido apos sucesso.
- [x] `[Infra]` Implementar incremento de tentativas. Criterio de pronto: falha aumenta contador.
- [x] `[Infra]` Implementar registro de ultimo erro. Criterio de pronto: falha salva mensagem de erro.
- [x] `[Infra]` Implementar intervalo entre ciclos. Criterio de pronto: dispatcher nao roda em loop quente.
- [x] `[Infra]` Implementar backoff simples. Criterio de pronto: mensagens com falha nao sao reprocessadas imediatamente sem criterio.

## 8. Infrastructure - E-mail

- [x] `[Infra]` Adicionar pacote MailKit. Criterio de pronto: projeto restaura pacote.
- [x] `[Infra]` Criar `EmailOptions`. Criterio de pronto: host, porta, usuario, senha, remetente e SSL estao representados.
- [x] `[Infra]` Configurar options de e-mail no DI. Criterio de pronto: `EmailOptions` recebe configuracao do appsettings.
- [x] `[Infra]` Criar `SmtpEmailSender`. Criterio de pronto: implementa `IEmailSender`.
- [x] `[Infra]` Implementar envio SMTP. Criterio de pronto: sender envia destinatario, assunto e corpo.
- [x] `[Infra]` Implementar template de tarefa atribuida. Criterio de pronto: e-mail informa mensagem e prazo.
- [x] `[Infra]` Implementar template de tarefa finalizada. Criterio de pronto: e-mail informa tarefa finalizada e subordinado.
- [x] `[Infra]` Conectar dispatcher ao `IEmailSender`. Criterio de pronto: Outbox pendente dispara envio.
- [x] `[Web]` Adicionar configuracao de e-mail em `appsettings.json`. Criterio de pronto: chaves existem sem segredos reais.
- [x] `[Docs]` Documentar uso de User Secrets para SMTP. Criterio de pronto: README explica credenciais locais.

## 9. Web - Composition Root

- [x] `[Web]` Revisar `Program.cs`. Criterio de pronto: arquivo e composition root da aplicacao.
- [x] `[Web]` Registrar MVC no `Program.cs`. Criterio de pronto: controllers com views funcionam.
- [x] `[Web]` Registrar `ApplicationDbContext` no `Program.cs`. Criterio de pronto: connection string e usada.
- [x] `[Web]` Registrar Identity no `Program.cs`. Criterio de pronto: login e roles funcionam.
- [x] `[Web]` Registrar servicos de Application no DI. Criterio de pronto: controllers resolvem services.
- [x] `[Web]` Registrar servicos de Infrastructure no DI. Criterio de pronto: repositories, storage, e-mail e outbox resolvem.
- [x] `[Web]` Registrar `OutboxDispatcherService`. Criterio de pronto: hosted service inicia com a aplicacao.
- [x] `[Web]` Configurar pipeline de autenticacao. Criterio de pronto: `UseAuthentication` esta antes de `UseAuthorization`.
- [x] `[Web]` Configurar pipeline de autorizacao. Criterio de pronto: roles sao avaliadas nas actions.
- [x] `[Web]` Configurar arquivos estaticos. Criterio de pronto: `wwwroot` serve assets e uploads.
- [x] `[Web]` Configurar HTTPS e HSTS. Criterio de pronto: `UseHttpsRedirection` e `UseHsts` (fora de dev) estao no pipeline.
- [x] `[Web]` Configurar anti-forgery global. Criterio de pronto: POSTs exigem token (`AutoValidateAntiforgeryToken` ou equivalente).
- [x] `[Web]` Configurar logging estruturado com Serilog. Criterio de pronto: logs sao estruturados e nao registram dados sensiveis (senha, foto, dados pessoais).
- [x] `[Web]` Criar middleware de excecao. Criterio de pronto: falhas inesperadas sao tratadas de forma padronizada.
- [x] `[Web]` Registrar middleware de excecao. Criterio de pronto: middleware esta no pipeline.

## 10. Web - Autenticacao

- [x] `[Web]` Criar `LoginViewModel`. Criterio de pronto: contem e-mail, senha e lembrar login se necessario.
- [x] `[Web]` Criar `AccountController`. Criterio de pronto: controller compila.
- [x] `[Web]` Criar action GET `Login`. Criterio de pronto: retorna view de login.
- [x] `[Web]` Criar action POST `Login`. Criterio de pronto: autentica via Identity.
- [x] `[Web]` Tratar login invalido. Criterio de pronto: usuario ve erro sem exception.
- [x] `[Web]` Criar action `Logout`. Criterio de pronto: encerra cookie de autenticacao.
- [x] `[Web]` Criar action `AccessDenied`. Criterio de pronto: retorna view de acesso negado.
- [x] `[Web]` Criar view `Account/Login`. Criterio de pronto: formulario posta e-mail e senha.
- [x] `[Web]` Criar view `Account/AccessDenied`. Criterio de pronto: usuario entende que nao tem permissao.
- [x] `[Web]` Criar `ChangePasswordViewModel`. Criterio de pronto: contem senha atual, nova senha e confirmacao.
- [x] `[Web]` Criar actions GET/POST `ChangePassword`. Criterio de pronto: usuario troca a senha via Identity e `MustChangePassword` vira `false` apos sucesso.
- [x] `[Web]` Forcar troca de senha no primeiro login. Criterio de pronto: middleware ou filtro redireciona usuario com `MustChangePassword = true` para `ChangePassword` antes de acessar outras telas.
- [x] `[Web]` Criar view `Account/ChangePassword`. Criterio de pronto: formulario posta nova senha com labels em portugues.
- [x] `[Web]` Adicionar links de login/logout no layout. Criterio de pronto: navegacao reflete usuario autenticado.

## 11. Web - Usuarios

- [ ] `[Web]` Criar `UserListItemViewModel`. Criterio de pronto: contem dados para lista.
- [ ] `[Web]` Criar `CreateUserViewModel`. Criterio de pronto: contem campos do cadastro e NAO contem campo de senha (senha e gerada pelo sistema).
- [ ] `[Web]` Criar mapeamento de view model para command de usuario. Criterio de pronto: controller nao monta entidade.
- [ ] `[Web]` Criar `UsersController`. Criterio de pronto: controller compila.
- [ ] `[Web]` Adicionar `[Authorize(Roles = "Manager")]` no `UsersController`. Criterio de pronto: subordinado nao acessa cadastro.
- [ ] `[Web]` Criar action GET `Index` em usuarios. Criterio de pronto: gestor ve lista de usuarios.
- [ ] `[Web]` Criar action GET `Criar` em usuarios. Criterio de pronto: retorna formulario.
- [ ] `[Web]` Criar action POST `Criar` em usuarios. Criterio de pronto: chama `IUserService`.
- [ ] `[Web]` Tratar erro de validacao no cadastro de usuario. Criterio de pronto: view reexibe mensagens.
- [ ] `[Web]` Integrar upload de foto no POST de usuario. Criterio de pronto: foto e salva via `IFileStorage`.
- [ ] `[Web]` Persistir `ProfilePhotoPath` no cadastro. Criterio de pronto: caminho salvo aparece no usuario.
- [ ] `[Web]` Criar view `Users/Index`. Criterio de pronto: lista usuarios com dados principais e labels em portugues.
- [ ] `[Web]` Criar view `Users/Create`. Criterio de pronto: formulario contem todos os campos exigidos e labels em portugues.
- [ ] `[Web]` Adicionar UIkit ao formulario de usuario. Criterio de pronto: formulario usa classes UIkit basicas.
- [ ] `[Web]` Garantir anti-forgery token no formulario de usuario. Criterio de pronto: POST de cadastro inclui token CSRF.
- [ ] `[Web]` Exibir feedback de sucesso no cadastro. Criterio de pronto: gestor sabe que usuario foi criado e que a senha foi enviada por e-mail para troca no primeiro login.

## 12. Web - Tarefas

- [ ] `[Web]` Criar `TaskAssignmentListItemViewModel`. Criterio de pronto: contem dados para lista.
- [ ] `[Web]` Criar `CreateTaskAssignmentViewModel`. Criterio de pronto: contem descricao, data limite e subordinado.
- [ ] `[Web]` Criar view model de filtro de tarefa. Criterio de pronto: status pode ser informado na listagem.
- [ ] `[Web]` Criar mapeamento de view model para command de criar tarefa. Criterio de pronto: controller nao monta entidade.
- [ ] `[Web]` Criar `TaskAssignmentsController`. Criterio de pronto: controller compila.
- [ ] `[Web]` Adicionar `[Authorize]` no `TaskAssignmentsController`. Criterio de pronto: somente autenticados acessam tarefas.
- [ ] `[Web]` Criar action GET `Index`. Criterio de pronto: lista muda conforme papel do usuario.
- [ ] `[Web]` Criar filtro por status no `Index`. Criterio de pronto: query string de status altera lista.
- [ ] `[Web]` Criar action GET `Criar`. Criterio de pronto: apenas gestor acessa formulario.
- [ ] `[Web]` Popular subordinados na action GET `Criar`. Criterio de pronto: select contem subordinados disponiveis.
- [ ] `[Web]` Criar action POST `Criar`. Criterio de pronto: chama `ITaskAssignmentService`.
- [ ] `[Web]` Tratar erro de validacao na criacao de tarefa. Criterio de pronto: formulario reexibe mensagens.
- [ ] `[Web]` Criar action POST `Iniciar`. Criterio de pronto: chama `ITaskAssignmentService.Start` (Pending para Started) usando o usuario autenticado.
- [ ] `[Web]` Criar action POST `Finalizar`. Criterio de pronto: chama `ITaskAssignmentService.Complete`.
- [ ] `[Web]` Garantir que finalizar usa usuario autenticado. Criterio de pronto: command recebe id do usuario atual.
- [ ] `[Web]` Criar action POST `Cancelar`. Criterio de pronto: chama `ITaskAssignmentService.Cancel`.
- [ ] `[Web]` Garantir que cancelar usa usuario autenticado. Criterio de pronto: command recebe id do usuario atual.
- [ ] `[Web]` Criar view `TaskAssignments/Index`. Criterio de pronto: tarefas aparecem com status e prazo, com labels em portugues.
- [ ] `[Web]` Criar view `TaskAssignments/Create`. Criterio de pronto: formulario permite escolher subordinado e prazo, com labels em portugues.
- [ ] `[Web]` Adicionar botao de iniciar tarefa. Criterio de pronto: aparece apenas quando a tarefa esta `Pending` e o usuario pode inicia-la.
- [ ] `[Web]` Adicionar botao de finalizar tarefa. Criterio de pronto: aparece apenas quando usuario pode finalizar.
- [ ] `[Web]` Adicionar botao de cancelar tarefa. Criterio de pronto: aparece apenas quando usuario pode cancelar.
- [ ] `[Web]` Exibir badge "Atrasada" na listagem de tarefas. Criterio de pronto: badge aparece quando `IsOverdue` e verdadeiro, sem alterar o status persistido.
- [ ] `[Web]` Garantir anti-forgery token nos formularios de tarefa. Criterio de pronto: POSTs de criar, iniciar, finalizar e cancelar incluem token CSRF.
- [ ] `[Web]` Adicionar UIkit nas views de tarefa. Criterio de pronto: views usam classes UIkit basicas.
- [ ] `[Web]` Exibir feedback de sucesso em tarefa criada. Criterio de pronto: gestor sabe que tarefa foi criada.
- [ ] `[Web]` Exibir feedback de sucesso em tarefa finalizada. Criterio de pronto: subordinado sabe que tarefa foi finalizada.

## 13. Testes

- [ ] `[Tests]` Adicionar pacotes xUnit. Criterio de pronto: projeto executa testes xUnit.
- [ ] `[Tests]` Adicionar FluentAssertions. Criterio de pronto: assertions fluent compilaram.
- [ ] `[Tests]` Adicionar NSubstitute. Criterio de pronto: mocks/substitutes podem ser criados.
- [ ] `[Tests]` Adicionar pacote de integration testing. Criterio de pronto: `WebApplicationFactory` pode ser usado.
- [ ] `[Tests]` Criar teste para `PhoneNumber` valido. Criterio de pronto: telefone aceito cria instancia.
- [ ] `[Tests]` Criar teste para `PhoneNumber` invalido. Criterio de pronto: telefone invalido falha.
- [ ] `[Tests]` Criar teste para `Address` valido. Criterio de pronto: endereco aceito cria instancia.
- [ ] `[Tests]` Criar teste para `Address` incompleto. Criterio de pronto: endereco incompleto falha.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Create` valido. Criterio de pronto: tarefa nasce pendente e com evento atribuido.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Create` com descricao vazia. Criterio de pronto: criacao falha.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Create` com data limite invalida. Criterio de pronto: criacao falha.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Start` em tarefa pendente. Criterio de pronto: status vira `Started`.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Start` em tarefa concluida ou cancelada. Criterio de pronto: regra bloqueia transicao invalida.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Complete` em tarefa pendente. Criterio de pronto: status vira concluida.
- [ ] `[Tests]` Criar teste para `TaskAssignmentCompletedEvent`. Criterio de pronto: evento e gerado ao finalizar.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Complete` em tarefa cancelada. Criterio de pronto: regra bloqueia.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Cancel` em tarefa pendente. Criterio de pronto: status vira cancelada.
- [ ] `[Tests]` Criar teste para `TaskAssignment.Cancel` em tarefa concluida. Criterio de pronto: regra bloqueia.
- [ ] `[Tests]` Criar teste para `TaskAssignment.IsOverdue`. Criterio de pronto: prazo vencido retorna verdadeiro quando aplicavel.
- [ ] `[Tests]` Criar teste de `UserService` criando usuario valido. Criterio de pronto: service retorna sucesso.
- [ ] `[Tests]` Criar teste de `UserService` com dados invalidos. Criterio de pronto: service retorna falha.
- [ ] `[Tests]` Criar teste de `UserService` com e-mail duplicado. Criterio de pronto: service retorna falha por unicidade.
- [ ] `[Tests]` Criar teste de validacao de `BirthDate` futura. Criterio de pronto: data de nascimento no futuro e rejeitada.
- [ ] `[Tests]` Criar teste de hashing de senha. Criterio de pronto: senha do usuario semeado nao e armazenada em texto plano.
- [ ] `[Tests]` Criar teste de `TaskAssignmentService` criando tarefa valida. Criterio de pronto: repository e unit of work sao chamados.
- [ ] `[Tests]` Criar teste de `TaskAssignmentService` criando tarefa para subordinado de outro gestor (anti-IDOR). Criterio de pronto: retorna falha.
- [ ] `[Tests]` Criar teste de `TaskAssignmentService` finalizando tarefa do subordinado correto. Criterio de pronto: retorna sucesso.
- [ ] `[Tests]` Criar teste de `TaskAssignmentService` finalizando tarefa de outro subordinado. Criterio de pronto: retorna falha.
- [ ] `[Tests]` Criar teste de `TaskAssignmentService` cancelando tarefa com permissao. Criterio de pronto: retorna sucesso.
- [ ] `[Tests]` Criar teste de login com gestor padrao. Criterio de pronto: autenticacao retorna sucesso.
- [ ] `[Tests]` Criar teste de cadastro de usuario por gestor. Criterio de pronto: POST autorizado cria usuario.
- [ ] `[Tests]` Criar teste de bloqueio de cadastro por subordinado. Criterio de pronto: subordinado recebe 403.
- [ ] `[Tests]` Criar teste de criacao de tarefa por gestor. Criterio de pronto: tarefa e persistida.
- [ ] `[Tests]` Criar teste de finalizacao de tarefa por subordinado. Criterio de pronto: status muda para concluida.
- [ ] `[Tests]` Criar teste de Outbox ao criar tarefa. Criterio de pronto: mensagem de tarefa atribuida e gerada.
- [ ] `[Tests]` Criar teste de Outbox ao finalizar tarefa. Criterio de pronto: mensagem de tarefa finalizada e gerada.
- [ ] `[Tests]` Executar `dotnet test`. Criterio de pronto: todos os testes passam.

## 14. Documentacao e Entrega

- [ ] `[Docs]` Criar ou atualizar `README.md` com pre-requisitos. Criterio de pronto: inclui .NET 10 SDK e SQL Server.
- [ ] `[Docs]` Documentar restauracao de pacotes. Criterio de pronto: README contem comando de restore.
- [ ] `[Docs]` Documentar aplicacao de migrations. Criterio de pronto: README contem comando de database update.
- [ ] `[Docs]` Documentar execucao da aplicacao. Criterio de pronto: README contem comando de run.
- [ ] `[Docs]` Documentar credenciais do gestor padrao. Criterio de pronto: README contem `ti@leveinvestimentos.com.br / teste123`.
- [ ] `[Docs]` Documentar configuracao de connection string. Criterio de pronto: README explica onde configurar.
- [ ] `[Docs]` Documentar configuracao SMTP local. Criterio de pronto: README cita Papercut, MailHog ou equivalente.
- [ ] `[Docs]` Documentar uso de User Secrets. Criterio de pronto: README nao exige segredo em arquivo versionado.
- [ ] `[Docs]` Atualizar `ARCHITECTURE.md` com decisoes finais. Criterio de pronto: documento reflete implementacao real.
- [ ] `[Docs]` Documentar fluxo de camadas. Criterio de pronto: controller, service, repository e infra aparecem no fluxo.
- [ ] `[Docs]` Documentar patterns usados. Criterio de pronto: MVC, Repository, Unit of Work, Result, Options, Outbox e Domain Events aparecem.
- [ ] `[Docs]` Conferir `docs/database/01_schema.sql`. Criterio de pronto: script existe e esta atualizado.
- [ ] `[Docs]` Conferir `docs/database/02_seed.sql`. Criterio de pronto: script ou orientacao existe e esta atualizada.
- [ ] `[Docs]` Documentar smoke test manual. Criterio de pronto: passos do login ao e-mail estao no README ou docs.
- [ ] `[Docs]` Documentar premissas assumidas no README. Criterio de pronto: cadastro exclusivo de gestor, senha aleatoria + troca no 1o login, modelo 1 gestor:N subordinados, e-mail desejavel assincrono e soft delete/auditoria FORA DE ESCOPO estao listados.
- [ ] `[Docs]` Documentar nota de LGPD no README. Criterio de pronto: README cita tratamento de dados pessoais, cookies seguros e ausencia de log de dado sensivel.
- [ ] `[Infra]` Criar `Dockerfile` da aplicacao Web. Criterio de pronto: imagem builda a aplicacao .NET 10.
- [ ] `[Infra]` Criar `docker-compose.yml` com app e SQL Server. Criterio de pronto: `docker compose up` sobe banco e aplicacao conectados.
- [ ] `[Docs]` Documentar uso do Docker Compose no README. Criterio de pronto: README explica `docker compose up` e variaveis de ambiente necessarias.
- [ ] `[Docs]` Confirmar repositorio publico no GitHub. Criterio de pronto: link final esta disponivel.

## 15. Smoke Tests Manuais

- [ ] `[Manual]` Executar `dotnet build`. Criterio de pronto: build passa sem erro.
- [ ] `[Manual]` Executar `dotnet test`. Criterio de pronto: suite passa.
- [ ] `[Manual]` Executar migrations no banco local. Criterio de pronto: schema e criado.
- [ ] `[Manual]` Subir a aplicacao via `docker compose up`. Criterio de pronto: app e SQL Server sobem e o site abre no navegador.
- [ ] `[Manual]` Subir a aplicacao. Criterio de pronto: site abre no navegador.
- [ ] `[Manual]` Fazer login com gestor padrao. Criterio de pronto: `ti@leveinvestimentos.com.br / teste123` autentica.
- [ ] `[Manual]` Cadastrar subordinado sem foto. Criterio de pronto: usuario e persistido.
- [ ] `[Manual]` Cadastrar subordinado com foto. Criterio de pronto: usuario e persistido e foto aparece em `wwwroot/uploads`.
- [ ] `[Manual]` Verificar e-mail de credenciais do novo subordinado no SMTP local. Criterio de pronto: e-mail com acesso e senha gerada e recebido.
- [ ] `[Manual]` Fazer login como subordinado. Criterio de pronto: login funciona.
- [ ] `[Manual]` Trocar senha obrigatoria no primeiro login. Criterio de pronto: subordinado e redirecionado para trocar a senha antes de acessar outras telas.
- [ ] `[Manual]` Tentar acessar cadastro de usuarios como subordinado. Criterio de pronto: acesso retorna 403 ou access denied.
- [ ] `[Manual]` Criar tarefa como gestor. Criterio de pronto: tarefa aparece na lista do gestor.
- [ ] `[Manual]` Tentar criar tarefa para subordinado de outro gestor (anti-IDOR). Criterio de pronto: requisicao e negada pelo backend.
- [ ] `[Manual]` Ver tarefa como subordinado. Criterio de pronto: tarefa aparece na lista do subordinado responsavel.
- [ ] `[Manual]` Confirmar Outbox de tarefa atribuida. Criterio de pronto: mensagem e criada ao atribuir tarefa.
- [ ] `[Manual]` Iniciar tarefa como subordinado. Criterio de pronto: status muda para `Started`.
- [ ] `[Manual]` Finalizar tarefa como subordinado. Criterio de pronto: status muda para concluida.
- [ ] `[Manual]` Confirmar Outbox de tarefa finalizada. Criterio de pronto: mensagem e criada ao finalizar tarefa.
- [ ] `[Manual]` Verificar e-mail de atribuicao no SMTP local. Criterio de pronto: subordinado recebe mensagem.
- [ ] `[Manual]` Verificar e-mail de finalizacao no SMTP local. Criterio de pronto: gestor recebe mensagem.
- [ ] `[Manual]` Executar README em maquina limpa ou ambiente limpo. Criterio de pronto: clone ate login funciona em ate 10 minutos.
