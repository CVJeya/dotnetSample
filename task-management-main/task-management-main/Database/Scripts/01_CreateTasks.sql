-- =============================================
-- Task Management — Tasks table (matches EF Core model)
-- Run against SQL Server / LocalDB after creating database or use with EF migrations.
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'taskmanagementdb')
BEGIN
    CREATE DATABASE taskmanagementdb;
END
GO

USE taskmanagementdb;
GO

IF OBJECT_ID(N'dbo.Tasks', N'U') IS NOT NULL
    DROP TABLE dbo.Tasks;
GO

CREATE TABLE dbo.Tasks
(
    Id               INT                NOT NULL IDENTITY (1, 1),
    TaskTitle        NVARCHAR(200)      NOT NULL,
    TaskDescription  NVARCHAR(2000)     NULL,
    TaskDueDate      DATETIME2(7)       NOT NULL,
    TaskStatus       NVARCHAR(50)       NOT NULL CONSTRAINT DF_Tasks_TaskStatus DEFAULT (N'Pending'),
    TaskRemarks      NVARCHAR(1000)     NULL,
    CreatedOn        DATETIME2(7)       NOT NULL CONSTRAINT DF_Tasks_CreatedOn DEFAULT (GETUTCDATE()),
    LastUpdatedOn    DATETIME2(7)       NOT NULL CONSTRAINT DF_Tasks_LastUpdatedOn DEFAULT (GETUTCDATE()),
    CreatedBy        NVARCHAR(100)      NOT NULL CONSTRAINT DF_Tasks_CreatedBy DEFAULT (N'System'),
    LastUpdatedBy    NVARCHAR(100)      NOT NULL CONSTRAINT DF_Tasks_LastUpdatedBy DEFAULT (N'System'),

    CONSTRAINT PK_Tasks PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT CK_Tasks_Status CHECK (TaskStatus IN (N'Pending', N'InProgress', N'Completed', N'Cancelled'))
);
GO

CREATE NONCLUSTERED INDEX IX_Tasks_TaskTitle
    ON dbo.Tasks (TaskTitle ASC);
GO

CREATE NONCLUSTERED INDEX IX_Tasks_TaskStatus
    ON dbo.Tasks (TaskStatus ASC);
GO
