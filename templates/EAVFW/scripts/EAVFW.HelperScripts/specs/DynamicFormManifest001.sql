IF OBJECT_ID(N'[BK001].[__MigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'BK001') IS NULL EXEC(N'CREATE SCHEMA [BK001];');
    CREATE TABLE [BK001].[__MigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___MigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [BK001].[__MigrationsHistory] WHERE [MigrationId] = N'tests_Initial')
BEGIN
    CREATE TABLE [tests].[FormSubmissions] (
        [Id] uniqueidentifier NOT NULL,
        [FastTrackUsed] nvarchar(100) NULL,
        CONSTRAINT [PK_FormSubmissions] PRIMARY KEY ([Id])
    );
    DECLARE @description AS sql_variant;
    SET @description = N'comment';
    EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'tests', 'TABLE', N'FormSubmissions';
END;
GO

IF NOT EXISTS(SELECT * FROM [BK001].[__MigrationsHistory] WHERE [MigrationId] = N'tests_Initial')
BEGIN
    CREATE TABLE [tests].[ApplicationFiles] (
        [Id] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NULL,
        [FormSubmissionId] uniqueidentifier NULL,
        [FileId] uniqueidentifier NULL,
        CONSTRAINT [PK_ApplicationFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApplicationFiles_FormSubmissions_FormSubmissionId] FOREIGN KEY ([FormSubmissionId]) REFERENCES [tests].[FormSubmissions] ([Id]),
        CONSTRAINT [FK_ApplicationFiles_Documents_FileId] FOREIGN KEY ([FileId]) REFERENCES [EAVFW].[Documents] ([Id])
    );
    DECLARE @description AS sql_variant;
    SET @description = N'comment';
    EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'tests', 'TABLE', N'ApplicationFiles';
END;
GO

IF NOT EXISTS(SELECT * FROM [BK001].[__MigrationsHistory] WHERE [MigrationId] = N'tests_Initial')
BEGIN
    CREATE TABLE [tests].[DocumentationFiles] (
        [Id] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NULL,
        [FormSubmissionId] uniqueidentifier NULL,
        [FileId] uniqueidentifier NULL,
        CONSTRAINT [PK_DocumentationFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DocumentationFiles_FormSubmissions_FormSubmissionId] FOREIGN KEY ([FormSubmissionId]) REFERENCES [tests].[FormSubmissions] ([Id]),
        CONSTRAINT [FK_DocumentationFiles_Documents_FileId] FOREIGN KEY ([FileId]) REFERENCES [EAVFW].[Documents] ([Id])
    );
    DECLARE @description AS sql_variant;
    SET @description = N'comment';
    EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'tests', 'TABLE', N'DocumentationFiles';
END;
GO

IF NOT EXISTS(SELECT * FROM [BK001].[__MigrationsHistory] WHERE [MigrationId] = N'tests_Initial')
BEGIN
    CREATE TABLE [tests].[BudgetFiles] (
        [Id] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NULL,
        [FormSubmissionId] uniqueidentifier NULL,
        [FileId] uniqueidentifier NULL,
        CONSTRAINT [PK_BudgetFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BudgetFiles_FormSubmissions_FormSubmissionId] FOREIGN KEY ([FormSubmissionId]) REFERENCES [tests].[FormSubmissions] ([Id]),
        CONSTRAINT [FK_BudgetFiles_Documents_FileId] FOREIGN KEY ([FileId]) REFERENCES [EAVFW].[Documents] ([Id])
    );
    DECLARE @description AS sql_variant;
    SET @description = N'comment';
    EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', N'tests', 'TABLE', N'BudgetFiles';
END;
GO

IF NOT EXISTS(SELECT * FROM [BK001].[__MigrationsHistory] WHERE [MigrationId] = N'tests_Initial')
BEGIN
    INSERT INTO [BK001].[__MigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'tests_Initial', N'5.0.10');
END;
GO

COMMIT;
GO

