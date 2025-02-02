using System.IO.Compression;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
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
using Path = System.IO.Path;

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

            var usuarios = await _context.Usuarios.ToListAsync();

            var clientesEntity = await _context.Clientes.ToListAsync();
            var clientes = clientesEntity.Select(ClienteMapper.ToModelFromEntity).ToList();

            var productos = await _context.ProductoBase.ToListAsync();

            var cuentasEntity = await _context.Cuentas.ToListAsync();
            var cuentas = cuentasEntity.Select(CuentaMapper.ToModelFromEntity).ToList();

            var tarjetasEntity = await _context.Tarjetas.ToListAsync();
            var tarjetas = tarjetasEntity.Select(TarjetaMapper.ToModelFromEntity).ToList();

            var movimientos = await _movimientoCollection.Find(_ => true).ToListAsync();
            var domiciliaciones = await _domiciliacionCollection.Find(_ => true).ToListAsync();

            ExportarDatosAJson(sourceDirectory, usuarios, clientes, productos, cuentas, tarjetas, movimientos, domiciliaciones);
            AgregarArchivosAlZip(sourceDirectory, zipFilePath);
            EliminarArchivosTemporales(sourceDirectory);

            _logger.LogInformation("Exportación de datos a ZIP finalizada.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al exportar datos a ZIP");
            throw new ExportFromZipException("Ocurrió un error al intentar exportar datos al archivo ZIP.", e);
        }
    }

    public void ExportarDatosAJson(string sourceDirectory, List<UserEntity> usuarios,  
        List<Cliente.Models.Cliente> clientes, List<ProductoEntity> productos, 
        List<Cuenta> cuentas, List<Tarjeta> tarjetas, List<Movimiento> movimientos, 
        List<Domiciliacion> domiciliaciones)
    {
        _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "usuarios.json")), usuarios ?? new List<UserEntity>());
        _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "clientes.json")), clientes ?? new List<Cliente.Models.Cliente>());
        _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "productos.json")), productos ?? new List<ProductoEntity>());
        _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "cuentas.json")), cuentas ?? new List<Cuenta>());
        _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "tarjetas.json")), tarjetas ?? new List<Tarjeta>());
        _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "movimientos.json")), movimientos ?? new List<Movimiento>());
        _storageJson.ExportJson(new FileInfo(Path.Combine(sourceDirectory, "domiciliaciones.json")), domiciliaciones ?? new List<Domiciliacion>());
    }

    public void AgregarArchivosAlZip(string sourceDirectory, string zipFilePath)
    {
        using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            string[] archivos = { "usuarios.json", "clientes.json", "productos.json", "cuentas.json", "tarjetas.json", "movimientos.json", "domiciliaciones.json" };

            foreach (var archivo in archivos)
            {
                zipArchive.CreateEntryFromFile(Path.Combine(sourceDirectory, archivo), archivo);
            }

            var avatarDirectory = Path.Combine("data", "avatares");
            if (Directory.Exists(avatarDirectory))
            {
                _logger.LogInformation("Agregando avatares al ZIP...");
                foreach (var file in Directory.GetFiles(avatarDirectory))
                {
                    var entryName = Path.Combine("avatares", Path.GetFileName(file));
                    zipArchive.CreateEntryFromFile(file, entryName);
                }
            }
        }
    }

    public void EliminarArchivosTemporales(string sourceDirectory)
    {
        string[] archivos = { "usuarios.json", "clientes.json", "productos.json", "cuentas.json", "tarjetas.json", "movimientos.json", "domiciliaciones.json" };

        foreach (var archivo in archivos)
        {
            var filePath = Path.Combine(sourceDirectory, archivo);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
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
            if (users == null || !users.Any())
            {
                _logger.LogWarning("No hay usuarios para importar.");
            }
            else
            {
                _logger.LogInformation("Almacenando Usuarios en la base de datos...");
                await SaveEntidadesSiNoExistenAsync(users, _context.Usuarios, u => u.Id.ToString(), u => u.Guid);
            }

            var clientesModel = _storageJson.ImportJson<Cliente.Models.Cliente>(clientesFile);
            var clientes = new List<ClienteEntity>();
            if (clientesModel != null && clientesModel.Any())
            {
                clientes = clientesModel.Select(ClienteMapper.ToEntityFromModel).ToList();
                _logger.LogInformation("Almacenando Clientes en la base de datos...");
                await SaveEntidadesSiNoExistenAsync(clientes, _context.Clientes, c => c.Id.ToString(), c => c.Guid);
            }
            else
            {
                _logger.LogWarning("No hay clientes para importar.");
            }

            var productos = _storageJson.ImportJson<ProductoEntity>(productoFile);
            if (productos == null || !productos.Any())
            {
                _logger.LogWarning("No hay productos para importar.");
            }
            else
            {
                _logger.LogInformation("Almacenando Productos en la base de datos...");
                await SaveEntidadesSiNoExistenAsync(productos, _context.ProductoBase, p => p.Id.ToString(), p => p.Guid);
            }

            var cuentasModel = _storageJson.ImportJson<Cuenta>(cuentasFile);
            var cuentas = new List<CuentaEntity>();
            if (cuentasModel != null && cuentasModel.Any())
            {
                cuentas = cuentasModel.Select(CuentaMapper.ToEntityFromModel).ToList();
                _logger.LogInformation("Almacenando Cuentas en la base de datos...");
                await SaveEntidadesSiNoExistenAsync(cuentas, _context.Cuentas, c => c.Id.ToString(), c => c.Guid);
            }
            else
            {
                _logger.LogWarning("No hay cuentas para importar.");
            }

            var tarjetasModel = _storageJson.ImportJson<Tarjeta>(tarjetasFile);
            var tarjetas = new List<TarjetaEntity>();
            if (tarjetasModel != null && tarjetasModel.Any())
            {
                tarjetas = tarjetasModel.Select(TarjetaMapper.ToEntityFromModel).ToList();
                _logger.LogInformation("Almacenando Tarjetas en la base de datos...");
                await SaveEntidadesSiNoExistenAsync(tarjetas, _context.Tarjetas, t => t.Id.ToString(), t => t.Guid);
            }
            else
            {
                _logger.LogWarning("No hay tarjetas para importar.");
            }

            var movimientos = _storageJson.ImportJson<Movimiento>(movimientosFile);
            if (movimientos == null || !movimientos.Any())
            {
                _logger.LogWarning("No hay movimientos para importar.");
            }
            else
            {
                _logger.LogInformation("Almacenando Movimientos en MongoDB...");
                await SaveSiNoExistenAsyncMongo(movimientos, _movimientoCollection, m => m.Guid.ToString());
            }

            var domiciliaciones = _storageJson.ImportJson<Domiciliacion>(domiciliacionesFile);
            if (domiciliaciones == null || !domiciliaciones.Any())
            {
                _logger.LogWarning("No hay domiciliaciones para importar.");
            }
            else
            {
                _logger.LogInformation("Almacenando Domiciliaciones en MongoDB...");
                await SaveSiNoExistenAsyncMongo(domiciliaciones, _domiciliacionCollection, d => d.Guid.ToString());
            }

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
    
    public async Task SaveEntidadesSiNoExistenAsync<TEntity>(
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
    
    public async Task SaveSiNoExistenAsyncMongo<TEntity>(
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