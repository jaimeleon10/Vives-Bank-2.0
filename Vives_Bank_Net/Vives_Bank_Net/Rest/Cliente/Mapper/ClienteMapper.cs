using System.Globalization;
using NSubstitute.ClearExtensions;
using Vives_Bank_Net.Rest.Cliente.Dtos;
using Vives_Bank_Net.Rest.Cliente.Models;
using Vives_Bank_Net.Utils.Generators;

namespace Vives_Bank_Net.Rest.Cliente.Mapper;

public class ClienteMapper
{
   public Models.Cliente fromSaveDtoToClienteModel(ClienteRequestSave createDto)
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

   public Models.Cliente fromUpdateToClienteModel(Models.Cliente oldCliente, ClienteRequestUpdate updateDto)
   {
      return new Models.Cliente
      {
         
      };
   }
   
   public ClienteResponse fromClienteToResponse(Models.Cliente cliente)
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
}