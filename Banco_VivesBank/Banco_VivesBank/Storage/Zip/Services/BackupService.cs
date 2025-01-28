using System.IO.Compression;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Producto.Tarjeta.Mappers;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.Storage.Zip.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Banco_VivesBank.Storage.Zip.Services;

public class BackupService : IBackupService
{
    private readonly GeneralDbContext _context;
    private readonly ILogger<BackupService> _logger;
    private readonly IMongoCollection<Movimiento> _movimientoCollection;
    private readonly IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private readonly IStorageJson _storageJson;
    
    public BackupService(
        ILogger<BackupService> logger, GeneralDbContext context,
        IStorageJson storageJson, IOptions<MovimientosMongoConfig> movimientosDatabaseSettings
        )
    {
        _logger = logger;
        _context = context;
        _storageJson = storageJson;
        var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _movimientoCollection = mongoDatabase.GetCollection<Movimiento>(movimientosDatabaseSettings.Value.MovimientosCollectionName);
        _domiciliacionCollection = mongoDatabase.GetCollection<Domiciliacion>(movimientosDatabaseSettings.Value.DomiciliacionesCollectionName);
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

            var users = await _context.Usuarios.ToListAsync();
            
            var clientesEntity = await _context.Clientes.ToListAsync();
            var clientes = new List<Cliente.Models.Cliente>();
            foreach (var clienteEntity in clientesEntity)
            {
                var clienteResponse = ClienteMapper.ToModelFromEntity(clienteEntity);
                clientes.Add(clienteResponse);
            }

            var productos = await _context.ProductoBase.ToListAsync();
            
            var CuentasEntity = await _context.Cuentas.ToListAsync();
            var cuentas = new List<Cuenta>();
            foreach (var cuentaEntity in CuentasEntity)
            {
                var cuenta = CuentaMapper.ToModelFromEntity(cuentaEntity);
                cuentas.Add(cuenta);
            }
            
            var tarjetasEntity = await _context.Tarjetas.ToListAsync();
            var tarjetas = new List<Tarjeta>();
            foreach (var tarjetaEntity in tarjetasEntity)
            {
                var tarjeta = TarjetaMapper.ToModelFromEntity(tarjetaEntity);
                tarjetas.Add(tarjeta);
            }
            
            var movimientos = await _movimientoCollection.Find(_ => true).ToListAsync();
            var domiciliaciones = await _domiciliacionCollection.Find(_ => true).ToListAsync();
            
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "usuarios.json")), users.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "clientes.json")), clientes.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "productos.json")), productos.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "cuentas.json")), cuentas.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "tarjetas.json")), tarjetas.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "movimientos.json")), movimientos.ToList());
            _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "domiciliaciones.json")), domiciliaciones.ToList());


            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "usuarios.json"), "usuarios.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "clientes.json"), "clientes.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "productos.json"), "productos.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "cuentas.json"), "cuentas.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "tarjetas.json"), "tarjetas.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "movimientos.json"), "movimientos.json");
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, "domiciliaciones.json"), "domiciliaciones.json");

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

            foreach (var fileName in new[] { "usuarios.json", "clientes.json", "productos.json", "cuentas.json", "tarjetas.json", "movimientos.json", "domiciliaciones.json" })
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

        if (string.IsNullOrEmpty(zipFilePath))
            throw new ArgumentException("El archivo ZIP no puede ser nulo o vacío.", nameof(zipFilePath));

        if (string.IsNullOrEmpty(destinationDirectory))
            throw new ArgumentException("El directorio de destino no puede ser nulo o vacío.", nameof(destinationDirectory));

        if (!File.Exists(zipFilePath))
            throw new ImportFromZipException("El archivo ZIP no existe.", null);
        
        try
        {
            if (!File.Exists(zipFilePath))
            {
                _logger.LogError($"El archivo ZIP no existe en la ruta: {zipFilePath}");
                throw new ImportFromZipException("El archivo ZIP no existe.", null);
            }
            else {
                _logger.LogInformation($"El archivo ZIP encontrado: {zipFilePath}");
            }

            var temporaryDirectory = Path.Combine(destinationDirectory, "archivos");
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, true);
            }

            Directory.CreateDirectory(temporaryDirectory);

            ZipFile.ExtractToDirectory(zipFilePath, temporaryDirectory, overwriteFiles: true);
            _logger.LogInformation($"Archivos descomprimidos en {temporaryDirectory}");

            var usuariosFile = new FileInfo(Path.Combine(temporaryDirectory, "usuarios.json"));
            var clientesFile = new FileInfo(Path.Combine(temporaryDirectory, "clientes.json"));
            var productoFile = new FileInfo(Path.Combine(temporaryDirectory, "productos.json"));
            var cuentasFile = new FileInfo(Path.Combine(temporaryDirectory, "cuentas.json"));
            var tarjetasFile = new FileInfo(Path.Combine(temporaryDirectory, "tarjetas.json"));
            var movimientosFile = new FileInfo(Path.Combine(temporaryDirectory, "movimientos.json"));
            var domiciliacionesFile = new FileInfo(Path.Combine(temporaryDirectory, "domiciliaciones.json"));

            if (!usuariosFile.Exists || !clientesFile.Exists || !productoFile.Exists || !cuentasFile.Exists ||
                !tarjetasFile.Exists || !movimientosFile.Exists || !domiciliacionesFile.Exists)
            {
                throw new ImportFromZipException("Uno o más archivos necesarios para importar no están presentes en el ZIP.", null);
            }

            var users = _storageJson.ImportJson<UserEntity>(usuariosFile);
            var clientesModel = _storageJson.ImportJson<Cliente.Models.Cliente>(clientesFile);
            var clientes = new List<ClienteEntity>();
            foreach (var clienteModel in clientesModel)
            {
                var clienteEntity = ClienteMapper.ToEntityFromModel(clienteModel);
                clientes.Add(clienteEntity);
            }

            var productos = _storageJson.ImportJson<ProductoEntity>(productoFile);
            var cuentasModel = _storageJson.ImportJson<Cuenta>(cuentasFile);
            var cuentas = new List<CuentaEntity>();
            foreach (var cuentaModel in cuentasModel)
            {
                var cuentaEntity = CuentaMapper.ToEntityFromModel(cuentaModel);
                cuentas.Add(cuentaEntity);
            }

            var tarjetasModel = _storageJson.ImportJson<Tarjeta>(tarjetasFile);
            var tarjetas = new List<TarjetaEntity>();
            foreach (var tarjetaModel in tarjetasModel)
            {
                var tarjetaEntity = TarjetaMapper.ToEntityFromModel(tarjetaModel);
                tarjetas.Add(tarjetaEntity);
            }

            var movimientos = _storageJson.ImportJson<Movimiento>(movimientosFile);
            var domiciliaciones = _storageJson.ImportJson<Domiciliacion>(domiciliacionesFile);

            _logger.LogInformation("Almacenando Usuarios en la base de datos...");
            await SaveEntidadesSiNoExistenAsync(users, _context.Usuarios, u => u.Id.ToString(), u => u.Guid);
            _logger.LogInformation("Almacenando Clientes en la base de datos...");
            await SaveEntidadesSiNoExistenAsync(clientes, _context.Clientes, c => c.Id.ToString(), c => c.Guid);
            _logger.LogInformation("Almacenando Productos en la base de datos...");
            await SaveEntidadesSiNoExistenAsync(productos, _context.ProductoBase, p => p.Id.ToString(), p => p.Guid);
            _logger.LogInformation("Almacenando Cuentas en la base de datos...");
            await SaveEntidadesSiNoExistenAsync(cuentas, _context.Cuentas, c => c.Id.ToString(), c => c.Guid);
            _logger.LogInformation("Almacenando Tarjetas en la base de datos...");
            await SaveEntidadesSiNoExistenAsync(tarjetas, _context.Tarjetas, t => t.Id.ToString(), t => t.Guid);

            _logger.LogInformation("Almacenando Movimientos en MongoDB...");
            await SaveSiNoExistenAsyncMongo(movimientos, _movimientoCollection, m => m.Guid.ToString());
            _logger.LogInformation("Almacenando Domiciliaciones en MongoDB...");
            await SaveSiNoExistenAsyncMongo(domiciliaciones, _domiciliacionCollection, d => d.Guid.ToString());

            var avatarDirectory = Path.Combine(temporaryDirectory, "avatares");
            if (Directory.Exists(avatarDirectory))
            {
                _logger.LogInformation("Agregando avatares al proyecto...");

                var directorioDestino = Path.Combine("data", "avatares");
                if (!Directory.Exists(directorioDestino))
                {
                    Directory.CreateDirectory(directorioDestino);
                }

                foreach (var file in Directory.GetFiles(avatarDirectory))
                {
                    var fileName = Path.GetFileName(file);
                    var avatarPath = Path.Combine(directorioDestino, fileName);

                    if (!File.Exists(avatarPath))
                    {
                        _logger.LogInformation($"Copiando el avatar {fileName}...");
                        File.Copy(file, avatarPath, overwrite: true);
                    }
                    else
                    {
                        _logger.LogInformation($"La foto {fileName} ya existe, saltando...");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"El directorio de avatares {avatarDirectory} no existe.");
            }

            _logger.LogInformation("Eliminando archivos descomprimidos...");
            Directory.Delete(temporaryDirectory, true);

            _logger.LogInformation("Importación de datos desde ZIP finalizada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la importación de datos desde ZIP");
            throw new ImportFromZipException("Ocurrió un error durante la importación de datos desde el archivo ZIP.", ex);
        }
    }
    
    async Task SaveEntidadesSiNoExistenAsync<TEntity>(
        IEnumerable<TEntity> entidades, 
        DbSet<TEntity> dbSet, 
        Func<TEntity, string> selectorId, 
        Func<TEntity, string> selectorGuid) where TEntity : class
    {
        var idsEntidades = entidades
            .Select(e => selectorId(e))
            .ToList();

        var guidsEntidades = entidades
            .Select(e => selectorGuid(e))
            .ToList();

        var entidadesExistentes = dbSet
            .AsEnumerable()
            .Where(e => 
                idsEntidades.Contains(selectorId(e)) || 
                guidsEntidades.Contains(selectorGuid(e)))
            .ToList();

        var nuevasEntidades = entidades
            .Where(e => !entidadesExistentes
                .Any(existing => 
                    selectorId(existing) == selectorId(e) || 
                    selectorGuid(existing) == selectorGuid(e)))
            .ToList();

        foreach (var entidad in nuevasEntidades)
        {
            var entidadExistente = dbSet.Local.FirstOrDefault(e => 
                selectorId(e) == selectorId(entidad) || 
                selectorGuid(e) == selectorGuid(entidad));

            if (entidadExistente != null)
            {
                dbSet.Entry(entidadExistente).State = EntityState.Detached;
            }
        }

        if (nuevasEntidades.Any())
        {
            await dbSet.AddRangeAsync(nuevasEntidades);
            await _context.SaveChangesAsync();
        }
    }
    
    async Task SaveSiNoExistenAsyncMongo<TEntity>(
        IEnumerable<TEntity> entidades, 
        IMongoCollection<TEntity> collection, 
        Func<TEntity, string> selectorId) where TEntity : class
    {
        var guidsEntidades = entidades
            .Select(e => selectorId(e))
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

        var filter = Builders<TEntity>.Filter.In("Guid", guidsEntidades);

        var documentosExistentes = await collection
            .Find(filter)
            .ToListAsync();

        var nuevasEntidades = entidades
            .Where(e => !documentosExistentes
                .Any(existing => selectorId(existing) == selectorId(e)))
            .ToList();

        if (nuevasEntidades.Any())
        {
            await collection.InsertManyAsync(nuevasEntidades);
        }
    }

}