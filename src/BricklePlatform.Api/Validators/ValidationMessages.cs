using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Api.Validators;

public static class ValidationMessages
{
    // Mensajes generales
    public const string REQUIREDFIELD = "El campo es requerido.";

    public const string INVALIDGUID = "El campo debe ser un GUID válido.";
    public const string MAXLENGTH = "El campo debe ser máximo de 50 caracteres";
    public const string MAXLENGTHFORTEXT = "El campo debe tener una longitud máxima de 200 caracteres";
    public const string FILENAMEMAXLENGTH = "El nombre del archivo no puede exceder los 255 caracteres";

    // Mensajes específicos de usuario
    public const string INVALIDEMAILFORMAT = "El formato del email no es válido";

    public const string PASSWORDMINLENGTH = "La contraseña debe tener al menos 8 caracteres";
    public const string PASSWORDREQUIREMENTS = "La contraseña debe contener al menos una letra mayúscula, una minúscula, un número y un carácter especial";
    public const string NAMEMINLENGTH = "El nombre debe tener al menos 2 caracteres";
    public const string NAMEMAXLENGTH = "El nombre debe tener máximo 100 caracteres";
    public const string EMAILMAXLENGTH = "El email debe tener máximo 100 caracteres";
    public const string PASSWORDMAXLENGTH = "La contraseña debe tener máximo 100 caracteres";

    // Mensajes de URLs y archivos
    public const string URLMAXLENGTH = "La URL no puede tener más de 255 caracteres";

    public const string INVALIDURL = "La URL de redirección no es válida";

    // Mensajes de Wallet
    public const string WALLETADDRESSMAXLENGTH = "La dirección de wallet no puede tener más de 42 caracteres";

    public const string INVALIDWALLETADDRESS = "La dirección de wallet debe comenzar con '0x' y tener 42 caracteres en total";

    // Mensajes de sesión
    public const string CURRENTSESSIONMAXLENGTH = "La sesión actual no puede tener más de 4000 caracteres";

    // Mensajes de empresa
    public const string COMPANYNAMEREQUIRED = "El nombre de la empresa es requerido";
    public const string COMPANYNAMEMAXLENGTH = "El nombre de la empresa no puede exceder los 200 caracteres";
    public const string OPERATIONTIMEGREATERTHANZERO = "El tiempo de operación debe ser mayor a 0";
    public const string OPERATIONMEASUREREQUIRED = "La medida de operación es requerida";
    public const string OPERATIONMEASUREVALID = "La medida de operación debe ser 'monthly' o 'yearly'";
    public const string CREDITRATINGREQUIRED = "La calificación crediticia es requerida";
    public const string CREDITRATINGMAXLENGTH = "La calificación crediticia no puede exceder los 50 caracteres";
    public const string LEASINGCONTRACTMAXLENGTH = "El contrato de leasing no puede exceder los 500 caracteres";
    public const string USERIDREQUIRED = "El ID del usuario es requerido";

    // Mensajes de teléfono
    public const string INVALIDPHONENUMBER = "Debe proporcionar al menos un destinatario válido";

    public const string INVALIDDESTINATIONS = "El destinatario no puede contener solo espacios en blanco";

    // Mensajes de Leasing
    public const string LEASINGPAGENUMBERREQUIRED = "El número de página es requerido";
    public const string LEASINGPAGENUMBERGREATERTHANZERO = "El número de página debe ser mayor que cero";
    public const string LEASINGLIMITREQUIRED = "El límite de registros es requerido";
    public const string LEASINGLIMITGREATERTHANZERO = "El límite de registros debe ser mayor que cero";
    public const string LEASINGLIMITBETWEENONEANDHUNDRED = "El límite de registros debe estar entre 1 y 100";
    public const string LEASINGINVALIDCATEGORIES = "Las categorías especificadas no son válidas";
    public const string LEASINGVALIDCATEGORIES = "Las categorías válidas son: {0}";
    public const string LEASING_CONTRACTTIME_NOT_BEFORE_NOW = "La fecha de contrato no puede ser menor a la fecha actual.";

    // Mensajes de UserLeasingAgreement
    public const string USERLEASINGAGREEMENT_USERID_REQUIRED = "El ID del usuario es requerido";
    public const string USERLEASINGAGREEMENT_LEASINGID_REQUIRED = "El ID del leasing es requerido";
    public const string USERLEASINGAGREEMENT_PAYMENTTERM_REQUIRED = "El plazo de pago es requerido";
    public const string USERLEASINGAGREEMENT_PAYMENTTERM_MAXLENGTH = "El plazo de pago no puede exceder los 50 caracteres";
    public const string USERLEASINGAGREEMENT_CURRENCY_REQUIRED = "La moneda es requerida";
    public const string USERLEASINGAGREEMENT_CURRENCY_MAXLENGTH = "La moneda no puede exceder los 10 caracteres";
    public const string USERLEASINGAGREEMENT_CONTRACTDETAILS_REQUIRED = "Los detalles del contrato son requeridos";
    public const string USERLEASINGAGREEMENT_CONTRACTDETAILS_MAXLENGTH = "Los detalles del contrato no pueden exceder los 500 caracteres";
    public const string USERLEASINGAGREEMENT_STARTDATE_REQUIRED = "La fecha de inicio es requerida";
    public const string USERLEASINGAGREEMENT_STARTDATE_BEFORE_ENDDATE = "La fecha de inicio debe ser anterior a la fecha de fin";
    public const string USERLEASINGAGREEMENT_ENDDATE_REQUIRED = "La fecha de fin es requerida";
    public const string USERLEASINGAGREEMENT_ENDDATE_AFTER_STARTDATE = "La fecha de fin debe ser posterior a la fecha de inicio";
    public const string USERLEASINGAGREEMENT_INSTALLMENTAMOUNT_GREATERTHANZERO = "El monto de la cuota debe ser mayor a 0";
    public const string USERLEASINGAGREEMENT_RESIDUAL_VALUE_GREATERTHANZERO = "El monto del valor residual del activo debe ser mayor a 0";
    public const string USERLEASINGAGREEMENT_TOKENSPURCHASED_GREATERTHANZERO = "La cantidad de tokens debe ser mayor a 0";
    public const string USERLEASINGAGREEMENT_ID_REQUIRED = "El ID del acuerdo de leasing es requerido";
    public const string USERLEASINGAGREEMENT_REMAININGBALANCE_GREATER_THAN_OR_EQUAL_TO_ZERO = "El saldo restante debe ser mayor o igual a cero";
    public const string USERLEASINGAGREEMENT_ENDDATE_GREATER_THAN_NOW = "La fecha de finalización debe ser mayor a la fecha actual";
    public const string USERLEASINGAGREEMENT_STATUS_REQUIRED = "El estado es requerido";
    public const string USERLEASINGAGREEMENT_STATUS_MAXLENGTH = "El estado no puede exceder los 50 caracteres";
    public const string USERLEASINGAGREEMENT_LEASING_ADDRESS_REQUIRED = "La dirección del contrato de leasing es requerida";

    // Mensajes de Payment
    public const string PAYMENT_HEADER_REQUIRED = "La información de cabecera es requerida";
    public const string PAYMENT_CORRELATIONID_REQUIRED = "El CorrelationId es requerido";
    public const string PAYMENT_BODY_REQUIRED = "Los datos del pago son requeridos";
    public const string PAYMENT_AMOUNT_GREATERTHANZERO = "El monto del pago debe ser mayor a 0";
    public const string PAYMENT_AMOUNT_NOTNEGATIVE = "El monto del pago no puede ser negativo";
    public const string PAYMENT_USERLEASINGAGREEMENTID_REQUIRED = "El ID del acuerdo de leasing de usuario es requerido";
    public const string PAYMENT_USERLEASINGAGREEMENTID_INVALID = "El ID del acuerdo de leasing de usuario debe ser un GUID válido";
    public const string PAYMENT_AMOUNT_EXCEEDS_REMAININGBALANCE = "El monto del pago no puede ser superior al saldo restante";
    public const string PAYMENT_REMAININGBALANCE_NOTNEGATIVE = "El saldo restante no puede ser negativo";
    public const string PAYMENT_TOTALVALUE_NOTNEGATIVE = "El valor total no puede ser negativo";

    // Mensajes de Contactos
    public const string CONTACT_USERID_REQUIRED = "El ID del usuario es requerido";
    public const string CONTACT_CONTACTID_REQUIRED = "El ID del contacto es requerido";
    public const string CONTACT_SELF_ADDITION_NOT_ALLOWED = "Un usuario no puede agregarse a sí mismo como contacto";
    public const string CONTACT_SEARCH_CRITERIA_REQUIRED = "Debe proporcionar al menos un criterio de búsqueda (email o número de teléfono)";
    public const string CONTACT_SEARCH_SINGLE_CRITERIA = "Debe proporcionar solo un criterio de búsqueda: email O número de teléfono, no ambos";
    public const string CONTACT_SEARCH_SELF_USER = "No se encontraron resultados porque estás intentando buscar tu propio usuario";
    public const string CONTACT_SEARCH_INVALID_TERM = "El término de búsqueda debe ser un email válido o un número de teléfono válido";

    // Mensajes de DocumentTypeEnum
    public static string INVALIDDOCUMENTTYPE => $"El tipo de documento no es válido. Los valores permitidos son: {string.Join(", ", Enum.GetNames(typeof(DocumentTypeEnum)))}.";
}