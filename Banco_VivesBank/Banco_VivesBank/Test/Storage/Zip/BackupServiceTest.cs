using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Base.Services;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Storage.Backup.Exceptions;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Service;
using Moq;
using NUnit.Framework;

namespace Banco_VivesBank.Test.Storage;

[TestFixture]
public class BackupServiceTests
{
    private Mock<ILogger<BackupService>> _loggerMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<IClienteService> _clienteServiceMock;
    private Mock<IBaseService> _baseServiceMock;
    private Mock<ICuentaService> _cuentaServiceMock;
    private Mock<ITarjetaService> _tarjetaServiceMock;
    private Mock<IStorageJson> _storageJsonMock;
    private BackupService _backupService;
    private string _testSourceDirectory;
    private string _testZipPath;
    private string _testDestinationDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _testSourceDirectory = Path.Combine(Path.GetTempPath(), "test_source_" + Guid.NewGuid());
        _testDestinationDirectory = Path.Combine(Path.GetTempPath(), "test_destination_" + Guid.NewGuid());
        _testZipPath = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid() + ".zip");
    }

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<BackupService>>();
        _userServiceMock = new Mock<IUserService>();
        _clienteServiceMock = new Mock<IClienteService>();
        _baseServiceMock = new Mock<IBaseService>();
        _cuentaServiceMock = new Mock<ICuentaService>();
        _tarjetaServiceMock = new Mock<ITarjetaService>();
        _storageJsonMock = new Mock<IStorageJson>();

        _backupService = new BackupService(
            _loggerMock.Object,
            _userServiceMock.Object,
            _clienteServiceMock.Object,
            _baseServiceMock.Object,
            _cuentaServiceMock.Object,
            _tarjetaServiceMock.Object,
            _storageJsonMock.Object
        );

        Directory.CreateDirectory(_testSourceDirectory);
        Directory.CreateDirectory(_testDestinationDirectory);
        
        SetupMockResponses();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testSourceDirectory))
            Directory.Delete(_testSourceDirectory, true);
        if (Directory.Exists(_testDestinationDirectory))
            Directory.Delete(_testDestinationDirectory, true);
        if (File.Exists(_testZipPath))
            File.Delete(_testZipPath);
    }

    private void SetupMockResponses()
    {
        // Mock de respuestas de servicios
        _userServiceMock.Setup(s => s.GetAllForStorage())
            .ReturnsAsync(new List<Banco_VivesBank.User.Models.User> { new Banco_VivesBank.User.Models.User() });

        _clienteServiceMock.Setup(s => s.GetAllForStorage())
            .ReturnsAsync(new List<Banco_VivesBank.Cliente.Models.Cliente> { new Banco_VivesBank.Cliente.Models.Cliente() });

        _baseServiceMock.Setup(s => s.GetAllForStorage())
            .ReturnsAsync(new List<BaseModel> { new BaseModel() });

        _tarjetaServiceMock.Setup(s => s.GetAllForStorage())
            .ReturnsAsync(new List<Tarjeta> { new Tarjeta() });
    }

    [Test]
    public void ExportToZipDirectorioInvalido()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _backupService.ExportToZip(null, _testZipPath));
    }

    [Test]
    public void ExportToZipRutaInvalida()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _backupService.ExportToZip(_testSourceDirectory, null));
    }

    [Test]
    public async Task ExportToZip_ARREGLAR()
    {
        await _backupService.ExportToZip(_testSourceDirectory, _testZipPath);

        Assert.That(File.Exists(_testZipPath), Is.True, "El archivo ZIP no fue creado");
        
        _userServiceMock.Verify(s => s.GetAllForStorage(), Times.Once);
        _clienteServiceMock.Verify(s => s.GetAllForStorage(), Times.Once);
        _baseServiceMock.Verify(s => s.GetAllForStorage(), Times.Once);
        _cuentaServiceMock.Verify(s => s.GetAllForStorage(), Times.Never);
        _tarjetaServiceMock.Verify(s => s.GetAllForStorage(), Times.Once);
        
        _storageJsonMock.Verify(s => s.ExportJson(
            It.IsAny<FileInfo>(), 
            It.IsAny<List<UserResponse>>()), Times.Once);
        _storageJsonMock.Verify(s => s.ExportJson(
            It.IsAny<FileInfo>(), 
            It.IsAny<List<ClienteResponse>>()), Times.Once);
    }

    [Test]
    public async Task ImportFromZip_ARREGLAR()
    {
        await CreateTestZipFile();

        _storageJsonMock.Setup(s => s.ImportJson<Banco_VivesBank.User.Models.User>(It.IsAny<FileInfo>()))
            .Returns(new List<Banco_VivesBank.User.Models.User> { new Banco_VivesBank.User.Models.User() });
        _storageJsonMock.Setup(s => s.ImportJson<Banco_VivesBank.Cliente.Models.Cliente>(It.IsAny<FileInfo>()))
            .Returns(new List<Banco_VivesBank.Cliente.Models.Cliente> { new Banco_VivesBank.Cliente.Models.Cliente() });
        _storageJsonMock.Setup(s => s.ImportJson<BaseModel>(It.IsAny<FileInfo>()))
            .Returns(new List<BaseModel> { new BaseModel() });
        _storageJsonMock.Setup(s => s.ImportJson<Cuenta>(It.IsAny<FileInfo>()))
            .Returns(new List<Cuenta> { new Cuenta() });
        _storageJsonMock.Setup(s => s.ImportJson<Tarjeta>(It.IsAny<FileInfo>()))
            .Returns(new List<Tarjeta> { new Tarjeta() });

        await _backupService.ImportFromZip(_testZipPath, _testDestinationDirectory);

        _userServiceMock.Verify(s => s.CreateAsync(It.IsAny<UserRequest>()), Times.Once);
        _clienteServiceMock.Verify(s => s.CreateAsync(It.IsAny<ClienteRequest>()), Times.Once);
        _baseServiceMock.Verify(s => s.CreateAsync(It.IsAny<BaseRequest>()), Times.Once);
        _cuentaServiceMock.Verify(s => s.CreateAsync(It.IsAny<CuentaRequest>()), Times.Once);
        _tarjetaServiceMock.Verify(s => s.CreateAsync(It.IsAny<TarjetaRequest>()), Times.Once);
    }

    [Test]
    public void ImportFromZipException()
    {
        string rutaInvalida = "invalid.zip";

        Assert.ThrowsAsync<ImportFromZipException>(async () =>
            await _backupService.ImportFromZip(rutaInvalida, _testDestinationDirectory));
    }

    private async Task CreateTestZipFile()
    {
        var files = new[] { "usuarios.json", "clientes.json", "bases.json", "cuentas.json", "tarjetas.json" };
        foreach (var file in files)
        {
            await File.WriteAllTextAsync(Path.Combine(_testSourceDirectory, file), "[]");
        }

        await _backupService.ExportToZip(_testSourceDirectory, _testZipPath);
    }
}
