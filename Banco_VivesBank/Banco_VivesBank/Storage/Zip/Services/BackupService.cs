using System.IO.Compression;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Services;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.Producto.ProductoBase.Services;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.Storage.Zip.Exceptions;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;

namespace Banco_VivesBank.Storage.Zip.Services;

public class BackupService : IBackupService
{
    private readonly ILogger<BackupService> _logger;
    
    //servicios
    private readonly IUserService _userService;
    private readonly IClienteService _clienteService;
    private readonly IProductoService _productoService;
    private readonly ICuentaService _cuentaService;
    private readonly ITarjetaService _tarjetaService;
    private readonly IMovimientoService _movimientoService;

    
    //storageJson
    private readonly IStorageJson _storageJson;
    
    public BackupService(ILogger<BackupService> logger, 
        IUserService userService, 
        IClienteService clienteService, 
        IProductoService productoService, 
        ICuentaService cuentaService, 
        ITarjetaService tarjetaService,
        IMovimientoService movimientoService,
        IStorageJson storageJson
        )
    {
        _logger = logger;
        
        //inicializamos los servicios
        _userService = userService;
        _clienteService = clienteService;
        _productoService = productoService;
        _cuentaService = cuentaService;
        _tarjetaService = tarjetaService;
        _movimientoService = movimientoService;
        
        //inicializamos el storageJson
        _storageJson = storageJson;
    }
    
    public async Task ExportToZip(string sourceDirectory, string zipFilePath)
    {
        _logger.LogInformation("Iniciando la Exportación de datos a ZIP...");

        if (string.IsNullOrWhiteSpace(sourceDirectory))
            throw new ArgumentNullException(nameof(sourceDirectory));
        if (string.IsNullOrWhiteSpace(zipFilePath))
            throw new ArgumentNullException(nameof(zipFilePath));

        try
        {
            if (File.Exists(zipFilePath))
            {
                _logger.LogWarning($"El archivo {zipFilePath} ya existe. Se eliminará antes de crear uno nuevo.");
                File.Delete(zipFilePath);
            }

            var users = await _userService.GetAllForStorage();
            var clientes = await _clienteService.GetAllForStorage();
            var bases = await _productoService.GetAllForStorage();
            var cuentas = await _cuentaService.GetAllForStorage();
            var tarjetas = await _tarjetaService.GetAllForStorage();
            var movimientos = await _movimientoService.GetAllAsync();

            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "usuarios.json")), users.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "clientes.json")), clientes.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "bases.json")), bases.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "cuentas.json")), cuentas.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "tarjetas.json")), tarjetas);
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "movimientos.json")), movimientos.ToList());

            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "usuarios.json"), "usuarios.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "clientes.json"), "clientes.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "bases.json"), "bases.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "cuentas.json"), "cuentas.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "tarjetas.json"), "tarjetas.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "movimientos.json"), "movimientos.json");

                var avatarDirectory = Path.Combine("data", "avatares");
                if (Directory.Exists(avatarDirectory))
                {
                    _logger.LogInformation($"Agregando avatares al ZIP...");
                    foreach (var file in Directory.GetFiles(avatarDirectory))
                    {
                        var entryName = Path.Combine("avatares", Path.GetFileName(file));
                        zipArchive.CreateEntryFromFile(file, entryName);
                    }
                }
            }

            foreach (var fileName in new[] { "usuarios.json", "clientes.json", "bases.json", "cuentas.json", "tarjetas.json", "movimientos.json" })
            {
                var filePath = Path.Combine(sourceDirectory, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            
            _logger.LogInformation("Exportación de datos a ZIP finalizada.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al exportar datos a ZIP");
            throw new ExportFromZipException("Ocurrió un error al intentar exportar datos al archivo ZIP.", e);
        }
    }
    
    public async Task ImportFromZip(string zipFilePath, string destinationDirectory)
    {
        _logger.LogInformation("Iniciando la importación de datos desde ZIP...");

        try
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory, overwriteFiles: true);
            _logger.LogInformation($"Archivos descomprimidos en {destinationDirectory}");

            var usuariosFile = new FileInfo(Path.Combine(destinationDirectory, "usuarios.json"));
            var clientesFile = new FileInfo(Path.Combine(destinationDirectory, "clientes.json"));
            var basesFile = new FileInfo(Path.Combine(destinationDirectory, "bases.json"));
            var cuentasFile = new FileInfo(Path.Combine(destinationDirectory, "cuentas.json"));
            var tarjetasFile = new FileInfo(Path.Combine(destinationDirectory, "tarjetas.json"));
            var movimientosFile = new FileInfo(Path.Combine(destinationDirectory, "movimientos.json"));

            if (usuariosFile.Exists)
            {
                var usuarios = _storageJson.ImportJson<UserRequest>(usuariosFile);
                //hacemos un bucle for ya que no hay un saveAll
                foreach (var user in usuarios)
                {
                    try
                    {
                        await _userService.CreateAsync(user);
                    }
                    catch (UserExistException ex)
                    {
                        _logger.LogWarning($"Usuario duplicado encontrado: {user.Username}. Saltando...");
                    }
                }
            }

            if (clientesFile.Exists)
            {
                var clientes = _storageJson.ImportJson<ClienteRequest>(clientesFile);
                foreach (var cliente in clientes)
                {
                    try
                    {
                        // TODO await _clienteService.CreateAsync(cliente);
                    }
                    catch (ClienteExistsException e)
                    {
                        _logger.LogWarning($"Cliente duplicado encontrado: {cliente.Nombre}. Saltando...");
                    }
                }
            }

            if (basesFile.Exists)
            {
                var bases = _storageJson.ImportJson<ProductoRequest>(basesFile);

                var processedProducts = new HashSet<string>();

                foreach (var baseEntity in bases)
                {
                    try
                    {
                        if (processedProducts.Contains(baseEntity.Nombre))
                        {
                            _logger.LogWarning($"Base duplicada encontrada: {baseEntity.Nombre}. Saltando...");
                            continue;
                        }

                        processedProducts.Add(baseEntity.Nombre);
                        await _productoService.CreateAsync(baseEntity);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error al crear la base: {baseEntity.Nombre}");
                    }
                }
            }
            
/*
            if (cuentasFile.Exists)
            {
                var cuentas = _storageJson.ImportJson<CuentaRequest>(cuentasFile);

                var processedCuentas = new HashSet<Guid>();

                foreach (var cuentaDto in cuentas)
                {
                    try
                    {
                        if (processedCuentas.Contains(cuentaDto.ClienteGuid))
                        {
                            _logger.LogWarning($"Cuenta duplicada encontrada: {cuentaDto.ClienteGuid}. Saltando...");
                            continue;
                        }

                        processedCuentas.Add(cuentaDto.ClienteGuid);
                        await _cuentaService.CreateAsync(cuentaDto);
                    }
                    catch (CuentaException e)
                    {
                        _logger.LogWarning($"Cuenta duplicada encontrada: {cuentaDto.ClienteGuid}. Saltando...");
                    }
                }
            }

            if (tarjetasFile.Exists)
            {
                var tarjetas = _storageJson.ImportJson<TarjetaRequest>(tarjetasFile);

                var processedTarjetas = new HashSet<string>();

                foreach (var tarjetaDto in tarjetas)
                {
                    try
                    {
                        if (processedTarjetas.Contains(tarjetaDto.NumeroTarjeta))
                        {
                            _logger.LogWarning($"Tarjeta duplicada encontrada: {tarjetaDto.NumeroTarjeta}. Saltando...");
                            continue;
                        }

                        processedTarjetas.Add(tarjetaDto.NumeroTarjeta);
                        await _tarjetaService.CreateAsync(tarjetaDto);
                    }
                    catch (TarjetaException e)
                    {
                        _logger.LogWarning($"Tarjeta duplicada encontrada: {tarjetaDto.NumeroTarjeta}. Saltando...");
                    }
                }
            }

            if (movimientosFile.Exists)
            {
                var movimientos = _storageJson.ImportJson<MovimientoRequest>(movimientosFile);

                var processedDomiciliaciones = new HashSet<string>();
                var processedPagosConTarjeta = new HashSet<string>();
                var processedTransferencias = new HashSet<string>();
                var processedIngresosNomina = new HashSet<string>();

                foreach (var movimiento in movimientos)
                {
                    try
                    {
                        if (movimiento.Domiciliacion != null)
                        {
                            var domiciliacionKey = $"{movimiento.Domiciliacion.ClienteGuid}-{movimiento.Domiciliacion.IbanCliente}-{movimiento.Domiciliacion.Acreedor}";
                            if (processedDomiciliaciones.Contains(domiciliacionKey))
                            {
                                _logger.LogWarning($"Domiciliación duplicada encontrada: {domiciliacionKey}. Saltando...");
                                continue;
                            }

                            processedDomiciliaciones.Add(domiciliacionKey);

                            var domiciliacionRequest = new DomiciliacionRequest
                            {
                                ClienteGuid = movimiento.Domiciliacion.ClienteGuid,
                                Acreedor = movimiento.Domiciliacion.Acreedor,
                                IbanEmpresa = movimiento.Domiciliacion.IbanEmpresa,
                                IbanCliente = movimiento.Domiciliacion.IbanCliente,
                                Importe = movimiento.Domiciliacion.Importe.ToString(),
                                Periodicidad = movimiento.Domiciliacion.Periodicidad.ToString(),
                                Activa = movimiento.Domiciliacion.Activa
                            };

                            await _movimientoService.CreateDomiciliacionAsync(domiciliacionRequest);
                        }

                        if (movimiento.PagoConTarjeta != null)
                        {
                            var pagoConTarjetaKey = $"{movimiento.PagoConTarjeta.NumeroTarjeta}-{movimiento.PagoConTarjeta.NombreComercio}-{movimiento.PagoConTarjeta.Importe}";
                            if (processedPagosConTarjeta.Contains(pagoConTarjetaKey))
                            {
                                _logger.LogWarning($"Pago con tarjeta duplicado encontrado: {pagoConTarjetaKey}. Saltando...");
                                continue;
                            }

                            processedPagosConTarjeta.Add(pagoConTarjetaKey);

                            var pagoConTarjetaRequest = new PagoConTarjetaRequest
                            {
                                NombreComercio = movimiento.PagoConTarjeta.NombreComercio,
                                Importe = movimiento.PagoConTarjeta.Importe.ToString(),
                                NumeroTarjeta = movimiento.PagoConTarjeta.NumeroTarjeta
                            };

                            await _movimientoService.CreatePagoConTarjetaAsync(pagoConTarjetaRequest);
                        }

                        if (movimiento.Transferencia != null)
                        {
                            var transferenciaKey = $"{movimiento.Transferencia.IbanOrigen}-{movimiento.Transferencia.IbanDestino}-{movimiento.Transferencia.Importe}";
                            if (processedTransferencias.Contains(transferenciaKey))
                            {
                                _logger.LogWarning($"Transferencia duplicada encontrada: {transferenciaKey}. Saltando...");
                                continue;
                            }

                            processedTransferencias.Add(transferenciaKey);

                            var transferenciaRequest = new TransferenciaRequest
                            {
                                IbanOrigen = movimiento.Transferencia.IbanOrigen,
                                NombreBeneficiario = movimiento.Transferencia.NombreBeneficiario,
                                IbanDestino = movimiento.Transferencia.IbanDestino,
                                Importe = movimiento.Transferencia.Importe.ToString()
                            };

                            await _movimientoService.CreateTransferenciaAsync(transferenciaRequest);
                        }

                        if (movimiento.IngresoNomina != null)
                        {
                            var ingresoNominaKey = $"{movimiento.IngresoNomina.CifEmpresa}-{movimiento.IngresoNomina.IbanCliente}-{movimiento.IngresoNomina.Importe}";
                            if (processedIngresosNomina.Contains(ingresoNominaKey))
                            {
                                _logger.LogWarning($"Ingreso de nómina duplicado encontrado: {ingresoNominaKey}. Saltando...");
                                continue;
                            }

                            processedIngresosNomina.Add(ingresoNominaKey);

                            var ingresoNominaRequest = new IngresoNominaRequest
                            {
                                NombreEmpresa = movimiento.IngresoNomina.NombreEmpresa,
                                CifEmpresa = movimiento.IngresoNomina.CifEmpresa,
                                IbanEmpresa = movimiento.IngresoNomina.IbanEmpresa,
                                IbanCliente = movimiento.IngresoNomina.IbanCliente,
                                Importe = movimiento.IngresoNomina.Importe.ToString()
                            };

                            await _movimientoService.CreateIngresoNominaAsync(ingresoNominaRequest);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error al procesar movimiento: {movimiento}");
                    }
                }
            }
*/
            
            var avatarDirectory = Path.Combine(destinationDirectory, "avatares");
            if (!Directory.Exists(avatarDirectory))
            {
                Directory.CreateDirectory(avatarDirectory);
            }

            var avatarFiles = Directory.GetFiles(avatarDirectory);
            foreach (var avatarFile in avatarFiles)
            {
                var avatarName = Path.GetFileName(avatarFile);
                var avatarPath = Path.Combine("data", "avatares");
                if (File.Exists(avatarPath))
                {
                    File.Copy(avatarPath, Path.Combine(avatarDirectory, avatarName), overwrite: true);
                }
                else
                {
                    _logger.LogWarning($"Archivo de avatar {avatarName} no encontrado.");
                }
            }
            _logger.LogInformation("Importación de datos desde ZIP finalizada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la importación de datos desde ZIP");
            throw new ImportFromZipException("Ocurrió un error durante la importación de datos desde el archivo ZIP.", ex);
        }
    }
}