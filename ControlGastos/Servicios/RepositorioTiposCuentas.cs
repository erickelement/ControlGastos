using ControlGastos.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Runtime.InteropServices;

namespace ControlGastos.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }

    public class RepositorioTiposCuentas: IRepositorioTiposCuentas
    {

        private readonly string connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta) //Se coloca el Task ya que este metodo utilizara asyn-await
        {
            using var connection = new SqlConnection(connectionString);
            var id =await connection.QuerySingleAsync<int>("TiposCuentas_Insertar", new {usuarioId = tipoCuenta.UsuarioId, 
                                                                                         nombre = tipoCuenta.Nombre},
                                                                                         commandType: System.Data.CommandType.StoredProcedure); //ESTO ME TRAE EL ID CREADO
            tipoCuenta.Id= id;
        }


        //Este metodo valida si hay existencia de una cuenta por usuario
        public async Task<bool> Existe(string nombre, int usuarioId)//En la tarea se coloca bool ya que obtendremos un booleano
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1
                                                                           FROM TiposCuentas
                                                                           WHERE Nombre = @Nombre AND UsuarioId = @usuarioId;", 
                                                                           new {nombre, usuarioId});//Estos bson los valores que se obtinene por medio de los parametros.
            return existe == 1; //Este retorno validara que en la query si se obtine que ya existe un registro devolvera 1 de lo contrario FirstOrDefaul traera 0.
        }

        //Este metodo realizara un listado de las cuentas creadas con un usuario
        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden
                                                             FROM TiposCuentas
                                                             WHERE UsuarioId = @UsuarioId
                                                             ORDER BY Orden", new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas
                                            SET Nombre = @Nombre
                                            WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden
                                                                            FROM TiposCuentas
                                                                            WHERE Id = @Id AND UsuarioId = @UsuarioId", new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE TiposCuentas WHERE Id = @Id", new { id });
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id";
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, tipoCuentasOrdenados);
        }
    }
}
