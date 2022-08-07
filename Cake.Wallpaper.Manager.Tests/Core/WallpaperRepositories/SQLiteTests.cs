using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Cake.Wallpaper.Manager.Core.DataAccess;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.Tests.Core.WallpaperRepositories;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.Core.WallpaperRepositories;

public class SQLiteTests {
    [Fact]
    public async void InsertPerson() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();
            var person = new Person() {
                Name = "Blah",
                ID = 1,
            };
            await db.InsertPersonAsync(person);

            SqliteCommand cmd = new SqliteCommand() {
                CommandText = "SELECT id FROM People",
                CommandType = CommandType.Text,
            };
            var result = db.ExecuteScalar<long>(cmd);

            Assert.Equal(person.ID, result);
        }
    }

    [Fact]
    public async void InsertPersonWithFranchise() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();
            var person = new Person() {
                Name = "Blah",
                ID = 1,
                Franchises = new HashSet<Franchise>() {
                    new Franchise() {
                        ID = 1,
                    }
                }
            };

            SqliteCommand franchiseInsertCmd = new SqliteCommand() {
                CommandText = @"INSERT INTO Franchise (Id, Name, ParentId) VALUES (1, 'f', NULL);"
            };
            db.ExecuteNonQuery(franchiseInsertCmd);

            await db.InsertPersonAsync(person);

            SqliteCommand cmd = new SqliteCommand() {
                CommandText = "SELECT id FROM People",
                CommandType = CommandType.Text,
            };
            var result = db.ExecuteScalar<long>(cmd);

            Assert.Equal(person.ID, result);

            cmd = new SqliteCommand() {
                CommandText = "SELECT Franchise FROM PeopleFranchises",
                CommandType = CommandType.Text,
            };
            result = db.ExecuteScalar<long>(cmd);
            Assert.Equal(person.Franchises.First().ID, result);
        }
    }

    [Fact]
    public async void AttemptToRetrieveFranchisesForPerson() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();


            SqliteCommand personInsertCommand = new SqliteCommand() {
                CommandText = @"
INSERT INTO Franchise (Id, Name, ParentId) VALUES (1, 'f', NULL);
INSERT INTO People (Id, Name, PrimaryFranchise) VALUES (1, 'fa', 1);
INSERT INTO PeopleFranchises (Franchise, Person) VALUES (1, 1)"
            };
            db.ExecuteNonQuery(personInsertCommand);

            var franchises = await db.RetrieveFranchisesForPersonAsync(1).ToListAsync();
            Assert.NotEmpty(franchises);
            Assert.Single(franchises);

            Assert.Equal(1, franchises.FirstOrDefault()?.ID);
        }
    }

    [Fact]
    public async void AttemptToInsertFranchise() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();
            var franchise = new Franchise() {
                Name = "Blah",
                ID = 1,
            };

            await db.InsertFranchiseAsync(franchise);

            SqliteCommand cmd = new SqliteCommand() {
                CommandText = "SELECT id FROM Franchise",
                CommandType = CommandType.Text,
            };
            var result = db.ExecuteScalar<long>(cmd);

            Assert.Equal(franchise.ID, result);
        }
    }

    [Fact]
    public async void InsertFranchiseWithParent() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();
            var franchise = new Franchise() {
                Name = "Blah",
                ID = 1,
            };

            var childFranchise = new Franchise() {
                Name = "Mega Blah",
                ID = 2,
                ParentID = franchise.ID,
            };

            await db.InsertFranchiseAsync(franchise);
            await db.InsertFranchiseAsync(childFranchise);

            SqliteCommand cmd = new SqliteCommand() {
                CommandText = "SELECT ParentId FROM Franchise WHERE Id = @id",
                CommandType = CommandType.Text,
            };
            cmd.Parameters.Add("@id", SqliteType.Integer).Value = childFranchise.ID;

            var result = db.ExecuteScalar<long>(cmd);

            Assert.Equal(franchise.ID, result);
        }
    }

    [Theory]
    [InlineData("f")]
    [InlineData("a")]
    public async void AttemptRetrieveFranchisesWithFilter(string searchTerm) {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();

            SqliteCommand cmd = new SqliteCommand() {
                CommandText = @"INSERT INTO Franchise (Id, Name, ParentId) VALUES (1, 'f', NULL);
INSERT INTO Franchise (Id, Name, ParentId) VALUES (2, 'a', NULL)"
            };
            db.ExecuteNonQuery(cmd);

            var result = await db.RetrieveFranchisesAsync(searchTerm).ToListAsync();
            Assert.NotEmpty(result);
            Assert.Single(result);

            var first = result.First();

            Assert.NotEqual(0, first.ID);
            Assert.NotNull(first.Name);
            Assert.Equal(searchTerm, first.Name);
            Assert.Equal(0, first.Depth);
        }
    }

    [Fact]
    public async void AttemptRetrieveFranchiseWithFilterAndParent() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();

            SqliteCommand cmd = new SqliteCommand() {
                CommandText = @"INSERT INTO Franchise (Id, Name, ParentId) VALUES (1, 'f', NULL);
INSERT INTO Franchise (Id, Name, ParentId) VALUES (2, 'a', 1)"
            };
            db.ExecuteNonQuery(cmd);

            var result = await db.RetrieveFranchisesAsync("a").ToListAsync();
            Assert.NotEmpty(result);
            Assert.Single(result);

            var first = result.First();

            Assert.NotEqual(0, first.ID);
            Assert.NotNull(first.Name);
            Assert.Equal(1, first.Depth);
            Assert.NotNull(first.ParentID);
            Assert.Equal(1, first.ParentID);
        }
    }

    [Fact]
    public async void AttemptRetrieveFranchises() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();

            SqliteCommand cmd = new SqliteCommand() {
                CommandText = @"INSERT INTO Franchise (Id, Name, ParentId) VALUES (1, 'f', NULL);
INSERT INTO Franchise (Id, Name, ParentId) VALUES (2, 'a', 1)"
            };
            db.ExecuteNonQuery(cmd);

            var result = await db.RetrieveFranchisesAsync().ToListAsync();
            Assert.NotEmpty(result);


            var first = result.First();
            var last = result.Last();

            Assert.NotEqual(0, first.ID);
            Assert.NotNull(first.Name);
            Assert.Null(first.ParentID);
            Assert.Equal(0, first.Depth);

            Assert.Equal(1, last.Depth);
            Assert.NotEqual(first.ID, last.ID);
            Assert.Equal(first.ID, last.ParentID);
        }
    }

    [Fact]
    public async void AttemptRetrieveFranchisesWithFilter_NullInputThrows() {
        using (var db = new SqlLiteMemory()) {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await db.RetrieveFranchisesAsync(null).ToListAsync());
        }
    }

    [Fact]
    public async void AttemptToDeleteWallpaperWithNoLinks() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();
//Insert the test data
            var insertWallpapercmd = new SqliteCommand() {
                CommandText = "INSERT INTO Wallpapers (id, Name, DateAdded, FileName, Author, Source) VALUES (1, 'Blah', 0, 'faf', 'fa', '');" +
                              "INSERT INTO Wallpapers (id, Name, DateAdded, FileName, Author, Source) VALUES (2, 'Blah', 0, 'aw', 'fa', '')"
            };
            db.ExecuteNonQuery(insertWallpapercmd);

            //Attempt to delete the frist wallpaper
            await db.DeleteWallpaperAsync(1);

            //Try to retrieve the first wallpaper
            var retrievecmd = new SqliteCommand() {
                CommandText = "SELECT * FROM Wallpapers WHERE id = 1",
                CommandType = CommandType.Text,
            };

            var result = db.ExecuteScalar<long?>(retrievecmd);
            //Check we got nothing back since it should no longer exist
            Assert.Null(result);

            //Now check we have not accidentally deleted the other wallpaper
            retrievecmd = new SqliteCommand() {
                CommandText = "SELECT * FROM Wallpapers WHERE id = 2",
                CommandType = CommandType.Text,
            };

            result = db.ExecuteScalar<long?>(retrievecmd);
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async void AttemptToDeleteWallpaperWithLinks() {
        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();
//Insert the test data
            var insertWallpapercmd = new SqliteCommand() {
                CommandText = "INSERT INTO People (id,Name, PrimaryFranchise) VALUES (1, 'hello', NULL);" +
                              "INSERT INTO Franchise (id, Name, ParentId) VALUES (1, 'fa', NULL);" +
                              "INSERT INTO Wallpapers (id, Name, DateAdded, FileName, Author, Source) VALUES (1, 'Blah', 0, 'faf', 'fa', '');" +
                              "INSERT INTO WallpaperPeople (wallpaperid, personid) VALUES (1,1);" +
                              "INSERT INTO WallpaperFranchise (WallpaperID, FranchiseID) VALUES (1,1);" +
                              "INSERT INTO Wallpapers (id, Name, DateAdded, FileName, Author, Source) VALUES (2, 'Blah', 0, 'aw', 'fa', '');" +
                              "INSERT INTO WallpaperPeople (wallpaperid, personid) VALUES (2,1);" +
                              "INSERT INTO WallpaperFranchise (WallpaperID, FranchiseID) VALUES (2,1);",
            };
            db.ExecuteNonQuery(insertWallpapercmd);

            //Attempt to delete the frist wallpaper
            await db.DeleteWallpaperAsync(1);

            //Try to retrieve the first wallpaper
            var retrievecmd = new SqliteCommand() {
                CommandText = "SELECT * FROM Wallpapers WHERE id = 1",
                CommandType = CommandType.Text,
            };

            var result = db.ExecuteScalar<long?>(retrievecmd);
            //Check we got nothing back since it should no longer exist
            Assert.Null(result);

            var wallpaperPersonLinkcommand = new SqliteCommand() {
                CommandText = "SELECT * FROM WallpaperPeople WHERE WallpaperID = 1",
                CommandType = CommandType.Text,
            };

            var personResult = db.ExecuteScalar<long?>(wallpaperPersonLinkcommand);
            Assert.Null(personResult);

            var wallpaperFranchiseLinkcommand = new SqliteCommand() {
                CommandText = "SELECT * FROM WallpaperFranchise WHERE WallpaperID = 1",
                CommandType = CommandType.Text,
            };

            var franchiseResult = db.ExecuteScalar<long?>(wallpaperFranchiseLinkcommand);
            Assert.Null(franchiseResult);

            //Now check we have not accidentally deleted the other wallpaper or its links
            retrievecmd = new SqliteCommand() {
                CommandText = "SELECT * FROM Wallpapers WHERE id = 2",
                CommandType = CommandType.Text,
            };

            result = db.ExecuteScalar<long?>(retrievecmd);
            Assert.NotNull(result);

            wallpaperPersonLinkcommand = new SqliteCommand() {
                CommandText = "SELECT * FROM WallpaperPeople WHERE WallpaperID = 2",
                CommandType = CommandType.Text,
            };

            personResult = db.ExecuteScalar<long?>(wallpaperPersonLinkcommand);
            Assert.NotNull(personResult);

            wallpaperFranchiseLinkcommand = new SqliteCommand() {
                CommandText = "SELECT * FROM WallpaperFranchise WHERE WallpaperID = 2",
                CommandType = CommandType.Text,
            };

            franchiseResult = db.ExecuteScalar<long?>(wallpaperFranchiseLinkcommand);
            Assert.NotNull(franchiseResult);
        }
    }

    [Fact]
    public async void AttemptSaveWallpaperWithNoLinksAsync() {
        var wallpaper = new Manager.Core.Models.Wallpaper() {
            Author = "Blah",
            Name = "Johnson",
            Source = "The eternal void of suffering",
            DateAdded = DateTime.Now,
            FileName = "Megafile.png",
            FilePath = "/path/to/file",
            ID = 1,
        };

        using (var db = new SqlLiteMemory()) {
            db.ScaffoldDB();

            await db.SaveWallpaperAsync(wallpaper);

            SqliteCommand command = new SqliteCommand() {
                CommandText = "SELECT id, Name, DateAdded, FileName, Author, Source FROM Wallpapers"
            };
            using (var wallpaperrdr = db.ExecuteReader(command)) {
                Assert.True(wallpaperrdr.HasRows);

                int wallpaperIdOrdinal = wallpaperrdr.GetOrdinal(nameof(wallpaper.ID)),
                    wallpaperNameOrdinal = wallpaperrdr.GetOrdinal(nameof(wallpaper.Name)),
                    dateAddedOrdinal = wallpaperrdr.GetOrdinal(nameof(wallpaper.DateAdded)),
                    authorOrdinal = wallpaperrdr.GetOrdinal(nameof(wallpaper.Author)),
                    fileNameOrdinal = wallpaperrdr.GetOrdinal(nameof(wallpaper.FileName)),
                    sourceOrdinal = wallpaperrdr.GetOrdinal(nameof(wallpaper.Source));

                wallpaperrdr.Read();

                var foundWallpaper = new Manager.Core.Models.Wallpaper() {
                    ID = wallpaperrdr.GetInt32(wallpaperIdOrdinal),
                    Name = await wallpaperrdr.IsDBNullAsync(wallpaperNameOrdinal) ? null : wallpaperrdr.GetString(wallpaperNameOrdinal),
                    Author = await wallpaperrdr.IsDBNullAsync(authorOrdinal) ? null : wallpaperrdr.GetString(authorOrdinal),
                    Source = await wallpaperrdr.IsDBNullAsync(sourceOrdinal) ? null : wallpaperrdr.GetString(sourceOrdinal),
                    DateAdded = DateTimeOffset.FromUnixTimeSeconds(wallpaperrdr.GetInt32(dateAddedOrdinal)).DateTime,
                    FileName = wallpaperrdr.GetString(fileNameOrdinal)
                };

                Assert.Equal(wallpaper.ID, foundWallpaper.ID);
                Assert.Equal(wallpaper.Author, foundWallpaper.Author);
                Assert.Equal(wallpaper.Name, foundWallpaper.Name);
                Assert.Equal(wallpaper.Source, foundWallpaper.Source);
                //Database does not store time in milliseconds, so we need to remove them
                //https://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
                Assert.Equal(wallpaper.DateAdded.AddTicks(-(wallpaper.DateAdded.Ticks % TimeSpan.TicksPerSecond)), foundWallpaper.DateAdded.ToLocalTime());
                Assert.Equal(wallpaper.FileName, foundWallpaper.FileName);
                // Assert.Equal(wallpaper.FilePath, foundWallpaper.FilePath);
            }
        }
    }
}