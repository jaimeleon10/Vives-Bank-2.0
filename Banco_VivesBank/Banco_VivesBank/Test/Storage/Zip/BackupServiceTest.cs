using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Services;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Service;
using Moq;
using NUnit.Framework;

public class BackupServiceTests
{
    private readonly Mock<ILogger<BackupService>> _loggerMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IClienteService> _clienteServiceMock;
    private readonly Mock<IBaseService> _baseServiceMock;
    private readonly Mock<ICuentaService> _cuentaServiceMock;
    private readonly Mock<ITarjetaService> _tarjetaServiceMock;
    private readonly Mock<IStorageJson> _storageJsonMock;
    private readonly BackupService _backupService;

    public BackupServiceTests()
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
    }

    [Test]
    public async Task ExporttarZip()
    {
        var mockLogger = new Mock<ILogger<BackupService>>();
        var mockUserService = new Mock<IUserService>();
        var mockClienteService = new Mock<IClienteService>();
        var mockBaseService = new Mock<IBaseService>();
        var mockCuentaService = new Mock<ICuentaService>();
        var mockTarjetaService = new Mock<ITarjetaService>();
        var mockStorageJson = new Mock<IStorageJson>();

        var backupService = new BackupService(
            mockLogger.Object,
            mockUserService.Object,
            mockClienteService.Object,
            mockBaseService.Object,
            mockCuentaService.Object,
            mockTarjetaService.Object,
            mockStorageJson.Object
        );

        string sourceDirectory = "test_source";
        string zipFilePath = "test.zip";

        Directory.CreateDirectory(sourceDirectory);

        await backupService.ExportToZip(sourceDirectory, zipFilePath);

        Assert.That(File.Exists(zipFilePath),Is.True , "El archivo ZIP no fue creado.");

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Iniciando la Exportación de datos a ZIP...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }



    [Test]
    public async Task ImportarZip()
    {
        var zipFilePath = "test.zip";
        var destinationDirectory = "test_destination";

        var usuariosFile = new FileInfo(Path.Combine(destinationDirectory, "usuarios.json"));
        var clientesFile = new FileInfo(Path.Combine(destinationDirectory, "clientes.json"));
        var basesFile = new FileInfo(Path.Combine(destinationDirectory, "bases.json"));
        var cuentasFile = new FileInfo(Path.Combine(destinationDirectory, "cuentas.json"));
        var tarjetasFile = new FileInfo(Path.Combine(destinationDirectory, "tarjetas.json"));

        _storageJsonMock.Setup(s => s.ImportJson<UserRequest>(usuariosFile)).Returns(new List<UserRequest>());
        _storageJsonMock.Setup(s => s.ImportJson<ClienteRequest>(clientesFile)).Returns(new List<ClienteRequest>());
        _storageJsonMock.Setup(s => s.ImportJson<BaseRequest>(basesFile)).Returns(new List<BaseRequest>());
        _storageJsonMock.Setup(s => s.ImportJson<CuentaRequest>(cuentasFile)).Returns(new List<CuentaRequest>());
        _storageJsonMock.Setup(s => s.ImportJson<TarjetaRequestDto>(tarjetasFile)).Returns(new List<TarjetaRequestDto>());

        await _backupService.ImportFromZip(zipFilePath, destinationDirectory);

        _userServiceMock.Verify(s => s.CreateAsync(It.IsAny<UserRequest>()), Times.AtLeastOnce);
        _clienteServiceMock.Verify(s => s.CreateAsync(It.IsAny<ClienteRequest>()), Times.AtLeastOnce);
        _baseServiceMock.Verify(s => s.CreateAsync(It.IsAny<BaseRequest>()), Times.AtLeastOnce);
        //_cuentaServiceMock.Verify(s => s.save(It.IsAny<Guid>(), It.IsAny<CuentaRequest>()), Times.AtLeastOnce);
        _tarjetaServiceMock.Verify(s => s.CreateAsync(It.IsAny<TarjetaRequestDto>()), Times.AtLeastOnce);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }
}
