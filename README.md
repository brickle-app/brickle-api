# Brickle API - Real Estate Tokenization Platform

## Scope of the Project

Brickle API is a comprehensive **real estate tokenization platform** built with .NET 8 that enables fractional real estate investment through blockchain technology. The platform allows users to invest in tokenized real estate assets, participate in leasing campaigns, and earn rental income from their investments.

**Key Business Capabilities:**

- **Real Estate Tokenization**: Convert physical real estate properties into digital tokens
- **Fractional Investment**: Enable multiple investors to participate in high-value properties
- **Leasing Management**: Comprehensive leasing contract management and rental distribution
- **Campaign System**: Crowdfunding-style investment campaigns for real estate projects
- **Wallet Integration**: Web3 wallet management and blockchain transactions
- **Portfolio Management**: Investment tracking and performance analytics
- **Payment Processing**: Secure handling of deposits, withdrawals, and rent distributions

**Target Market**: Individual and institutional investors seeking fractional real estate investment opportunities through blockchain technology.

## Project Description

Brickle API is a robust web application developed in .NET 8 that implements **Clean Architecture** with **CQRS (Command Query Responsibility Segregation)** and **Mediator** patterns. The platform manages users, leasing contracts, payments, campaigns, and notifications in a tokenized real estate ecosystem.

## Arquitectura del Proyecto

El proyecto está organizado en 4 capas principales que siguen los principios de Clean Architecture:

```
src/
├── BricklePlatform.Api/          # Capa de Presentación (API)
├── BricklePlatform.Application/  # Capa de Aplicación (vacía - lógica en Api)
├── BricklePlatform.Domain/       # Capa de Dominio
└── BricklePlatform.Infrastructure/ # Capa de Infraestructura
```

### 1. Capa de Dominio (BricklePlatform.Domain)

**Propósito**: Contiene la lógica de negocio central, entidades, interfaces y reglas de dominio.

**Contenido**:

- **Entities/**: Entidades del dominio con lógica de negocio

  - `User.cs`: Entidad principal de usuario
  - `Leasing.cs`: Entidad de contratos de arrendamiento
  - `UserLeasingAgreement.cs`: Relación usuario-contrato
  - `UserContact.cs`: Contactos de usuarios
  - `LogEntry.cs`: Registro de eventos

- **DTOs/**: Objetos de transferencia de datos

  - `CreateUserDto.cs`: DTO para creación de usuarios
  - `UpdateUserDto.cs`: DTO para actualización de usuarios
  - `UserDto.cs`: DTO de respuesta de usuario
  - `LeasingDto.cs`: DTO de contratos
  - `PaymentDto.cs`: DTO de pagos

- **Interfaces/**: Contratos de servicios y repositorios

  - `IUserRepository.cs`: Contrato del repositorio de usuarios
  - `IUserService.cs`: Contrato del servicio de usuarios
  - `ILeasingRepository.cs`: Contrato del repositorio de contratos
  - `IWeb3Service.cs`: Contrato para servicios blockchain

- **Enums/**: Enumeraciones del dominio
- **ValueObjects/**: Objetos de valor
- **Exceptions/**: Excepciones específicas del dominio

### 2. Capa de Infraestructura (BricklePlatform.Infrastructure)

**Propósito**: Implementa las interfaces del dominio y maneja la persistencia, servicios externos y configuraciones.

**Contenido**:

- **Repositories/**: Implementaciones de repositorios

  - `UserRepository.cs`: Implementación del repositorio de usuarios
  - `LeasingRepository.cs`: Implementación del repositorio de contratos
  - `BlobStorageRepository.cs`: Manejo de archivos en blob storage

- **Services/**: Implementaciones de servicios

  - `UserService.cs`: Lógica de negocio de usuarios
  - `Web3Service.cs`: Integración con blockchain
  - `PasswordService.cs`: Manejo de contraseñas
  - `NotificationService.cs`: Envío de notificaciones

- **Persistence/**: Configuración de base de datos

  - `ApplicationDbContext.cs`: Contexto de Entity Framework

- **Settings/**: Configuraciones de infraestructura
- **Utilities/**: Utilidades y helpers

### 3. Capa de API (BricklePlatform.Api)

**Propósito**: Punto de entrada de la aplicación, maneja las peticiones HTTP y coordina las operaciones.

**Contenido**:

- **Controllers/**: Controladores de la API REST

  - `UserController.cs`: Endpoints de gestión de usuarios
  - `LeasingController.cs`: Endpoints de contratos
  - `PaymentController.cs`: Endpoints de pagos
  - `NotificationController.cs`: Endpoints de notificaciones

- **Application/**: Lógica de aplicación usando CQRS

  - **Commands/**: Comandos para operaciones de escritura

    - `User/CreateUserCommand.cs`: Comando para crear usuario
    - `User/UpdateUserCommand.cs`: Comando para actualizar usuario
    - `User/DeleteUserCommand.cs`: Comando para eliminar usuario

  - **Queries/**: Consultas para operaciones de lectura

    - `User/GetUserQuery.cs`: Consulta para obtener usuario
    - `User/SearchUsersQuery.cs`: Consulta para buscar usuarios

  - **Handlers/**: Manejadores de comandos y consultas

    - `User/CreateUserHandler.cs`: Maneja la creación de usuarios
    - `User/UpdateUserHandler.cs`: Maneja la actualización de usuarios

  - **Dtos/**: DTOs específicos de la API
  - **Models/**: Modelos de request/response

- **Extensions/**: Configuraciones y extensiones

  - `WebApplicationExtension.cs`: Configuración principal
  - `MediatorExtension.cs`: Configuración de MediatR
  - `SwaggerExtension.cs`: Configuración de Swagger

- **Behaviors/**: Comportamientos transversales

  - `ValidationBehavior.cs`: Validación automática

- **Validators/**: Validadores usando FluentValidation
- **Middleware/**: Middleware personalizado
- **Filters/**: Filtros de acción

### 4. Capa de Aplicación (BricklePlatform.Application)

**Nota**: Esta capa está actualmente vacía ya que la lógica de aplicación se encuentra en `BricklePlatform.Api/Application/`.

## Patrones Implementados

### CQRS (Command Query Responsibility Segregation)

- **Commands**: Para operaciones de escritura (Create, Update, Delete)
- **Queries**: Para operaciones de lectura (Get, Search)
- **Handlers**: Procesan los comandos y consultas

### Mediator Pattern

- Utiliza **MediatR** para desacoplar controladores de la lógica de negocio
- Los controladores envían comandos/consultas al mediador
- Los handlers procesan las operaciones

### Repository Pattern

- Abstrae el acceso a datos
- Interfaces en Domain, implementaciones en Infrastructure

### Dependency Injection

- Configurado en `DependencyContainer.cs`
- Registro de servicios y repositorios

## Cómo Agregar un Nuevo Endpoint

### Ejemplo: Crear endpoint para gestionar "Propiedades"

#### 1. Crear la Entidad de Dominio

**Ubicación**: `src/BricklePlatform.Domain/Entities/Property.cs`

```csharp
namespace BricklePlatform.Domain.Entities;

public class Property
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public decimal Price { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Property() { }

    public static Property Create(string name, string address, decimal price, string description)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            Name = name,
            Address = address,
            Price = price,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string address, decimal price, string description)
    {
        Name = name;
        Address = address;
        Price = price;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

#### 2. Crear DTOs

**Ubicación**: `src/BricklePlatform.Domain/DTOs/`

**CreatePropertyDto.cs**:

```csharp
namespace BricklePlatform.Domain.DTOs;

public class CreatePropertyDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
```

**PropertyDto.cs**:

```csharp
namespace BricklePlatform.Domain.DTOs;

public class PropertyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### 3. Crear Interface del Repositorio

**Ubicación**: `src/BricklePlatform.Domain/Interfaces/IPropertyRepository.cs`

```csharp
using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id);
    Task<IEnumerable<Property>> GetAllAsync();
    Task<Property> AddAsync(Property property);
    Task UpdateAsync(Property property);
    Task DeleteAsync(Guid id);
}
```

#### 4. Implementar el Repositorio

**Ubicación**: `src/BricklePlatform.Infrastructure/Repositories/PropertyRepository.cs`

```csharp
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _context;

    public PropertyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        return await _context.Properties.FindAsync(id);
    }

    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        return await _context.Properties.ToListAsync();
    }

    public async Task<Property> AddAsync(Property property)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task UpdateAsync(Property property)
    {
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var property = await GetByIdAsync(id);
        if (property != null)
        {
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
        }
    }
}
```

#### 5. Crear Comandos y Consultas

**Ubicación**: `src/BricklePlatform.Api/Application/Commands/Property/`

**CreatePropertyCommand.cs**:

```csharp
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Property;

public record CreatePropertyCommand(
    HeaderRequestModel Header,
    CreatePropertyDto Body
) : IRequest<PropertyDto>;
```

**Ubicación**: `src/BricklePlatform.Api/Application/Queries/Property/`

**GetPropertyQuery.cs**:

```csharp
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Property;

public record GetPropertyQuery(
    HeaderRequestModel Header,
    Guid Id
) : IRequest<PropertyDto>;
```

#### 6. Crear Handlers

**Ubicación**: `src/BricklePlatform.Api/Application/Handlers/Property/`

**CreatePropertyHandler.cs**:

```csharp
using BricklePlatform.Api.Application.Commands.Property;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Property;

public class CreatePropertyHandler : IRequestHandler<CreatePropertyCommand, PropertyDto>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly ILogger<CreatePropertyHandler> _logger;

    public CreatePropertyHandler(
        IPropertyRepository propertyRepository,
        ILogger<CreatePropertyHandler> logger)
    {
        _propertyRepository = propertyRepository;
        _logger = logger;
    }

    public async Task<PropertyDto> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating property: {Name} - CorrelationId: {CorrelationId}",
            request.Body.Name, request.Header.CorrelationId);

        var property = Domain.Entities.Property.Create(
            request.Body.Name,
            request.Body.Address,
            request.Body.Price,
            request.Body.Description);

        await _propertyRepository.AddAsync(property);

        return new PropertyDto
        {
            Id = property.Id,
            Name = property.Name,
            Address = property.Address,
            Price = property.Price,
            Description = property.Description,
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt
        };
    }
}
```

#### 7. Crear el Controlador

**Ubicación**: `src/BricklePlatform.Api/Controllers/PropertyController.cs`

```csharp
using BricklePlatform.Api.Application.Commands.Property;
using BricklePlatform.Api.Application.Queries.Property;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PropertyController> _logger;

    public PropertyController(IMediator mediator, ILogger<PropertyController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PropertyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProperty(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] CreatePropertyDto createProperty)
    {
        try
        {
            var command = new CreatePropertyCommand(header, createProperty);
            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property - CorrelationId: {CorrelationId}",
                header.CorrelationId);
            throw;
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProperty(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id)
    {
        try
        {
            var query = new GetPropertyQuery(header, id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting property - CorrelationId: {CorrelationId}",
                header.CorrelationId);
            throw;
        }
    }
}
```

#### 8. Registrar Dependencias

**Ubicación**: `src/BricklePlatform.Infrastructure/DependencyContainer.cs`

Agregar en el método `AddInfrastructureServices`:

```csharp
// Repositories
services.AddTransient<IPropertyRepository, PropertyRepository>();
```

#### 9. Crear Validadores (Opcional)

**Ubicación**: `src/BricklePlatform.Api/Validators/CreatePropertyDtoValidator.cs`

```csharp
using BricklePlatform.Domain.DTOs;
using FluentValidation;

namespace BricklePlatform.Api.Validators;

public class CreatePropertyDtoValidator : AbstractValidator<CreatePropertyDto>
{
    public CreatePropertyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la propiedad es requerido")
            .MaximumLength(200)
            .WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("La dirección es requerida");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("El precio debe ser mayor a 0");
    }
}
```

## Estructura de Directorios Recomendada para Nuevos Endpoints

```
src/BricklePlatform.Domain/
├── Entities/
│   └── [NuevaEntidad].cs
├── DTOs/
│   ├── Create[Entidad]Dto.cs
│   ├── Update[Entidad]Dto.cs
│   └── [Entidad]Dto.cs
└── Interfaces/
    └── I[Entidad]Repository.cs

src/BricklePlatform.Infrastructure/
├── Repositories/
│   └── [Entidad]Repository.cs
└── Services/
    └── [Entidad]Service.cs (si es necesario)

src/BricklePlatform.Api/
├── Controllers/
│   └── [Entidad]Controller.cs
├── Application/
│   ├── Commands/
│   │   └── [Entidad]/
│   │       ├── Create[Entidad]Command.cs
│   │       ├── Update[Entidad]Command.cs
│   │       └── Delete[Entidad]Command.cs
│   ├── Queries/
│   │   └── [Entidad]/
│   │       ├── Get[Entidad]Query.cs
│   │       └── Search[Entidad]Query.cs
│   └── Handlers/
│       └── [Entidad]/
│           ├── Create[Entidad]Handler.cs
│           ├── Update[Entidad]Handler.cs
│           ├── Delete[Entidad]Handler.cs
│           ├── Get[Entidad]Handler.cs
│           └── Search[Entidad]Handler.cs
└── Validators/
    ├── Create[Entidad]DtoValidator.cs
    └── Update[Entidad]DtoValidator.cs
```

## Technologies Used

### Backend Framework & Core

- **.NET 8**: Primary application framework
- **Entity Framework Core 8.0.3**: ORM for data access and migrations
- **MediatR 12.2.0**: Mediator pattern implementation for CQRS
- **FluentValidation 11.11.0**: Model validation and business rules
- **Newtonsoft.Json 13.0.3**: JSON serialization and processing

### API Documentation & Development

- **Swagger/OpenAPI (Swashbuckle.AspNetCore 6.5.0)**: API documentation and testing interface
- **ASP.NET Core**: RESTful API development

### Database & Storage

- **SQL Server**: Primary relational database
- **Azure Blob Storage 12.24.0**: File and document storage
- **Azure Table Storage 12.11.0**: NoSQL storage for logs and activity tracking

### Blockchain & Web3 Integration

- **Nethereum.Web3 5.0.0**: Ethereum blockchain integration
- **Smart Contracts**: Custom ABI contracts for leasing, campaigns, and tokenization
- **Polygon Network**: Primary blockchain network for transactions

### External Services & APIs

- **Resend 0.1.5**: Email service for notifications and communications
- **Microsoft Application Insights**: Telemetry, monitoring, and logging
- **Microsoft Application Insights Kubernetes**: Container orchestration insights

### Security & Authentication

- **API Key Authentication**: Custom API key validation system
- **Memory Caching**: API key caching for performance
- **CORS Configuration**: Cross-origin resource sharing setup

### Development & Testing

- **Docker**: Containerization support
- **User Secrets**: Secure configuration management
- **XUnit**: Unit testing framework (inferred from test structure)

### Infrastructure & DevOps

- **Azure Services**: Cloud infrastructure
- **Kubernetes**: Container orchestration
- **Application Insights**: Monitoring and diagnostics

## Project Statistics

### Code Metrics

- **Total C# Files**: 351 source files
- **Lines of Code**: 29,026 total lines
- **Configuration Files**: 11 JSON files (settings, contracts)
- **Database Migrations**: 20+ Entity Framework migrations
- **Smart Contract ABIs**: 6 blockchain contract interfaces

### API Endpoints Overview

**Total Endpoints**: 50+ RESTful endpoints across 12 controllers

- **Campaign Management**: 4 endpoints (Create, List, Get, Finalize)
- **Company Management**: 5 endpoints (CRUD operations + user association)
- **File Management**: 2 endpoints (Upload, Retrieve)
- **Investment Management**: 5 endpoints (Create, List, Get by user/ID)
- **Leasing Management**: 6 endpoints (CRUD, filtering, group categories)
- **Notification System**: 2 endpoints (Single, Bulk notifications)
- **Payment Processing**: 1 endpoint (Create payments)
- **Portfolio Analytics**: 2 endpoints (Overview, Projections)
- **User Management**: 14 endpoints (CRUD, search, contacts, transactions)
- **User Bank Accounts**: 5 endpoints (CRUD operations)
- **User Activity Logs**: 1 endpoint (Activity tracking)
- **User Leasing Agreements**: 4 endpoints (CRUD operations)

### Database Entities

- **Core Entities**: 9 domain entities (User, Company, Leasing, Campaign, etc.)
- **Supporting Entities**: 3 infrastructure entities (Keys, Logs)
- **Total Database Tables**: 12 configured tables

## Services Complexity Analysis

### 1. Web3 & Blockchain Integration (High Complexity)

**Components**: Web3Service, Smart Contract ABIs, Blockchain transactions
**Complexity Factors**:

- Ethereum/Polygon blockchain integration using Nethereum
- 6 smart contract interfaces (Leasing Core, Campaign, NFT, Token, Paymaster, Threshold Factory)
- Transaction signing, gas estimation, and contract function execution
- Multi-network support with configurable RPC endpoints
- Account creation from private keys and balance management

**Key Features**:

- Contract function calls and transaction execution
- Gas estimation and transaction signing
- Network abstraction and multi-chain support
- Real-time blockchain interaction for tokenization

### 2. External API Integrations (Medium Complexity)

**Components**: Email service (Resend), Push notifications (Expo), Azure services
**Integration Points**:

- **Email Service**: Automated email notifications for transactions, confirmations
- **Push Notifications**: Real-time mobile notifications through Expo
- **Azure Integration**: Blob storage, Table storage, Application Insights
- **HTTP Client Services**: Configurable external API calls with retry policies

### 3. Brickle Platform APIs (High Complexity)

**Internal Service Architecture**:

- **CQRS Implementation**: 45+ Commands and Queries with dedicated handlers
- **Repository Pattern**: 10+ repositories with Entity Framework integration
- **Domain Services**: Business logic encapsulation (User, Leasing, File management)
- **File Management**: Azure Blob storage with SAS token generation
- **Activity Logging**: Comprehensive user activity tracking in Azure Table Storage

**Business Logic Complexity**:

- **Campaign Lifecycle**: Creation, funding, finalization, and tokenization
- **Investment Processing**: Multi-user investment handling with blockchain settlement
- **Leasing Operations**: Contract creation, rent distribution, claim processing
- **Portfolio Management**: Real-time calculation of investment performance
- **Payment Processing**: Secure handling of deposits, withdrawals, and rent payments

## How to Access the Platform

### Development Environment

```bash
# Run the application
dotnet run --project src/BricklePlatform.Api

# Run tests
dotnet test

# Database migrations
dotnet ef migrations add InitialCreate --project src/BricklePlatform.Infrastructure --startup-project src/BricklePlatform.Api
dotnet ef database update --project src/BricklePlatform.Infrastructure --startup-project src/BricklePlatform.Api
```

### API Access

- **Base URL**: `https://localhost:5187` (Development)
- **Swagger Documentation**: `/swagger/v1/swagger.json`
- **API Key Required**: All endpoints require `api-key` header
- **Standard Headers**: `correlationId`, `user`, `source`, `requestDate`

### Authentication

- **API Key Authentication**: Custom implementation with memory caching
- **Header-based**: API key validation through custom filter
- **Environment-specific**: Different security levels for development/production

### Supported Operations

- **Real Estate Investment**: Browse and invest in tokenized properties
- **Campaign Participation**: Join crowdfunding campaigns for real estate
- **Wallet Management**: Web3 wallet integration and transaction handling
- **Portfolio Tracking**: Monitor investment performance and returns
- **Rent Collection**: Automated distribution of rental income
- **Document Management**: Secure file upload and storage

## Platform Capabilities

### For Investors

- **Fractional Real Estate Investment**: Invest in high-value properties with small amounts
- **Portfolio Diversification**: Spread investments across multiple properties
- **Passive Income Generation**: Earn monthly rental income from investments
- **Liquidity Options**: Secondary market trading capabilities (future)
- **Transparent Reporting**: Real-time investment tracking and performance metrics

### For Property Owners/Developers

- **Property Tokenization**: Convert real estate into digital tokens
- **Funding Access**: Raise capital through crowdfunding campaigns
- **Automated Management**: Streamlined rent collection and distribution
- **Compliance Tools**: Built-in regulatory compliance features
- **Analytics Dashboard**: Investment and performance tracking

### For Platform Administrators

- **Campaign Management**: Oversee and approve tokenization campaigns
- **User Administration**: Comprehensive user management and KYC
- **Financial Operations**: Monitor transactions, deposits, and withdrawals
- **Compliance Monitoring**: Regulatory compliance and reporting tools
- **System Analytics**: Platform performance and usage metrics

## Architecture Overview

The Brickle API follows **Clean Architecture** principles with clear separation of concerns:

```
src/
├── BricklePlatform.Api/          # 🌐 Presentation Layer (Controllers, API)
│   ├── Controllers/              # 12 REST API controllers
│   ├── Application/              # CQRS Commands, Queries & Handlers
│   │   ├── Commands/            # 15+ command operations
│   │   ├── Queries/             # 15+ query operations
│   │   └── Handlers/            # 30+ CQRS handlers
│   ├── Validators/              # FluentValidation rules
│   ├── Middleware/              # Error handling, logging
│   └── Extensions/              # Service configuration
├── BricklePlatform.Domain/       # 🏛️ Domain Layer (Business Logic)
│   ├── Entities/                # 9 core business entities
│   ├── DTOs/                    # Data transfer objects
│   ├── Interfaces/              # Repository and service contracts
│   ├── Enums/                   # Business enumerations
│   └── ValueObjects/            # Domain value objects
├── BricklePlatform.Infrastructure/ # 🔧 Infrastructure Layer
│   ├── Services/                # External service implementations
│   ├── Repositories/            # Data access implementations
│   ├── Persistence/             # Entity Framework configuration
│   └── Constants/               # Smart contract ABIs
└── BricklePlatform.Application/  # (Empty - logic in API layer)
```

### Layer Responsibilities

1. **API Layer**: HTTP endpoints, request/response handling, authentication
2. **Domain Layer**: Business entities, rules, and interfaces
3. **Infrastructure Layer**: External services, database, blockchain integration

## Azure Services Integration

### Core Azure Services Used

1. **Azure Blob Storage**

   - **Purpose**: Document and file storage for property documents, user profiles
   - **Implementation**: Custom repository with SAS token generation
   - **Features**: Hierarchical folder structure, automatic cleanup, secure access
   - **Configuration**: Connection string and container name in settings

2. **Azure Table Storage**

   - **Purpose**: NoSQL storage for activity logs and payment history
   - **Tables**: `UserActivityLogs`, `PaymentLogs`
   - **Features**: High-performance logging, automatic table creation
   - **Retention**: Configurable data retention policies

3. **Azure Application Insights**

   - **Purpose**: Application monitoring, telemetry, and diagnostics
   - **Features**: Request tracking, dependency monitoring, custom metrics
   - **Integration**: Kubernetes enrichment for container environments
   - **Logging**: Structured logging with correlation IDs

4. **Azure SQL Database** (Inferred)
   - **Purpose**: Primary relational database for core business data
   - **Entities**: Users, Companies, Leasings, Campaigns, Investments
   - **Features**: Entity Framework migrations, ACID transactions

### Azure Configuration Structure

```json
{
  "AzureSettings": {
    "ConnectionString": "Azure Storage connection string",
    "BlobName": "Container name for file storage",
    "LogsTableName": "Payment logs table",
    "UserActivityLogsTableName": "User activity tracking table"
  }
}
```

## Data Flow Architecture

### 1. Investment Flow

```
User Request → API Controller → Command Handler → Domain Service → Repository → Database
                    ↓
Blockchain Service ← Web3 Integration ← Smart Contract ← Tokenization
                    ↓
Notification Service → Email/Push → User Confirmation
```

### 2. File Management Flow

```
File Upload → API Endpoint → Validation → Azure Blob Storage → URL Generation → Database Update
```

### 3. Campaign Processing Flow

```
Campaign Creation → Validation → Database Storage → Blockchain Deployment → Token Minting → Investor Notification
```

### 4. Rent Distribution Flow

```
Rent Collection → Smart Contract → Blockchain Calculation → Distribution → User Wallets → Activity Logging
```

## Security Implementation

### Authentication & Authorization

- **API Key Authentication**: Custom filter with memory caching (20-minute cache)
- **Header Validation**: Required headers for all requests (`correlationId`, `user`, `source`, `requestDate`)
- **Environment-specific Security**: Different validation levels for development/production
- **Key Management**: Database-stored API keys with active/inactive status

### Data Protection

- **Encrypted Configuration**: User secrets for sensitive data
- **Secure File Storage**: Azure Blob with SAS tokens and time-limited access
- **Database Security**: Entity Framework with parameterized queries
- **CORS Configuration**: Controlled cross-origin access

### Blockchain Security

- **Private Key Management**: Secure wallet creation and transaction signing
- **Smart Contract Validation**: ABI-based contract interaction
- **Gas Estimation**: Automatic gas calculation for transaction safety
- **Network Configuration**: Multi-network support with fallback mechanisms

### Error Handling & Logging

- **Correlation ID Tracking**: Request tracing across all services
- **Structured Logging**: Comprehensive application insights integration
- **Error Boundaries**: Global exception handling middleware
- **Security Event Logging**: API key validation failures and unauthorized access attempts

## Communication Services

### Email Notifications (Resend Integration)

**Service**: Professional email service with HTML templates

**Email Types**:

- **Recharge Notifications**: Admin alerts for user deposit requests
- **Withdrawal Notifications**: Admin alerts for withdrawal processing
- **Transaction Confirmations**: User confirmations for successful operations
- **Campaign Updates**: Investment opportunity notifications
- **Leasing Activation**: Property activation and rental income alerts

**Features**:

- Professional HTML email templates with Brickle branding
- Dynamic content injection (user data, amounts, timestamps)
- Admin and user-facing communications
- Error handling and retry logic
- Logging and monitoring integration

### Push Notifications (Expo)

**Service**: Real-time mobile push notifications

**Notification Types**:

- Transaction confirmations (deposits, withdrawals)
- Investment updates and campaign status
- Rental income distribution alerts
- Platform announcements and updates

**Features**:

- iOS and Android support through Expo
- Rich notification payloads with custom data
- User token management and targeting
- Delivery tracking and error handling

## Configuration Management

Configuration is managed through environment-specific JSON files:

- `appsettings.json`: Base configuration and defaults
- `appsettings.Development.json`: Development environment settings
- `appsettings.Production.json`: Production configuration
- `appsettings.Test.json`: Testing environment settings

### Key Configuration Areas

- **Database Connections**: Entity Framework connection strings
- **Azure Services**: Storage accounts and service endpoints
- **Web3 Settings**: Blockchain RPC URLs and network configuration
- **Email Settings**: Resend API configuration and templates
- **Logging Levels**: Application Insights and console logging
- **Security Settings**: API key configuration and CORS policies

### Variables de entorno sensibles (recomendado)

Los secretos no deben guardarse en `appsettings.*.json`. Use variables de entorno (o User Secrets en desarrollo):

| Variable | Descripción |
|----------|-------------|
| `InfrastructureSettings__Web3Settings__WalletPrivateKey` | Clave privada de la **wallet de operaciones** en el API: firma txs on-chain (campañas, cierre residual, etc.) y **paga gas**. En `finalize-residual`, el **LeasingCore** acumula residual + incentivo final a inversores (según versión desplegada). **Obligatoria** donde haya escritura en blockchain. |
| `InfrastructureSettings__EmailSettings__LogoImageUrl` | URL HTTPS completa de lectura del PNG oficial usado en los emails. Como el blob de marca es privado, producción debe suministrar externamente una URL SAS de solo lectura mediante su configuración segura de runtime. |

El valor vacío conservado en `appsettings.Production.json` no configura producción. El entorno de despliegue debe definir exactamente `InfrastructureSettings__EmailSettings__LogoImageUrl`; no guarde el SAS en archivos versionados.

En .NET, el doble guión bajo `__` se interpreta como jerarquía de configuración. Ejemplo en Linux/macOS:

```bash
export InfrastructureSettings__Web3Settings__WalletPrivateKey="0x..."
```

## Next Steps & Roadmap

### Immediate Enhancements

- **Mobile Application**: Native iOS and Android applications for investor access
- **Secondary Market**: Peer-to-peer trading of tokenized real estate shares
- **Advanced Analytics**: Enhanced portfolio analytics with predictive modeling
- **Multi-language Support**: Platform localization for international markets
- **Enhanced KYC/AML**: Advanced identity verification and compliance tools

### Platform Scaling

- **Multi-chain Support**: Additional blockchain networks (Ethereum, BSC, Avalanche)
- **Institutional Features**: Advanced tools for institutional investors
- **API Rate Limiting**: Enhanced API protection and usage management
- **Microservices Migration**: Service decomposition for improved scalability
- **Advanced Caching**: Redis integration for improved performance

### Business Expansion

- **International Markets**: Multi-currency and regulatory compliance
- **Property Types Expansion**: Commercial, industrial, and specialized real estate
- **DeFi Integration**: Yield farming and liquidity mining opportunities
- **Partnership Integrations**: Real estate brokers and property management companies
- **Regulatory Compliance**: Enhanced compliance frameworks for multiple jurisdictions

### Technology Improvements

- **Real-time Updates**: WebSocket integration for live updates
- **Enhanced Security**: Multi-factor authentication and hardware wallet support
- **Performance Optimization**: Database indexing and query optimization
- **Monitoring Enhancement**: Advanced APM and alerting systems
- **CI/CD Pipeline**: Automated testing and deployment workflows

## Development Principles

1. **Separation of Concerns**: Each layer has specific responsibilities
2. **Dependency Inversion**: Higher layers don't depend on lower layers
3. **SOLID Principles**: Applied throughout the architecture design
4. **DRY (Don't Repeat Yourself)**: Code reusability and maintainability
5. **KISS (Keep It Simple, Stupid)**: Maintain simplicity in design
6. **YAGNI (You Aren't Gonna Need It)**: Avoid unnecessary feature implementation

## Contribution Guidelines

1. Follow established folder structure and naming conventions
2. Implement comprehensive unit tests for new functionality
3. Document all endpoints in Swagger with detailed descriptions
4. Follow C# coding standards and best practices
5. Validate all input models using FluentValidation
6. Implement proper exception handling with correlation IDs
7. Add structured logging with appropriate log levels
8. Update this README when adding new features or services

## Contact & Support

For questions, suggestions, or technical support regarding the Brickle API architecture and implementation, please contact the development team.

---

**Brickle API** - Revolutionizing Real Estate Investment through Blockchain Technology
