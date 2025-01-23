using Banco_VivesBank.Movimientos.Models;
using GraphQL.Types;

namespace Banco_VivesBank.GraphQL;

public sealed class MovimientoType : ObjectGraphType<Movimiento>
{
    public MovimientoType()
    {
        Field(x => x.Guid).Description("GUID del movimiento");
        Field(x => x.ClienteGuid).Description("GUID del cliente asociado al movimiento");
        Field(x => x.Domiciliacion).Description("Información de domiciliación (si el movimiento es de tipo domiciliación");
        Field(x => x.IngresoNomina).Description("Información de ingreso por nómina (si el movimiento es de tipo ingreso de nómina");
        Field(x => x.PagoConTarjeta).Description("Información de pago con tarjeta (si el movimiento es de tipo pago con tarjeta");
        Field(x => x.Transferencia).Description("Información de transferencia (si el movimiento es de tipo transferencia");
        Field(x => x.CreatedAt).Description("Fecha de creación del movimiento");
    }
}