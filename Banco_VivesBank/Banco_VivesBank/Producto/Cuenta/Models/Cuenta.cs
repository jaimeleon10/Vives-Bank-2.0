﻿using System.Numerics;
using System.Text.Json.Serialization;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Producto.Cuenta.Models;


public class Cuenta
{
    [JsonPropertyName("id")]
    public long Id { get; set; } = 0;
    
    [JsonPropertyName("guid")]
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    [JsonPropertyName("iban")]
    public string Iban { get; set; } = IbanGenerator.GenerateIban();

    [JsonPropertyName("saldo")] 
    public BigInteger Saldo { get; set; } = 0;
    
    [JsonPropertyName("tarjeta")] 
    public Tarjeta.Models.Tarjeta? Tarjeta { get; set; } = null;
    
    [JsonPropertyName("cliente")] 
    public Cliente.Models.Cliente Cliente { get; set; }
    
    [JsonPropertyName("producto")] 
    public BaseModel Producto { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;
    
}