﻿using System.Numerics;

namespace Banco_VivesBank.Movimientos.Dto;

public class PagoConTarjetaResponse
{
    public string NombreComercio { get; set; }
    
    public string Importe { get; set; }
    
    public string NumeroTarjeta { get; set; }
}