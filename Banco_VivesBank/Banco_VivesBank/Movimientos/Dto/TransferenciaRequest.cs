﻿using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Movimientos.Dto;

public class TransferenciaRequest
{
    [IbanValidator]
    public string IbanOrigen { get; set; }
    
    [Required(ErrorMessage = "El nombre del beneficiario es un campo obligatorio")]
    public string NombreBeneficiario { get; set; }
    
    [IbanValidator]
    public string IbanDestino { get; set; }

    [Required(ErrorMessage = "El importe es un campo obligatorio.")]
    [RegularExpression(@"^-?\d+(\.\d{1,2})?$", ErrorMessage = "El importe debe ser un número entero válido.")]
    public string Importe { get; set; }
}