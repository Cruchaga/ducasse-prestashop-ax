# CLAUDE.md

Guía para trabajar en este repositorio. Léela antes de hacer cambios.

## Qué es este proyecto

Integración entre **Microsoft Dynamics AX** (ERP) y **PrestaShop 1.7.8.7** (eCommerce)
para Ducasse. La pieza central es una **DLL .NET Framework** que AX invoca para:

- **Bajar órdenes** desde PrestaShop hacia AX (lo más complejo: enriquece cada orden
  con productos, cliente, direcciones, estado, moneda, comentarios, descuentos e
  **información del medio de pago**).
- **Subir productos / stock / precios** desde AX hacia PrestaShop.
- **Subir tracking / despacho** de las órdenes.

El consumidor final del modelo (`OrdenDeVenta` y sus hijos) es **AX**. Por eso el
contrato de esas clases es sagrado (ver más abajo).

## Estructura de la solución

`DapDucasse.sln` contiene:

- **`IntegraciónPrestashopLib`** — la DLL principal (el grueso del trabajo).
- **`IntegracionEnviame`** — integración con el courier Envíame.
- **`Test`** — proyecto de consola para probar la DLL a mano (`Program.cs`).

### Dentro de `IntegraciónPrestashopLib`

- **`Operations/`** — orquestación de alto nivel (DAL).
  - `OrdenDAL.cs` — baja y enriquece las órdenes. Es el archivo más grande/importante.
  - `ProductoDAL.cs` — alta/actualización de productos, stock y combinaciones.
- **`Bussiness/Clientes/`** — clientes HTTP contra la webservice de PrestaShop
  (`PSOrdenClient`, `PSProductoClient`, `PSRecursoClient`).
- **`Bussiness/Util/`** — utilidades transversales:
  - `Functions.cs` — helpers (parseo invariante, `BuildQueryUrl`, serialización XML).
  - `Traza.cs` — logger **thread-safe** (envuelve `Console.WriteLine` + acumula el log).
  - `Parser.cs`, `HttpRequestFunctions.cs`.
- **`Schema/ERP/`** — modelo que se entrega a **AX** (`OrdenDeVenta`, `PaymentInfo`,
  `DetalleDeVenta`, `Direccion`, etc.). **Contrato externo — no romper.**
- **`Schema/PS/`** — modelos de deserialización de la webservice de PrestaShop
  (`BanchilePayment`, `WPPayment`, `Customer`, `Address`, `Currency`, …) y sus
  `Responses/`.

## Cómo se comunica con PrestaShop

- Webservice REST de PrestaShop bajo `https://<host>/api/` (en test:
  `https://bch.dapducasse.cl/api/`). Auth: la **API key como usuario** de Basic Auth,
  sin contraseña.
- **Endpoints PHP personalizados** bajo `https://<host>/WS/` para datos que la
  webservice estándar no expone, principalmente el **detalle del pago**:
  - `webpayinfo.php` — pagos Webpay Plus (Transbank).
  - `banchileinfo.php` — pagos Banchile (Banco de Chile).
  - Ambos validan una **firma**: `(order_id + 3) * 29 + (cart_id) * 17`.

### Sintaxis de filtros de la webservice (PrestaShop 1.7)

- `filter[campo]=[2|3]` → lista OR (IN). **Así filtramos los estados de orden.**
- `filter[campo]=[2,3]` → rango BETWEEN. (No es lo mismo, cuidado.)

Las órdenes que se procesan están en estados **2 (Pago aceptado)** y
**3 (En preparación)**.

## Restricciones / convenciones del repo

1. **No modificar `Schema/ERP/OrdenDeVenta.cs` ni sus clases hijas.** Cualquier cambio
   de firma obliga a tocar AX. Si necesitas renombrar algo, hazlo solo en miembros
   **privados** internos de la DLL.
2. **`PaymentInfo`** usa nombres de campos provenientes de Transbank como canónicos
   (Token, Amount, AuthorizationCode, …); los datos de Banchile se mapean a esos
   mismos campos. El campo `MedioPago` indica `"Webpay Plus"` o `"Banchile Pagos"`.
   En `OrdenDeVenta` la propiedad se llama `TransactionInfo` (nombre conservado por
   compatibilidad con AX).
3. **No agregar `try/catch` por orden** para tragarse errores. Si algo falla, que
   falle: así nos enteramos en producción (decisión explícita del equipo).
4. **No borrar campos sin uso** de los modelos (pueden ser parte del contrato).
5. **`.NET Framework v3.5`, csproj estilo antiguo (no-SDK):** todo `.cs` nuevo
   **debe** agregarse manualmente a `<Compile Include>` del `.csproj`.
   Limitaciones del lenguaje a recordar:
   - `string.Join` no tiene overload con `IEnumerable` → usar `.ToArray()`.
   - No existe `out var` ni interpolación con C# 7 features avanzados; usar sintaxis
     clásica donde el compilador se queje.
6. **Parseo de decimales:** SIEMPRE `Functions.ParseDecimal` / `Functions.ToInvariant`
   (cultura invariante, punto decimal como entrega PrestaShop). Nunca
   `Decimal.Parse(x.Replace(".", ","))` — depende del locale de la máquina.
7. **Logging:** usar `Traza` (thread-safe). No compartir `StringBuilder` entre tareas
   paralelas. `Traza.Log()` escribe a consola y acumula a la vez.
8. **Concurrencia:** `OrdenDAL.GetOrdenes` enriquece cada orden con varias
   `Task.Run` en paralelo y luego `Task.WaitAll`. Cualquier estado compartido debe
   ser thread-safe.

## Build

Abrir `DapDucasse.sln` en Visual Studio y compilar (Framework 3.5). El proyecto `Test`
sirve para correr la integración manualmente.

## Git

- Trabajamos en `master`. **No hacer push** — el usuario commitea/pushea.
- Excluir de los commits los archivos ruido habituales: `obj/`, `*.DotSettings.user`,
  `*.rej`, y cambios no relacionados (p.ej. `Test.csproj`, `Operaciones.cs`) salvo que
  se pidan explícitamente.
- Cerrar los mensajes de commit con:
  `Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>`

## Documentación relacionada (fuera de este repo)

- `D:\code\DucasseInfra\BCH pagos\` — bitácora del ambiente de pruebas BCH, runbook y
  `paso_a_prod.md` (cómo desplegar el botón de pago Banchile a producción).
