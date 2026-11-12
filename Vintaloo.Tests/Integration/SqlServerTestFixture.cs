using System.Text.Json;
using System.Text.RegularExpressions;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    public sealed class SqlServerTestFixture : IAsyncLifetime
    {
        private const string ConexionPorDefecto =
            "Server=FGABRIEL14\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True";

        public string SourceDatabase { get; }
        public string TestDatabase { get; }
        public string MasterConnectionString { get; }
        public string TestConnectionString { get; }

        public SqlServerTestFixture()
        {
            SourceDatabase = Environment.GetEnvironmentVariable("VINTALOO_TEST_SOURCE_DB") ?? "vintaloo_db";
            TestDatabase = Environment.GetEnvironmentVariable("VINTALOO_TEST_DB") ?? "vintaloo_test_db";

            ValidarNombreBase(SourceDatabase);
            ValidarNombreBase(TestDatabase);

            var builder = new SqlConnectionStringBuilder(ObtenerConexionBase())
            {
                InitialCatalog = "master"
            };
            MasterConnectionString = builder.ConnectionString;

            builder.InitialCatalog = TestDatabase;
            TestConnectionString = builder.ConnectionString;
        }

        public async Task InitializeAsync()
        {
            await using var master = new SqlConnection(MasterConnectionString);
            await master.OpenAsync();

            var existeOrigen = await ExisteBaseAsync(master, SourceDatabase);
            if (!existeOrigen)
                throw new InvalidOperationException($"No existe la base de datos de origen '{SourceDatabase}'.");

            await EjecutarAsync(master, $@"
IF DB_ID(N'{TestDatabase}') IS NOT NULL
BEGIN
    ALTER DATABASE [{TestDatabase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{TestDatabase}];
END");

            await EjecutarAsync(master,
                $"DBCC CLONEDATABASE ([{SourceDatabase}], [{TestDatabase}]) WITH NO_INFOMSGS;");
            await EjecutarAsync(master, $"ALTER DATABASE [{TestDatabase}] SET READ_WRITE;");

            await using var db = new SqlConnection(TestConnectionString);
            await db.OpenAsync();
            await EjecutarAsync(db, @"
SET IDENTITY_INSERT roles ON;
IF NOT EXISTS (SELECT 1 FROM roles WHERE id_rol = 1)
    INSERT INTO roles (id_rol, nombre) VALUES (1, 'admin');
IF NOT EXISTS (SELECT 1 FROM roles WHERE id_rol = 2)
    INSERT INTO roles (id_rol, nombre) VALUES (2, 'usuario');
SET IDENTITY_INSERT roles OFF;
");
        }

        public async Task ResetAsync()
        {
            await using var db = new SqlConnection(TestConnectionString);
            await db.OpenAsync();
            await EjecutarAsync(db, @"
DELETE FROM imagenes_articulos;
DELETE FROM favoritos;
DELETE FROM reportes;
DELETE FROM articulos;
DELETE FROM usuario_roles;
DELETE FROM usuarios;
DELETE FROM categorias;
");
        }

        public VintalooDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<VintalooDbContext>()
                .UseSqlServer(TestConnectionString)
                .Options;

            return new VintalooDbContext(options);
        }

        public async Task DisposeAsync()
        {
            SqlConnection.ClearAllPools();

            try
            {
                await using var master = new SqlConnection(MasterConnectionString);
                await master.OpenAsync();
                await EjecutarAsync(master, $@"
IF DB_ID(N'{TestDatabase}') IS NOT NULL
BEGIN
    ALTER DATABASE [{TestDatabase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{TestDatabase}];
END");
            }
            catch
            {
                // La siguiente ejecución vuelve a crear la base de pruebas.
            }
        }

        private static async Task<bool> ExisteBaseAsync(SqlConnection connection, string nombre)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT DB_ID(@nombre)";
            command.Parameters.AddWithValue("@nombre", nombre);
            var result = await command.ExecuteScalarAsync();
            return result != DBNull.Value && result != null;
        }

        private static async Task EjecutarAsync(SqlConnection connection, string sql)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 120;
            await command.ExecuteNonQueryAsync();
        }

        private static string ObtenerConexionBase()
        {
            var conexionEnv = Environment.GetEnvironmentVariable("VINTALOO_TEST_SQL_CONNECTION");
            if (!string.IsNullOrWhiteSpace(conexionEnv))
                return conexionEnv;

            var directorio = new DirectoryInfo(AppContext.BaseDirectory);
            while (directorio != null)
            {
                foreach (var archivo in new[] { "appsettings.Development.json", "appsettings.json" })
                {
                    var appsettings = Path.Combine(directorio.FullName, "Vintaloo.WebApp", archivo);
                    if (!File.Exists(appsettings))
                        continue;

                    using var documento = JsonDocument.Parse(File.ReadAllText(appsettings));
                    if (documento.RootElement.TryGetProperty("ConnectionStrings", out var conexiones) &&
                        conexiones.TryGetProperty("DefaultConnection", out var porDefecto) &&
                        !string.IsNullOrWhiteSpace(porDefecto.GetString()))
                    {
                        return porDefecto.GetString()!;
                    }
                }

                directorio = directorio.Parent;
            }

            return ConexionPorDefecto;
        }

        private static void ValidarNombreBase(string nombre)
        {
            if (!Regex.IsMatch(nombre, "^[A-Za-z0-9_]+$"))
                throw new ArgumentException($"Nombre de base de datos inválido: '{nombre}'.", nameof(nombre));
        }
    }

    [CollectionDefinition("SqlServer")]
    public class SqlServerCollection : ICollectionFixture<SqlServerTestFixture>
    {
    }
}
