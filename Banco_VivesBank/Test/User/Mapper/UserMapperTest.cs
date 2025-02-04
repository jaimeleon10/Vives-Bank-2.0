using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Mapper;

namespace Test.User.Mapper;
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
            
        };
        var result = UserMapper.ToModelFromRequest(userRequest);

        Assert.Multiple(() =>
        {
            Assert.That(userRequest.Username, Is.EqualTo(result.Username));
            Assert.That(userRequest.Password, Is.EqualTo(result.Password));
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
            Role = Banco_VivesBank.User.Models.Role.User,
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
            Role = Banco_VivesBank.User.Models.Role.User,
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
            Assert.That(user.IsDeleted, Is.EqualTo(result.IsDeleted));
        });
        
    }

    [Test]
    public void ToModelFromEntity()
    {
        var userEntity = new UserEntity
        {
            Id = 1,
            Guid = "guid",
            Username = "test",
            Password = "test",
            Role = Banco_VivesBank.User.Models.Role.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        var result = UserMapper.ToModelFromEntity(userEntity);
        
            Assert.That(userEntity.Id, Is.EqualTo(result.Id));
            Assert.That(userEntity.Guid, Is.EqualTo(result.Guid));
            Assert.That(userEntity.Username, Is.EqualTo(result.Username));
            Assert.That(userEntity.Password, Is.EqualTo(result.Password));
            Assert.That(userEntity.Role, Is.EqualTo(result.Role));
            Assert.That(userEntity.CreatedAt, Is.EqualTo(result.CreatedAt));
            Assert.That(userEntity.UpdatedAt, Is.EqualTo(result.UpdatedAt));
            Assert.That(userEntity.IsDeleted, Is.EqualTo(result.IsDeleted));
    }

    [Test]
    public void ToResponseFromEntity()
    {
        var userEntity = new UserEntity
        {
            Guid = "guid",
            Username = "test",
            Role = Banco_VivesBank.User.Models.Role.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        var result = UserMapper.ToResponseFromEntity(userEntity);
        
            Assert.That(userEntity.Guid, Is.EqualTo(result.Guid));
            Assert.That(userEntity.Username, Is.EqualTo(result.Username));
            Assert.That(userEntity.Role.ToString(), Is.EqualTo(result.Role));
            Assert.That(userEntity.IsDeleted, Is.EqualTo(result.IsDeleted));
    }
}