# FIAP Cloud Gaming - Identity Service

Microsservico de identidade e autenticacao da FIAP Cloud Gaming. Gerencia cadastro de usuarios, login, tokens JWT e controle de acesso baseado em roles.

## Arquitetura

Clean Architecture com Domain-Driven Design (DDD) e CQRS:

```
src/
├── Identity.Domain/           Entidades, Value Objects, Eventos de Dominio
├── Identity.Infrastructure/   EF Core, PostgreSQL, JWT, Repositorios
└── Identity.API/              Endpoints, Behaviors, Validacoes, Middleware
```

## Funcionalidades

### Autenticacao
- Registro de usuario com validacao de email e forca de senha
- Login com JWT access token + refresh token
- Rotacao de refresh token
- Validacao de token via endpoint
- Troca de senha com revogacao de tokens

### Gerenciamento de Usuarios
- Perfil do usuario autenticado (`/me`)
- Consulta por ID (admin ou proprio perfil)
- Listagem paginada (admin/manager)

### Controle de Acesso
- Roles: ADMIN, USER, MANAGER
- Policies: RequireAuthenticatedUser, RequireAdminRole, CanManageUsers, RequireUserRole
- Atribuicao e remocao de roles

### Eventos de Dominio
- UserRegistered, UserLoggedIn, PasswordChanged
- RefreshTokenGenerated, RefreshTokenRevoked
- RoleAssigned, RoleRemoved
- UserActivated, UserDeactivated

## Endpoints

### Auth (`/api/auth`)
| Metodo | Rota | Descricao | Autenticacao |
|--------|------|-----------|--------------|
| POST | `/register` | Registrar usuario | Nao |
| POST | `/login` | Login | Nao |
| POST | `/refresh` | Renovar token | Nao |
| PUT | `/change-password` | Alterar senha | Sim |
| GET | `/validate` | Validar token | Sim |

### Users (`/api/users`)
| Metodo | Rota | Descricao | Autenticacao |
|--------|------|-----------|--------------|
| GET | `/me` | Perfil atual | Sim |
| GET | `/{id}` | Buscar por ID | Sim (admin ou proprio) |
| GET | `/` | Listar usuarios | Sim (admin/manager) |

## Stack

| Componente | Tecnologia |
|-----------|------------|
| Framework | .NET 10.0 |
| CQRS | FiapCloudGaming.Mediator |
| DDD | FiapCloudGaming.SeedWork |
| Observabilidade | FiapCloudGaming.ServiceDefaults (OpenTelemetry + Serilog) |
| Banco de dados | PostgreSQL 18 + EF Core |
| Autenticacao | JWT Bearer (HS256) |
| Validacao | FluentValidation |
| Logging | Serilog + Grafana Loki |
| Tracing | Jaeger (OTLP) |
| Dashboards | Grafana |
| Documentacao API | Scalar (OpenAPI) |

## Executar localmente

### Pre-requisitos
- .NET 10 SDK
- Docker e Docker Compose

### Subir com Docker Compose

```bash
docker compose up -d
```

Servicos disponiveis:
| Servico | URL |
|---------|-----|
| Identity API | http://localhost:5001 |
| Grafana | http://localhost:3000 (admin/admin) |
| Jaeger UI | http://localhost:16686 |
| PostgreSQL | localhost:5432 |

### Executar sem Docker

```bash
dotnet run --project src/Identity.API
```

## Seguranca

- **Senha**: PBKDF2-SHA512, 500.000 iteracoes, salt de 16 bytes
- **Access Token**: JWT HS256, expiracao configuravel (padrao 60min)
- **Refresh Token**: 64 bytes aleatorios (base64), expiracao 7 dias
- **Comparacao**: Constant-time (FixedTimeEquals)
- **Idempotencia**: Header `x-requestid` para operacoes criticas

## Observabilidade

- **Logs**: Serilog estruturado com sink para Grafana Loki
- **Traces**: OpenTelemetry exportando para Jaeger via OTLP
- **Health Checks**: `/health` (geral) e `/alive` (liveness)
- **Dashboard**: Grafana com painel pre-configurado (logins, registros, erros)

## Dependencias (NuGet)

- `FiapCloudGaming.Mediator` - Mediator/CQRS
- `FiapCloudGaming.SeedWork` - Building blocks DDD
- `FiapCloudGaming.ServiceDefaults` - Observabilidade compartilhada

## Licenca

MIT
