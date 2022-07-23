-- we don't know how to generate root <with-no-name> (class Root) :(
create table Franchise
(
    Id       INTEGER not null
        primary key autoincrement,
    Name     TEXT    not null,
    ParentId INTEGER
);

create table People
(
    Id               integer not null
        constraint People_pk
            primary key autoincrement,
    Name             TEXT    not null,
    PrimaryFranchise integer
        references Franchise
            on delete set null
            deferrable initially deferred
);

create unique index People_Id_uindex
    on People (Id);

create table PeopleFranchises
(
    Franchise integer not null
        references Franchise
            on delete cascade
            deferrable initially deferred,
    Person    integer not null
        references People
            on delete cascade
            deferrable initially deferred,
    constraint PeopleFranchises_pk
        primary key (Franchise, Person)
);

create table Wallpapers
(
    id        integer not null
        constraint Wallpapers_pk
            primary key autoincrement,
    Name      TEXT,
    DateAdded INTEGER not null,
    FileName  TEXT    not null,
    Author    text,
    Source    text
);

create table WallpaperFranchise
(
    WallpaperID integer not null
        references Wallpapers
            on delete cascade
            deferrable initially deferred,
    FranchiseID integer not null
        references Franchise
            deferrable initially deferred,
    constraint WallpaperFranchise_pk
        primary key (WallpaperID, FranchiseID)
);

create table WallpaperPeople
(
    WallpaperID integer not null
        references Wallpapers,
    PersonID    integer not null
        references People
            on delete cascade
            deferrable initially deferred,
    constraint WallpaperPeople_pk
        primary key (WallpaperID, PersonID)
);

create unique index Wallpapers_FileName_uindex
    on Wallpapers (FileName);

