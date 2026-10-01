# Bitácora

Registro de lo realizado sobre la integración AX ↔ PrestaShop.

> Contexto de infraestructura (Apache del ambiente de pruebas, endpoints PHP en el
> servidor) está documentado aparte en `D:\code\DucasseInfra\BCH pagos\`.

---


# Sesión 2026-10-01 — `prestashop-net.dll` 2.0.0: DLL nueva para PrestaShop 9.1 (DD-073 / dap-004)

Diseño aprobado por Iván (30 decisiones) en `D:\code\DapDucasse\ps9.1\dll_ax_dap004\DISEÑO.md`. Trabajo en la rama **`ps91`**, sin push.

- **`master`:** commit `ddd94bc` con el hardening del 12-ago que estaba sin commitear (HTTPS forzado, propagación del error
  en `GetOrdenes`, versión 1.46, bitácora). Se descartó el ruido (`Test.csproj` StartupObject, caché de `obj/`, `OrdenDAL.cs.rej`).
- **`ps91`:** commit `aaf948f` con `Prestashop.Net`, `Prestashop.Net.Tests` y `Prestashop.Net.Console`; se eliminó `Test`.
- **Decisión de implementación:** sin Newtonsoft.Json (vulnerabilidad GHSA-5crp-9r3c-p9vr en la 12.0.3 y conflicto de versiones
  en la carpeta de AX). JSON propio de ~250 líneas con límite de anidamiento, cubierto por tests.
- **Pruebas:** 48/48 NUnit; E2E contra `v3.dapducasse.cl` con más de 80 llamadas (las 10 operaciones y sus errores). 50 órdenes
  con 80 envíos y 134 líneas en 27–30 ms de servidor y 0,9 s totales, sin paralelizar. Evidencia en
  `D:\code\DapDucasse\ps9.1\dll_ax_dap004\pruebas\`.
- **Entrega:** `D:\code\DapDucasse\ps9.1\dll_ax_dap004\entrega-prestashop-net-v2.0\` (+ zip): DLL, consola, instructivo y `ejemplo.xpp`.
- **Ojo:** al compilar la solución completa se regeneraron los binarios de la 1.46 en `bin/Release`; se restauraron desde el
  paquete `ps1.7/entrega-dll-ax/` (mismo SHA-256 que el del 15-ago). Compilar solo los proyectos nuevos (ver CLAUDE.md).
- **No probado:** carga dentro de AX y ejecución sobre CLR 2 (este PC solo tiene .NET 4.8). Lo verifica Iván Jerez.
- **Pendiente en este repo:** las claves que estaban en `Test/Program.cs` siguen en el historial de git; conviene rotarlas.


# Sesión 2026-08-20 — Consulta de Iván Jerez tras instalar la v1.46: "el código de autorización de Webpay es muy chico"

**Veredicto: falsa alarma. El dato es correcto y la DLL no lo transforma.**

AX mostraba `Webpay-022873` (el prefijo `Webpay-` lo concatena el X++). Verificado contra la
BD de producción: la transacción de la orden **15913** tiene `authorizationCode = 022873`
**exacto**. El mapeo es correcto de punta a punta.

**Por qué "parece" chico:** el código trae **cero a la izquierda** (`022873`), y leído como
número da 22.873 — cinco cifras. Pero es un **código de 6 caracteres**, no un número. El
**38% de los códigos históricos empieza con cero** (5.439 de 14.256). Ejemplos del mismo día:
`617165`, `073572`, `583173`, `895549`, `007794`, `493417` — todos de 6.

**El formato de 6 dígitos es constante desde hace 5 años:**

| Largo | Transacciones | Rango |
|---|---|---|
| 6 | **14.219** | 2021-02-05 → 2026-08-20 |
| 4 | 37 | 2021-01-06 → 2021-02-05 (primeras semanas del módulo) |

**Descartado como regresión de la v1.46 por comparación de código:**
`AuthorizationCode = tbkInfo.authorizationCode` es **idéntico** en la versión previa
(`git show 3254594~1`) y en la actual; el tipo sigue siendo `string` en `TransbankInfo` →
`PaymentInfo`; y `Schema/PS/WPPayment.cs` **no cambió** en el refactor (diff vacío). La DLL
pasa el valor tal cual llega de `WS/webpayinfo.php`.

⚠️ **Punto de atención para el X++:** si el valor se asigna en algún tramo a un campo
**numérico**, `022873` se convertiría en `22873` y perdería el cero — ahí sí habría pérdida de
dato. En la captura llega bien; conviene que se mantenga como **string** en todo el flujo
(incluidos reportes y conciliación).

*Probable origen de la impresión:* el `buy_order` de la misma transacción es `Order:562216`,
bastante más largo. Si antes ese campo de AX mostraba el buy_order o el token, el cambio a un
código de 6 dígitos se percibe como "más chico".

---

# Sesión 2026-08-15 — Release v1.46: paquete de entrega para AX (soporte Banco de Chile)

**Gatillo:** Banchile Pagos **certificó** la integración el 2026-08-14 y entregó las llaves
productivas. Antes de habilitar el botón en producción hay que actualizar la DLL en AX
(coordinación con **Iván Jerez**).

## Versión

`AssemblyVersion`/`AssemblyFileVersion` **1.41.0.0 → 1.46.0.0**. El AssemblyInfo estaba
desfasado: el último paquete distribuido fue `IntegracionDLLv1.45.rar` (2023) pero el
assembly nunca se bumpeó. Se salta a 1.46 para que la entrega sea inequívocamente posterior
a ambas y para que un binding con *strong name* no resuelva a la vieja.

## Compilación

.NET Framework 3.5, Release, **limpia** (solo los 2 warnings preexistentes: `Parser.cs`
CS0168 y `StateResponse.cs` CS0649).

⚠️ **Gotcha de build:** el proyecto usa **interpolación de strings** (`$"..."`, C# 6+), así
que el MSBuild de `C:\Windows\Microsoft.NET\Framework64\v4.0.30319` **NO sirve** (compilador
pre-Roslyn → `error CS1056: Unexpected character '$'`). Hay que usar el MSBuild del .NET SDK
(`dotnet msbuild`). Como no hay Developer Pack de .NET 3.5 instalado, se resolvió sin tocar
el sistema, con los reference assemblies del paquete NuGet:

```bash
# 1) bajar y extraer Microsoft.NETFramework.ReferenceAssemblies.net35 (1.0.3)
# 2) compilar apuntando TargetFrameworkRootPath a build/ del paquete
dotnet msbuild "IntegraciónPrestashopLib/IntegracionPrestashopLib.csproj" \
  /t:Rebuild /p:Configuration=Release \
  /p:TargetFrameworkRootPath=<pkg>/build/
```

## Entregable

`D:\code\DucasseInfra\BCH pagos\IntegracionPrestashopLib-v1.46-BancoDeChile.zip` (464 KB):
DLL + `.dll.config` (binding redirect de Newtonsoft) + `.pdb` + las 3 dependencias
(Newtonsoft.Json 12.0.3, Newtonsoft.Json.Schema 1.0.11, System.Threading.Tasks.NET35) +
**README.md** con el instructivo (qué cambió, contrato, instalación, verificación, rollback).
Fuente sin comprimir en `BCH pagos/entrega-dll-v1.46/`.

## Qué incluye respecto de lo que hoy corre en AX

Los commits `3254594` + `431b4a8` + `90d8da0` (soporte BCH y refactor) **y** el hardening de
la sesión 2026-08-12 (`NormalizarUrlBase` + error no silencioso), que seguía **sin desplegar**.

## Punto de atención para el X++ (comunicado a Iván Jerez)

`TransbankInfo` → **`PaymentInfo`**. `OrdenDeVenta.TransactionInfo` conserva nombre y todos
sus campos previos; solo rompe si el X++ **declara el tipo explícitamente**.

## Hallazgo bloqueante para producción (lado servidor)

**`WS/banchileinfo.php` responde 404 en `dapducasse.cl`** (verificado; en `bch.` responde 200).
Es un archivo custom fuera del módulo, así que el despliegue del plugin **no lo instala solo**.
Sin él, las órdenes Banchile llegarán a AX con `MedioPago` seteado pero el resto del pago vacío.
Sumar a `paso_a_prod.md`.

## Regresión de la v1.46 contra Stock Insumos (producción, solo lectura)

Se validó la **retrocompatibilidad** en un sitio **sin** Banco de Chile. Harness aislado
(`GetOrdenes` únicamente); **no** se usó el `Test/Program.cs` del repo porque su `Main`
contiene llamadas a `ProductoDAL.UpdateStock` — escritura contra producción.

| # | Caso | Resultado |
|---|---|---|
| A | HTTPS, ventana 05–08 ago | 3 órdenes completas (451 `ENYTDBMMI`, 452 `QBEIBMELG`, 453 `JJLSZUYIG`), `MedioPago="Webpay Plus"`, authcode y montos OK |
| B | **HTTP**, misma ventana | **Idéntico a A** → `NormalizarUrlBase` cumple (antes: 0 órdenes en silencio) |
| A vs B | Comparación campo a campo | Mismos id/total/nº líneas |
| C | Ventana sin compras | 0 órdenes, `ErrorCode=0` (sin falso positivo) |
| D | API key inválida | `ErrorCode=1` + `(401) Unauthorized` (ya no silencioso) |

**0 llamadas** a `banchileinfo.php` (solo se invoca con `PaymMode=="Banchile Pagos"`) → que
ese endpoint no exista en stockinsumos es irrelevante. **0 excepciones**. `TransactionInfo`
nunca nulo.

**El cambio de filtro `[3]` → `[2|3]` no altera nada en Stock Insumos:** el estado 2
("Payment accepted") existe pero tiene **0 órdenes en todo el histórico** (medido en la BD
`ps_stock`: estado 3 = 95, 5 = 320, 6 = 23, 4 = 13, 24 = 3, 8 = 1). Único medio de pago:
`Webpay Plus` (84 órdenes/12 meses). **Verificar lo mismo en dapducasse.cl** antes de asumir
igual comportamiento — ahí sí habrá órdenes en estado 2 por el flujo de Banchile.

Harness y log en el scratchpad de la sesión (`scratchpad/regresion/`).

## Pendientes menores detectados (no tocados)

- `IntegraciónPrestashopLib/Operations/OrdenDAL.cs.rej` — hunk rechazado de un refactor
  cosmético (tipar `var prods` + `regla.id_item.ToString()`). El código compila; es basura
  de un patch fallido, se puede borrar.
- `WsUrl()` hace `baseUrl.Replace("api/", ...)`: **exige que la URL base termine en `/api/`**.
  Si no, la consulta del pago apunta a una URL inválida. Documentado en el README como
  requisito de configuración; endurecerlo queda como mejora futura.
- `ResponseCode` sale de `int.TryParse(payment.Reason)`: un `reason` no numérico (en julio el
  sandbox devolvía `"?2"`) cae a **0**, que en convención Transbank significa aprobado. Hoy es
  inocuo porque `banchileinfo.php` filtra `status = 1` (solo pagos aprobados), pero conviene
  mapearlo explícito si algún día el endpoint devuelve rechazos.

**Sin commit** — los cambios (incluido el bump de versión) quedan en el working tree de `master`.

---

# Sesión 2026-08-12 — Stock Insumos: la DLL dejó de bajar órdenes (falla silenciosa)

> Instancia afectada: **stockinsumos.cl** (misma base de código que dapducasse.cl).
> Seguimiento del incidente: `D:\code\StockInsumos\tasks\pendientes.md` → **SI-009**.

## Síntoma

AX dejó de recibir órdenes **sin reportar ningún error**. Las compras del 6 y 7 de
agosto existían y estaban correctas en PrestaShop: órdenes **452** (`QBEIBMELG`,
$11.210) y **453** (`JJLSZUYIG`, $53.627), ambas en estado 3 y pagadas con Webpay.

## Causa raíz: URL base con `http://`

La URL del webservice configurada en AX usaba `http://`. La cadena de fallo:

1. **Cloudflare** responde `301` hacia HTTPS (force-SSL).
2. Al seguir el redirect, `WebClient`/`HttpWebRequest` de .NET **no reenvían el
   header `Authorization`** → PrestaShop responde **401**.
3. El `catch` de `PSOrdenClient.GetOrdenes` se tragaba la excepción y devolvía una
   lista vacía.
4. AX recibía **0 órdenes con `ErrorCode = 0`** — indistinguible de "no hubo ventas".

**Disparador identificado:** el **2026-08-06**, en la auditoría de seguridad de
stockinsumos.cl posterior al incidente de dapducasse, se cambió en Cloudflare
`always_use_https` **off → on**. Hasta entonces la URL `http://` configurada en AX
funcionaba tal cual. Encaja exacto con los síntomas: la orden 451 (05-08) llegó a AX,
la 452 (06-08) ya no.

⚠️ **Aplica igual a dapducasse.cl y a cualquier otra instancia**: si la URL base
configurada en AX apunta por `http://`, el día que se fuerce HTTPS en Cloudflare la
integración se cae en silencio. Conviene verificar el esquema en todas las instancias.

**Descartado por medición** (no re-investigar): el filtro `filter[current_state]=[2|3]`
es correcto; TLS está bien (el sitio exige 1.2+ y la DLL lo fuerza explícitamente);
no hay bloqueo de WAF por User-Agent; y existen productos, clientes, direcciones,
`states` y el endpoint `WS/webpayinfo.php` de las tres órdenes.

## Solución

En AX se cambió la URL a `https://stockinsumos.cl/api/` → **resuelto y confirmado**.

## Hardening aplicado en la DLL

1. **Normalización de la URL base** — nuevo `Functions.NormalizarUrlBase()`, aplicado
   en los constructores de `OrdenDAL` y **`ProductoDAL`** (la subida de productos y
   stock arrastraba el mismo defecto latente). Una base en `http://` ya no rompe la
   integración.
2. **El error deja de ser silencioso** — `PS.Responses.OrdenesResponse` gana
   `HuboError`/`MensajeError` (ambos `[XmlIgnore]`/`[JsonIgnore]`), y `OrdenDAL.GetOrdenes`
   corta devolviendo `ErrorCode = PRESTASHOP_DISCONNECTED` con el mensaje en vez de una
   lista vacía. Esto **no** contradice la decisión de no envolver cada orden en
   try/catch (A3 de la sesión anterior): acá no se agrega un catch, se propaga uno que
   ya existía y ocultaba la falla.

**Archivos tocados** (5, +58 líneas): `Bussiness/Util/Functions.cs`,
`Bussiness/Clientes/PSOrdenClient.cs`, `Operations/OrdenDAL.cs`,
`Operations/ProductoDAL.cs`, `Schema/PS/Responses/OrdenesResponse.cs`.
**Sin commit ni push** — quedan en el working tree de `master`.

## Verificación

Compilación limpia (solo los 2 warnings preexistentes: `Parser.cs` CS0168 y
`StateResponse.cs` CS0649). DLL ejecutada contra el sitio productivo:

| Escenario | Antes | Ahora |
|---|---|---|
| `http://` + ventana 6–8 ago | 0 órdenes, `ErrorCode=0` | 2 órdenes completas |
| `https://` + ventana 6–8 ago | 2 órdenes | 2 órdenes |
| API key inválida | 0 órdenes, `ErrorCode=0` | `ErrorCode=1` + mensaje |
| Ventana sin compras | 0 órdenes, `ErrorCode=0` | 0 órdenes, `ErrorCode=0` |

Las dos últimas filas son el punto: "no se pudo consultar" y "no hubo ventas" ya son
distinguibles para AX.

**Falta recompilar y desplegar la DLL en AX** para que este hardening quede activo;
hoy la integración funciona por la URL corregida, no por el fix.

## Nota

`WS/banchileinfo.php` **no existe en stockinsumos.cl** (404). Solo importa si se
habilita ahí el botón de pago Banco de Chile — habría que desplegar el endpoint,
igual que se hizo en el ambiente BCH de DAP.

---

# Sesión anterior — Banco de Chile (Banchile Pagos) + refactor de calidad

Enfocada en habilitar el medio de pago **Banco de Chile** y en mejorar la calidad del
código de la DLL.

---

## 1. Filtro de órdenes: incluir "Pago aceptado"

- **Antes:** solo se bajaban órdenes en estado **3 (En preparación)**.
- **Ahora:** se incluyen también las de estado **2 (Pago aceptado)**.
- Cambio en `PSOrdenClient.cs`: filtro de la webservice a
  `filter[current_state]=[2|3]` (sintaxis OR/IN de PrestaShop 1.7; `[2,3]` sería
  un rango BETWEEN y **no** es lo que queremos).

## 2. Integración del pago Banco de Chile (Banchile Pagos)

Replicamos la mecánica que ya existía para Webpay, pero para el módulo
`banchilechile` (tabla `ps_payment_banchilechile`).

- **Endpoint PHP** `WS/banchileinfo.php` (creado en el servidor): análogo a
  `webpayinfo.php`. Consulta el último pago aprobado de la orden
  (`WHERE id_order = ? AND status = 1 ORDER BY id_payment DESC LIMIT 1`, donde
  `id_order` en esa tabla es en realidad el `cart_id`), valida la **firma**
  `(order_id + 3) * 29 + (cart_id) * 17` y devuelve un JSON plano. Lee
  `app/config/parameters.php`, así que se adapta solo a la BD de producción.
- **Modelo de deserialización** `Schema/PS/BanchilePayment.cs` (creado): propiedades
  con `[JsonProperty]` en snake_case que mapean el JSON del PHP.
- **Cliente** `PSOrdenClient.GetBanchilePayment` (creado): pide el detalle al endpoint.
- **Enriquecimiento** en `OrdenDAL`: cuando la orden pagó con Banchile se rellena el
  modelo de pago desde `banchileinfo.php` (método `EnriquecerPagoBancoChile`),
  espejo del flujo Webpay (`EnriquecerPagoWebpay`).

## 3. Modelo de pago unificado: `TransbankInfo` → `PaymentInfo`

- Se **renombró** `Schema/ERP/TransbankInfo.cs` → `PaymentInfo.cs`.
- Se **fusionaron** los campos de Webpay y Banco de Chile en una sola clase, sin
  duplicar campos equivalentes. Se **prefieren los nombres de Transbank** como
  canónicos (Token, Amount, AuthorizationCode, BuyOrder, …) para simplificar la
  adopción del lado de AX.
- Se agregó el campo **`MedioPago`** con valor `"Webpay Plus"` o `"Banchile Pagos"`.
- Campos exclusivos de Banchile que no tienen equivalente Transbank se sumaron
  (IdPayment, CartId, Franchise, FranchiseName, Bank, Receipt, Conversion,
  IpAddress, PayerEmail, RequestId, Reference, …).
- En `OrdenDeVenta` la propiedad se mantiene como **`TransactionInfo`** (mismo nombre)
  para no romper el contrato con AX.
- Se eliminó la clase intermedia `BancoChilePaymentInfo` (ya no se necesita).

## 4. Asincronía con espera de todas las tareas

- El enriquecimiento de cada orden corre en paralelo (`Task.Run`), pero ahora se
  **espera a que todas las tareas terminen** (`Task.WaitAll`) antes de retornar la
  lista de órdenes.
- De paso se corrigió un bug latente: el `Task` de pago Webpay **nunca se await-eaba**.

## 5. Refactor de calidad

Aprobado por categorías. **A3 fue rechazado a propósito** (no envolver cada orden en
try/catch: si algo falla, que falle para detectarlo en producción).

- **A1 — correctness/concurrencia:** nueva clase `Bussiness/Util/Traza.cs`
  **thread-safe** (StringBuilder + lock). Reemplaza el `StringBuilder` que se
  compartía entre tareas paralelas (race condition real). `Traza.Log()` envuelve
  `Console.WriteLine` + acumulación de la traza en un solo método.
  Migrados: `PSRecursoClient`, `PSOrdenClient`, `OrdenDAL`, `ProductoDAL`
  (`ref StringBuilder sbTraza` → `Traza sbTraza`).
- **A2 — correctness/locale:** `Functions.ParseDecimal` / `Functions.ToInvariant`
  (cultura invariante). Se eliminaron los `Decimal.Parse(x.Replace(".", ","))` y los
  `.Replace(",", ".")` que dependían del locale de la máquina.
- **B — legibilidad:** se descompuso el método gigante `OrdenDAL.GetOrdenes`. Cada
  bloque de enriquecimiento es ahora un método con nombre: `EnriquecerProductos`,
  `EnriquecerEstado`, `EnriquecerCliente`, `EnriquecerMoneda`,
  `EnriquecerDireccionFactura`, `EnriquecerComentarios`, `EnriquecerPago`
  (→ `EnriquecerPagoWebpay` / `EnriquecerPagoBancoChile`), `AplicarDescuentos`
  (→ `AplicarRegla`) y `DefinirFacturaOBoleta`. `GetOrdenes` queda como orquestación
  (Task.Run + Task.WaitAll). Se eliminó una lista `tasks` muerta.
- **C — duplicación:** helpers genéricos: `Functions.BuildQueryUrl` (armado de query
  string, estaba 4×), `PSOrdenClient.BuildAuthHeader` (estaba 2×), y en `OrdenDAL`
  `const IVA = 1.19m`, `CalcularFirmaPago(orderId, cartId)` y `WsUrl(phpFile)`.
- **D — tipos explícitos:** se eliminó `var` a favor de tipos/clases concretas en
  `OrdenDAL` y `ProductoDAL`.
- **E — nombres:** se corrigieron nombres de **miembros privados** (`OpenPackDetail`,
  `GetItemSku`, `GetItemPrice`, `psEstado`, `psDelivery`). **No** se tocó
  `OrdenDeVenta` ni sus hijos (contrato con AX intacto).
- **F — logging:** `Console.WriteLine` y la traza quedaron unificados en
  `Traza.Log()`. **No** se borraron campos sin uso (decisión explícita).

### Pendiente menor

- `EnriquecerDireccionDespacho` sigue **inline** dentro de `GetOrdenes` (bloque
  `tDeliveryAddress`): contiene código comentado con tabs y se prefirió no arriesgar
  el match exacto. Se puede extraer en un ajuste posterior.

## 6. Limpieza

- Se eliminaron archivos duplicados generados por el editor (`OrdenDAL (1).cs` y
  similares).

---

## Commits de la sesión (en `master`, sin push)

| Commit | Descripción |
|--------|-------------|
| `3254594` | Integración botón de pago Banco de Chile (`banchilechile`): endpoint, modelos, enriquecimiento, `PaymentInfo` unificado. |
| `431b4a8` | Refactor de calidad: traza thread-safe, parseo invariante, helpers y tipos explícitos (A1, A2, C, D, E). |
| `90d8da0` | Refactor (B): descomponer `GetOrdenes` en métodos cortos + barrido de `var` en `ProductoDAL`. |

Compilación verificada y limpia (solo 2 warnings preexistentes: `Parser.cs` CS0168
variable `e` sin usar; `StateResponse.cs` CS0649).
