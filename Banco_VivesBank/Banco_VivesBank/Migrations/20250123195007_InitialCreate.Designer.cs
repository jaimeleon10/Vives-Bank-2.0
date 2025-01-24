﻿// <auto-generated />
using System;
using System.Numerics;
using Banco_VivesBank.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Banco_VivesBank.Migrations
{
    [DbContext(typeof(GeneralDbContext))]
    [Migration("20250123195007_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Banco_VivesBank.Cliente.Models.Cliente", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Apellidos")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasAnnotation("Relational:JsonPropertyName", "createdAt");

                    b.Property<string>("Dni")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FotoDni")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "fotoDni");

                    b.Property<string>("FotoPerfil")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "fotopPerfil");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "isDeleted");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Telefono")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasAnnotation("Relational:JsonPropertyName", "updatedAt");

                    b.HasKey("Id");

                    b.ToTable("Cliente");
                });

            modelBuilder.Entity("Banco_VivesBank.Database.Entities.BaseEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<double>("Tae")
                        .HasColumnType("double precision");

                    b.Property<string>("TipoProducto")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("ProductosBase");
                });

            modelBuilder.Entity("Banco_VivesBank.Database.Entities.ClienteEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Apellidos")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Dni")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FotoDni")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FotoPerfil")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Telefono")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Clientes");
                });

            modelBuilder.Entity("Banco_VivesBank.Database.Entities.CuentaEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("ClienteId")
                        .HasColumnType("bigint")
                        .HasColumnName("cliente_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Iban")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<long>("ProductoId")
                        .HasColumnType("bigint")
                        .HasColumnName("producto_id");

                    b.Property<BigInteger>("Saldo")
                        .HasColumnType("numeric");

                    b.Property<long?>("TarjetaId")
                        .HasColumnType("bigint")
                        .HasColumnName("tarjeta_id");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ClienteId");

                    b.HasIndex("ProductoId");

                    b.HasIndex("TarjetaId")
                        .IsUnique();

                    b.ToTable("Cuentas");
                });

            modelBuilder.Entity("Banco_VivesBank.Database.Entities.TarjetaEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Cvv")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FechaVencimiento")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<double>("LimiteDiario")
                        .HasColumnType("double precision");

                    b.Property<double>("LimiteMensual")
                        .HasColumnType("double precision");

                    b.Property<double>("LimiteSemanal")
                        .HasColumnType("double precision");

                    b.Property<string>("Numero")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Pin")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Titular")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Tarjetas");
                });

            modelBuilder.Entity("Banco_VivesBank.Database.Entities.UserEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("Banco_VivesBank.Producto.Base.Models.BaseModel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasAnnotation("Relational:JsonPropertyName", "createdAt");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "descripcion");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "guid ");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "isDeleted");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "nombre");

                    b.Property<double>("Tae")
                        .HasColumnType("double precision")
                        .HasAnnotation("Relational:JsonPropertyName", "tae");

                    b.Property<string>("TipoProducto")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "tipo_producto");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasAnnotation("Relational:JsonPropertyName", "updatedAt");

                    b.HasKey("Id");

                    b.ToTable("BaseModel");
                });

            modelBuilder.Entity("Banco_VivesBank.Producto.Tarjeta.Models.TarjetaModel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasAnnotation("Relational:JsonPropertyName", "createdAt");

                    b.Property<string>("Cvv")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "cvv");

                    b.Property<string>("FechaVencimiento")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "fecha_vencimiento");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "Guid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "is_deleted");

                    b.Property<double>("LimiteDiario")
                        .HasColumnType("double precision")
                        .HasAnnotation("Relational:JsonPropertyName", "limiteDiario");

                    b.Property<double>("LimiteMensual")
                        .HasColumnType("double precision")
                        .HasAnnotation("Relational:JsonPropertyName", "limiteMensual");

                    b.Property<double>("LimiteSemanal")
                        .HasColumnType("double precision")
                        .HasAnnotation("Relational:JsonPropertyName", "limiteSemanal");

                    b.Property<string>("Numero")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "numero");

                    b.Property<string>("Pin")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "pin");

                    b.Property<string>("Titular")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "titular");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasAnnotation("Relational:JsonPropertyName", "updatedAt");

                    b.HasKey("Id");

                    b.ToTable("TarjetaModel");
                });

            modelBuilder.Entity("Banco_VivesBank.Cliente.Models.Cliente", b =>
                {
                    b.OwnsOne("Banco_VivesBank.Cliente.Models.Direccion", "Direccion", b1 =>
                        {
                            b1.Property<long>("ClienteId")
                                .HasColumnType("bigint");

                            b1.Property<string>("Calle")
                                .IsRequired()
                                .HasMaxLength(150)
                                .HasColumnType("character varying(150)")
                                .HasAnnotation("Relational:JsonPropertyName", "calle");

                            b1.Property<string>("CodigoPostal")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasAnnotation("Relational:JsonPropertyName", "codigoPostal");

                            b1.Property<string>("Letra")
                                .IsRequired()
                                .HasMaxLength(2)
                                .HasColumnType("character varying(2)")
                                .HasAnnotation("Relational:JsonPropertyName", "letra");

                            b1.Property<string>("Numero")
                                .IsRequired()
                                .HasMaxLength(5)
                                .HasColumnType("character varying(5)")
                                .HasAnnotation("Relational:JsonPropertyName", "numero");

                            b1.Property<string>("Piso")
                                .IsRequired()
                                .HasMaxLength(3)
                                .HasColumnType("character varying(3)")
                                .HasAnnotation("Relational:JsonPropertyName", "piso");

                            b1.HasKey("ClienteId");

                            b1.ToTable("Cliente");

                            b1.WithOwner()
                                .HasForeignKey("ClienteId");
                        });

                    b.Navigation("Direccion")
                        .IsRequired();
                });

            modelBuilder.Entity("Banco_VivesBank.Database.Entities.ClienteEntity", b =>
                {
                    b.HasOne("Banco_VivesBank.Database.Entities.UserEntity", "User")
                        .WithOne()
                        .HasForeignKey("Banco_VivesBank.Database.Entities.ClienteEntity", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Banco_VivesBank.Cliente.Models.Direccion", "Direccion", b1 =>
                        {
                            b1.Property<long>("ClienteEntityId")
                                .HasColumnType("bigint");

                            b1.Property<string>("Calle")
                                .IsRequired()
                                .HasMaxLength(150)
                                .HasColumnType("character varying(150)")
                                .HasColumnName("Calle")
                                .HasAnnotation("Relational:JsonPropertyName", "calle");

                            b1.Property<string>("CodigoPostal")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("CodigoPostal")
                                .HasAnnotation("Relational:JsonPropertyName", "codigoPostal");

                            b1.Property<string>("Letra")
                                .IsRequired()
                                .HasMaxLength(2)
                                .HasColumnType("character varying(2)")
                                .HasColumnName("Letra")
                                .HasAnnotation("Relational:JsonPropertyName", "letra");

                            b1.Property<string>("Numero")
                                .IsRequired()
                                .HasMaxLength(5)
                                .HasColumnType("character varying(5)")
                                .HasColumnName("Numero")
                                .HasAnnotation("Relational:JsonPropertyName", "numero");

                            b1.Property<string>("Piso")
                                .IsRequired()
                                .HasMaxLength(3)
                                .HasColumnType("character varying(3)")
                                .HasColumnName("Piso")
                                .HasAnnotation("Relational:JsonPropertyName", "piso");

                            b1.HasKey("ClienteEntityId");

                            b1.ToTable("Clientes");

                            b1.WithOwner()
                                .HasForeignKey("ClienteEntityId");
                        });

                    b.Navigation("Direccion")
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Banco_VivesBank.Database.Entities.CuentaEntity", b =>
                {
                    b.HasOne("Banco_VivesBank.Cliente.Models.Cliente", "Cliente")
                        .WithMany()
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Banco_VivesBank.Producto.Base.Models.BaseModel", "Producto")
                        .WithMany()
                        .HasForeignKey("ProductoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Banco_VivesBank.Producto.Tarjeta.Models.TarjetaModel", "Tarjeta")
                        .WithOne()
                        .HasForeignKey("Banco_VivesBank.Database.Entities.CuentaEntity", "TarjetaId");

                    b.Navigation("Cliente");

                    b.Navigation("Producto");

                    b.Navigation("Tarjeta");
                });
#pragma warning restore 612, 618
        }
    }
}
