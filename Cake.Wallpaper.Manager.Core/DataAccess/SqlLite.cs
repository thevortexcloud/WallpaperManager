using System.Data;
using Cake.Wallpaper.Manager.Core.Models;
using Microsoft.Data.Sqlite;

namespace Cake.Wallpaper.Manager.Core.DataAccess;

internal sealed class SqlLite : SqlLiteBase {
    #region Public constructor
    public SqlLite(string connectionString) : base(connectionString) {
    }
    #endregion

    #region Public methods
    public Task InsertFranchiseAsync(Franchise franchise) {
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"INSERT INTO main.Franchise (Id, Name, ParentId)
            values (@id, @Name, @parent)
            ON CONFLICT(id) DO
            UPDATE SET
            Name = @Name,
            ParentId = @parent
            WHERE id = @id;"
        };
        cmd.Parameters.Add("@id", SqliteType.Integer).Value = franchise.ID == 0 ? DBNull.Value : franchise.ID;
        cmd.Parameters.Add("@Name", SqliteType.Text).Value = franchise.Name;
        cmd.Parameters.Add("@parent", SqliteType.Integer).Value = franchise?.ParentID == null ? DBNull.Value : franchise.ParentID;

        return this.ExecuteNonQueryAsync(cmd);
    }

    public async Task InsertFranchiseListAsync(IEnumerable<Franchise> franchises) {
        await using (var tran = await this.CreateTransactionAsync()) {
            try {
                foreach (var franchise in franchises) {
                    SqliteCommand cmd = new SqliteCommand() {
                        CommandText = @"INSERT INTO main.Franchise (Id, Name, ParentId)
                                        values (@id, @name, @parent)
                                        ON CONFLICT(id) DO UPDATE SET
                                        Name = @name,
                                        ParentId = @parent
                                        WHERE id = @id;",
                        Transaction = tran,
                    };
                    cmd.Parameters.Add("@id", SqliteType.Integer).Value = franchise.ID == 0 ? DBNull.Value : franchise.ID;
                    cmd.Parameters.Add("@name", SqliteType.Text).Value = franchise.Name;
                    cmd.Parameters.Add("@parent", SqliteType.Integer).Value = franchise?.ParentID == null ? DBNull.Value : franchise.ParentID;

                    await this.ExecuteNonQueryAsync(cmd, tran);
                }

                await tran.CommitAsync();
            } catch {
                await tran.RollbackAsync();
                throw;
            }
        }
    }

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
 ORDER BY under_part.level + 1 DESC,
             parentid, name
            )
            SELECT id, name, parentid, level
            FROM under_part;"
        };

        return this.ParseFranchiseListQuery(cmd);
    }

    /// <summary>
    /// Retrieves a filtered list of franchises, complete with nesting of child franchises if the parent is included in the result
    /// </summary>
    /// <returns>A list of all top level and child franchises</returns>
    public IAsyncEnumerable<Franchise> RetrieveFranchises(string filter) {
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
                ORDER BY under_part.level + 1 DESC,
             parentid, name
            )
            SELECT id, name, parentid, level
            FROM under_part
            WHERE name LIKE '%' || @filter || '%'
            ;"
        };
        cmd.Parameters.Add("@filter", SqliteType.Text).Value = filter;

        return this.ParseFranchiseListQuery(cmd);
    }


    /// <summary>
    /// Retrieves a list of all wallpapers
    /// </summary>
    /// <returns>A list of all wallpapers contained in the database</returns>
    public async IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync(string? searchTerm) {
        SqliteCommand? wallpapercmd = null;
        if (string.IsNullOrWhiteSpace(searchTerm)) {
            wallpapercmd = new SqliteCommand() {
                CommandText = @"SELECT id,Name,DateAdded, Author, FileName, Source FROM Wallpapers",
            };
        } else {
            wallpapercmd = new SqliteCommand() {
                CommandText = @"
SELECT wallpapers.id,wallpapers.Name,DateAdded, Author, FileName, Source FROM Wallpapers wallpapers
LEFT OUTER JOIN WallpaperPeople WP on Wallpapers.id = WP.WallpaperID
LEFT OUTER JOIN People P on WP.PersonID = P.Id
WHERE wallpapers.name LIKE '%' || @name ||'%' OR wallpapers.Filename LIKE '%' || @name ||'%' OR p.name LIKE '%'|| @name || '%'
GROUP BY wallpapers.id, wallpapers.Name, DateAdded, Author, FileName, Source",
            };
            wallpapercmd.Parameters.Add("@name", SqliteType.Text).Value = searchTerm;
        }

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
                        DateAdded = DateTimeOffset.FromUnixTimeSeconds(wallpaperrdr.GetInt32(dateAddedOrdinal)).DateTime,
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
                                //TODO: Do a join on this since we can do it entirely in the DB
                                int? primaryfranchiseid = await personrdr.IsDBNullAsync(primaryFranchiseOrdinal) ? null : personrdr.GetInt32(primaryFranchiseOrdinal);
                                if (primaryfranchiseid.HasValue && primaryfranchiseid != 0) {
                                    person.PrimaryFranchise = person.Franchises.FirstOrDefault(o => o.ID == primaryfranchiseid);
                                }

                                wallpaper.People.Add(person);
                            }
                        }
                    }

                    //TODO: Fix the depth here so it shows the correct depth for the franchise
                    SqliteCommand wallpaperfranchisecmd = new SqliteCommand() {
                        CommandText = @"SELECT Id, Name, ParentId, 0 as level FROM WallpaperFranchise
INNER JOIN Franchise F on F.Id = WallpaperFranchise.FranchiseID
WHERE WallpaperID = @wallpaper"
                    };
                    wallpaperfranchisecmd.Parameters.Add("@wallpaper", SqliteType.Integer).Value = wallpaper.ID;

                    wallpaper.Franchises.AddRange(this.ParseFranchiseListQuery(wallpaperfranchisecmd).ToEnumerable());

                    yield return wallpaper;
                }
            }
        }
    }

    /// <summary>
    /// Retrieves a list of all wallpapers
    /// </summary>
    /// <returns>A list of all wallpapers contained in the database</returns>
    public IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync() {
        return this.RetrieveWallpapersAsync(null);
    }

    /// <summary>
    /// Retrieves a list of all people
    /// </summary>
    /// <returns>A list of all people that exist in the system</returns>
    public IAsyncEnumerable<Person> RetrievePeople() {
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"SELECT Id, Name, PrimaryFranchise
FROM People;"
        };

        return this.ParseRetrievePersonCommand(cmd);
    }

    /// <summary>
    /// Retrieves a filtered list of people from the database
    /// </summary>
    /// <param name="searchTerm">The search term to filter the results by</param>
    /// <returns>A list of all people that exist in the system</returns>
    public IAsyncEnumerable<Person> RetrievePeople(string searchTerm) {
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"SELECT people.Id, people.Name, PrimaryFranchise
FROM People people
LEFT OUTER JOIN PeopleFranchises PF on People.Id = PF.Person
LEFT OUTER JOIN Franchise F on F.Id = PF.Franchise
WHERE people.Name LIKE '%' || @term || '%' OR f.Name LIKE '%' || @term || '%'
GROUP BY people.Id, people.Name, PrimaryFranchise"
        };
        cmd.Parameters.Add("@term", SqliteType.Text).Value = searchTerm;

        return this.ParseRetrievePersonCommand(cmd);
    }


    /// <summary>
    /// Deletes the given person from the person related related tables
    /// </summary>
    /// <param name="personID">The person to delete</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="personID"/> is 0</exception>
    public async Task DeletePersonAsync(int personID) {
        //Validate values
        if (personID == 0) {
            throw new ArgumentException("Person id can not be 0", nameof(personID));
        }

        await using (var tran = await this.CreateTransactionAsync()) {
            //Delete links for the person and the person themselves
            SqliteCommand cmd = new SqliteCommand() {
                CommandText = @"
            DELETE FROM PeopleFranchises WHERE Person = @person;
            DELETE FROM People WHERE ID = @person;
            DELETE FROM WallpaperPeople WHERE PersonID = @person",
                Transaction = tran,
                CommandType = CommandType.Text,
            };
            cmd.Parameters.Add("@person", SqliteType.Integer).Value = personID;
            try {
                await this.ExecuteNonQueryAsync(cmd, tran);
                await tran.CommitAsync();
            } catch {
                await tran.RollbackAsync();
                throw;
            }
        }
    }

    /// <summary>
    /// Inserts a person into the database, or updates them if they already exist
    /// </summary>
    /// <param name="person">The person to insert</param>
    /// <exception cref="ArgumentNullException">Thrown when <param name="person"> is null</param></exception>
    public async Task InsertPersonAsync(Person person) {
        using (var tran = await this.CreateTransactionAsync()) {
            try {
                await this.InsertPerson(person, tran);
                await tran.CommitAsync();
            } catch {
                await tran.RollbackAsync();
                throw;
            }
        }
    }


    /// <summary>
    /// Attempts to insert or update the given wallpaper transactionally with all related linked data
    /// </summary>
    /// <param name="wallpaper">The wallpaper to attempt to insert</param>
    public async Task SaveWallpaperAsync(Models.Wallpaper wallpaper) {
        //Create a transaction since we are going to be doing a lot of things that could fail
        using (var tran = await this.CreateTransactionAsync()) {
            try {
                //Update/insert the wallpaper header data
                var createdID = await this.CreateOrInsertWallpaperHeaderAsync(wallpaper, tran);
                //If we have a different id (id of 0 is for new wallpapers) we need to update the references we have since it's used for all the links we are about to do
                //Luckily records make this very simple for us
                if (createdID != wallpaper.ID) {
                    wallpaper = wallpaper with {
                        ID = createdID
                    };
                }

                //Create the links in the people table
                await this.CreateOrUpdateWallpaperPeopleLinkAsync(wallpaper, tran);
                //Create the links in the franchise tables
                await this.CreateOrUpdateWallpaperFranchisesLinkAsync(wallpaper, tran);

                //If everything worked, commit the transaction
                await tran.CommitAsync();
            } catch {
                //If we get any errors at all, abort and rethrow
                await tran.RollbackAsync();
                throw;
            }
        }
    }

    /// <summary>
    /// Deletes the given franchise from the franchise table
    /// </summary>
    /// <param name="franchiseID">The franchise to delete</param>
    /// <returns></returns>
    public Task DeleteFranchiseAsync(int franchiseID) {
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = "DELETE FROM Franchise WHERE Id = @id",
            CommandType = CommandType.Text,
        };
        cmd.Parameters.Add("@id", SqliteType.Integer).Value = franchiseID;
        return this.ExecuteNonQueryAsync(cmd);
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Attempts to execute the given command and parse a list of franchises
    /// </summary>
    /// <param name="cmd">The command to execute</param>
    /// <returns>A list of franchises found</returns>
    private async IAsyncEnumerable<Franchise> ParseFranchiseListQuery(SqliteCommand cmd) {
        using (var rdr = await this.ExecuteDataReaderAsync(cmd)) {
            if (rdr.HasRows) {
                int idOrdinal = rdr.GetOrdinal("id"),
                    parentIDOrdinal = rdr.GetOrdinal("parentid"),
                    nameOrdinal = rdr.GetOrdinal("name"),
                    levelOrdinal = rdr.GetOrdinal("level");

                while (await rdr.ReadAsync()) {
                    yield return new Franchise() {
                        Name = rdr.GetString(nameOrdinal),
                        ID = rdr.GetInt32(idOrdinal),
                        ParentID = await rdr.IsDBNullAsync(parentIDOrdinal) ? null : rdr.GetInt32(parentIDOrdinal),
                        Depth = rdr.GetInt32(levelOrdinal)
                    };
                }
            }
        }
    }

    /// <summary>
    ///  Attempts to parse the given command and load instantiate a person object with all related data
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private async IAsyncEnumerable<Person> ParseRetrievePersonCommand(SqliteCommand cmd) {
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
                        //TODO: Do this in the database instead
                        int? primaryfranchiseid = await rdr.IsDBNullAsync(primaryFranchiseOrdinal) ? null : rdr.GetInt32(primaryFranchiseOrdinal);
                        if (primaryfranchiseid != 0) {
                            person.PrimaryFranchise = person.Franchises.FirstOrDefault(o => o.ID == primaryfranchiseid);
                        }

                        yield return person;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Inserts a person into the database, or updates them if they already exist
    /// </summary>
    /// <param name="person">The person to insert</param>
    /// <param name="transaction">The transaction to execute commands with</param>
    /// <exception cref="ArgumentNullException">Thrown when <param name="person"> is null</param></exception>
    private async Task InsertPerson(Person person, SqliteTransaction transaction) {
        if (person is null) {
            throw new ArgumentNullException(nameof(person));
        }

        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"
INSERT INTO People (id, Name, PrimaryFranchise)
VALUES (@id, @Name, @PrimaryFranchise)
ON CONFLICT(id) DO UPDATE SET
Name = @Name,
     PrimaryFranchise = @PrimaryFranchise
WHERE id = @id
RETURNING id;",
            CommandType = CommandType.Text,
            Transaction = transaction,
        };

        cmd.Parameters.Add("@id", SqliteType.Integer).Value = person.ID == 0 ? DBNull.Value : person.ID;
        cmd.Parameters.Add("@Name", SqliteType.Text).Value = person.Name;
        cmd.Parameters.Add("@PrimaryFranchise", SqliteType.Integer).Value = person?.PrimaryFranchise?.ID == null ? DBNull.Value : person.PrimaryFranchise.ID;

        //Person is an auto increment column, as such we should use the ID the database gives us
        long personid = 0;
        personid = await this.ExecuteScalerAsync<long>(cmd, transaction);

        //This should never happen, but it's worth doing as a sanity check in the event the SQL gets broken for some reason
        if (person!.ID != 0 && personid != person.ID) {
            throw new ApplicationException("The inserted or updated person did not return a consistent ID");
        }

        //Delete all franchise links for the given person as it's easier than trying to diff them, and since we are in a transaction we can easily roll back if we need to
        cmd = new SqliteCommand() {
            CommandText = "DELETE FROM PeopleFranchises WHERE Person = @Person",
            CommandType = CommandType.Text,
        };
        cmd.Parameters.Add("@Person", SqliteType.Integer).Value = personid;
        await this.ExecuteNonQueryAsync(cmd, transaction);

        //For every franchise we have, we need to insert a new link
        foreach (var franchise in person.Franchises) {
            cmd = new SqliteCommand() {
                CommandText = @"
INSERT INTO PeopleFranchises (Franchise, Person)
VALUES (@Franchise, @Person);",
            };

            cmd.Parameters.Add("@Franchise", SqliteType.Integer).Value = franchise.ID;
            cmd.Parameters.Add("@Person", SqliteType.Integer).Value = personid;

            await this.ExecuteNonQueryAsync(cmd, transaction);
        }
    }

    /// <summary>
    /// Attempts insert or update the given wallpaper's people links
    /// </summary>
    /// <param name="wallpaper">The wallpaper containing the franchises it should link to</param>
    /// <param name="transaction">The transaction to use for the operation</param>
    private async Task CreateOrUpdateWallpaperPeopleLinkAsync(Models.Wallpaper wallpaper, SqliteTransaction transaction) {
        //Delete all existing links since it's far easier than trying to work out what actually changed and in this instance nothing else should change
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"DELETE FROM WallpaperPeople WHERE WallpaperID = @WallpaperID",
            Transaction = transaction,
            CommandType = CommandType.Text,
        };
        cmd.Parameters.Add("@WallpaperID", SqliteType.Integer).Value = wallpaper.ID;
        await this.ExecuteNonQueryAsync(cmd, transaction);

        foreach (var person in wallpaper!.People!) {
            cmd = new SqliteCommand() {
                CommandText = @"INSERT INTO WallpaperPeople (WallpaperID, PersonID)
values (@WallpaperID, @PersonID);",
                Transaction = transaction,
                CommandType = CommandType.Text,
            };
            cmd.Parameters.Add("@WallpaperID", SqliteType.Integer).Value = wallpaper.ID;
            cmd.Parameters.Add("@PersonID", SqliteType.Integer).Value = person.ID;

            await this.ExecuteNonQueryAsync(cmd, transaction);
        }
    }

    /// <summary>
    /// Attempts insert or update the given wallpaper's franchise links
    /// </summary>
    /// <param name="wallpaper">The wallpaper containing the franchises it should link to</param>
    /// <param name="transaction">The transaction to use for the operation</param>
    private async Task CreateOrUpdateWallpaperFranchisesLinkAsync(Models.Wallpaper wallpaper, SqliteTransaction transaction) {
        if (wallpaper == null) {
            throw new ArgumentNullException(nameof(wallpaper));
        }

        //If this is blank we have no idea what to do
        if (wallpaper.Franchises == null) {
            return;
        }

        //Delete any existing links
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"DELETE FROM WallpaperFranchise WHERE WallpaperID = @WallpaperID;",
            CommandType = CommandType.Text,
            Transaction = transaction,
        };
        cmd.Parameters.Add("@WallpaperID", SqliteType.Integer).Value = wallpaper.ID;

        await this.ExecuteNonQueryAsync(cmd, transaction);

        //Now repopulate the links based on the data we have
        foreach (var franchise in wallpaper.Franchises) {
            cmd = new SqliteCommand() {
                CommandText = @"
INSERT INTO WallpaperFranchise (WallpaperID, FranchiseID)
values (@WallpaperID, @FranchiseID);",
                Transaction = transaction,
                CommandType = CommandType.Text,
            };
            cmd.Parameters.Add("@WallpaperID", SqliteType.Integer).Value = wallpaper.ID;
            cmd.Parameters.Add("@FranchiseID", SqliteType.Integer).Value = franchise.ID;

            await this.ExecuteNonQueryAsync(cmd, transaction);
        }
    }

    /// <summary>
    /// Attempts To create or insert a wallpaper into the wallpaper table
    /// </summary>
    /// <remarks>Does NOT insert related records such as people or franchises
    /// <para>The caller is responsible for committing or rolling back the transaction</para></remarks>
    /// <param name="wallpaper">The wallpaper to insert</param>
    /// <param name="transaction">The transaction to use for the insert</param>
    /// <returns>The ID of the wallpaper that was created or modified</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="wallpaper"/> is null</exception>
    private async Task<int> CreateOrInsertWallpaperHeaderAsync(Models.Wallpaper wallpaper, SqliteTransaction transaction) {
        if (wallpaper is null) {
            throw new ArgumentNullException(nameof(wallpaper));
        }

        //It's very important we do an UPSERT here or we will accidentally delete all the foreign key links
        SqliteCommand cmd = new SqliteCommand() {
            CommandText = @"
INSERT INTO Wallpapers (id, Name, DateAdded, FileName, Author, Source)
values (@id, @name, @dateadded, @filename, @author, @source)
 ON CONFLICT(id) DO UPDATE SET
Name = @name,
     DateAdded = @dateadded,
     FileName = @filename,
     Author = @author,
     Source = @source
WHERE id = @id
RETURNING id;
    --SELECT last_insert_rowid();",
            Transaction = transaction,
            CommandType = CommandType.Text,
        };
        cmd.Parameters.Add("@id", SqliteType.Integer).Value = wallpaper.ID == 0 ? DBNull.Value : wallpaper.ID;
        cmd.Parameters.Add("@name", SqliteType.Text).Value = string.IsNullOrWhiteSpace(wallpaper.Name) ? DBNull.Value : wallpaper.Name;
        cmd.Parameters.Add("@dateadded", SqliteType.Integer).Value = wallpaper.DateAdded == DateTime.MinValue || wallpaper.DateAdded == DateTime.UnixEpoch ? ((DateTimeOffset) DateTime.Now).ToUnixTimeSeconds() : ((DateTimeOffset) wallpaper.DateAdded).ToUnixTimeSeconds();
        cmd.Parameters.Add("@filename", SqliteType.Text).Value = wallpaper.FileName;
        cmd.Parameters.Add("@author", SqliteType.Text).Value = string.IsNullOrWhiteSpace(wallpaper.Author) ? DBNull.Value : wallpaper.Author;
        cmd.Parameters.Add("@source", SqliteType.Text).Value = string.IsNullOrWhiteSpace(wallpaper.Source) ? DBNull.Value : wallpaper.Source;

        //Unlikely this cast will cause an overflow, but it's probably something should know about just in case
        checked {
            var result = (int) await this.ExecuteScalerAsync<long>(cmd, transaction);
            //This really should never happen, but it does not hurt to be safe here as a basic sanity check
            if (wallpaper.ID != 0 && result != wallpaper.ID) {
                throw new ApplicationException("The inserted/updated wallpaper ID did NOT match what was attempted to be inserted or updated");
            }

            return result;
        }
    }
    #endregion
}