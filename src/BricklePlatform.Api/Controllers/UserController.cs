using BricklePlatform.Api.Application.Commands.User;
using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Application.Queries.User;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BricklePlatform.Api.Application.Commands.UserDocument;
using BricklePlatform.Api.Application.Queries.UserDocument;
using BricklePlatform.Api.Application.Commands.UserDocumentSignature;
using BricklePlatform.Api.Application.Queries.UserDocumentSignature;


namespace BricklePlatform.Api.Controllers;

/// <summary>
/// Controlador responsable de la gestión de usuarios en el sistema.
/// Implementa operaciones CRUD sobre la entidad usuario.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IUserBankAccountRepository _userBankAccountRepository;
    private readonly ILogger<UserController> _logger;
    private readonly IMediator _mediator;
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de usuarios.
    /// </summary>
    /// <param name="userService">Servicio de dominio para operaciones específicas de usuario.</param>
    /// <param name="userRepository">Repositorio para acceso a datos de usuarios.</param>
    /// <param name="userBankAccountRepository">Repositorio para acceso a cuentas bancarias de usuarios.</param>
    /// <param name="logger">Logger para el registro de eventos y errores.</param>
    /// <param name="mediator">Mediador para el manejo de comandos y consultas CQRS.</param>
    /// <param name="userActivityLogService">Servicio para registro de actividades de usuario.</param>
    /// <param name="emailService">Servicio para envío de emails.</param>
    /// <param name="notificationService">Servicio para envío de notificaciones push.</param>
    public UserController(
        IUserService userService,
        IUserRepository userRepository,
        IUserBankAccountRepository userBankAccountRepository,
        ILogger<UserController> logger,
        IMediator mediator,
        IUserActivityLogService userActivityLogService,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _userService = userService;
        _userRepository = userRepository;
        _userBankAccountRepository = userBankAccountRepository;
        _logger = logger;
        _mediator = mediator;
        _userActivityLogService = userActivityLogService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="createUser">Datos del usuario a crear, incluyendo nombre, email y dirección de wallet.</param>
    /// <returns>
    /// 201 Created: Retorna el usuario creado con su ID asignado.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 409 Conflict: Si ya existe un usuario con el mismo email.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] CreateUserDto createUser)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de usuario - CorrelationId: {CorrelationId}", header.CorrelationId);

            CreateUserCommand command = new CreateUserCommand(header, createUser);
            UserDto result = await _mediator.Send(command);

            _logger.LogInformation("Usuario creado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene un usuario específico por su identificador único.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del usuario a buscar.</param>
    /// <returns>
    /// 200 OK: Retorna los datos del usuario encontrado.
    /// 404 Not Found: Si no existe un usuario con el ID proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUser(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id)
    {
        try
        {
            _logger.LogInformation("Obteniendo usuario con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            Domain.Entities.User? user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new ApplicationException($"Usuario con ID {id} no encontrado");
            }

            UserDto result = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                WalletAddress = user.WalletAddress,
                ProfilePictureUrl = user.ProfilePictureUrl,
                PhoneNumber = user.PhoneNumber,
                TermsAccepted = user.TermsAccepted,
                DateOfBirth = user.DateOfBirth,
                Nationality = user.Nationality,
                CountryOfResidence = user.CountryOfResidence,
                DocumentType = user.DocumentType,
                DocumentNumber = user.DocumentNumber,
                KycCustomerId = user.KycCustomerId,
                KycSubmissionId = user.KycSubmissionId,
                PushNotificationToken = user.PushNotificationToken,
                CurrentSession = user.CurrentSession,
                ExternalWalletId = user.ExternalWalletId,
                CreatedAt = user.CreatedAt,
                IsBasicProfileComplete = HasCompleteBasicProfile(user),
                IsFullProfileComplete = user.IsFullProfileComplete,
                IsProfileUnderReview = user.IsProfileUnderReview,
                Company = user.Company != null ? new CompanyDto
                {
                    Id = user.Company.Id,
                    Name = user.Company.Name,
                    OperationTime = user.Company.OperationTime,
                    OperationMeasure = user.Company.OperationMeasure,
                    CreditRating = user.Company.CreditRating,
                    LeasingContract = user.Company.LeasingContract,
                    UserId = user.Company.UserId,
                    CreatedAt = user.Company.CreatedAt,
                    UpdatedAt = user.Company.UpdatedAt
                } : null
            };

            _logger.LogInformation("Usuario obtenido exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene un usuario específico por su dirección de email.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="email">Dirección de email del usuario a buscar.</param>
    /// <returns>
    /// 200 OK: Retorna los datos del usuario encontrado.
    /// 404 Not Found: Si no existe un usuario con el email proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByEmail(
        [FromHeaderModel] HeaderRequestModel header,
        string email)
    {
        try
        {
            _logger.LogInformation("Obteniendo usuario con email: {Email} - CorrelationId: {CorrelationId}",
                email, header.CorrelationId);

            Domain.Entities.User? user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new ApplicationException($"Usuario con email {email} no encontrado");
            }

            UserDto result = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                WalletAddress = user.WalletAddress,
                ProfilePictureUrl = user.ProfilePictureUrl,
                PhoneNumber = user.PhoneNumber,
                TermsAccepted = user.TermsAccepted,
                DateOfBirth = user.DateOfBirth,
                Nationality = user.Nationality,
                CountryOfResidence = user.CountryOfResidence,
                DocumentType = user.DocumentType,
                DocumentNumber = user.DocumentNumber,
                KycCustomerId = user.KycCustomerId,
                KycSubmissionId = user.KycSubmissionId,
                PushNotificationToken = user.PushNotificationToken,
                CurrentSession = user.CurrentSession,
                ExternalWalletId = user.ExternalWalletId,
                CreatedAt = user.CreatedAt,
                IsBasicProfileComplete = HasCompleteBasicProfile(user),
                IsFullProfileComplete = user.IsFullProfileComplete,
                IsProfileUnderReview = user.IsProfileUnderReview,
                Company = user.Company != null ? new CompanyDto
                {
                    Id = user.Company.Id,
                    Name = user.Company.Name,
                    OperationTime = user.Company.OperationTime,
                    OperationMeasure = user.Company.OperationMeasure,
                    CreditRating = user.Company.CreditRating,
                    LeasingContract = user.Company.LeasingContract,
                    UserId = user.Company.UserId,
                    CreatedAt = user.Company.CreatedAt,
                    UpdatedAt = user.Company.UpdatedAt
                } : null
            };

            _logger.LogInformation("Usuario obtenido exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del usuario a actualizar.</param>
    /// <param name="updateUser">Datos actualizados del usuario.</param>
    /// <returns>
    /// 200 OK: Retorna los datos actualizados del usuario.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 404 Not Found: Si no existe un usuario con el ID proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id,
        [FromBody] UpdateUserDto updateUser)
    {
        try
        {
            _logger.LogInformation("Actualizando usuario con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            UpdateUserCommand command = new UpdateUserCommand(header, id, updateUser);
            UserDto result = await _mediator.Send(command);

            _logger.LogInformation("Usuario actualizado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Elimina un usuario existente del sistema.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del usuario a eliminar.</param>
    /// <returns>
    /// 204 No Content: Si el usuario fue eliminado exitosamente.
    /// 404 Not Found: Si no existe un usuario con el ID proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id)
    {
        try
        {
            _logger.LogInformation("Eliminando usuario con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            DeleteUserCommand command = new DeleteUserCommand(header, id);
            await _mediator.Send(command);

            _logger.LogInformation("Usuario eliminado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Busca usuarios existentes por email o número de teléfono usando un término de búsqueda único.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="searchTerm">Término de búsqueda que puede ser un email o número de teléfono.</param>
    /// <param name="excludeUserId">ID del usuario a excluir de la búsqueda (opcional).</param>
    /// <returns>
    /// 200 OK: Retorna la lista de usuarios encontrados.
    /// 400 Bad Request: Si el término de búsqueda es inválido.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ContactDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchUsers(
        [FromHeaderModel] HeaderRequestModel header,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? excludeUserId = null)
    {
        try
        {
            _logger.LogInformation("Buscando usuarios - SearchTerm: {SearchTerm}, ExcludeUserId: {ExcludeUserId} - CorrelationId: {CorrelationId}",
                searchTerm, excludeUserId, header.CorrelationId);

            SearchUsersQuery query = new SearchUsersQuery(header, searchTerm, excludeUserId);
            IEnumerable<ContactDto> result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("No se encontraron usuarios - SearchTerm: {SearchTerm} - CorrelationId: {CorrelationId} - Error: {Error}",
                searchTerm, header.CorrelationId, ex.Message);

            return NotFound(new NotFoundResponseDto
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuarios - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Agrega un contacto a un usuario.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del usuario que agregará el contacto.</param>
    /// <param name="addContactDto">Datos del contacto a agregar.</param>
    /// <returns>
    /// 201 Created: Retorna el contacto agregado.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 404 Not Found: Si no existe el usuario o el contacto.
    /// 409 Conflict: Si el contacto ya está agregado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPost("{id}/contacts")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ContactDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddContact(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id,
        [FromBody] AddContactDto addContactDto)
    {
        try
        {
            _logger.LogInformation("Agregando contacto {ContactId} al usuario {UserId} - CorrelationId: {CorrelationId}",
                addContactDto.ContactId, id, header.CorrelationId);

            AddContactCommand command = new AddContactCommand(header, id, addContactDto);
            ContactDto result = await _mediator.Send(command);

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar contacto - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene la lista de contactos asociados a un usuario.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del usuario.</param>
    /// <returns>
    /// 200 OK: Retorna la lista de contactos del usuario.
    /// 404 Not Found: Si no existe el usuario.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("{id}/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ContactDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserContacts(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id)
    {
        try
        {
            _logger.LogInformation("Obteniendo contactos del usuario {UserId} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            GetUserContactsQuery query = new GetUserContactsQuery(header, id);
            IEnumerable<ContactDto> result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contactos - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Crea una nueva solicitud de recarga para un usuario.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="createRecharge">Datos de la recarga a crear.</param>
    /// <returns>
    /// 201 Created: Retorna la información de la recarga creada.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 404 Not Found: Si no existe el usuario.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPost("recharge")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RechargeResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRecharge(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] CreateRechargeDto createRecharge)
    {
        try
        {
            _logger.LogInformation("Creando solicitud de recarga para usuario: {UserId}, monto: {Amount} - CorrelationId: {CorrelationId}",
                createRecharge.UserId, createRecharge.Amount, header.CorrelationId);

            // Verificar que el usuario existe
            var user = await _userRepository.GetByIdAsync(createRecharge.UserId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para recarga: {UserId} - CorrelationId: {CorrelationId}",
                    createRecharge.UserId, header.CorrelationId);
                return NotFound(new { error = $"Usuario con ID {createRecharge.UserId} no encontrado" });
            }

            // Crear el log de actividad de usuario
            var transactionData = new UserActivityLogDto
            {
                UserId = createRecharge.UserId,
                Type = "RECHARGE",
                TxAmount = createRecharge.Amount,
                Status = "PENDING",
                Receipt = createRecharge.Receipt,
                Hash = "N/A",
                Reference = createRecharge.Reference ?? "Recarga",
                Timestamp = DateTime.UtcNow
            };

            // Registrar la actividad
            await _userActivityLogService.LogUserActivityAsync(transactionData);

            // Enviar notificación por email al administrador
            try
            {
                await _emailService.SendRechargeNotificationAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    createRecharge.Amount,
                    createRecharge.Receipt,
                    user.WalletAddress ?? "N/A");
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Error enviando email de notificación de recarga - CorrelationId: {CorrelationId}",
                    header.CorrelationId);
            }

            var response = new RechargeResponseDto
            {
                Id = Guid.NewGuid(), // Este ID debería venir del registro en la base de datos
                UserId = transactionData.UserId,
                TxAmount = transactionData.TxAmount,
                Status = transactionData.Status,
                Receipt = transactionData.Receipt,
                Hash = transactionData.Hash,
                Reference = transactionData.Reference,
                Timestamp = transactionData.Timestamp
            };

            _logger.LogInformation("Solicitud de recarga creada exitosamente para usuario: {UserId} - CorrelationId: {CorrelationId}",
                createRecharge.UserId, header.CorrelationId);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear solicitud de recarga - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza el estado de una solicitud de recarga existente.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="userId">ID del usuario propietario de la recarga.</param>
    /// <param name="updateRecharge">Datos actualizados de la recarga.</param>
    /// <returns>
    /// 200 OK: Retorna la información actualizada de la recarga.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 404 Not Found: Si no existe la recarga o el usuario.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPut("recharge/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RechargeResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRecharge(
        [FromHeaderModel] HeaderRequestModel header,
        Guid userId,
        [FromBody] UpdateRechargeDto updateRecharge)
    {
        try
        {
            _logger.LogInformation("Actualizando recarga para usuario: {UserId}, estado: {Status} - CorrelationId: {CorrelationId}",
                userId, updateRecharge.Status, header.CorrelationId);

            // Verificar que el usuario existe
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para actualización de recarga: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);
                return NotFound(new { error = $"Usuario con ID {userId} no encontrado" });
            }

            // Obtener los logs de recarga del usuario (último registro PENDING)
            var recentLogs = await _userActivityLogService.GetUserActivityLogsAsync(userId, 30, null, "RECHARGE", "PENDING");
            var pendingRecharge = recentLogs.FirstOrDefault();

            if (pendingRecharge == null)
            {
                _logger.LogWarning("No se encontró recarga pendiente para el usuario: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);
                return NotFound(new { error = "No se encontró una recarga pendiente para este usuario" });
            }

            // Crear nuevo log con estado actualizado
            var updatedTransactionData = new UserActivityLogDto
            {
                UserId = userId,
                Type = "RECHARGE",
                TxAmount = pendingRecharge.TxAmount,
                Status = updateRecharge.Status,
                Receipt = pendingRecharge.Receipt,
                Hash = updateRecharge.Hash ?? "N/A",
                Reference = pendingRecharge.Reference,
                Timestamp = DateTime.UtcNow
            };

            // Registrar la actividad actualizada
            await _userActivityLogService.LogUserActivityAsync(updatedTransactionData);

            // Si la recarga fue confirmada, enviar notificaciones
            if (updateRecharge.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                // Enviar email de confirmación al usuario
                try
                {
                    await _emailService.SendRechargeConfirmationAsync(
                        user.Email,
                        $"{user.FirstName} {user.LastName}",
                        pendingRecharge.TxAmount);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Error enviando email de confirmación de recarga - CorrelationId: {CorrelationId}",
                        header.CorrelationId);
                }

                // Enviar notificación push al usuario
                if (!string.IsNullOrEmpty(user.PushNotificationToken))
                {
                    try
                    {
                        await _notificationService.SendNotificationAsync(
                            user.PushNotificationToken,
                            "Recarga Confirmada",
                            $"Tu recarga de ${pendingRecharge.TxAmount:N2} COP ha sido confirmada y ya está disponible en tu cuenta.",
                            new
                            {
                                type = "RECHARGE_CONFIRMED",
                                amount = pendingRecharge.TxAmount,
                                timestamp = DateTime.UtcNow
                            });
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogWarning(notificationEx, "Error enviando notificación push de recarga - CorrelationId: {CorrelationId}",
                            header.CorrelationId);
                    }
                }
            }

            var response = new RechargeResponseDto
            {
                Id = Guid.NewGuid(),
                UserId = updatedTransactionData.UserId,
                TxAmount = updatedTransactionData.TxAmount,
                Status = updatedTransactionData.Status,
                Receipt = updatedTransactionData.Receipt,
                Hash = updatedTransactionData.Hash,
                Reference = updatedTransactionData.Reference,
                Timestamp = updatedTransactionData.Timestamp
            };

            _logger.LogInformation("Recarga actualizada exitosamente para usuario: {UserId} - CorrelationId: {CorrelationId}",
                userId, header.CorrelationId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar recarga - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva solicitud de retiro para un usuario.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="createWithdraw">Datos del retiro a crear.</param>
    /// <returns>
    /// 201 Created: Retorna la información del retiro creado.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 404 Not Found: Si no existe el usuario.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPost("withdraw")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WithdrawResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateWithdraw(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] CreateWithdrawDto createWithdraw)
    {
        try
        {
            _logger.LogInformation("Creando solicitud de retiro para usuario: {UserId}, monto: {Amount} - CorrelationId: {CorrelationId}",
                createWithdraw.UserId, createWithdraw.Amount, header.CorrelationId);

            // Verificar que el usuario existe
            var user = await _userRepository.GetByIdAsync(createWithdraw.UserId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para retiro: {UserId} - CorrelationId: {CorrelationId}",
                    createWithdraw.UserId, header.CorrelationId);
                return NotFound(new { error = $"Usuario con ID {createWithdraw.UserId} no encontrado" });
            }

            // Crear el log de actividad de usuario
            var transactionData = new UserActivityLogDto
            {
                UserId = createWithdraw.UserId,
                Type = "WITHDRAW",
                TxAmount = createWithdraw.Amount,
                Status = "PENDING",
                Receipt = "N/A",
                Hash = "N/A",
                Reference = createWithdraw.Reference ?? "Retiro",
                Timestamp = DateTime.UtcNow
            };

            // Registrar la actividad
            await _userActivityLogService.LogUserActivityAsync(transactionData);

            // Obtener información bancaria del usuario
            string bankAccountInfo = "Información bancaria no disponible";
            try
            {
                var userBankAccounts = await _userBankAccountRepository.GetByUserIdAsync(createWithdraw.UserId);
                var primaryAccount = userBankAccounts.FirstOrDefault();
                if (primaryAccount != null)
                {
                    bankAccountInfo = $"{primaryAccount.BankName} - {primaryAccount.AccountType} - {primaryAccount.AccountNumber} - {primaryAccount.AccountHolder}";
                }
            }
            catch (Exception bankEx)
            {
                _logger.LogWarning(bankEx, "Error obteniendo información bancaria del usuario: {UserId} - CorrelationId: {CorrelationId}",
                    createWithdraw.UserId, header.CorrelationId);
            }

            // Enviar notificación por email al administrador
            try
            {
                await _emailService.SendWithdrawNotificationAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    createWithdraw.Amount,
                    bankAccountInfo,
                    "https://polygonscan.com/tx/PLACEHOLDER-TOKEN-BURN-HASH"); // TODO: Generate actual token burn link when implementing blockchain integration
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Error enviando email de notificación de retiro - CorrelationId: {CorrelationId}",
                    header.CorrelationId);
            }

            var response = new WithdrawResponseDto
            {
                Id = Guid.NewGuid(),
                UserId = transactionData.UserId,
                TxAmount = transactionData.TxAmount,
                Status = transactionData.Status,
                Hash = transactionData.Hash,
                Reference = transactionData.Reference,
                Timestamp = transactionData.Timestamp
            };

            _logger.LogInformation("Solicitud de retiro creada exitosamente para usuario: {UserId} - CorrelationId: {CorrelationId}",
                createWithdraw.UserId, header.CorrelationId);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear solicitud de retiro - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza el estado de una solicitud de retiro existente.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="userId">ID del usuario propietario del retiro.</param>
    /// <param name="updateWithdraw">Datos actualizados del retiro.</param>
    /// <returns>
    /// 200 OK: Retorna la información actualizada del retiro.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 404 Not Found: Si no existe el retiro o el usuario.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPut("withdraw/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WithdrawResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateWithdraw(
        [FromHeaderModel] HeaderRequestModel header,
        Guid userId,
        [FromBody] UpdateWithdrawDto updateWithdraw)
    {
        try
        {
            _logger.LogInformation("Actualizando retiro para usuario: {UserId}, estado: {Status} - CorrelationId: {CorrelationId}",
                userId, updateWithdraw.Status, header.CorrelationId);

            // Verificar que el usuario existe
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para actualización de retiro: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);
                return NotFound(new { error = $"Usuario con ID {userId} no encontrado" });
            }

            // Obtener los logs de retiro del usuario (último registro PENDING)
            var recentLogs = await _userActivityLogService.GetUserActivityLogsAsync(userId, 30, null, "WITHDRAW", "PENDING");
            var pendingWithdraw = recentLogs.FirstOrDefault();

            if (pendingWithdraw == null)
            {
                _logger.LogWarning("No se encontró retiro pendiente para el usuario: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);
                return NotFound(new { error = "No se encontró un retiro pendiente para este usuario" });
            }

            // Crear nuevo log con estado actualizado
            var updatedTransactionData = new UserActivityLogDto
            {
                UserId = userId,
                Type = "WITHDRAW",
                TxAmount = pendingWithdraw.TxAmount,
                Status = updateWithdraw.Status,
                Receipt = "N/A",
                Hash = updateWithdraw.Hash ?? "N/A",
                Reference = pendingWithdraw.Reference,
                Timestamp = DateTime.UtcNow
            };

            // Registrar la actividad actualizada
            await _userActivityLogService.LogUserActivityAsync(updatedTransactionData);

            // Si el retiro fue confirmado, enviar notificaciones
            if (updateWithdraw.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                // Enviar email de confirmación al usuario
                try
                {
                    await _emailService.SendWithdrawConfirmationAsync(
                        user.Email,
                        $"{user.FirstName} {user.LastName}",
                        pendingWithdraw.TxAmount);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Error enviando email de confirmación de retiro - CorrelationId: {CorrelationId}",
                        header.CorrelationId);
                }

                // Enviar notificación push al usuario
                if (!string.IsNullOrEmpty(user.PushNotificationToken))
                {
                    try
                    {
                        await _notificationService.SendNotificationAsync(
                            user.PushNotificationToken,
                            "Retiro Procesado",
                            $"Tu retiro de ${pendingWithdraw.TxAmount:N2} COP ha sido procesado y será transferido en los próximos días hábiles.",
                            new
                            {
                                type = "WITHDRAW_PROCESSED",
                                amount = pendingWithdraw.TxAmount,
                                timestamp = DateTime.UtcNow
                            });
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogWarning(notificationEx, "Error enviando notificación push de retiro - CorrelationId: {CorrelationId}",
                            header.CorrelationId);
                    }
                }
            }

            var response = new WithdrawResponseDto
            {
                Id = Guid.NewGuid(),
                UserId = updatedTransactionData.UserId,
                TxAmount = updatedTransactionData.TxAmount,
                Status = updatedTransactionData.Status,
                Hash = updatedTransactionData.Hash,
                Reference = updatedTransactionData.Reference,
                Timestamp = updatedTransactionData.Timestamp
            };

            _logger.LogInformation("Retiro actualizado exitosamente para usuario: {UserId} - CorrelationId: {CorrelationId}",
                userId, header.CorrelationId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar retiro - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Sube un documento de identidad para un usuario.
    /// </summary>
    /// <param name="header">Cabecera con CorrelationId.</param>
    /// <param name="request">Datos del documento y el archivo.</param>
    /// <returns>El documento creado.</returns>
    [HttpPost("documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDocumentDto))]
    public async Task<IActionResult> UploadDocument(
        [FromHeaderModel] HeaderRequestModel header,
        [FromForm] UploadUserDocumentRequestDto request)
    {
        try
        {
            var command = new UploadUserDocumentCommand(header, request);
            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir documento - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene los documentos subidos por un usuario.
    /// </summary>
    /// <param name="header">Cabecera con CorrelationId.</param>
    /// <param name="userId">ID del usuario.</param>
    /// <returns>Lista de documentos.</returns>
    [HttpGet("{userId}/documents")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDocumentDto>))]
    public async Task<IActionResult> GetUserDocuments(
        [FromHeaderModel] HeaderRequestModel header,
        [FromRoute] Guid userId)
    {
        try
        {
            var query = new GetUserDocumentsQuery(header, userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todos los documentos de usuario (para administración).
    /// </summary>
    /// <param name="header">Cabecera con CorrelationId.</param>
    /// <param name="status">Filtro opcional por estado.</param>
    /// <returns>Lista de documentos.</returns>
    [HttpGet("documents/all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDocumentDto>))]
    public async Task<IActionResult> GetAllDocuments(
        [FromHeaderModel] HeaderRequestModel header,
        [FromQuery] string? status = null)
    {
        try
        {
            var query = new GetAllUserDocumentsQuery(status);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los documentos - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el estado de un documento (para administración).
    /// </summary>
    /// <param name="header">Cabecera con CorrelationId.</param>
    /// <param name="id">ID del documento.</param>
    /// <param name="request">Nuevo estado y observación opcional.</param>
    /// <returns>El documento actualizado.</returns>
    [HttpPut("documents/{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDocumentDto))]
    public async Task<IActionResult> UpdateDocumentStatus(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id,
        [FromBody] UpdateUserDocumentStatusCommand request)
    {
        try
        {
            // Asegurar que el ID del comando sea el mismo que el de la ruta
            request.Id = id;
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado del documento - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Registra la firma electrónica en pantalla de un usuario sobre un documento legal
    /// (contrato de colaboración empresarial, declaración de origen de fondos, etc.)
    /// que no se sirve como PDF externo.
    /// </summary>
    /// <param name="header">Cabecera con CorrelationId.</param>
    /// <param name="request">Tipo de documento, versión, firmante y trazos de la firma.</param>
    /// <returns>La evidencia de firma registrada.</returns>
    [HttpPost("document-signatures")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDocumentSignatureDto))]
    public async Task<IActionResult> SignDocument(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] SignUserDocumentRequestDto request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var command = new SignUserDocumentCommand(header, request, ipAddress);
            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar firma de documento - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene las firmas electrónicas de documentos legales de un usuario.
    /// </summary>
    /// <param name="header">Cabecera con CorrelationId.</param>
    /// <param name="userId">ID del usuario.</param>
    /// <returns>Lista de firmas registradas.</returns>
    [HttpGet("{userId}/document-signatures")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDocumentSignatureDto>))]
    public async Task<IActionResult> GetDocumentSignatures(
        [FromHeaderModel] HeaderRequestModel header,
        [FromRoute] Guid userId)
    {
        try
        {
            var query = new GetUserDocumentSignaturesQuery(header, userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener firmas de documentos - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    private static bool HasCompleteBasicProfile(Domain.Entities.User user)
    {
        return !string.IsNullOrWhiteSpace(user.FirstName) &&
            !string.IsNullOrWhiteSpace(user.LastName) &&
            !string.IsNullOrWhiteSpace(user.PhoneNumber) &&
            user.DateOfBirth.HasValue &&
            !string.IsNullOrWhiteSpace(user.Nationality) &&
            !string.IsNullOrWhiteSpace(user.CountryOfResidence) &&
            user.DocumentType.HasValue &&
            !string.IsNullOrWhiteSpace(user.DocumentNumber);
    }
}
