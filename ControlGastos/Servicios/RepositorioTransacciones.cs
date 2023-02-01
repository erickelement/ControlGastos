﻿using Azure.Core;
using ControlGastos.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ControlGastos.Servicios
{

    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtnerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }

    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
                new 
                {
                    transaccion.UsuarioId, 
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                },
                commandType: System.Data.CommandType.StoredProcedure);
            transaccion.Id = id;
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT	t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,
                                                            cu.Nombre as Centa, c.TipoOperacionId
                                                            FROM Transacciones t
                                                            INNER JOIN Categorias c
                                                            ON c.Id = t.CategoriaId
                                                            INNER JOIN Cuentas cu
                                                            ON cu.Id = t.CuentaId
                                                            WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId
                                                            AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT	t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,
                                                            cu.Nombre as Centa, c.TipoOperacionId
                                                            FROM Transacciones t
                                                            INNER JOIN Categorias c
                                                            ON c.Id = t.CategoriaId
                                                            INNER JOIN Cuentas cu
                                                            ON cu.Id = t.CuentaId
                                                            WHERE t.UsuarioId = @UsuarioId
                                                            AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                                                            ORDER BY t.FechaTransaccion DESC;", modelo);
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT * 
                FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.Id = Transacciones.CategoriaId
                WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId", new {id, usuarioId});
        }

        public async Task<IEnumerable<ResultadoObtnerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtnerPorSemana>(@"
                SELECT DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana,
                SUM(Monto) as Monto, cat.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.Id = Transacciones.CategoriaId
                WHERE Transacciones.UsuarioId = @usuarioId AND
                FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
                GROUP BY DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId", modelo);
        }


        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
