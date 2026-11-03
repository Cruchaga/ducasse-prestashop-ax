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

## PrestaShopNet.dll — DLL nueva para PrestaShop 9.1 (rama `ps91`)

Desde el 2026-10-01 el repo tiene **dos DLL independientes**:

| | `IntegraciónPrestashopLib` (1.46) | `Prestashop.Net` (2.0) |
|---|---|---|
| Tienda | PS 1.7 (dapducasse.cl hasta el cutover, stockinsumos.cl) | PS 9.1 (módulo `dapintegracion`) |
| Ensamblado | `IntegracionPrestashopLib.dll` | **`PrestaShopNet.dll`** (namespace `Prestashop.Net`; hasta la 2.0.1 se llamaba `prestashop-net.dll`) |
| Dependencias | Newtonsoft.Json 12.0.3, TPL backport | **ninguna** (JSON propio en `Interno/Json.cs`) |
| Estado | congelada: solo correcciones | activa |

Proyectos nuevos: `Prestashop.Net` (la DLL), `Prestashop.Net.Tests` (NUnit 2.6.4, 61 tests, servidor HTTP falso en
`ServidorFalso.cs`) y `Prestashop.Net.Console` (escenarios; reemplaza al antiguo `Test`, que se eliminó por tener claves de
producción en el código). Diseño, pruebas y paquete de entrega: `D:\code\DapDucasse\ps9.1\dll_ax_dap004\`.

Reglas propias de `Prestashop.Net`:
- **Contrato con AX:** todo lo público (`Cliente`, `Configuracion`, `Respuesta*`, `Lote*`, `*V2`, `Errores`, `Estados`). Solo se
  **agrega**; nunca se renombra ni se quita. Lo demás va en `Interno/` como `internal`.
- **X++ no ve campos ni genéricos:** exponer **propiedades** y listas como `XxxCount` + `GetXxx(i)`; lotes con `Agregar(...)`.
- **Ningún método público lanza:** todo pasa por `Cliente.Ejecutar`, que convierte cualquier excepción en `ErrorCode 900`.
- **Catálogo de errores** (`Errores.cs`) idéntico al de `src/Errores.php` del módulo: un código nuevo se agrega en los dos lados.
- **Firma** (`Interno/Firma.cs`) idéntica a `src/Service/Firma.php`: hay un vector compartido en `PruebasFirma` y en
  `tests/unit_test.php` del módulo. Si cambia un lado, cambian los dos y el vector.
- **HTTPS obligatorio, sin seguir redirecciones, síncrona y de un hilo** (sin TPL). `LangVersion 3`: compila también en VS antiguo.
- **Sin secretos en el repo:** la consola lee `PSNET_URL`, `PSNET_KEYID`, `PSNET_SECRETO` (+ `PSNET_BASIC_*` en staging) del
  entorno o de `--env=archivo`. Los archivos `.env` viven fuera del repo.

Compilar y probar **solo los proyectos nuevos** (compilar la solución completa regenera los binarios de la 1.46 en su
`bin/Release`, que son los entregados a AX el 15-ago-2026):

```bash
REF="C:/Users/<usuario>/.nuget/packages/microsoft.netframework.referenceassemblies.net35/1.0.3/build/"
dotnet msbuild Prestashop.Net.Tests/Prestashop.Net.Tests.csproj -p:Configuration=Release "-p:TargetFrameworkRootPath=$REF"
dotnet msbuild Prestashop.Net.Console/Prestashop.Net.Console.csproj -p:Configuration=Release "-p:TargetFrameworkRootPath=$REF"
packages/NUnit.Runners.2.6.4/tools/nunit-console.exe Prestashop.Net.Tests/bin/Release/Prestashop.Net.Tests.dll -process:Single
packages/NUnit.Runners.2.6.4/tools/nunit-console.exe Prestashop.Net.Tests/bin/Release/Prestashop.Net.Tests.dll -framework=net-3.5   # CLR 2, como AX
Prestashop.Net.Console/bin/Release/Prestashop.Net.Console.exe ping --env=<archivo.env>
```

- `packages/` está en `.gitignore`: NUnit se restaura desde `Prestashop.Net.Tests/packages.config` (o copiando
  `nunit.framework.dll` y `NUnit.Runners/tools` desde la caché de NuGet).
- En una máquina sin .NET 3.5, `nunit-console.exe` no arranca: agregar `<supportedRuntime version="v4.0.30319" />` dentro de
  `<startup>` en su `.config`. La consola ya trae ese ajuste en su `app.config`.
- En el shell de Claude Code de este equipo los comandos se truncan cerca de 8 KB y la barra invertida doble se vuelve simple:
  escribir archivos por trozos y usar `(char)92` o cadenas verbatim en vez de barras dobles.

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
