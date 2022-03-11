using System.Data;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Transactions;
using Microsoft.Data.Sqlite;

namespace Cake.Wallpaper.Manager.Core.DataAccess;

internal abstract class SqlLiteBase : IDisposable, IAsyncDisposable {
    private readonly string _connectionString;
    private SqliteConnection _connection;

    public SqlLiteBase(string connectionString) {
        this._connectionString = connectionString;
        this._connection = new SqliteConnection(_connectionString);
    }

    protected async ValueTask<SqliteTransaction> CreateTransaction() {
        await this.OpenConnection();
        return (SqliteTransaction) await this._connection.BeginTransactionAsync();
    }

    protected async Task ExecuteNonQueryAsync(SqliteCommand cmd, SqliteTransaction transaction = null) {
        await this.OpenConnection();
        cmd.Connection = this._connection;
        cmd.Transaction = transaction;
        await cmd.ExecuteNonQueryAsync();
    }


    protected async Task<SqliteDataReader> ExecuteDataReaderAsync(SqliteCommand cmd) {
        await this.OpenConnection();
        cmd.Connection = this._connection;
        return await cmd.ExecuteReaderAsync(CommandBehavior.Default);
    }

    protected async ValueTask<T> ExecuteScalerAsync<T>(SqliteCommand cmd, T defaultvalue = default) {
        await this.OpenConnection();
        cmd.Connection = this._connection;
        var value = await cmd.ExecuteScalarAsync();
        if (value is T) {
            if (value is null) {
                return defaultvalue;
            } else {
                return (T) value;
            }
        }

        throw new InvalidCastException("Specified cast is not valid");
    }

    protected ValueTask<T> ExecuteScalerAsync<T>(SqliteCommand cmd, SqliteTransaction transaction, T defaultValue = default) {
        cmd.Transaction = transaction;
        return this.ExecuteScalerAsync(cmd, defaultValue);
    }

    private async Task OpenConnection() {
        if (this._connection == null) {
            this._connection = new SqliteConnection(this._connectionString);
        }

        if (this._connection.State is ConnectionState.Closed or ConnectionState.Broken) {
            await this._connection.OpenAsync();
        }
    }

    public void Dispose() {
        this._connection.Dispose();
    }

    public ValueTask DisposeAsync() {
        return this._connection.DisposeAsync();
    }
}