using System.Data;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Transactions;
using Microsoft.Data.Sqlite;

namespace Cake.Wallpaper.Manager.Core.DataAccess;

public abstract class SqlLiteBase : IDisposable, IAsyncDisposable {
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    private SqliteConnection? Connection {
        get {
            if (this._connection is null) {
                this.OpenConnection();
                return this._connection;
            }

            return this._connection;
        }
        set => this._connection = value;
    }

    protected SqlLiteBase(string connectionString) {
        this._connectionString = connectionString;
        this.Connection = new SqliteConnection(_connectionString);
    }

    protected async ValueTask<SqliteTransaction> CreateTransactionAsync() {
        await this.OpenConnectionAsync();
        return (SqliteTransaction) await this.Connection.BeginTransactionAsync();
    }

    protected async Task ExecuteNonQueryAsync(SqliteCommand cmd, SqliteTransaction transaction = null) {
        await this.OpenConnectionAsync();
        cmd.Connection = this.Connection;
        cmd.Transaction = transaction;
        await cmd.ExecuteNonQueryAsync();
    }

    protected void ExecuteNonQuery(SqliteCommand cmd, SqliteTransaction transaction = null) {
        this.OpenConnection();
        cmd.Connection = this.Connection;
        cmd.Transaction = transaction;
        cmd.ExecuteNonQuery();
    }

    protected async Task<SqliteDataReader> ExecuteDataReaderAsync(SqliteCommand cmd) {
        await this.OpenConnectionAsync();
        cmd.Connection = this.Connection;
        return await cmd.ExecuteReaderAsync(CommandBehavior.Default);
    }

    protected SqliteDataReader ExecuteDataReader(SqliteCommand cmd) {
        this.OpenConnection();
        cmd.Connection = this.Connection;
        return cmd.ExecuteReader(CommandBehavior.Default);
    }

    protected async ValueTask<T> ExecuteScalerAsync<T>(SqliteCommand cmd, T? defaultvalue = default) {
        await this.OpenConnectionAsync();
        cmd.Connection = this.Connection;
        var value = await cmd.ExecuteScalarAsync();
        if (value is T casted) {
            return casted;
        }

        throw new InvalidCastException("Specified cast is not valid");
    }

    protected T ExecuteScaler<T>(SqliteCommand cmd, T? defaultvalue = default) {
        this.OpenConnection();
        cmd.Connection = this.Connection;
        var value = cmd.ExecuteScalar();
        if (value is T casted) {
            return casted;
        }

        throw new InvalidCastException("Specified cast is not valid");
    }

    protected ValueTask<T> ExecuteScalerAsync<T>(SqliteCommand cmd, SqliteTransaction transaction, T defaultValue = default) {
        cmd.Transaction = transaction;
        return this.ExecuteScalerAsync(cmd, defaultValue);
    }

    private async Task OpenConnectionAsync() {
        if (this.Connection == null) {
            this.Connection = new SqliteConnection(this._connectionString);
        }

        if (this.Connection.State is ConnectionState.Closed or ConnectionState.Broken) {
            await this.Connection.OpenAsync();
        }
    }

    private void OpenConnection() {
        if (this.Connection == null) {
            this.Connection = new SqliteConnection(this._connectionString);
        }

        if (this.Connection.State is ConnectionState.Closed or ConnectionState.Broken) {
            this.Connection.OpenAsync();
        }
    }

    public void Dispose() {
        this.Connection?.Close();
        this.Connection?.Dispose();
    }

    public async ValueTask DisposeAsync() {
        await (this._connection?.CloseAsync() ?? Task.CompletedTask);
        await (this.Connection?.DisposeAsync() ?? ValueTask.CompletedTask);
    }
}