-- =============================================
-- Task Management — Tasks table (MySQL 8+)
-- Matches EF Core / API model. Use when database is MySQL/MariaDB (not SQL Server).
-- Run after: CREATE DATABASE IF NOT EXISTS TaskManagementDb ...
-- =============================================

SET NAMES utf8mb4;

-- Uncomment if creating schema in a new database:
-- CREATE DATABASE IF NOT EXISTS TaskManagementDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
-- USE TaskManagementDb;

DROP TABLE IF EXISTS `Tasks`;

CREATE TABLE `Tasks` (
  `Id`              INT            NOT NULL AUTO_INCREMENT,
  `TaskTitle`       VARCHAR(200)   NOT NULL,
  `TaskDescription` VARCHAR(2000)  NULL,
  `TaskDueDate`     DATETIME(6)    NOT NULL,
  `TaskStatus`      VARCHAR(50)    NOT NULL DEFAULT 'Pending',
  `TaskRemarks`     VARCHAR(1000)  NULL,
  `CreatedOn`       DATETIME(6)    NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
  `LastUpdatedOn`   DATETIME(6)    NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
  `CreatedBy`       VARCHAR(100)   NOT NULL DEFAULT 'System',
  `LastUpdatedBy`   VARCHAR(100)   NOT NULL DEFAULT 'System',
  PRIMARY KEY (`Id`),
  CONSTRAINT `CHK_Tasks_Status` CHECK (
    `TaskStatus` IN ('Pending','InProgress','Completed','Cancelled')
  )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX `IX_Tasks_TaskTitle` ON `Tasks` (`TaskTitle`);
CREATE INDEX `IX_Tasks_TaskStatus` ON `Tasks` (`TaskStatus`);
