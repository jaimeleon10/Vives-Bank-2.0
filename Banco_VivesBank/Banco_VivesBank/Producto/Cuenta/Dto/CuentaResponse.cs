﻿using System.Numerics;

namespace Banco_VivesBank.Producto.Cuenta.Dto;

public class CuentaResponse
{
    public required string Guid { get; set; }
    public required string Iban { get; set; } 
    public BigInteger Saldo { get; set; }
    public string? TarjetaGuid { get; set; }
    public string ClienteGuid { get; set; } 
    public string ProductoGuid { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}