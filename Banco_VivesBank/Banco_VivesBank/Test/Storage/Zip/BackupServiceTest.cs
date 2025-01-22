using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Services;
using Banco_VivesBank.Producto.Cuenta.Dto;
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
        _userServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<UserResponse> { new UserResponse() });

        _clienteServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<ClienteResponse> { new ClienteResponse() });

        _baseServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<BaseResponse> { new BaseResponse() });

        /*_cuentaServiceMock.Setup(s => s.GetAll(
                It.IsAny<int?>(), 
                It.IsAny<double?>(), 
                It.IsAny<string>(), 
                It.IsAny<PageRequest>()))
            .ReturnsAsync(new Page<Cuenta>
            {
                Content = new List<Cuenta> { new Cuenta() }, // Lista de cuentas
                TotalPages = 1, // Total de páginas
                TotalElements = 1, // Total de elementos
                PageSize = 10, // Tamaño de la página
                PageNumber = 0, // Número de la página (índice base 0)
                IsFirst = true, // Es la primera página
                IsLast = true // Es la última página
            });*/



        _tarjetaServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<TarjetaResponse> { new TarjetaResponse() });
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
        /*await _backupService.ExportToZip(_testSourceDirectory, _testZipPath);

        Assert.That(File.Exists(_testZipPath), Is.True, "El archivo ZIP no fue creado");
        
        _userServiceMock.Verify(s => s.GetAllAsync(), Times.Once);
        _clienteServiceMock.Verify(s => s.GetAllAsync(), Times.Once);
        _baseServiceMock.Verify(s => s.GetAllAsync(), Times.Once);
        _tarjetaServiceMock.Verify(s => s.GetAllAsync(), Times.Once);
        
        _storageJsonMock.Verify(s => s.ExportJson(
            It.IsAny<FileInfo>(), 
            It.IsAny<List<UserResponse>>()), Times.Once);
        _storageJsonMock.Verify(s => s.ExportJson(
            It.IsAny<FileInfo>(), 
            It.IsAny<List<ClienteResponse>>()), Times.Once);*/
    }

    [Test]
    public async Task ImportFromZip_ARREGLAR()
    {
        /*await CreateTestZipFile();

        _storageJsonMock.Setup(s => s.ImportJson<UserRequest>(It.IsAny<FileInfo>()))
            .Returns(new List<UserRequest> { new UserRequest() });
        _storageJsonMock.Setup(s => s.ImportJson<ClienteRequest>(It.IsAny<FileInfo>()))
            .Returns(new List<ClienteRequest> { new ClienteRequest() });
        _storageJsonMock.Setup(s => s.ImportJson<BaseRequest>(It.IsAny<FileInfo>()))
            .Returns(new List<BaseRequest> { new BaseRequest() });
        _storageJsonMock.Setup(s => s.ImportJson<CuentaRequest>(It.IsAny<FileInfo>()))
            .Returns(new List<CuentaRequest> 
            {
                new CuentaRequest { TipoCuenta = "Ahorro" } 
            });

        _storageJsonMock.Setup(s => s.ImportJson<TarjetaRequestDto>(It.IsAny<FileInfo>()))
            .Returns(new List<TarjetaRequestDto> { new TarjetaRequestDto() });

        await _backupService.ImportFromZip(_testZipPath, _testDestinationDirectory);

        _userServiceMock.Verify(s => s.CreateAsync(It.IsAny<UserRequest>()), Times.Once);
        _clienteServiceMock.Verify(s => s.CreateAsync(It.IsAny<ClienteRequest>()), Times.Once);
        _baseServiceMock.Verify(s => s.CreateAsync(It.IsAny<BaseRequest>()), Times.Once);
        _cuentaServiceMock.Verify(s => s.save(It.IsAny<string>(), It.IsAny<CuentaRequest>()), Times.Once);
        _tarjetaServiceMock.Verify(s => s.CreateAsync(It.IsAny<TarjetaRequestDto>()), Times.Once);*/
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
