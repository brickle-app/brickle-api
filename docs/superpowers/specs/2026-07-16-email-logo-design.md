# Logo oficial de Brickle en emails

## Objetivo

Usar en todos los correos enviados por la API el logo oficial suministrado, con el wordmark azul y naranja y el lema "Donde crecer es más fácil". La imagen debe conservar la calidad del SVG original y no debe subirse a Azure hasta ser validada local y visualmente.

## Alcance

- Conservar el SVG suministrado como fuente del recurso.
- Generar un PNG compatible con clientes de correo.
- Validar el PNG antes de realizar cualquier operación sobre Azure Blob Storage.
- Mostrar una vista previa del PNG dentro del encabezado real de email y solicitar aprobación del usuario.
- Tras la aprobación, subir el PNG al contenedor Azure configurado y actualizar la URL usada por las plantillas.
- Aplicar el encabezado compartido a los nueve tipos de correo existentes en `EmailService`.

No se crearán endpoints administrativos, cargas automáticas al iniciar la API ni cambios generales al diseño de los emails.

## Recurso gráfico

El SVG original tiene una relación de aspecto de `259:82`. El PNG principal se generará a `518 x 164` píxeles, con fondo transparente y sin modificar colores, trazos ni proporciones. La salida debe ser PNG real, no un SVG renombrado ni una imagen WebP.

El repositorio conservará tanto el SVG fuente como el PNG aprobado para que el recurso pueda reproducirse y revisarse en futuros cambios.

## Flujo de validación

1. Generar el PNG localmente desde el SVG.
2. Comprobar automáticamente formato, dimensiones, relación de aspecto, canal alfa y ausencia de recorte.
3. Mostrar el PNG dentro de una vista previa del encabezado compartido del email, sobre el fondo actual de la plantilla.
4. Esperar aprobación explícita del usuario.
5. No conectar con Azure ni subir archivos antes de esa aprobación.

Si la imagen presenta pérdida de nitidez, recorte, fondo opaco o colores incorrectos, se regenera y se repite la revisión visual.

## Publicación en Azure

Después de la aprobación visual, el PNG se subirá una sola vez al contenedor definido por `InfrastructureSettings.AzureSettings.BlobName`. Se usará la ruta versionada `branding/email/brickle-logo-2026-07.png` para no sobrescribir recursos existentes y permitir una reversión por configuración.

El blob se publicará con tipo de contenido `image/png`. La plantilla recibirá una URL HTTPS de lectura estable. Si el contenedor permanece privado, se usará el mecanismo SAS de lectura existente en el proyecto.

## Integración con emails

`BuildBrandHeaderRow` seguirá siendo el único punto de construcción del encabezado. Se eliminará el wordmark verde y morado generado con HTML y la referencia predeterminada al WebP anterior. Todos los generadores de email continuarán usando el mismo layout compartido.

La URL seguirá siendo configurable mediante `EmailSettings.LogoImageUrl`. Una URL inválida o no compatible no debe recuperar la identidad anterior; el encabezado mostrará únicamente el texto alternativo `Brickle` como degradación segura.

La imagen tendrá atributos explícitos de ancho y alto que respeten `259:82`, estilos compatibles con email y texto alternativo `Brickle - Donde crecer es más fácil`.

## Pruebas y aceptación

Las pruebas automatizadas verificarán:

- El PNG local tiene formato, dimensiones, transparencia y relación de aspecto correctos.
- El encabezado genera un elemento `img` para la URL PNG aprobada.
- Los atributos visuales mantienen la proporción del logo.
- Una URL inválida no introduce el WebP ni el wordmark verde/morado anterior.
- Cada método público de envío obtiene su HTML del layout compartido con el logo oficial.

La aceptación final requiere una vista previa local aprobada, una carga exitosa al blob versionado, acceso HTTPS de lectura al PNG y pruebas del proyecto satisfactorias.

## Seguridad y reversión

No se guardarán cadenas de conexión ni tokens SAS nuevos en el repositorio. La carga usará la configuración local o las credenciales Azure ya disponibles. Para revertir, se restaura `EmailSettings.LogoImageUrl` a una URL previamente aprobada; ningún blob existente se elimina ni sobrescribe.
