using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Dto;

namespace Banco_VivesBank.Cliente.Mapper;

public static class ClienteMapper
{
    public static Models.Cliente ToModelFromEntity(this ClienteEntity entity)
    {
        return new Models.Cliente
        {
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
    
    public static Models.Cliente ToModelFromRequest(this ClienteRequest userRequest, User.Models.User user)
    {
        return new Models.Cliente
        {
            Dni = userRequest.Dni,
            Nombre = userRequest.Nombre.Trim(),
            Apellidos = userRequest.Apellidos.Trim(),
            Direccion = new Direccion
            {
                Calle = userRequest.Calle.Trim(),
                Numero = userRequest.Numero,
                CodigoPostal = userRequest.CodigoPostal,
                Piso = userRequest.Piso,
                Letra = userRequest.Letra,
            },
            Email = userRequest.Email,
            Telefono = userRequest.Telefono,
            User = user,
            IsDeleted = userRequest.IsDeleted
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
              Role = cliente.User.Role.ToString(),
              CreatedAt  = cliente.User.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
              UpdatedAt = cliente.User.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"), IsDeleted = cliente.User.IsDeleted
            },
            CreatedAt = cliente.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = cliente.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
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
                Role = clienteEntity.User.Role.ToString(),
                CreatedAt  = clienteEntity.User.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
                UpdatedAt = clienteEntity.User.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"), 
                IsDeleted = clienteEntity.User.IsDeleted
            },
            CreatedAt = clienteEntity.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = clienteEntity.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = clienteEntity.IsDeleted
        };
    }
    
    public static ClienteEntity ToModelFromRequestUpdate(this ClienteEntity oldCliente, ClienteRequestUpdate updateDto)
    {
        if (!string.IsNullOrWhiteSpace(updateDto.Dni))
        {
            oldCliente.Dni = updateDto.Dni;
        }
        if (!string.IsNullOrWhiteSpace(updateDto.Nombre))
        {
            oldCliente.Nombre = updateDto.Nombre.Trim();
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Apellidos))
        {
            oldCliente.Apellidos = updateDto.Apellidos.Trim();
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Calle))
        {
            oldCliente.Direccion.Calle = updateDto.Calle.Trim();
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Numero))
        {
            oldCliente.Direccion.Numero = updateDto.Numero;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.CodigoPostal))
        {
            oldCliente.Direccion.CodigoPostal = updateDto.CodigoPostal;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Piso))
        {
            oldCliente.Direccion.Piso = updateDto.Piso;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Letra))
        {
            oldCliente.Direccion.Letra = updateDto.Letra;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Email))
        {
            oldCliente.Email = updateDto.Email;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Telefono))
        {
            oldCliente.Telefono = updateDto.Telefono;
        }
        oldCliente.UpdatedAt = DateTime.UtcNow;
        return oldCliente;
    }
}