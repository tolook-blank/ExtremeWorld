
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 02/27/2024 11:38:10
-- Generated from EDMX file: E:\0VipSkill_Unity_MMORPG\mymmo\Src\Server\GameServer\GameServer\Entities.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [ExtremeWorld];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_UserPlayer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_UserPlayer];
GO
IF OBJECT_ID(N'[dbo].[FK_PlayerCharacter]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Characters] DROP CONSTRAINT [FK_PlayerCharacter];
GO
IF OBJECT_ID(N'[dbo].[FK_CharacterItem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CharacterItems] DROP CONSTRAINT [FK_CharacterItem];
GO
IF OBJECT_ID(N'[dbo].[FK_CharacterBag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Characters] DROP CONSTRAINT [FK_CharacterBag];
GO
IF OBJECT_ID(N'[dbo].[FK_TCharacterTcharacterQuest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[characterQuests] DROP CONSTRAINT [FK_TCharacterTcharacterQuest];
GO
IF OBJECT_ID(N'[dbo].[FK_TCharacterTCharacterFriend]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TCharacterFriends] DROP CONSTRAINT [FK_TCharacterTCharacterFriend];
GO
IF OBJECT_ID(N'[dbo].[FK_TGuildTGuildMember]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TGuildMembers] DROP CONSTRAINT [FK_TGuildTGuildMember];
GO
IF OBJECT_ID(N'[dbo].[FK_TGuildTGuildApply]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TGuildApplies] DROP CONSTRAINT [FK_TGuildTGuildApply];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[Players]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Players];
GO
IF OBJECT_ID(N'[dbo].[Characters]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Characters];
GO
IF OBJECT_ID(N'[dbo].[CharacterItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CharacterItems];
GO
IF OBJECT_ID(N'[dbo].[TCharacterBags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TCharacterBags];
GO
IF OBJECT_ID(N'[dbo].[characterQuests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[characterQuests];
GO
IF OBJECT_ID(N'[dbo].[TCharacterFriends]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TCharacterFriends];
GO
IF OBJECT_ID(N'[dbo].[TGuilds]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TGuilds];
GO
IF OBJECT_ID(N'[dbo].[TGuildMembers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TGuildMembers];
GO
IF OBJECT_ID(N'[dbo].[TGuildApplies]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TGuildApplies];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [ID] bigint IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(50)  NOT NULL,
    [Password] nvarchar(50)  NOT NULL,
    [RegisterDate] datetime  NULL,
    [Player_ID] int  NOT NULL
);
GO

-- Creating table 'Players'
CREATE TABLE [dbo].[Players] (
    [ID] int IDENTITY(1,1) NOT NULL
);
GO

-- Creating table 'Characters'
CREATE TABLE [dbo].[Characters] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [TID] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Class] int  NOT NULL,
    [Level] int  NOT NULL,
    [Exp] bigint  NOT NULL,
    [MapID] int  NOT NULL,
    [MapPosX] int  NOT NULL,
    [MapPosY] int  NOT NULL,
    [MapPosZ] int  NOT NULL,
    [Gold] bigint  NOT NULL,
    [Equips] binary(28)  NOT NULL,
    [GuildId] int  NOT NULL,
    [Player_ID] int  NOT NULL,
    [Bag_Id] int  NOT NULL
);
GO

-- Creating table 'CharacterItems'
CREATE TABLE [dbo].[CharacterItems] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ItemID] int  NOT NULL,
    [ItemCount] int  NOT NULL,
    [CharacterID] int  NOT NULL
);
GO

-- Creating table 'TCharacterBags'
CREATE TABLE [dbo].[TCharacterBags] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Items] varbinary(max)  NOT NULL,
    [Unlocked] int  NOT NULL
);
GO

-- Creating table 'characterQuests'
CREATE TABLE [dbo].[characterQuests] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [QuestID] int  NOT NULL,
    [Target1] int  NOT NULL,
    [Target2] int  NOT NULL,
    [Target3] int  NOT NULL,
    [Status] int  NOT NULL,
    [TCharacterID] int  NOT NULL
);
GO

-- Creating table 'TCharacterFriends'
CREATE TABLE [dbo].[TCharacterFriends] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FriendID] int  NOT NULL,
    [FriendName] nvarchar(max)  NOT NULL,
    [Class] int  NOT NULL,
    [Level] int  NOT NULL,
    [TCharacterID] int  NOT NULL
);
GO

-- Creating table 'TGuilds'
CREATE TABLE [dbo].[TGuilds] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [LeaderID] int  NOT NULL,
    [LeaderName] nvarchar(max)  NOT NULL,
    [Notice] nvarchar(max)  NOT NULL,
    [CreateDate] datetime  NOT NULL
);
GO

-- Creating table 'TGuildMembers'
CREATE TABLE [dbo].[TGuildMembers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CharacterId] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Class] int  NOT NULL,
    [Level] int  NOT NULL,
    [Title] int  NOT NULL,
    [JoinTime] datetime  NOT NULL,
    [LastTime] datetime  NOT NULL,
    [TGuildId] int  NOT NULL
);
GO

-- Creating table 'TGuildApplies'
CREATE TABLE [dbo].[TGuildApplies] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CharacterId] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Class] int  NOT NULL,
    [Level] int  NOT NULL,
    [Result] int  NOT NULL,
    [ApplyTime] datetime  NOT NULL,
    [GuildId] int  NOT NULL,
    [TGuildId] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Players'
ALTER TABLE [dbo].[Players]
ADD CONSTRAINT [PK_Players]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Characters'
ALTER TABLE [dbo].[Characters]
ADD CONSTRAINT [PK_Characters]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [Id] in table 'CharacterItems'
ALTER TABLE [dbo].[CharacterItems]
ADD CONSTRAINT [PK_CharacterItems]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TCharacterBags'
ALTER TABLE [dbo].[TCharacterBags]
ADD CONSTRAINT [PK_TCharacterBags]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'characterQuests'
ALTER TABLE [dbo].[characterQuests]
ADD CONSTRAINT [PK_characterQuests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TCharacterFriends'
ALTER TABLE [dbo].[TCharacterFriends]
ADD CONSTRAINT [PK_TCharacterFriends]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TGuilds'
ALTER TABLE [dbo].[TGuilds]
ADD CONSTRAINT [PK_TGuilds]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TGuildMembers'
ALTER TABLE [dbo].[TGuildMembers]
ADD CONSTRAINT [PK_TGuildMembers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TGuildApplies'
ALTER TABLE [dbo].[TGuildApplies]
ADD CONSTRAINT [PK_TGuildApplies]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Player_ID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_UserPlayer]
    FOREIGN KEY ([Player_ID])
    REFERENCES [dbo].[Players]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserPlayer'
CREATE INDEX [IX_FK_UserPlayer]
ON [dbo].[Users]
    ([Player_ID]);
GO

-- Creating foreign key on [Player_ID] in table 'Characters'
ALTER TABLE [dbo].[Characters]
ADD CONSTRAINT [FK_PlayerCharacter]
    FOREIGN KEY ([Player_ID])
    REFERENCES [dbo].[Players]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PlayerCharacter'
CREATE INDEX [IX_FK_PlayerCharacter]
ON [dbo].[Characters]
    ([Player_ID]);
GO

-- Creating foreign key on [CharacterID] in table 'CharacterItems'
ALTER TABLE [dbo].[CharacterItems]
ADD CONSTRAINT [FK_CharacterItem]
    FOREIGN KEY ([CharacterID])
    REFERENCES [dbo].[Characters]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CharacterItem'
CREATE INDEX [IX_FK_CharacterItem]
ON [dbo].[CharacterItems]
    ([CharacterID]);
GO

-- Creating foreign key on [Bag_Id] in table 'Characters'
ALTER TABLE [dbo].[Characters]
ADD CONSTRAINT [FK_CharacterBag]
    FOREIGN KEY ([Bag_Id])
    REFERENCES [dbo].[TCharacterBags]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CharacterBag'
CREATE INDEX [IX_FK_CharacterBag]
ON [dbo].[Characters]
    ([Bag_Id]);
GO

-- Creating foreign key on [TCharacterID] in table 'characterQuests'
ALTER TABLE [dbo].[characterQuests]
ADD CONSTRAINT [FK_TCharacterTcharacterQuest]
    FOREIGN KEY ([TCharacterID])
    REFERENCES [dbo].[Characters]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TCharacterTcharacterQuest'
CREATE INDEX [IX_FK_TCharacterTcharacterQuest]
ON [dbo].[characterQuests]
    ([TCharacterID]);
GO

-- Creating foreign key on [TCharacterID] in table 'TCharacterFriends'
ALTER TABLE [dbo].[TCharacterFriends]
ADD CONSTRAINT [FK_TCharacterTCharacterFriend]
    FOREIGN KEY ([TCharacterID])
    REFERENCES [dbo].[Characters]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TCharacterTCharacterFriend'
CREATE INDEX [IX_FK_TCharacterTCharacterFriend]
ON [dbo].[TCharacterFriends]
    ([TCharacterID]);
GO

-- Creating foreign key on [TGuildId] in table 'TGuildMembers'
ALTER TABLE [dbo].[TGuildMembers]
ADD CONSTRAINT [FK_TGuildTGuildMember]
    FOREIGN KEY ([TGuildId])
    REFERENCES [dbo].[TGuilds]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TGuildTGuildMember'
CREATE INDEX [IX_FK_TGuildTGuildMember]
ON [dbo].[TGuildMembers]
    ([TGuildId]);
GO

-- Creating foreign key on [TGuildId] in table 'TGuildApplies'
ALTER TABLE [dbo].[TGuildApplies]
ADD CONSTRAINT [FK_TGuildTGuildApply]
    FOREIGN KEY ([TGuildId])
    REFERENCES [dbo].[TGuilds]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TGuildTGuildApply'
CREATE INDEX [IX_FK_TGuildTGuildApply]
ON [dbo].[TGuildApplies]
    ([TGuildId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------