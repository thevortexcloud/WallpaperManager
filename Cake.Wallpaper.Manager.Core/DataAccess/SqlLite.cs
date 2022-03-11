using System.Data;
using System.Diagnostics.CodeAnalysis;
using Cake.Wallpaper.Manager.Core.Models;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Cake.Wallpaper.Manager.Core.DataAccess;

internal sealed class SqlLite : SqlLiteBase {
    #region Public constructor
    public SqlLite(string connectionString) : base(connectionString) {
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Retrieves a list of franchises for a person
    /// </summary>
    /// <param name="personID">The person to search franchises for</param>
    /// <returns>A filtered list of franchises for the person</returns>
    /// <remarks>This does NOT retrieve the full franchise tree for every franchises</remarks>
    public IAsyncEnumerable<Franchise> RetrieveFranchisesForPerson(int personID) {
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"SELECT fran.id AS id, fran.name as NAME, fran.parentid AS ParentId, 0 AS level FROM Franchise fran
INNER JOIN PeopleFranchises PF on fran.Id = PF.Franchise
WHERE PF.Person = @person"
        };
        cmd.Parameters.Add("@person", SqliteType.Integer).Value = personID;

        return this.ParseFranchiseListQuery(cmd);
    }


    /// <summary>
    /// Retrieves a list of ALL franchises, complete with correct nesting of child franchises
    /// </summary>
    /// <returns>A list of all top level and child franchises</returns>
    public IAsyncEnumerable<Franchise> RetrieveFranchises() {
        SqliteCommand cmd = new SqliteCommand() {
            //Recursive Common Table Expression can find a parent and all children
            CommandText = @"WITH RECURSIVE under_part(id, name, parentid, level) AS (
            SELECT Id, Name, ParentId, 0
            FROM Franchise
            WHERE Franchise.ParentId IS NULL
            UNION ALL
            SELECT Franchise.id, Franchise.name, Franchise.parentid, under_part.level + 1
            FROM Franchise,
            under_part
            WHERE Franchise.ParentId = under_part.id
            )
            SELECT id, name, parentid, level
            FROM under_part
            ;"
        };

        return this.ParseFranchiseListQuery(cmd);
    }

    private async IAsyncEnumerable<Franchise> ParseFranchiseListQuery(SqliteCommand cmd) {
        using (var rdr = await this.ExecuteDataReaderAsync(cmd)) {
            if (rdr.HasRows) {
                int idOrdinal = rdr.GetOrdinal("id"),
                    parnetIdOrdinal = rdr.GetOrdinal("parentid"),
                    nameOrdinal = rdr.GetOrdinal("name"),
                    levelOrdinal = rdr.GetOrdinal("level");

                var franchises = new List<Franchise>();
                var result = new HashSet<Franchise>();
                while (await rdr.ReadAsync()) {
                    franchises.Add(new Franchise() {
                        Name = rdr.GetString(nameOrdinal),
                        ID = rdr.GetInt32(idOrdinal),
                        ParentID = await rdr.IsDBNullAsync(parnetIdOrdinal) ? null : rdr.GetInt32(parnetIdOrdinal),
                    });
                }

                //We can use a dictionary to build the child parent relationships, keyed by the id
                var dict = franchises.ToDictionary(o => o.ID);
                foreach (var val in dict.Values) {
                    //If we have no parent ID this is a top level item and can be added directly
                    if (val.ParentID is null) {
                        result.Add(val);
                        //Check if we have a parent we can link to
                    } else if (dict.ContainsKey(val.ParentID.Value)) {
                        dict[val.ParentID.Value].ChildFranchises.Add(val);
                    } else {
                        //All else fails just add the value to the list as a fake top level item
                        //This could happen if the parent was deleted for some reason
                        result.Add(val);
                    }
                }

                //HACK: FOR NOW JUST YIED EVERYTHING
                foreach (var resultval in result) {
                    yield return resultval;
                }
            }
        }
    }

    /// <summary>
    /// Retrieves a list of all wallpapers
    /// </summary>
    /// <returns>A list of all wallpapers contained in the database</returns>
    public async IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync() {
        SqliteCommand wallpapercmd = new SqliteCommand() {
            CommandText = @"SELECT id,Name,DateAdded, Author, FileName, Source FROM Wallpapers",
        };
        await using (var wallpaperrdr = await this.ExecuteDataReaderAsync(wallpapercmd)) {
            if (wallpaperrdr.HasRows) {
                int wallpaperIdOrdinal = wallpaperrdr.GetOrdinal(nameof(Models.Wallpaper.ID)),
                    wallpaperNameOrdinal = wallpaperrdr.GetOrdinal(nameof(Models.Wallpaper.Name)),
                    dateAddedOrdinal = wallpaperrdr.GetOrdinal(nameof(Models.Wallpaper.DateAdded)),
                    authorOrdinal = wallpaperrdr.GetOrdinal(nameof(Models.Wallpaper.Author)),
                    fileNameOrdinal = wallpaperrdr.GetOrdinal(nameof(Models.Wallpaper.FileName)),
                    sourceOrdinal = wallpaperrdr.GetOrdinal(nameof(Models.Wallpaper.Source));
                while (await wallpaperrdr.ReadAsync()) {
                    var wallpaper = new Models.Wallpaper() {
                        ID = wallpaperrdr.GetInt32(wallpaperIdOrdinal),
                        Name = await wallpaperrdr.IsDBNullAsync(wallpaperNameOrdinal) ? null : wallpaperrdr.GetString(wallpaperNameOrdinal),
                        Author = await wallpaperrdr.IsDBNullAsync(authorOrdinal) ? null : wallpaperrdr.GetString(authorOrdinal),
                        Source = await wallpaperrdr.IsDBNullAsync(sourceOrdinal) ? null : wallpaperrdr.GetString(sourceOrdinal),
                        //DateAdded = rdr.GetDateTime(dateAddedOrdinal),
                        FileName = wallpaperrdr.GetString(fileNameOrdinal)
                    };

                    SqliteCommand personcmd = new SqliteCommand() {
                        CommandText = @"SELECT p.id, name, primaryfranchise
FROM WallpaperPeople
INNER JOIN People P on P.Id = WallpaperPeople.PersonID
WHERE WallpaperID = @wallpaper"
                    };
                    personcmd.Parameters.Add("@wallpaper", SqliteType.Integer).Value = wallpaper.ID;

                    using (var personrdr = await this.ExecuteDataReaderAsync(personcmd)) {
                        if (personrdr.HasRows) {
                            int personIDOrdinal = personrdr.GetOrdinal(nameof(Person.ID)),
                                personNameOrdinal = personrdr.GetOrdinal(nameof(Person.Name)),
                                primaryFranchiseOrdinal = personrdr.GetOrdinal(nameof(Person.PrimaryFranchise));

                            while (await personrdr.ReadAsync()) {
                                var person = new Person() {
                                    ID = personrdr.GetInt32(personIDOrdinal),
                                    Name = personrdr.GetString(personNameOrdinal),
                                    Franchises = await this.RetrieveFranchisesForPerson(personrdr.GetInt32(personIDOrdinal)).ToHashSetAsync(),
                                };
                                var primaryfranchiseid = personrdr.GetInt32(primaryFranchiseOrdinal);
                                if (primaryfranchiseid != 0) {
                                    person.PrimaryFranchise = DataUtilities.FlattenFranchiseList(person.Franchises).FirstOrDefault(o => o.ID == primaryfranchiseid);
                                }

                                wallpaper.People.Add(person);
                            }
                        }
                    }

                    yield return wallpaper;
                }
            }
        }
    }

    /// <summary>
    /// Retrieves a list of all people
    /// </summary>
    /// <returns>A list of all people that exist in the system</returns>
    public async IAsyncEnumerable<Person> RetrievePeople() {
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"SELECT Id, Name, PrimaryFranchise
FROM People;"
        };

        using (var rdr = await this.ExecuteDataReaderAsync(cmd)) {
            if (rdr.HasRows) {
                int idOrdinal = rdr.GetOrdinal("Id"),
                    nameOrdinal = rdr.GetOrdinal("Name"),
                    primaryFranchiseOrdinal = rdr.GetOrdinal("PrimaryFranchise");
                while (await rdr.ReadAsync()) {
                    var person = new Person() {
                        Name = rdr.GetString(nameOrdinal),
                        ID = rdr.GetInt32(idOrdinal),
                    };

                    //Find every franchise this person is linked to
                    var franchisecmd = new SqliteCommand() {
                        CommandText = @"SELECT fran.id AS id, fran.name as NAME, fran.parentid AS ParentId FROM Franchise fran
                                                                            INNER JOIN PeopleFranchises PF on fran.Id = PF.Franchise
WHERE PF.Person = @person",
                        CommandType = CommandType.Text
                    };
                    franchisecmd.Parameters.Add("@person", SqliteType.Integer).Value = person.ID;

                    using (var franchsiserdr = await this.ExecuteDataReaderAsync(franchisecmd)) {
                        if (franchsiserdr.HasRows) {
                            int franchiseIdOrdinal = franchsiserdr.GetOrdinal("id"),
                                franchiseNameOrdinal = franchsiserdr.GetOrdinal("name"),
                                parentOrdinal = franchsiserdr.GetOrdinal("parentid");
//Build a simple flat list of franchsies and add it to the person as it's easier than worrying about the links which really only the UI cares about
                            var rawfranchises = new List<Franchise>();
                            while (await franchsiserdr.ReadAsync()) {
                                rawfranchises.Add(new Franchise() {
                                    Name = franchsiserdr.GetString(franchiseNameOrdinal),
                                    ID = franchsiserdr.GetInt32(franchiseIdOrdinal),
                                    ParentID = (await franchsiserdr.IsDBNullAsync(parentOrdinal)) ? null : franchsiserdr.GetInt32(parentOrdinal)
                                });
                            }

                            person.Franchises.UnionWith(rawfranchises);
                        }

                        //Find the primary franchise in the list and set it
                        var primaryfranchiseid = rdr.GetInt32(primaryFranchiseOrdinal);
                        if (primaryfranchiseid != 0) {
                            person.PrimaryFranchise = DataUtilities.FlattenFranchiseList(person.Franchises).FirstOrDefault(o => o.ID == primaryfranchiseid);
                        }

                        yield return person;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Deletes the given person from the database and some related data
    /// </summary>
    /// <param name="personID">The person to delete</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="personID"/> is 0</exception>
    public Task DeletePersonAsync(int personID) {
        //Validate values
        if (personID == 0) {
            throw new ArgumentException("Person id can not be 0", nameof(personID));
        }

        //Delete links for the person and the person themselves
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"
BEGIN TRANSACTION;
            DELETE FROM People WHERE ID = @person;
            DELETE FROM PeopleFranchises WHERE Person = @person;
COMMIT;"
        };
        cmd.Parameters.Add("@person", SqliteType.Integer).Value = personID;

        return this.ExecuteNonQueryAsync(cmd);
    }

    /// <summary>
    /// Inserts a person into the database, or updates them if they already exist
    /// </summary>
    /// <param name="person">The person to insert</param>
    /// <exception cref="ArgumentNullException">Thrown when <param name="person"> is null</param></exception>
    public async Task InsertPersonAsync(Person person) {
        if (person is null) {
            throw new ArgumentNullException(nameof(person));
        }

        using (var tran = await this.CreateTransaction()) {
            try {
                SqliteCommand cmd = new SqliteCommand() {
                    CommandText = @"
INSERT OR REPLACE
INTO People (id, Name, PrimaryFranchise)
VALUES (@id, @Name, @PrimaryFranchise);

SELECT last_insert_rowid()",
                    CommandType = CommandType.Text,
                };

                cmd.Parameters.Add("@id", SqliteType.Integer).Value = person.ID == 0 ? DBNull.Value : person.ID;
                cmd.Parameters.Add("@Name", SqliteType.Text).Value = person.Name;
                cmd.Parameters.Add("@PrimaryFranchise", SqliteType.Integer).Value = person?.PrimaryFranchise?.ID == null ? DBNull.Value : person.PrimaryFranchise.ID;

                //Perso is an auto increment column, however we do  not always want to retrieve the new value
                //from the database if we are just doing a simple update
                long personid = 0;
                if (person.ID == 0) {
                    personid = await this.ExecuteScalerAsync<long?>(cmd, tran) ?? 0;
                } else {
                    personid = person.ID;
                    await this.ExecuteNonQueryAsync(cmd, tran);
                }

                //Delete all franchise links as it's easier than trying to diff them
                cmd = new SqliteCommand() {
                    CommandText = "DELETE FROM PeopleFranchises WHERE Person = @Person",
                };
                cmd.Parameters.Add("@Person", SqliteType.Integer).Value = personid;
                await this.ExecuteNonQueryAsync(cmd, tran);

                //For every franchise we have, we need to insert a new link
                foreach (var franchise in person.Franchises) {
                    cmd = new SqliteCommand() {
                        CommandText = @"
INSERT OR
REPLACE INTO PeopleFranchises (Franchise, Person)
VALUES (@Franchise, @Person);",
                    };

                    cmd.Parameters.Add("@Franchise", SqliteType.Integer).Value = franchise.ID;
                    cmd.Parameters.Add("@Person", SqliteType.Integer).Value = personid;

                    await this.ExecuteNonQueryAsync(cmd, tran);
                }

                await tran.CommitAsync();
            } catch {
                await tran.RollbackAsync();
                throw;
            }
        }
    }
    #endregion
}