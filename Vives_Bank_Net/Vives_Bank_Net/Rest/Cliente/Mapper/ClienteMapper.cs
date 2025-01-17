using System.Globalization;
using NSubstitute.ClearExtensions;
using Vives_Bank_Net.Rest.Cliente.Database;
using Vives_Bank_Net.Rest.Cliente.Dtos;
using Vives_Bank_Net.Rest.Cliente.Models;
using Vives_Bank_Net.Utils.Generators;
using Vives_Bank_Net.Utils.Generators;

namespace Vives_Bank_Net.Rest.Cliente.Mapper;

public class ClienteMapper
{
   public Models.Cliente FromSaveDtoToClienteModel(ClienteRequestSave createDto)
   {
      return new Models.Cliente
      {
         Guid = GuidGenerator.GenerarId(),
         Dni = createDto.Dni,
         Nombre = createDto.Nombre,
         Apellidos = createDto.Apellidos,
         Direccion = new Direccion
         {
            Calle = createDto.Calle,
            Numero = createDto.Numero,
            CodigoPostal = createDto.CodigoPostal,
            Piso = createDto.Piso,
            Letra = createDto.Letra,
         },
         Email = createDto.Email,
         Telefono = createDto.Telefono
      };
   }
   
   public ClienteResponse FromModelClienteToResponse(Models.Cliente cliente)
   {
      return new ClienteResponse
      {
         Id = cliente.Guid,
         Nombre = cliente.Nombre,
         Apellidos = cliente.Apellidos,
         Calle = cliente.Direccion.Calle,
         Numero = cliente.Direccion.Numero,
         CodigoPostal = cliente.Direccion.CodigoPostal,
         Piso = cliente.Direccion.Piso,
         Letra = cliente.Direccion.Letra,
         Email = cliente.Email,
         Telefono = cliente.Telefono,
         FotoPerfil = cliente.FotoPerfil,
         FotoDni = cliente.FotoDni,
         UserId = cliente.User.Guid,
         Username = cliente.User.UserName,
         CreatedAt = cliente.CreatedAt.ToString(CultureInfo.InvariantCulture),
         UpdatedAt = cliente.UpdatedAt.ToString(CultureInfo.InvariantCulture),
         IsDeleted = cliente.IsDeleted
      };
   }

   public ClienteEntity FromClienteModelToEntity(Models.Cliente model)
   {
      return new ClienteEntity
      {
         Id = model.Id,
         Guid = model.Guid,
         Dni = model.Dni,
         Nombre = model.Nombre,
         Apellidos = model.Apellidos,
         Direccion = model.Direccion,
         Email = model.Email,
         Telefono = model.Telefono,
         FotoPerfil = model.FotoPerfil,
         FotoDni = model.FotoDni,
      //   Cuentas = model.Cuentas,
       //  User = model.User,
         CreatedAt = model.CreatedAt,
         UpdatedAt = model.UpdatedAt,
         IsDeleted = model.IsDeleted
      };
   }

   public Models.Cliente FromEntityClienteToModel(ClienteEntity clienteEntity)
   {
      return new Models.Cliente
      {
         Id = clienteEntity.Id,
         Guid = clienteEntity.Guid,
         Dni = clienteEntity.Dni,
         Nombre = clienteEntity.Nombre,
         Apellidos = clienteEntity.Apellidos,
         Direccion = clienteEntity.Direccion,
         Email = clienteEntity.Email,
         Telefono = clienteEntity.Telefono,
         FotoPerfil = clienteEntity.FotoPerfil,
         FotoDni = clienteEntity.FotoDni,
       //  Cuentas = clienteEntity.Cuentas,
        // User = clienteEntity.User,
         CreatedAt = clienteEntity.CreatedAt,
         UpdatedAt = clienteEntity.UpdatedAt,
         IsDeleted = clienteEntity.IsDeleted
      };
   }

   public ClienteResponse FromEntityClienteToResponse(ClienteEntity clienteEntity)
   {
      return new ClienteResponse
      {
         Id = clienteEntity.Guid,
         Nombre = clienteEntity.Nombre,
         Apellidos = clienteEntity.Apellidos,
         Calle = clienteEntity.Direccion.Calle,
         Numero = clienteEntity.Direccion.Numero,
         CodigoPostal = clienteEntity.Direccion.CodigoPostal,
         Piso = clienteEntity.Direccion.Piso,
         Letra = clienteEntity.Direccion.Letra,
         Email = clienteEntity.Email,
         Telefono = clienteEntity.Telefono,
         FotoPerfil = clienteEntity.FotoPerfil,
         FotoDni = clienteEntity.FotoDni,
       //  UserId = clienteEntity.User.Guid,
        // Username = clienteEntity.User.UserName,
         CreatedAt = clienteEntity.CreatedAt.ToString(CultureInfo.InvariantCulture),
         UpdatedAt = clienteEntity.UpdatedAt.ToString(CultureInfo.InvariantCulture),
         IsDeleted = clienteEntity.IsDeleted
      };
   }
   
   public ClienteEntity FromSaveDtoToClienteEntity(ClienteRequestSave createDto)
   {
      return new ClienteEntity
      {
         Guid = GuidGenerator.GenerarId(),
         Dni = createDto.Dni,
         Nombre = createDto.Nombre.Trim(),
         Apellidos = createDto.Apellidos.Trim(),
         Direccion = new Direccion
         {
            Calle = createDto.Calle.Trim(),
            Numero = createDto.Numero.Trim(),
            CodigoPostal = createDto.CodigoPostal.Trim(),
            Piso = createDto.Piso.Trim(),
            Letra = createDto.Letra.Trim(),
         },
         Email = createDto.Email,
         Telefono = createDto.Telefono
      };
   }
   
   public ClienteEntity FromUpdateDtoToClienteEntity(ClienteEntity oldCliente, ClienteRequestUpdate updateDto)
   {
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
         oldCliente.Direccion.Numero = updateDto.Numero.Trim();
      }

      if (!string.IsNullOrWhiteSpace(updateDto.CodigoPostal))
      {
         oldCliente.Direccion.CodigoPostal = updateDto.CodigoPostal;
      }

      if (!string.IsNullOrWhiteSpace(updateDto.Piso))
      {
         oldCliente.Direccion.Piso = updateDto.Piso.Trim();
      }

      if (!string.IsNullOrWhiteSpace(updateDto.Letra))
      {
         oldCliente.Direccion.Letra = updateDto.Letra.Trim();
      }

      if (!string.IsNullOrWhiteSpace(updateDto.Email))
      {
         oldCliente.Email = updateDto.Email;
      }

      if (!string.IsNullOrWhiteSpace(updateDto.Telefono))
      {
         oldCliente.Telefono = updateDto.Telefono;
      }
      oldCliente.UpdatedAt=DateTime.UtcNow;
      return oldCliente;
   }
   
}