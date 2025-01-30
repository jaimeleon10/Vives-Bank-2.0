const { MongoClient, ObjectId } = require("mongodb");

// Configuración de conexión
const uri = "mongodb://admin:password@localhost:27017";
const dbName = "MovimientosDB";

(async () => {
    const client = new MongoClient(uri);

    try {
        await client.connect();
        console.log("🟢 Conectado a MongoDB");

        const db = client.db(dbName);
        const movimientosCollection = db.collection("Movimientos");
        const domiciliacionesCollection = db.collection("Domiciliaciones");

        // Verificar si ya hay datos
        const movimientosCount = await movimientosCollection.countDocuments();
        if (movimientosCount > 0) {
            console.log("ℹ️ Los datos ya existen. No se realizarán inserciones.");
            return;
        }

        // Crear domiciliaciones
        const domiciliacion1 = {
            guid: "JsE9iOqm8Lz",
            clienteGuid: "GbJtJkggUOM",
            acreedor: "Empresa A",
            ibanEmpresa: "ES7604878673285989969615",
            ibanCliente: "ES7730046576085345979538",
            importe: 50.00,
            periodicidad: "Semanal",
            activa: true,
            fechaInicio: new Date(),
            ultimaEjecucion: new Date(),
        };

        const domiciliacion2 = {
            guid: "WQZGKKCuy1q",
            clienteGuid: "JdHsgzoHlrb",
            acreedor: "Empresa B",
            ibanEmpresa: "ES6001824625518689664196",
            ibanCliente: "ES2114656261103572788444",
            importe: 100.50,
            periodicidad: "Mensual",
            activa: true,
            fechaInicio: new Date(),
            ultimaEjecucion: new Date(),
        };

        const domiciliaciones = await domiciliacionesCollection.insertMany([domiciliacion1, domiciliacion2]);

        // Crear movimientos
        const movimientos = [
            {
                guid: "iFDVeS3riQn",
                clienteGuid: "GbJtJkggUOM",
                ingresoNomina: {
                    nombreEmpresa: "Empresa Nómina 1",
                    cifEmpresa: "A12345678",
                    ibanEmpresa: "ES7604878673285989969615",
                    ibanCliente: "ES7730046576085345979538",
                    importe: 3000.00,
                },
                createdAt: new Date(),
            },
            {
                guid: "KHIZUrPReLA",
                clienteGuid: "JdHsgzoHlrb",
                ingresoNomina: {
                    nombreEmpresa: "Empresa Nómina 2",
                    cifEmpresa: "B98765432",
                    ibanEmpresa: "ES6001824625518689664196",
                    ibanCliente: "ES2114656261103572788444",
                    importe: 4000.75,
                },
                createdAt: new Date(),
            },
            {
                guid: "5nryAueJt51",
                clienteGuid: domiciliacion1.clienteGuid,
                domiciliacion: domiciliacion1,
                createdAt: new Date(),
            },
            {
                guid: "5wxNX3fOpZn",
                clienteGuid: domiciliacion2.clienteGuid,
                domiciliacion: domiciliacion2,
                createdAt: new Date(),
            },
            {
                guid: "s17MJHSzcED",
                clienteGuid: "GbJtJkggUOM",
                pagoConTarjeta: {
                    nombreComercio: "Supermercado A",
                    importe: 200.00,
                    numeroTarjeta: "0606579225434779",
                },
                createdAt: new Date(),
            },
            {
                guid: "NnrMDBh7RR4",
                clienteGuid: "JdHsgzoHlrb",
                pagoConTarjeta: {
                    nombreComercio: "Supermercado B",
                    importe: 500.25,
                    numeroTarjeta: "0751528101703123",
                },
                createdAt: new Date(),
            },
            {
                guid: "4yTkeQeZZiV",
                clienteGuid: "GbJtJkggUOM",
                transferencia: {
                    clienteOrigen: "Pedro Picapiedra",
                    ibanOrigen: "ES7730046576085345979538",
                    nombreBeneficiario: "Ana Martinez",
                    ibanDestino: "ES2114656261103572788444",
                    importe: -1000.00,
                    revocada: false
                },
                createdAt: new Date(),
            },
            {
                guid: "2hikrQeQQEj",
                clienteGuid: "JdHsgzoHlrb",
                transferencia: {
                    clienteOrigen: "Pedro Picapiedra",
                    ibanOrigen: "ES7730046576085345979538",
                    nombreBeneficiario: "Ana Martinez",
                    ibanDestino: "ES2114656261103572788444",
                    importe: 1000.00,
                    revocada: false
                },
                createdAt: new Date(),
            },
            {
                guid: "v3dSF4c87Ff",
                clienteGuid: "JdHsgzoHlrb",
                transferencia: {
                    clienteOrigen: "Ana Martinez",
                    ibanOrigen: "ES2114656261103572788444",
                    nombreBeneficiario: "Pedro Picapiedra",
                    ibanDestino: "ES7730046576085345979538",
                    importe: -2000.00,
                    revocada: false
                },
                createdAt: new Date(),
            },
            {
                guid: "bzWVG68dcsP",
                clienteGuid: "GbJtJkggUOM",
                transferencia: {
                    clienteOrigen: "Ana Martinez",
                    ibanOrigen: "ES2114656261103572788444",
                    nombreBeneficiario: "Pedro Picapiedra",
                    ibanDestino: "ES7730046576085345979538",
                    importe: 2000.00,
                    revocada: false
                },
                createdAt: new Date(),
            }
        ];

        await movimientosCollection.insertMany(movimientos);

        console.log("✅ Datos iniciales insertados correctamente.");
    } catch (err) {
        console.error(" Error al inicializar los datos:", err);
    } finally {
        await client.close();
    }
})();