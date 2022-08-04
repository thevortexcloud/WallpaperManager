using System;
using System.Data;
using System.IO;
using Cake.Wallpaper.Manager.Core.DataAccess;
using Microsoft.Data.Sqlite;

namespace Cake.Wallpaper.Manager.Tests.Core.WallpaperRepositories;

public class SqlLiteMemory : SqlLite {
    public SqlLiteMemory() : base("Data Source=:memory:;") {
    }

    public T ExecuteScalar<T>(SqliteCommand cmd) {
        return base.ExecuteScaler<T>(cmd);
    }

    public void ExecuteNonQuery(SqliteCommand command) {
        base.ExecuteNonQuery(command);
    }

    public SqliteDataReader ExecuteReader(SqliteCommand command) {
        return base.ExecuteDataReader(command);
    }

    public void ScaffoldDB() {
        var schemaFile = new FileInfo("/home/zac/Projects/Cake.Wallpaper.Manager/Cake.Wallpaper.Manager.Core/Schema.sql");
        if (!schemaFile.Exists) {
            throw new FileNotFoundException("Schema was not found");
        }

        string schemaText = null;
        using (var fs = schemaFile.OpenRead()) {
            using (var sr = new StreamReader(fs)) {
                schemaText = sr.ReadToEnd();
            }
        }

        if (string.IsNullOrWhiteSpace(schemaText)) {
            throw new ArgumentNullException(nameof(schemaText), "No schema was found");
        }

        SqliteCommand cmd = new SqliteCommand() {
            CommandText = schemaText,
            CommandType = CommandType.Text,
        };

        this.ExecuteNonQuery(cmd);
    }
}