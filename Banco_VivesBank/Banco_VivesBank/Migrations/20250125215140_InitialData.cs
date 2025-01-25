using System;
using System.Globalization;
using System.Numerics;
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
                    Saldo = table.Column<BigInteger>(type: "numeric", nullable: false),
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
                    { 1L, new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(499), "Producto para cuenta bancaria de ahorros", "qPhLl0U8tp2", false, "Cuenta de ahorros", 2.5, "cuentaAhorros", new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(500) },
                    { 2L, new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(525), "Producto para cuenta bancaria corriente", "cnIwcBlx3aK", false, "Cuenta corriente", 1.5, "cuentaCorriente", new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(525) }
                });

            migrationBuilder.InsertData(
                table: "Tarjetas",
                columns: new[] { "Id", "CreatedAt", "Cvv", "FechaVencimiento", "Guid", "IsDeleted", "LimiteDiario", "LimiteMensual", "LimiteSemanal", "Numero", "Pin", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(5063), "583", "03/30", "AjfDu5GtFa5", false, 500.0, 10000.0, 2500.0, "425541356569219", "1234", new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(5064) },
                    { 2L, new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(5141), "120", "07/27", "zIcZcNT6YfQ", false, 100.0, 2500.0, 1500.0, "482844745909789", "4321", new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(5141) }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "CreatedAt", "Guid", "IsDeleted", "Password", "Role", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 1, 25, 21, 51, 40, 287, DateTimeKind.Utc).AddTicks(7066), "k7Ul0hazNyk", false, "password", 0, new DateTime(2025, 1, 25, 21, 51, 40, 287, DateTimeKind.Utc).AddTicks(7067), "pedrito" },
                    { 2L, new DateTime(2025, 1, 25, 21, 51, 40, 287, DateTimeKind.Utc).AddTicks(7210), "26TEPZPxh9B", false, "password", 1, new DateTime(2025, 1, 25, 21, 51, 40, 287, DateTimeKind.Utc).AddTicks(7211), "anita" }
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Calle", "CodigoPostal", "Letra", "Numero", "Piso", "Apellidos", "CreatedAt", "Dni", "Email", "FotoDni", "FotoPerfil", "Guid", "IsDeleted", "Nombre", "Telefono", "UpdatedAt", "user_id" },
                values: new object[,]
                {
                    { 1L, "Calle Uno", "28001", "A", "123", "1", "Picapiedra", new DateTime(2025, 1, 25, 21, 51, 40, 287, DateTimeKind.Utc).AddTicks(9939), "12345678Z", "pedro.picapiedra@gmail.com", "https://example.com/fotoDniPedro.jpg", "https://example.com/fotoPerfilPedro.jpg", "PeOrWgILMV5", false, "Pedro", "612345678", new DateTime(2025, 1, 25, 21, 51, 40, 287, DateTimeKind.Utc).AddTicks(9939), 1L },
                    { 2L, "Calle Dos", "28002", "B", "456", "2", "Martinez", new DateTime(2025, 1, 25, 21, 51, 40, 287, DateTimeKind.Utc).AddTicks(9978), "21240915R", "ana.martinez@gmail.com", "https://example.com/fotoDniAna.jpg", "https://example.com/fotoPerfilAna.jpg", "kJJ8DciSW0J", false, "Ana", "623456789", new DateTime(2025, 1, 25, 21, 51, 40, 287, DateTimeKind.Utc).AddTicks(9979), 2L }
                });

            migrationBuilder.InsertData(
                table: "Cuentas",
                columns: new[] { "Id", "cliente_id", "CreatedAt", "Guid", "Iban", "IsDeleted", "producto_id", "Saldo", "tarjeta_id", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(4434), "yrjDQoinTAY", "ES7730046576085345979538", false, 1L, BigInteger.Parse("5000", NumberFormatInfo.InvariantInfo), 1L, new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(4435) },
                    { 2L, 2L, new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(4544), "TN1gbkYP4fM", "ES2114656261103572788444", false, 2L, BigInteger.Parse("7000", NumberFormatInfo.InvariantInfo), 2L, new DateTime(2025, 1, 25, 21, 51, 40, 288, DateTimeKind.Utc).AddTicks(4544) }
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
