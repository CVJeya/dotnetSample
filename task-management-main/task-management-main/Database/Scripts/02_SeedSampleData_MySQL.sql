-- =============================================
-- Sample seed data (MySQL 8+) — optional demo rows
-- Aligns with Database/Scripts/02_SeedSampleData.sql (SQL Server version).
-- Only inserts when Tasks table is empty.
-- =============================================

SET NAMES utf8mb4;

INSERT INTO `Tasks`
  (`TaskTitle`, `TaskDescription`, `TaskDueDate`, `TaskStatus`, `TaskRemarks`, `CreatedBy`, `LastUpdatedBy`)
SELECT * FROM (
  SELECT 'Set up CI pipeline' AS `TaskTitle`,
         'Configure builds, tests, and artifact publishing.' AS `TaskDescription`,
         DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 14 DAY) AS `TaskDueDate`,
         'InProgress' AS `TaskStatus`,
         'Owned by platform team.' AS `TaskRemarks`,
         'demo-user' AS `CreatedBy`,
         'demo-user' AS `LastUpdatedBy`
  UNION ALL
  SELECT 'Finalize API documentation',
         'Cover endpoints, models, and error contracts in Swagger.',
         DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 7 DAY),
         'Pending',
         NULL,
         'demo-user',
         'demo-user'
  UNION ALL
  SELECT 'Cross-browser smoke test',
         'Validate responsive UI on major browsers.',
         DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 21 DAY),
         'Pending',
         NULL,
         'demo-user',
         'demo-user'
) AS seed
WHERE NOT EXISTS (SELECT 1 FROM `Tasks` LIMIT 1);
