# DemoAndBank

Sistema para gestão de posições financeiras em .NET 8, composto por uma API REST e uma aplicação console para importação automática de dados.

## Arquitetura

- **AndBank.Domain**: Entidades de domínio e interfaces de repositório
- **AndBank.Application**: DTOs e objetos de transferência de dados
- **AndBank.Infrastructure**: Repositórios, contexto de banco e acesso a dados
- **AndBank.Api**: API REST para consulta de posições
- **AndBank.ConsoleApp**: Aplicação console para importação agendada
- **AndBank.Test**: Projeto de testes automatizados (unitários e de integração)

## Requisitos

- .NET SDK 8.0
- PostgreSQL 15 (ou Docker Desktop)

## Configuração

Defina a string de conexão no `appsettings.json` ou via variável de ambiente `ConnectionStrings__Postgres`.

## Instalação

```bash
dotnet restore
```

## Execução

API:

```bash
cd AndBank/AndBank.Api
dotnet run
```

A API estará disponível em `http://localhost:5000` e a documentação Swagger em `http://localhost:5000/swagger`.

Docker Compose:

```bash
cd AndBank
docker-compose up -d
```

## Endpoints

- GET `/api/positions/client/{clientId}`
- GET `/api/positions/client/{clientId}/summary`
- GET `/api/positions/top10`

## Testes

Execução da suíte de testes:

```bash
dotnet test
```

Execução focada no projeto de testes:

```bash
cd AndBank.Test
dotnet test
```

Observação: testes do repositório de posições em `AndBank.Test/Repositories/PositionRepositoryTests.cs`.

## Banco de dados

Entidade `Position`:
- PositionId
- ProductId
- ClientId
- Date
- Value
- Quantity

Chave primária: `PositionId` + `Date`. Índices em `ClientId` e `Value`.