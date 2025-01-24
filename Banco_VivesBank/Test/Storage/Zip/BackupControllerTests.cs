﻿using Banco_VivesBank.Storage.Backup.Controller;
using Banco_VivesBank.Storage.Backup.Service;

namespace Test.Storage;

using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

[TestFixture]
public class BackupControllerTests
{
    private Mock<ILogger<BackupService>> _loggerMock;
    private Mock<IBackupService> _backupServiceMock;
    private BackupController _controller;
    private string _testDataDirectory;
    private string _testZipPath;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<BackupService>>();
        _backupServiceMock = new Mock<IBackupService>();

        _testDataDirectory = Path.Combine(Path.GetTempPath(), "test_backup_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDataDirectory);
        _testZipPath = Path.Combine(_testDataDirectory, "backup.zip");

        _controller = new BackupController(
            _loggerMock.Object, 
            _backupServiceMock.Object, 
            _testDataDirectory, 
            _testZipPath
        );
    }


    [TearDown]
    public void Cleanup()
    {
        if (Directory.Exists(_testDataDirectory))
            Directory.Delete(_testDataDirectory, true);
    }

    [Test]
    public async Task ExportToZip()
    {
        _backupServiceMock
            .Setup(s => s.ExportToZip(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.ExportToZip();

        Assert.That(result, Is.Not.Null, "El resultado es nulo.");
        Assert.That(result, Is.InstanceOf<OkResult>(), "El resultado no es un OkResult como se esperaba.");
        _backupServiceMock.Verify(
            s => s.ExportToZip(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once, "El método ExportToZip no fue invocado correctamente."
        );
    }



    [Test]
    public async Task ExportToZipException()
    {
        _backupServiceMock
            .Setup(s => s.ExportToZip(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _controller.ExportToZip();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(500, Is.EqualTo(objectResult.StatusCode));
    }

    [Test]
    public async Task ImportFromZip()
    {
        _backupServiceMock
            .Setup(s => s.ImportFromZip(
                It.Is<string>(path => path == _testZipPath), 
                It.Is<string>(dir => dir == _testDataDirectory)
            ))
            .Returns(Task.CompletedTask);

        var controller = new BackupController(
            _loggerMock.Object, 
            _backupServiceMock.Object, 
            _testDataDirectory, 
            _testZipPath
        );

        var result = await controller.ImportFromZip();

        Assert.That(result, Is.Not.Null, "El resultado es nulo.");
        Assert.That(result, Is.InstanceOf<OkResult>(), "El resultado no es un OkResult.");
        _backupServiceMock.Verify(
            s => s.ImportFromZip(_testZipPath, _testDataDirectory),
            Times.Once, "El método ImportFromZip no fue invocado correctamente."
        );
    }


    [Test]
    public async Task ImportFromZipException()
    {
        _backupServiceMock
            .Setup(s => s.ImportFromZip(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _controller.ImportFromZip();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(500, Is.EqualTo(objectResult.StatusCode));
    }
}