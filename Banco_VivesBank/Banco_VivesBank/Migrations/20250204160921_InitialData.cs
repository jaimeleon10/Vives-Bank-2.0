using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Banco_VivesBank.Migrations
{
    /// <inheritdoc />
    public partial class InitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Guid = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TipoProducto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tae = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tarjetas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Guid = table.Column<string>(type: "text", nullable: false),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    FechaVencimiento = table.Column<string>(type: "text", nullable: false),
                    Cvv = table.Column<string>(type: "text", nullable: false),
                    Pin = table.Column<string>(type: "text", nullable: false),
                    LimiteDiario = table.Column<double>(type: "double precision", nullable: false),
                    LimiteSemanal = table.Column<double>(type: "double precision", nullable: false),
                    LimiteMensual = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarjetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Guid = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Guid = table.Column<string>(type: "text", nullable: false),
                    Dni = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Calle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Numero = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    CodigoPostal = table.Column<string>(type: "text", nullable: false),
                    Piso = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Letra = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    FotoPerfil = table.Column<string>(type: "text", nullable: false),
                    FotoDni = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cuentas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Guid = table.Column<string>(type: "text", nullable: false),
                    Iban = table.Column<string>(type: "text", nullable: false),
                    Saldo = table.Column<double>(type: "double precision", nullable: false),
                    tarjeta_id = table.Column<long>(type: "bigint", nullable: true),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    producto_id = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cuentas_Clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cuentas_Productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cuentas_Tarjetas_tarjeta_id",
                        column: x => x.tarjeta_id,
                        principalTable: "Tarjetas",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "CreatedAt", "Descripcion", "Guid", "IsDeleted", "Nombre", "Tae", "TipoProducto", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(8074), "Producto para cuenta bancaria de ahorros", "yFlOirSXTaL", false, "Cuenta de ahorros", 2.5, "cuentaAhorros", new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(8075) },
                    { 2L, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(8085), "Producto para cuenta bancaria corriente", "dEmAjXpMTmy", false, "Cuenta corriente", 1.5, "cuentaCorriente", new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(8086) }
                });

            migrationBuilder.InsertData(
                table: "Tarjetas",
                columns: new[] { "Id", "CreatedAt", "Cvv", "FechaVencimiento", "Guid", "IsDeleted", "LimiteDiario", "LimiteMensual", "LimiteSemanal", "Numero", "Pin", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 2, 4, 16, 9, 20, 775, DateTimeKind.Utc).AddTicks(4877), "298", "04/27", "HGyMfulgniP", false, 500.0, 10000.0, 2500.0, "0606579225434779", "1234", new DateTime(2025, 2, 4, 16, 9, 20, 775, DateTimeKind.Utc).AddTicks(4878) },
                    { 2L, new DateTime(2025, 2, 4, 16, 9, 20, 775, DateTimeKind.Utc).AddTicks(5002), "425", "06/26", "W71vOHuFzS4", false, 100.0, 2500.0, 1500.0, "0751528101703123", "4321", new DateTime(2025, 2, 4, 16, 9, 20, 775, DateTimeKind.Utc).AddTicks(5003) }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "CreatedAt", "Guid", "IsDeleted", "Password", "Role", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(4560), "vz2AWLK8YPS", false, "$2a$11$H8eSJTQ0cZjHNmozhjcW6ep/5jUQDnt7FrUmgbNKxww897iMniVfe", 1, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(4560), "pedrito" },
                    { 2L, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(4587), "6t8gVeTQt2w", false, "$2a$11$H8eSJTQ0cZjHNmozhjcW6ep/5jUQDnt7FrUmgbNKxww897iMniVfe", 1, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(4588), "anita" },
                    { 3L, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(4594), "u6b6NDClz5o", false, "$2a$11$H8eSJTQ0cZjHNmozhjcW6ep/5jUQDnt7FrUmgbNKxww897iMniVfe", 2, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(4594), "admin" },
                    { 4L, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(4600), "03IBwamDHa5", false, "$2a$11$H8eSJTQ0cZjHNmozhjcW6ep/5jUQDnt7FrUmgbNKxww897iMniVfe", 0, new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(4600), "user" }
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Calle", "CodigoPostal", "Letra", "Numero", "Piso", "Apellidos", "CreatedAt", "Dni", "Email", "FotoDni", "FotoPerfil", "Guid", "IsDeleted", "Nombre", "Telefono", "UpdatedAt", "user_id" },
                values: new object[,]
                {
                    { 1L, "Calle Uno", "28001", "A", "123", "1", "Picapiedra", new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(7427), "12345678Z", "pedro.picapiedra@gmail.com", "https://example.com/fotoDniPedro.jpg", "https://example.com/fotoPerfilPedro.jpg", "GbJtJkggUOM", false, "Pedro", "612345678", new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(7427), 1L },
                    { 2L, "Calle Dos", "28002", "B", "456", "2", "Martinez", new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(7449), "21240915R", "ana.martinez@gmail.com", "https://example.com/fotoDniAna.jpg", "https://example.com/fotoPerfilAna.jpg", "JdHsgzoHlrb", false, "Ana", "623456789", new DateTime(2025, 2, 4, 16, 9, 20, 774, DateTimeKind.Utc).AddTicks(7449), 2L }
                });

            migrationBuilder.InsertData(
                table: "Cuentas",
                columns: new[] { "Id", "cliente_id", "CreatedAt", "Guid", "Iban", "IsDeleted", "producto_id", "Saldo", "tarjeta_id", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2025, 2, 4, 16, 9, 20, 775, DateTimeKind.Utc).AddTicks(3599), "VWt47641yDI", "ES7730046576085345979538", false, 1L, 5000.0, 1L, new DateTime(2025, 2, 4, 16, 9, 20, 775, DateTimeKind.Utc).AddTicks(3600) },
                    { 2L, 2L, new DateTime(2025, 2, 4, 16, 9, 20, 775, DateTimeKind.Utc).AddTicks(3663), "oVUzRuFwMlf", "ES2114656261103572788444", false, 2L, 7000.0, 2L, new DateTime(2025, 2, 4, 16, 9, 20, 775, DateTimeKind.Utc).AddTicks(3664) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_user_id",
                table: "Clientes",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_cliente_id",
                table: "Cuentas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_producto_id",
                table: "Cuentas",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_tarjeta_id",
                table: "Cuentas",
                column: "tarjeta_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cuentas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Tarjetas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
