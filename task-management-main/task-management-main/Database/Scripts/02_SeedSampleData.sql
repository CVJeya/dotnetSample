USE taskmanagementdb;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks)
BEGIN
    INSERT INTO dbo.Tasks
        (TaskTitle, TaskDescription, TaskDueDate, TaskStatus, TaskRemarks, CreatedBy, LastUpdatedBy)
    VALUES
        (N'Set up CI pipeline', N'Configure builds, tests, and artifact publishing.', DATEADD(day, 14, SYSUTCDATETIME()), N'InProgress', N'Owned by platform team.', N'demo-user', N'demo-user'),
        (N'Finalize API documentation', N'Cover endpoints, models, and error contracts in Swagger.', DATEADD(day, 7, SYSUTCDATETIME()), N'Pending', NULL, N'demo-user', N'demo-user'),
        (N'Cross-browser smoke test', N'Validate responsive UI on major browsers.', DATEADD(day, 21, SYSUTCDATETIME()), N'Pending', NULL, N'demo-user', N'demo-user');
END
GO
