## 1. Segurança — onde uma financeira mais reprova

### 🔴 Senha em texto plano (a armadilha #1)

O desafio entrega `teste123` de propósito. O candidato júnior salva como veio. Você **nunca** armazena senha em texto plano, nem MD5/SHA1.

- Use **ASP.NET Core Identity** (PBKDF2 por padrão) ou **BCrypt/Argon2**.
- O **seed** do usuário inicial tem que **gerar o hash**, não gravar a string.
- Esse único ponto separa sênior de júnior aqui.

### 🔴 Broken Access Control (OWASP #1)

"Somente gestores cadastram usuários." A pegadinha é esconder o botão na tela mas deixar o endpoint `POST /users` aberto.

- **Autorização tem que ser no servidor**, com `[Authorize(Policy/Roles)]`, não só na UI.
- Um gestor só pode criar tarefa para **seus** subordinados — valide no backend que o `subordinadoId` realmente pertence àquele gestor (evita **IDOR**).

### 🔴 Mass assignment / over-posting

Em Razor, fazer model binding direto na entidade (`User`) deixa um atacante mandar `IsGestor=true` ou `ManagerId=X` no form e escalar privilégio.

- **Use ViewModels/InputModels dedicados**, nunca bind direto na entidade.

### 🔴 Upload de foto

Pegadinhas clássicas: aceitar qualquer extensão/content-type, confiar no nome do arquivo (path traversal `../../`), sem limite de tamanho, salvar executável.

- Valide extensão **e** magic bytes.
- Gere nome novo (GUID).
- Limite o tamanho.
- **Abstraia o storage atrás de uma interface** (`IFileStorage`) — extensibilidade que o sênior demonstra.

### 🟠 LGPD

Empresa financeira + dados pessoais (foto, nascimento, endereço, telefones). Demonstre consciência:

- Não logar dado sensível.
- Cookies `HttpOnly` / `Secure` / `SameSite`.
- HTTPS / HSTS.
- **Anti-forgery token (CSRF)** nos POSTs.
- Cuidado com `Html.Raw` (XSS).
- Mencionar LGPD no README pesa a favor.

### 🟠 Segredos no repositório público

Repo é público no GitHub.

- **Não commite** connection string com senha nem `appsettings` com segredos.
- Use **User Secrets** / variáveis de ambiente.
- `.gitignore` correto.
- Histórico de commits limpo.

---

## 2. Modelagem de domínio — armadilhas escondidas no texto

### Relação gestor ↔ subordinado

O texto fala em "subordinados" mas muita gente só cria um flag `IsManager`. Isso **não** modela _quem é subordinado de quem_.

- Você precisa de uma relação (FK auto-referenciada `ManagerId` ou tabela de associação).
- Sem isso, a regra "gestor cadastra tarefa para _seus_ subordinados" é impossível de cumprir corretamente.

### Estados da tarefa

"Acompanhar o andamento" + "tarefa finalizada" implica **status** (Pendente / Em andamento / Concluída) e provavelmente "Atrasada" (data limite < hoje).

- Modele como enum / máquina de estados, com timestamps.
- Defina **quem** pode finalizar (o subordinado responsável).

### Quem define a senha do novo usuário?

Ambiguidade real: o cadastro é feito pelo gestor, mas a auth é por e-mail/senha. Quem cria a senha inicial?

- Decisão sênior: gestor define senha temporária + **forçar troca no primeiro login**.
- Documente como premissa.

### Validações de domínio

- Data de nascimento não pode ser futura (idade mínima?).
- E-mail único (trate a constraint **e** a race condition).
- Formato de telefone fixo vs celular.
- E-mail válido.
- **Validação no servidor** sempre (FluentValidation ou DataAnnotations) — nunca confie só no cliente.

---

## 3. E-mail — a pegadinha de arquitetura

O envio de e-mail é marcado como **"desejável"**, mas é onde se erra feio:

- **Não envie e-mail síncrono dentro do request.** Se o SMTP cair, o cadastro/atribuição não pode falhar junto. Use `BackgroundService` / `Channel`, fila, Hangfire, ou **Outbox pattern**.
- Não deixe falha de e-mail dar rollback na transação de negócio (problema do **dual-write**).
- Coloque atrás de uma interface (`IEmailSender`) — pluggável e testável. Em dev, use um fake/MailHog.

---

## 4. Arquitetura — nem de menos, nem demais

A maior armadilha de sênior aqui é o **extremo errado**:

- **De menos:** tudo no controller/PageModel, sem camadas, sem DI → não é "padrão para o time".
- **De mais:** DDD tático completo + CQRS + MediatR + microserviços para um CRUD → over-engineering que assusta o time.

O ponto certo:

- Arquitetura em camadas limpa (Domain / Application / Infrastructure / Web).
- **DI** (injeção de dependência).
- Separação clara de responsabilidades.
- SOLID visível.
- Abstrações onde **agregam** valor.
- **Justifique as escolhas no README** — explicar o porquê é sinal de sênior.

---

## 5. Desafios específicos de C# / .NET (onde a linguagem te trai)

| Armadilha                        | Cuidado                                                                                                                                                                |
| -------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`async`/`await`**              | Nunca `async void` (exceto event handlers); nunca `.Result`/`.Wait()` (deadlock); use os métodos `Async` do EF e propague `CancellationToken`. Async de ponta a ponta. |
| **Nullable reference types**     | Habilite `<Nullable>enable</Nullable>` e trate os avisos — evita `NullReferenceException` em produção.                                                                 |
| **Lifetimes de DI**              | `DbContext` é **Scoped**; não injete Scoped em Singleton (captive dependency).                                                                                         |
| **`DateTime`**                   | Use `DateTimeOffset`/UTC para armazenar; `DateTime.UtcNow`, não `Now`. Cuidado com `Kind` e timezone na data limite.                                                   |
| **EF Core N+1**                  | Carregar tarefas + usuários sem `Include`/projeção gera N+1. Use `AsNoTracking()` em leitura, projete para DTO.                                                        |
| **Entidade vazando para a View** | Não passe entidade EF direto para Razor — use ViewModel/DTO.                                                                                                           |
| **`HttpClient`**                 | `IHttpClientFactory`, nunca `new HttpClient()` em loop (socket exhaustion).                                                                                            |
| **`decimal` vs `double`**        | Qualquer valor monetário futuro → `decimal`. Numa financeira isso é dogma.                                                                                             |
| **Exceções**                     | Não engula; use middleware global de exceção + log estruturado (Serilog).                                                                                              |
| **Cultura/encoding**             | UTF-8 em tudo (o próprio PDF veio com mojibake), datas `dd/MM/yyyy`, cultura pt-BR.                                                                                    |
| **LINQ**                         | Cuidado com execução adiada (deferred execution) e múltipla enumeração do mesmo `IEnumerable`.                                                                         |
| **`IDisposable` / `using`**      | Garanta dispose de recursos (DbContext, streams de upload).                                                                                                            |
| **Options pattern**              | Configuração via binding fortemente tipado (`IOptions<T>`), não ler `IConfiguration` espalhado.                                                                        |

---

## 6. Entrega — o que reprova na largada

- **Tem que rodar em máquina limpa.** Pegadinha: caminho hardcoded, migration faltando, seed faltando. Teste num ambiente do zero. **Docker Compose** (app + SQL Server) é um diferencial forte de sênior aqui.
- **Scripts de banco são exigidos**: modelagem + população. Entregue via **EF Migrations** ou scripts `.sql` versionados — e deixe o seed do usuário `ti@leveinvestimentos.com.br` (com hash) automático.
- **README detalhado** é requisito explícito. Inclua:
  - Pré-requisitos.
  - Como subir o banco.
  - Connection string via variável de ambiente.
  - Como rodar.
  - Credenciais do usuário inicial.
  - **Decisões de arquitetura**.
- **Testes.** Ausência de testes é red flag para sênior. Cubra ao menos:
  - Hashing de senha.
  - Autorização (gestor vs não-gestor).
  - Regra "só meus subordinados".
  - Validações de domínio.

---

## 7. Premissas que você deve documentar (não decidir em silêncio)

Sênior não inventa em silêncio — liste premissas no README:

1. Cadastro é **exclusivo de gestores** (não há self-registration).
2. Gestor define senha temporária → troca obrigatória no 1º login.
3. Modelo de subordinação (1 gestor : N subordinados) — defina e declare.
4. E-mail é "desejável": implementado assíncrono e degradando com segurança.
5. Soft delete + auditoria (CreatedAt/CreatedBy) por ser financeira.

---

## Checklist rápido (antes de enviar)

- [ ] Nenhuma senha em texto plano; seed com hash.
- [ ] Autorização validada no servidor (não só na UI).
- [ ] ViewModels/InputModels (sem bind direto em entidade).
- [ ] Upload validado (extensão + magic bytes + tamanho + nome novo).
- [ ] Relação gestor↔subordinado modelada e respeitada.
- [ ] Status de tarefa + regra de quem finaliza.
- [ ] E-mail assíncrono, atrás de interface, sem quebrar transação.
- [ ] Async de ponta a ponta; sem `.Result`/`.Wait()`.
- [ ] Nullable habilitado e tratado.
- [ ] Datas em UTC; cultura pt-BR na exibição.
- [ ] Nenhum segredo commitado; `.gitignore` correto.
- [ ] CSRF, HTTPS/HSTS, cookies seguros, sem `Html.Raw`.
- [ ] Testes nas regras críticas.
- [ ] README com passo a passo + scripts de banco + decisões de arquitetura.
- [ ] Roda em máquina limpa (idealmente Docker Compose).
- [ ] Premissas documentadas.
