# Grafana - Identity Service

Dashboards pre-configurados do Grafana para monitoramento do Identity Service.

## Dashboard Disponivel

### Identity API - Authentication Service
**Arquivo:** `provisioning/dashboards/identity-api-dashboard.json`

Monitoramento do servico de autenticacao:
- Total de logins realizados
- Total de registros de novos usuarios
- Total de erros no servico
- Distribuicao de niveis de log
- Atividade de autenticacao ao longo do tempo (Logins vs Registros)
- Logs em tempo real
- Logs de erro filtrados
- Logs de atividade de login
- Logs de atividade de registro

**Datasources utilizados:**
- Loki (logs)
- Jaeger (tracing)

**Auto-refresh:** 5 segundos

## Como Usar

### Acessar Grafana
```
URL: http://localhost:3000
Usuario: admin
Senha: admin
```

### Navegar ate o Dashboard
1. Clique no menu no topo esquerdo
2. Va em **Dashboards** > **Browse**
3. Abra a pasta **FCG** e selecione **Identity API**

## Estrutura de Arquivos

```
grafana/
├── README.md
└── provisioning/
    ├── datasources/
    │   └── datasources.yml          # Loki + Jaeger
    └── dashboards/
        ├── dashboards.yml           # Configuracao de provisionamento
        └── identity-api-dashboard.json
```

## Datasources Configurados

### Loki
- **URL:** http://loki:3100
- **Uso:** Logs da Identity API

### Jaeger
- **URL:** http://jaeger:16686
- **Uso:** Rastreamento distribuido de requisicoes
- **Integracao:** Correlacao automatica com logs do Loki

## Queries Uteis (LogQL)

**Logs de erro:**
```
{container="fcg-identity-api"} | json | level=~"Error|Fatal"
```

**Logs de login:**
```
{container="fcg-identity-api"} |~ "(?i)login"
```

**Logs de registro:**
```
{container="fcg-identity-api"} |~ "(?i)register"
```

## Recarregar Dashboards

```bash
docker restart fcg-grafana
```

Os dashboards sao recarregados automaticamente via provisioning.
