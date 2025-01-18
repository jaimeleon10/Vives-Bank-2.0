﻿namespace Vives_Bank_Net.Utils.Generators;

public class ExpDateGenerator
{
    public string GenerarExpDate()
    {
        // Fecha actual
        DateTime hoy = DateTime.UtcNow;

        // Rango de caducidad: entre 1 y 5 años
        Random random = new Random();
        int anosCaducidad = random.Next(1, 6); // Generar un año entre 1 y 5
        int mesCaducidad = random.Next(1, 13); // Generar un mes entre 1 y 12

        // Generar fecha de caducidad
        DateTime fechaCaducidad = hoy.AddYears(anosCaducidad).AddMonths(mesCaducidad - hoy.Month);

        // Formatear la fecha al formato MM/YY
        return fechaCaducidad.ToString("MM_yy");
    }
    
}