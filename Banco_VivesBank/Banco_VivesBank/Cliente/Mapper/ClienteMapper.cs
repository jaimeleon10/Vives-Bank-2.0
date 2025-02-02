using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Dto;

namespace Banco_VivesBank.Cliente.Mapper;
/// <summary>
/// Mapea los objetos Cliente de la capa de datos a los de la capa de negocio y viceversa
/// </summary>
public static class ClienteMapper
{
    public static Models.Cliente ToModelFromEntity(this ClienteEntity entity)
    {
        return new Models.Cliente
        {
            Id = entity.Id,
            Guid = entity.Guid,
            Dni = entity.Dni,
            Nombre = entity.Nombre,
            Apellidos = entity.Apellidos,
            Direccion = new Direccion
            {
                Calle = entity.Direccion.Calle,
                Numero = entity.Direccion.Numero,
                CodigoPostal = entity.Direccion.CodigoPostal,
                Piso = entity.Direccion.Piso,
                Letra = entity.Direccion.Letra,
            },
            Email = entity.Email,
            Telefono = entity.Telefono,
            User = new User.Models.User
            { 
                Id = entity.User.Id,
                Guid = entity.User.Guid,
                Username = entity.User.Username,
                Password = entity.User.Password,
                Role = entity.User.Role,
                CreatedAt = entity.User.CreatedAt,
                UpdatedAt = entity.User.UpdatedAt,
                IsDeleted = entity.User.IsDeleted
            },
            IsDeleted = entity.IsDeleted
        };
    }
    
    public static Models.Cliente ToModelFromRequest(this ClienteRequest clienteRequest, User.Models.User user)
    {
        return new Models.Cliente
        {
            Dni = clienteRequest.Dni,
            Nombre = clienteRequest.Nombre.Trim(),
            Apellidos = clienteRequest.Apellidos.Trim(),
            Direccion = new Direccion
            {
                Calle = clienteRequest.Calle.Trim(),
                Numero = clienteRequest.Numero,
                CodigoPostal = clienteRequest.CodigoPostal,
                Piso = clienteRequest.Piso,
                Letra = clienteRequest.Letra,
            },
            Email = clienteRequest.Email,
            Telefono = clienteRequest.Telefono,
            User = user,
            IsDeleted = clienteRequest.IsDeleted
        };
    }
    
    public static ClienteEntity ToEntityFromModel(this Models.Cliente cliente)
    {
        return new ClienteEntity
        {
            Id = cliente.Id,
            Guid = cliente.Guid,
            Dni = cliente.Dni,
            Nombre = cliente.Nombre,
            Apellidos = cliente.Apellidos,
            Direccion = cliente.Direccion,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            FotoPerfil = cliente.FotoPerfil,
            FotoDni = cliente.FotoDni,
            UserId = cliente.User?.Id ?? 0,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt,
            IsDeleted = cliente.IsDeleted
        };
    }

    public static ClienteResponse ToResponseFromModel(this Models.Cliente cliente)
    {
        return new ClienteResponse
        {
            Guid = cliente.Guid,
            Dni = cliente.Dni,
            Nombre = cliente.Nombre,
            Apellidos = cliente.Apellidos,
            Direccion = cliente.Direccion,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            FotoPerfil = cliente.FotoPerfil,
            FotoDni = cliente.FotoDni,
            UserResponse = new UserResponse
            {
              Guid =   cliente.User.Guid,
              Username = cliente.User.Username,
              Password = cliente.User.Password,
              Role = cliente.User.Role.ToString(),
              CreatedAt  = cliente.User.CreatedAt.ToString(),
              UpdatedAt = cliente.User.UpdatedAt.ToString(), 
              IsDeleted = cliente.User.IsDeleted
            },
            CreatedAt = cliente.CreatedAt.ToString(),
            UpdatedAt = cliente.UpdatedAt.ToString(),
            IsDeleted = cliente.IsDeleted
        };
    }

    public static ClienteResponse ToResponseFromEntity(this ClienteEntity clienteEntity)
    {
        return new ClienteResponse
        {
            Guid = clienteEntity.Guid,
            Dni = clienteEntity.Dni,
            Nombre = clienteEntity.Nombre,
            Apellidos = clienteEntity.Apellidos,
            Direccion = clienteEntity.Direccion,
            Email = clienteEntity.Email,
            Telefono = clienteEntity.Telefono,
            FotoPerfil = clienteEntity.FotoPerfil,
            FotoDni = clienteEntity.FotoDni,
            UserResponse = new UserResponse
            {
                Guid =   clienteEntity.User.Guid,
                Username = clienteEntity.User.Username,
                Password = clienteEntity.User.Password,
                Role = clienteEntity.User.Role.ToString(),
                CreatedAt  = clienteEntity.User.CreatedAt.ToString(),
                UpdatedAt = clienteEntity.User.UpdatedAt.ToString(), 
                IsDeleted = clienteEntity.User.IsDeleted
            },
            CreatedAt = clienteEntity.CreatedAt.ToString(),
            UpdatedAt = clienteEntity.UpdatedAt.ToString(),
            IsDeleted = clienteEntity.IsDeleted
        };
    }
}