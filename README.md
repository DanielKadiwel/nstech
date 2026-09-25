# Order Service API - Teste Técnico Sênior .NET 8

Uma API robusta de gestão de Pedidos (Order Service) desenvolvida com **Clean Architecture**, **Domain-Driven Design (DDD)**, **CQRS (MediatR)**, **EF Core 8**, **PostgreSQL** e **Docker**.

---

## 🛠️ Tecnologias e Stack
- **.NET 8 (C#)** - Web API RESTful
- **Entity Framework Core 8** - ORM com Migrations automáticas
- **PostgreSQL 15** - Banco de dados relacional (via Docker)
- **MediatR** - CQRS (Handlers, Commands e Queries)
- **xUnit + FluentAssertions + NSubstitute** - Testes Unitários de Domínio
- **JWT (JSON Web Token)** - Autenticação e Segurança
- **Swagger / OpenAPI** - Documentação interativa configurada com suporte a Bearer Token
- **Docker & Docker Compose** - Conteinerização completa da solução

---

## 🏛️ Arquitetura e Decisões de Design

A solução foi construída respeitando os princípios da **Clean Architecture** e **SOLID**:

```text
Nstech.OrderService
├── Nstech.OrderService.Domain          # Entidades ricas, Invariantes, Enums, Interfaces e Exceções de Domínio (Sem dependências externas)
├── Nstech.OrderService.Application     # Casos de Uso (Commands, Queries, Handlers MediatR) e DTOs
├── Nstech.OrderService.Infrastructure  # EF Core DbContext, Repositories, Migrations e Mapeamentos Fluent API
├── Nstech.OrderService.Api             # Controllers REST, Auth JWT, Swagger UI, Middleware e Program.cs
└── Nstech.OrderService.Tests           # Testes Unitários xUnit focados nas Regras de Negócio do Domínio
```

### 🎯 DDD Prático & Invariantes de Negócio
- **Encapsulamento Risco:** As propriedades das entidades têm *setters* privados. Mudanças de estado ocorrem através de métodos de domínio (`Place()`, `Confirm()`, `Cancel()`, `ReserveStock()`, `ReleaseStock()`).
- **Estados do Pedido:** `Draft` -> `Placed` -> `Confirmed` / `Canceled`.
- **Invariantes Garantidas:**
  - Não é possível criar pedidos sem itens ou com quantidade <= 0.
  - Não é possível exceder o estoque disponível do produto.
  - Cálculo do valor total automático: $\sum (\text{unitPrice} \times \text{quantity})$.
  - **Idempotência:** As operações de confirmação (`/confirm`) e cancelamento (`/cancel`) são idempotentes. Se chamadas múltiplas vezes, mantêm o estado sem duplicar a baixa ou a devolução de estoque.

---

## 📊 Estratégia de Carga Inicial de Dados (Data Seeding)

### 💡 Por que esta estratégia foi adotada?
Para proporcionar a **melhor experiência de avaliação imediata**, a API executa uma carga de dados inteligente ao inicializar (`Program.cs`):

1. **Auto-Migrations:** Executa `dbContext.Database.Migrate()` automaticamente para criar as tabelas e índices no PostgreSQL sem exigir intervenção manual.
2. **Produtos Pré-Cadastrados:** Verifica e insere **10 produtos diversos** no catálogo (Notebooks, Monitores, Periféricos, etc.) com estoques e preços reais.
3. **Pedidos de Amostragem:** Cria **2 pedidos prontos** no banco:
   - **Pedido 1 (Status: `Placed`):** ID `10000000-0000-0000-0000-000000000001`
   - **Pedido 2 (Status: `Confirmed`):** ID `20000000-0000-0000-0000-000000000002`

> **Benefício para o Avaliador:** Permite testar **todos** os endpoints de leitura (`GET /Products`, `GET /Orders`, `GET /Orders/{id}`) e ações (`POST /Orders`, `/confirm`, `/cancel`) imediatamente após subir o projeto, sem precisar cadastrar dados manualmente primeiro via SQL ou DBeaver.

---

## 🚀 Como Executar o Projeto

### Opção 1: Via Docker Compose (Conteinerização Completa - Recomendado)
1. Certifique-se de que o **Docker Desktop** está aberto.
2. No terminal, na pasta raiz do projeto, execute:
   ```bash
   docker compose up --build -d
   ```
3. Acesse o Swagger no navegador:
   👉 **[http://localhost:8080/swagger](http://localhost:8080/swagger)**

---

### Opção 2: Via Visual Studio (F5) / dotnet run (Desenvolvimento Local)
1. Suba apenas o banco PostgreSQL no Docker:
   ```bash
   docker compose up -d postgres_db
   ```
2. Abra a solução `Nstech.OrderService.sln` no **Visual Studio** e pressione **F5** (ou execute `dotnet run --project Nstech.OrderService.Api`).
3. Acesse o Swagger no navegador:
   👉 **[http://localhost:5115/swagger](http://localhost:5115/swagger)**

---

## 🧪 Como Executar os Testes Unitários

No terminal, execute:
```bash
dotnet test
```
*Executa a suíte de testes xUnit validando regras de domínio, exceções customizadas, idempotência e cálculo de totais.*

---

## 📑 Tabela de Endpoints da API

| Método | Endpoint | Descrição | Auth Exigida |
| :--- | :--- | :--- | :---: |
| `POST` | `/Auth/token` | Gera Token JWT de teste (válido por 2h) | Não |
| `GET` | `/Products` | Lista todos os produtos do catálogo com estoque | Sim 🔒 |
| `POST` | `/Products` | Cadastra um novo produto no catálogo | Sim 🔒 |
| `POST` | `/Orders` | Cria um novo pedido no status `Placed` | Sim 🔒 |
| `POST` | `/Orders/{id}/confirm` | Confirma o pedido e baixa estoque (Idempotente) | Sim 🔒 |
| `POST` | `/Orders/{id}/cancel` | Cancela o pedido e estorna estoque (Idempotente) | Sim 🔒 |
| `GET` | `/Orders/{id}` | Consulta detalhes do pedido por ID | Sim 🔒 |
| `GET` | `/Orders` | Lista pedidos paginados com filtros | Sim 🔒 |

---

## 📝 Passo a Passo de Testes no Swagger e Postman

### 1. Autenticação (Obter Token JWT)
* **Endpoint:** `POST /Auth/token`
* **URL:** `http://localhost:5115/Auth/token` (ou `http://localhost:8080/Auth/token` no Docker)
* **Body:** *(Vazio)*
* **Response (`200 OK`):**
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
  ```

#### No Swagger:
* Clique no botão verde **Authorize 🔓** no topo da página.
* Digite: `Bearer SEU_TOKEN_AQUI` (com a palavra `Bearer` seguida de espaço e seu token).
* Clique em **Authorize** e feche a janela.

#### No Postman:
* Adicione o Header em suas requisições:
  * **Key:** `Authorization`
  * **Value:** `Bearer <TOKEN_COPIADO>`

---

### 2. Listar Produtos (`GET /Products`)
* **URL:** `http://localhost:5115/Products`
* **Response (`200 OK`):** Retorna a lista dos 10 produtos semeados automaticamente.
  ```json
  [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Notebook Gamer Dell G15",
      "unitPrice": 4500.00,
      "availableQuantity": 10
    },
    {
      "id": "22222222-2222-2222-2222-222222222222",
      "name": "Mouse Sem Fio Logitech MX Master 3S",
      "unitPrice": 150.00,
      "availableQuantity": 50
    }
  ]
  ```

---

### 3. Cadastrar Novo Produto (`POST /Products`)
* **URL:** `http://localhost:5115/Products`
* **Request Body:**
  ```json
  {
    "name": "Webcam Full HD Logitech C920",
    "unitPrice": 399.90,
    "availableQuantity": 20
  }
  ```

---

### 4. Criar Pedido (`POST /Orders`)
* **URL:** `http://localhost:5115/Orders`
* **Request Body:**
  ```json
  {
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "currency": "BRL",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "quantity": 1
      },
      {
        "productId": "22222222-2222-2222-2222-222222222222",
        "quantity": 2
      }
    ]
  }
  ```
* **Response (`201 Created`):**
  ```json
  {
    "id": "b4c2d3e4-5678-90ab-cdef-1234567890ab"
  }
  ```

---

### 5. Confirmar Pedido (`POST /Orders/{id}/confirm`)
* **URL:** `http://localhost:5115/Orders/10000000-0000-0000-0000-000000000001/confirm`
* **Response (`200 OK`):**
  ```json
  {
    "message": "Order confirmed successfully."
  }
  ```
*(Promove de `Placed` para `Confirmed` e baixa o estoque dos produtos no banco).*

---

### 6. Cancelar Pedido (`POST /Orders/{id}/cancel`)
* **URL:** `http://localhost:5115/Orders/10000000-0000-0000-0000-000000000001/cancel`
* **Response (`200 OK`):**
  ```json
  {
    "message": "Order canceled successfully."
  }
  ```
*(Altera status para `Canceled` e devolve a quantidade reservada ao estoque).*

---

### 7. Consultar Pedido por ID (`GET /Orders/{id}`)
* **URL:** `http://localhost:5115/Orders/10000000-0000-0000-0000-000000000001`
* **Response (`200 OK`):**
  ```json
  {
    "id": "10000000-0000-0000-0000-000000000001",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Placed",
    "currency": "BRL",
    "total": 4800.00,
    "createdAt": "2026-09-24T23:45:00Z",
    "items": [
      {
        "id": "7f8a9b0c-1234-5678-90ab-cdef12345678",
        "productId": "11111111-1111-1111-1111-111111111111",
        "unitPrice": 4500.00,
        "quantity": 1
      },
      {
        "id": "8a9b0c1d-2345-6789-0abc-def123456789",
        "productId": "22222222-2222-2222-2222-222222222222",
        "unitPrice": 150.00,
        "quantity": 2
      }
    ]
  }
  ```

---

### 8. Listar Pedidos Paginados (`GET /Orders`)
* **URL:** `http://localhost:5115/Orders?page=1&pageSize=10`
* **Response (`200 OK`):**
  ```json
  {
    "items": [
      {
        "id": "10000000-0000-0000-0000-000000000001",
        "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "status": "Placed",
        "currency": "BRL",
        "total": 4800.00,
        "createdAt": "2026-09-24T23:45:00Z",
        "items": [...]
      },
      {
        "id": "20000000-0000-0000-0000-000000000002",
        "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "status": "Confirmed",
        "currency": "BRL",
        "total": 1949.90,
        "createdAt": "2026-09-24T23:45:00Z",
        "items": [...]
      }
    ],
    "totalCount": 2,
    "page": 1,
    "pageSize": 10
  }
  ```
