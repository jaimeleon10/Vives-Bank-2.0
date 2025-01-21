using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Mapper;
using NUnit.Framework;

namespace Banco_VivesBank.Test.User.Mapper;
[TestFixture]
public class UserMapperTest
{
    [Test]
    public void ToModelFromRequest()
    {
        var userRequest = new UserRequest
        {
            Username = "test",
            Password = "test",
            Role = "USER",
        };
        var result = UserMapper.ToModelFromRequest(userRequest);

        Assert.Multiple(() =>
        {
            Assert.That(userRequest.Username, Is.EqualTo(result.Username));
            Assert.That(userRequest.Password, Is.EqualTo(result.Password));
            Assert.That(userRequest.Role, Is.EqualTo(result.Role.ToString()));
        });
    }
    
     [Test]
    public void ToEntityFromModel()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "guid",
            Username = "test",
            Password = "test",
            Role = Banco_VivesBank.User.Models.Role.USER,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        var result = UserMapper.ToEntityFromModel(user);

        Assert.Multiple(() =>
        {
            Assert.That(user.Id, Is.EqualTo(result.Id));
            Assert.That(user.Guid, Is.EqualTo(result.Guid));
            Assert.That(user.Username, Is.EqualTo(result.Username));
            Assert.That(user.Password, Is.EqualTo(result.Password));
            Assert.That(user.Role, Is.EqualTo(result.Role));
            Assert.That(user.CreatedAt, Is.EqualTo(result.CreatedAt));
            Assert.That(user.UpdatedAt, Is.EqualTo(result.UpdatedAt));
            Assert.That(user.IsDeleted, Is.EqualTo(result.IsDeleted));
        });
    }
     [Test]
    public void ToResponseFromModel()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = "guid",
            Username = "test",
            Role = Banco_VivesBank.User.Models.Role.USER,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        var result = UserMapper.ToResponseFromModel(user);

        Assert.Multiple(() =>
        {
            Assert.That(user.Guid, Is.EqualTo(result.Guid));
            Assert.That(user.Username, Is.EqualTo(result.Username));
            Assert.That(user.Role.ToString(), Is.EqualTo(result.Role));
            Assert.That(user.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"), Is.EqualTo(result.CreatedAt));
            Assert.That(user.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"), Is.EqualTo(result.UpdatedAt));
            Assert.That(user.IsDeleted, Is.EqualTo(result.IsDeleted));
        });
    }
}