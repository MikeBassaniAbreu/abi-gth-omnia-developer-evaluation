
# Projeto Ambev Developer Evaluation - Sistema de Gestão de Vendas
Este projeto é uma solução de API RESTful para gestão de vendas e seus itens, desenvolvida em .NET 8 com arquitetura Onion/Clean Architecture. Ela segue os princípios de CQRS (Comandos e Queries) com MediatR, utiliza Entity Framework Core para persistência em PostgreSQL, e possui testes unitários abrangentes.

## Pré-requisitos

Certifique-se de ter as seguintes ferramentas instaladas em sua máquina:

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (ou a versão mais recente compatível)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (necessário para rodar os serviços de banco de dados e cache)
* [Git](https://git-scm.com/downloads)

## Configuração e Execução do Projeto

Siga os passos abaixo para configurar, executar e testar o projeto em sua máquina local.

### 1. Clonar o Repositório
### 2. O Peojeto está completo na 👉 👉  DEVELOP, MUDE PARA ESSA BRANCH 👈👈

### 3. Iniciar os Serviços de Banco de Dados e Cache com Docker Compose
Neste projeto, utilizamos Docker Compose para orquestrar o banco de dados PostgreSQL e outros serviços.

Importante: Embora os serviços de MongoDB e Redis estejam configurados no docker-compose.yml e serão iniciados, as funcionalidades principais de gestão de vendas da API utilizam o PostgreSQL como base de dados primária, conforme a DefaultConnection no appsettings.Development.json.

No diretório raiz do projeto (onde está o docker-compose.yml), execute o seguinte comando:


#### docker compose up -d

Este comando irá:

Construir a imagem da sua API (ambev.developerevaluation.webapi).
Criar e iniciar os contêineres para:
ambev.developerevaluation.webapi (sua API .NET)
ambev.developerevaluation.database (PostgreSQL): Mapeia a porta 5432 do contêiner para a 5432 da sua máquina local. As credenciais são developer/ev@luAt10n e o DB é developer_evaluation.
ambev.developerevaluation.nosql (MongoDB): Expõe a porta 27017 do contêiner para uma porta efêmera na sua máquina local.
ambev.developerevaluation.cache (Redis): Expõe a porta 6379 do contêiner para uma porta efêmera na sua máquina local.
Executar todos esses serviços em segundo plano (-d de "detached").

### 4. Executar o Projeto da API (.NET) - Migrações Automáticas
Com os serviços de backend rodando via Docker Compose (e o contêiner da sua API também já pode estar rodando se você usou docker compose up -d), você pode iniciar sua aplicação Web API do .NET. As migrações do Entity Framework Core serão aplicadas automaticamente durante o startup da aplicação ao se conectar ao PostgreSQL.

# Testando o Projeto
1. Acessar a Documentação da API (Swagger UI)
Uma vez que a API esteja rodando, você pode acessar a documentação interativa via Swagger UI em seu navegador.
Através do Swagger, você pode explorar todos os endpoints disponíveis, entender suas requisições/respostas e testá-los diretamente.

## payload para teste:

### Endpointe POST /api/Sales

```json
{
  "saleNumber": "VENDA-001",
  "saleDate": "2025-05-29T04:53:08.346Z",
  "customerId": "00000000-0000-0000-0000-000000000001",
  "customerName": "Cliente Teste 123",
  "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "branchName": "Filial Central Ambev",
  "items": [
    {
      "productId": "00000000-0000-0000-0000-000000000002",
      "productName": "Fanta",
      "quantity": 4,
      "unitPrice": 7.5
    },
    {
      "productId": "00000000-0000-0000-0000-000000000003",
      "productName": "Fanta uva",
      "quantity": 5,
      "unitPrice": 8
    },
    {
      "productId": "00000000-0000-0000-0000-000000000004",
      "productName": "Água Mineral 500ml",
      "quantity": 5,
      "unitPrice": 8
    }
  ]
}
```
---
## Observação para o Avaliador

O escopo original para este projeto incluía a implementação de autenticação (Auth) e mecanismos de eventos. Devido à minha dedicação intensa ao core do projeto e ao desejo de entregar as funcionalidades principais de gestão de vendas em um tempo significativamente reduzido (concluído em 2 dias, em vez dos 7 dias previstos), priorizei a entrega robusta e testada das funcionalidades essenciais de vendas e seus itens.

Portanto, a implementação da autenticação e de eventos não foi abordada nesta entrega, permitindo-me focar na qualidade e velocidade de desenvolvimento do domínio principal da avaliação.

---

# PEDIDO DO DESAFIO 👇👇👇👇



# Developer Evaluation Project

`READ CAREFULLY`

## Instructions
**The test below will have up to 7 calendar days to be delivered from the date of receipt of this manual.**

- The code must be versioned in a public Github repository and a link must be sent for evaluation once completed
- Upload this template to your repository and start working from it
- Read the instructions carefully and make sure all requirements are being addressed
- The repository must provide instructions on how to configure, execute and test the project
- Documentation and overall organization will also be taken into consideration

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)
