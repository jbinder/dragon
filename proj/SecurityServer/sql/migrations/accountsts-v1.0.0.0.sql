-- Script Date: 12/14/2015 12:07 PM  - ErikEJ.SqlCeScripting version 3.5.2.56
CREATE TABLE [UserModel] (
  [Id] bigint NOT NULL PRIMARY KEY IDENTITY
, [UserId] uniqueidentifier NOT NULL
, [AppId] uniqueidentifier NOT NULL
, [ServiceId] uniqueidentifier NOT NULL
, [Enabled] bit NOT NULL
, [CreatedAt] datetime NOT NULL
);
GO
CREATE TABLE [IdentityUserLogin] (
  [LoginProvider] nvarchar(128) NOT NULL
, [ProviderKey] nvarchar(128) NOT NULL
, [UserId] nvarchar(128) NOT NULL
);
GO
CREATE TABLE [IdentityUserClaim] (
  [Id] nvarchar(50) NOT NULL
, [UserId] nvarchar(50) NULL
, [ClaimType] nvarchar(1000) NULL
, [ClaimValue] nvarchar(1000) NULL
);
GO
CREATE TABLE [IdentityService] (
  [UserId] nvarchar(50) NOT NULL
, [ServiceId] nvarchar(50) NOT NULL
);
GO
CREATE TABLE [Exceptions] (
  [Id] bigint IDENTITY (2,1) NOT NULL
, [GUID] uniqueidentifier NOT NULL
, [ApplicationName] nvarchar(50) NOT NULL
, [MachineName] nvarchar(50) NOT NULL
, [CreationDate] datetime NOT NULL
, [Type] nvarchar(100) NOT NULL
, [IsProtected] bit DEFAULT 0 NOT NULL
, [Host] nvarchar(100) NULL
, [Url] nvarchar(500) NULL
, [HTTPMethod] nvarchar(10) NULL
, [IPAddress] nvarchar(40) NULL
, [Source] nvarchar(100) NULL
, [Message] nvarchar(1000) NULL
, [Detail] ntext NULL
, [StatusCode] int NULL
, [SQL] ntext NULL
, [DeletionDate] datetime NULL
, [FullJson] ntext NULL
, [ErrorHash] int NULL
, [DuplicateCount] int DEFAULT 1 NOT NULL
);
GO
CREATE TABLE [AppModel] (
  [Id] int NOT NULL PRIMARY KEY IDENTITY
, [AppId] uniqueidentifier NOT NULL
, [ServiceId] uniqueidentifier NOT NULL
, [Secret] nvarchar(50) NULL
, [Enabled] bit NOT NULL
, [CreatedAt] datetime NOT NULL
, [Name] nvarchar(50) NULL
);
GO
CREATE TABLE [AppMember] (
  [Id] nvarchar(50) NOT NULL
, [UserName] nvarchar(50) NULL
, [Email] nvarchar(50) NULL
, [EmailConfirmed] bit NOT NULL
, [PasswordHash] nvarchar(100) NULL
, [SecurityStamp] nvarchar(50) NULL
, [TwoFactorEnabled] bit NOT NULL
, [LockoutEndDateUtc] datetime NULL
, [LockoutEnabled] bit NOT NULL
, [AccessFailedCount] int NOT NULL
);
GO
CREATE TABLE [LegacyLocalAccount] (
  [UserId] uniqueidentifier NOT NULL
, [Key] nvarchar(128) NOT NULL
, [Secret] nvarchar(128) NOT NULL
);
GO
SET IDENTITY_INSERT [Exceptions] OFF;
GO
INSERT INTO [AppModel] ([AppId],[ServiceId],[Secret],[Enabled],[CreatedAt],[Name]) VALUES ('00000001-0001-0003-0003-200000000001','00000001-0001-0003-0003-000000000001',N'secret_accountSTS',1,{ts '2015-12-14 10:15:41.020'},N'AccountSTSClientTest');
GO
ALTER TABLE [IdentityUserLogin] ADD CONSTRAINT [PK_IdentityUserLogin] PRIMARY KEY ([LoginProvider],[ProviderKey],[UserId]);
GO
ALTER TABLE [IdentityUserClaim] ADD CONSTRAINT [PK_IdentityUserClaim] PRIMARY KEY ([Id]);
GO
ALTER TABLE [IdentityService] ADD CONSTRAINT [PK_IdentityService] PRIMARY KEY ([UserId],[ServiceId]);
GO
ALTER TABLE [Exceptions] ADD CONSTRAINT [PK_Exceptions] PRIMARY KEY ([Id]);
GO
ALTER TABLE [AppMember] ADD CONSTRAINT [PK_AppMember] PRIMARY KEY ([Id]);
GO
ALTER TABLE [LegacyLocalAccount] ADD CONSTRAINT [PK_LegacyLocalAccount] PRIMARY KEY ([UserId]);
GO
CREATE INDEX [IX_Exceptions_ApplicationName_DeletionDate_CreationDate_Filtered] ON [Exceptions] ([ApplicationName] ASC,[DeletionDate] ASC,[CreationDate] DESC);
GO
CREATE INDEX [IX_Exceptions_ErrorHash_ApplicationName_CreationDate_DeletionDate] ON [Exceptions] ([ErrorHash] ASC,[ApplicationName] ASC,[CreationDate] DESC,[DeletionDate] ASC);
GO
ALTER TABLE [Exceptions] ADD CONSTRAINT [IX_Exceptions_GUID_ApplicationName_DeletionDate_CreationDate] UNIQUE ([GUID],[ApplicationName],[DeletionDate],[CreationDate]);
GO

