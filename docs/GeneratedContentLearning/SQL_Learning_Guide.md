# SQL Learning Guide: Queries, Functions, Procedures & Indexes

A comprehensive guide covering SQL queries, stored procedures, functions, and indexes across **MySQL**, **SQL Server**, **Oracle**, and **PostgreSQL**.

---

## Table of Contents

1. [SQL Queries](#sql-queries)
2. [Functions](#functions)
3. [Stored Procedures](#stored-procedures)
4. [Indexes](#indexes)
5. [Comparison Matrix](#comparison-matrix)

---

## SQL Queries

### Basic SELECT Statement

#### MySQL
```sql
-- Simple SELECT
SELECT * FROM employees;
SELECT id, name, salary FROM employees WHERE salary > 50000;

-- WITH clause (CTE)
WITH high_earners AS (
  SELECT id, name, salary FROM employees WHERE salary > 50000
)
SELECT * FROM high_earners;

-- LIMIT for pagination
SELECT * FROM employees LIMIT 10 OFFSET 20;
```

#### SQL Server
```sql
-- Simple SELECT
SELECT * FROM employees;
SELECT id, name, salary FROM employees WHERE salary > 50000;

-- WITH clause (CTE)
WITH high_earners AS (
  SELECT id, name, salary FROM employees WHERE salary > 50000
)
SELECT * FROM high_earners;

-- TOP for limiting results
SELECT TOP 10 * FROM employees WHERE salary > 50000;
```

#### Oracle
```sql
-- Simple SELECT
SELECT * FROM employees;
SELECT id, name, salary FROM employees WHERE salary > 50000;

-- WITH clause (CTE)
WITH high_earners AS (
  SELECT id, name, salary FROM employees WHERE salary > 50000
)
SELECT * FROM high_earners;

-- ROWNUM for limiting results (deprecated approach)
SELECT * FROM employees WHERE ROWNUM <= 10;

-- FETCH NEXT (newer approach, Oracle 12c+)
SELECT * FROM employees FETCH FIRST 10 ROWS ONLY;
```

#### PostgreSQL
```sql
-- Simple SELECT
SELECT * FROM employees;
SELECT id, name, salary FROM employees WHERE salary > 50000;

-- WITH clause (CTE)
WITH high_earners AS (
  SELECT id, name, salary FROM employees WHERE salary > 50000
)
SELECT * FROM high_earners;

-- LIMIT for pagination
SELECT * FROM employees LIMIT 10 OFFSET 20;
```

### Joins

#### MySQL
```sql
-- INNER JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
INNER JOIN departments d ON e.dept_id = d.id;

-- LEFT JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
LEFT JOIN departments d ON e.dept_id = d.id;

-- RIGHT JOIN (less common)
SELECT e.id, e.name, d.department_name 
FROM employees e
RIGHT JOIN departments d ON e.dept_id = d.id;

-- FULL OUTER JOIN (simulated)
SELECT e.id, e.name, d.department_name 
FROM employees e
LEFT JOIN departments d ON e.dept_id = d.id
UNION
SELECT e.id, e.name, d.department_name 
FROM employees e
RIGHT JOIN departments d ON e.dept_id = d.id;
```

#### SQL Server
```sql
-- INNER JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
INNER JOIN departments d ON e.dept_id = d.id;

-- LEFT JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
LEFT JOIN departments d ON e.dept_id = d.id;

-- RIGHT JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
RIGHT JOIN departments d ON e.dept_id = d.id;

-- FULL OUTER JOIN (native support)
SELECT e.id, e.name, d.department_name 
FROM employees e
FULL OUTER JOIN departments d ON e.dept_id = d.id;

-- CROSS JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
CROSS JOIN departments d;
```

#### Oracle
```sql
-- INNER JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
INNER JOIN departments d ON e.dept_id = d.id;

-- LEFT JOIN (OUTER added for clarity)
SELECT e.id, e.name, d.department_name 
FROM employees e
LEFT OUTER JOIN departments d ON e.dept_id = d.id;

-- RIGHT JOIN (OUTER added for clarity)
SELECT e.id, e.name, d.department_name 
FROM employees e
RIGHT OUTER JOIN departments d ON e.dept_id = d.id;

-- FULL OUTER JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
FULL OUTER JOIN departments d ON e.dept_id = d.id;

-- Legacy Syntax (still supported)
SELECT e.id, e.name, d.department_name 
FROM employees e, departments d
WHERE e.dept_id = d.id(+);  -- + indicates OUTER JOIN
```

#### PostgreSQL
```sql
-- INNER JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
INNER JOIN departments d ON e.dept_id = d.id;

-- LEFT JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
LEFT JOIN departments d ON e.dept_id = d.id;

-- RIGHT JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
RIGHT JOIN departments d ON e.dept_id = d.id;

-- FULL OUTER JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
FULL OUTER JOIN departments d ON e.dept_id = d.id;

-- CROSS JOIN
SELECT e.id, e.name, d.department_name 
FROM employees e
CROSS JOIN departments d;
```

### Aggregate Functions

#### MySQL
```sql
-- Aggregate functions
SELECT 
  COUNT(*) as total_employees,
  AVG(salary) as avg_salary,
  MAX(salary) as max_salary,
  MIN(salary) as min_salary,
  SUM(salary) as total_salary
FROM employees;

-- GROUP BY
SELECT 
  dept_id,
  COUNT(*) as employee_count,
  AVG(salary) as avg_salary
FROM employees
GROUP BY dept_id;

-- HAVING clause
SELECT 
  dept_id,
  COUNT(*) as employee_count,
  AVG(salary) as avg_salary
FROM employees
GROUP BY dept_id
HAVING COUNT(*) > 5;

-- GROUP_CONCAT (MySQL specific)
SELECT 
  dept_id,
  GROUP_CONCAT(name SEPARATOR ', ') as employee_names
FROM employees
GROUP BY dept_id;
```

#### SQL Server
```sql
-- Aggregate functions
SELECT 
  COUNT(*) as total_employees,
  AVG(salary) as avg_salary,
  MAX(salary) as max_salary,
  MIN(salary) as min_salary,
  SUM(salary) as total_salary
FROM employees;

-- GROUP BY
SELECT 
  dept_id,
  COUNT(*) as employee_count,
  AVG(salary) as avg_salary
FROM employees
GROUP BY dept_id;

-- HAVING clause
SELECT 
  dept_id,
  COUNT(*) as employee_count,
  AVG(salary) as avg_salary
FROM employees
GROUP BY dept_id
HAVING COUNT(*) > 5;

-- STRING_AGG (SQL Server 2017+)
SELECT 
  dept_id,
  STRING_AGG(name, ', ') as employee_names
FROM employees
GROUP BY dept_id;
```

#### Oracle
```sql
-- Aggregate functions
SELECT 
  COUNT(*) as total_employees,
  AVG(salary) as avg_salary,
  MAX(salary) as max_salary,
  MIN(salary) as min_salary,
  SUM(salary) as total_salary
FROM employees;

-- GROUP BY
SELECT 
  dept_id,
  COUNT(*) as employee_count,
  AVG(salary) as avg_salary
FROM employees
GROUP BY dept_id;

-- HAVING clause
SELECT 
  dept_id,
  COUNT(*) as employee_count,
  AVG(salary) as avg_salary
FROM employees
GROUP BY dept_id
HAVING COUNT(*) > 5;

-- LISTAGG (Oracle specific)
SELECT 
  dept_id,
  LISTAGG(name, ', ') WITHIN GROUP (ORDER BY name) as employee_names
FROM employees
GROUP BY dept_id;
```

#### PostgreSQL
```sql
-- Aggregate functions
SELECT 
  COUNT(*) as total_employees,
  AVG(salary) as avg_salary,
  MAX(salary) as max_salary,
  MIN(salary) as min_salary,
  SUM(salary) as total_salary
FROM employees;

-- GROUP BY
SELECT 
  dept_id,
  COUNT(*) as employee_count,
  AVG(salary) as avg_salary
FROM employees
GROUP BY dept_id;

-- HAVING clause
SELECT 
  dept_id,
  COUNT(*) as employee_count,
  AVG(salary) as avg_salary
FROM employees
GROUP BY dept_id
HAVING COUNT(*) > 5;

-- STRING_AGG (PostgreSQL specific)
SELECT 
  dept_id,
  STRING_AGG(name, ', ' ORDER BY name) as employee_names
FROM employees
GROUP BY dept_id;
```

### Window Functions

#### MySQL (8.0+)
```sql
-- ROW_NUMBER
SELECT 
  id, name, salary,
  ROW_NUMBER() OVER (PARTITION BY dept_id ORDER BY salary DESC) as salary_rank
FROM employees;

-- RANK and DENSE_RANK
SELECT 
  id, name, salary,
  RANK() OVER (ORDER BY salary DESC) as salary_rank,
  DENSE_RANK() OVER (ORDER BY salary DESC) as dense_rank
FROM employees;

-- LAG and LEAD
SELECT 
  id, name, salary,
  LAG(salary) OVER (ORDER BY salary) as prev_salary,
  LEAD(salary) OVER (ORDER BY salary) as next_salary
FROM employees;

-- Running total
SELECT 
  id, name, salary,
  SUM(salary) OVER (ORDER BY id) as running_total
FROM employees;
```

#### SQL Server
```sql
-- ROW_NUMBER
SELECT 
  id, name, salary,
  ROW_NUMBER() OVER (PARTITION BY dept_id ORDER BY salary DESC) as salary_rank
FROM employees;

-- RANK and DENSE_RANK
SELECT 
  id, name, salary,
  RANK() OVER (ORDER BY salary DESC) as salary_rank,
  DENSE_RANK() OVER (ORDER BY salary DESC) as dense_rank
FROM employees;

-- LAG and LEAD
SELECT 
  id, name, salary,
  LAG(salary) OVER (ORDER BY salary) as prev_salary,
  LEAD(salary) OVER (ORDER BY salary) as next_salary
FROM employees;

-- NTILE (quartile)
SELECT 
  id, name, salary,
  NTILE(4) OVER (ORDER BY salary) as quartile
FROM employees;

-- Running total
SELECT 
  id, name, salary,
  SUM(salary) OVER (ORDER BY id ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) as running_total
FROM employees;
```

#### Oracle
```sql
-- ROW_NUMBER
SELECT 
  id, name, salary,
  ROW_NUMBER() OVER (PARTITION BY dept_id ORDER BY salary DESC) as salary_rank
FROM employees;

-- RANK and DENSE_RANK
SELECT 
  id, name, salary,
  RANK() OVER (ORDER BY salary DESC) as salary_rank,
  DENSE_RANK() OVER (ORDER BY salary DESC) as dense_rank
FROM employees;

-- LAG and LEAD
SELECT 
  id, name, salary,
  LAG(salary) OVER (ORDER BY salary) as prev_salary,
  LEAD(salary) OVER (ORDER BY salary) as next_salary
FROM employees;

-- FIRST_VALUE and LAST_VALUE
SELECT 
  id, name, salary,
  FIRST_VALUE(salary) OVER (ORDER BY salary DESC) as highest_salary,
  LAST_VALUE(salary) OVER (ORDER BY salary DESC ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) as lowest_salary
FROM employees;
```

#### PostgreSQL
```sql
-- ROW_NUMBER
SELECT 
  id, name, salary,
  ROW_NUMBER() OVER (PARTITION BY dept_id ORDER BY salary DESC) as salary_rank
FROM employees;

-- RANK and DENSE_RANK
SELECT 
  id, name, salary,
  RANK() OVER (ORDER BY salary DESC) as salary_rank,
  DENSE_RANK() OVER (ORDER BY salary DESC) as dense_rank
FROM employees;

-- LAG and LEAD
SELECT 
  id, name, salary,
  LAG(salary) OVER (ORDER BY salary) as prev_salary,
  LEAD(salary) OVER (ORDER BY salary) as next_salary
FROM employees;

-- Running total
SELECT 
  id, name, salary,
  SUM(salary) OVER (ORDER BY id) as running_total
FROM employees;
```

---

## Functions

### String Functions

#### MySQL
```sql
-- CONCAT
SELECT CONCAT(first_name, ' ', last_name) as full_name FROM employees;

-- LENGTH
SELECT name, LENGTH(name) as name_length FROM employees;

-- SUBSTRING
SELECT SUBSTRING(email, 1, POSITION('@' IN email) - 1) as username FROM employees;

-- UPPER / LOWER
SELECT UPPER(name), LOWER(email) FROM employees;

-- TRIM
SELECT TRIM(name) FROM employees;

-- REPLACE
SELECT REPLACE(email, '.com', '.org') FROM employees;

-- INSTR (position of substring)
SELECT INSTR(email, '@') FROM employees;

-- Date functions
SELECT DATE_ADD(hire_date, INTERVAL 1 YEAR) as one_year_later FROM employees;
SELECT DATEDIFF(NOW(), hire_date) as days_employed FROM employees;
```

#### SQL Server
```sql
-- CONCAT (SQL Server 2012+)
SELECT CONCAT(first_name, ' ', last_name) as full_name FROM employees;

-- String concatenation (legacy)
SELECT first_name + ' ' + last_name as full_name FROM employees;

-- LEN
SELECT name, LEN(name) as name_length FROM employees;

-- SUBSTRING
SELECT SUBSTRING(email, 1, CHARINDEX('@', email) - 1) as username FROM employees;

-- UPPER / LOWER
SELECT UPPER(name), LOWER(email) FROM employees;

-- LTRIM / RTRIM
SELECT LTRIM(RTRIM(name)) FROM employees;

-- REPLACE
SELECT REPLACE(email, '.com', '.org') FROM employees;

-- CHARINDEX (position of substring)
SELECT CHARINDEX('@', email) FROM employees;

-- Date functions
SELECT DATEADD(YEAR, 1, hire_date) as one_year_later FROM employees;
SELECT DATEDIFF(DAY, hire_date, GETDATE()) as days_employed FROM employees;
SELECT FORMAT(hire_date, 'yyyy-MM-dd') FROM employees;
```

#### Oracle
```sql
-- CONCAT
SELECT CONCAT(first_name, ' ', last_name) as full_name FROM employees;

-- String concatenation (||)
SELECT first_name || ' ' || last_name as full_name FROM employees;

-- LENGTH
SELECT name, LENGTH(name) as name_length FROM employees;

-- SUBSTR
SELECT SUBSTR(email, 1, INSTR(email, '@') - 1) as username FROM employees;

-- UPPER / LOWER
SELECT UPPER(name), LOWER(email) FROM employees;

-- TRIM
SELECT TRIM(name) FROM employees;

-- REPLACE
SELECT REPLACE(email, '.com', '.org') FROM employees;

-- INSTR (position of substring)
SELECT INSTR(email, '@') FROM employees;

-- Date functions
SELECT ADD_MONTHS(hire_date, 12) as one_year_later FROM employees;
SELECT TRUNC(SYSDATE) - hire_date as days_employed FROM employees;
SELECT TO_CHAR(hire_date, 'YYYY-MM-DD') FROM employees;
```

#### PostgreSQL
```sql
-- CONCAT
SELECT CONCAT(first_name, ' ', last_name) as full_name FROM employees;

-- String concatenation (||)
SELECT first_name || ' ' || last_name as full_name FROM employees;

-- LENGTH
SELECT name, LENGTH(name) as name_length FROM employees;

-- SUBSTRING
SELECT SUBSTRING(email FROM 1 FOR POSITION('@' IN email) - 1) as username FROM employees;

-- UPPER / LOWER
SELECT UPPER(name), LOWER(email) FROM employees;

-- TRIM
SELECT TRIM(name) FROM employees;

-- REPLACE
SELECT REPLACE(email, '.com', '.org') FROM employees;

-- POSITION (position of substring)
SELECT POSITION('@' IN email) FROM employees;

-- Date functions
SELECT hire_date + INTERVAL '1 year' as one_year_later FROM employees;
SELECT EXTRACT(DAY FROM CURRENT_DATE - hire_date) as days_employed FROM employees;
SELECT TO_CHAR(hire_date, 'YYYY-MM-DD') FROM employees;
```

### Numeric Functions

#### All Databases
```sql
-- ABS
SELECT ABS(-100);

-- CEIL/CEILING
SELECT CEIL(10.4), CEILING(10.4);

-- FLOOR
SELECT FLOOR(10.9);

-- ROUND
SELECT ROUND(10.567, 2);  -- Returns 10.57

-- TRUNCATE/TRUNC
SELECT TRUNCATE(10.567, 2);  -- Returns 10.56 (MySQL)
SELECT TRUNC(10.567, 2);     -- Returns 10.56 (Oracle)
SELECT CAST(TRUNCATE(10.567, 2) AS DECIMAL(10,2));  -- SQL Server

-- POWER
SELECT POWER(2, 3);  -- Returns 8

-- SQRT
SELECT SQRT(16);  -- Returns 4

-- MOD / remainder
SELECT MOD(17, 5);  -- Returns 2

-- GREATEST / LEAST
SELECT GREATEST(10, 20, 30);  -- Returns 30
SELECT LEAST(10, 20, 30);     -- Returns 10
```

### Date/Time Functions

#### MySQL
```sql
-- Current date/time
SELECT NOW();           -- Returns current date and time
SELECT CURDATE();       -- Returns current date
SELECT CURTIME();       -- Returns current time

-- Extract parts
SELECT YEAR(hire_date), MONTH(hire_date), DAY(hire_date) FROM employees;
SELECT HOUR(NOW()), MINUTE(NOW()), SECOND(NOW());

-- Date arithmetic
SELECT DATE_ADD(hire_date, INTERVAL 30 DAY);
SELECT DATE_SUB(hire_date, INTERVAL 6 MONTH);
SELECT DATEDIFF(CURDATE(), hire_date);

-- Date formatting
SELECT DATE_FORMAT(hire_date, '%Y-%m-%d %H:%i:%s');
SELECT DATE_FORMAT(hire_date, '%d/%m/%Y');

-- Last day of month
SELECT LAST_DAY(hire_date);
```

#### SQL Server
```sql
-- Current date/time
SELECT GETDATE();            -- Returns current date and time
SELECT CAST(GETDATE() AS DATE);  -- Returns only date
SELECT CAST(GETDATE() AS TIME);  -- Returns only time

-- Extract parts
SELECT YEAR(hire_date), MONTH(hire_date), DAY(hire_date) FROM employees;
SELECT DATEPART(HOUR, GETDATE()), DATEPART(MINUTE, GETDATE());

-- Date arithmetic
SELECT DATEADD(DAY, 30, hire_date);
SELECT DATEADD(MONTH, -6, hire_date);
SELECT DATEDIFF(DAY, hire_date, GETDATE());

-- Date formatting
SELECT FORMAT(hire_date, 'yyyy-MM-dd HH:mm:ss');
SELECT FORMAT(hire_date, 'dd/MM/yyyy');

-- End of month
SELECT EOMONTH(hire_date);
```

#### Oracle
```sql
-- Current date/time
SELECT SYSDATE FROM DUAL;              -- Returns current date and time
SELECT TRUNC(SYSDATE) FROM DUAL;       -- Returns only date
SELECT TO_CHAR(SYSDATE, 'HH24:MI:SS') FROM DUAL;

-- Extract parts
SELECT EXTRACT(YEAR FROM hire_date), EXTRACT(MONTH FROM hire_date), EXTRACT(DAY FROM hire_date) FROM employees;

-- Date arithmetic
SELECT hire_date + 30 FROM employees;        -- Add 30 days
SELECT ADD_MONTHS(hire_date, 6) FROM employees;  -- Add 6 months
SELECT TRUNC(SYSDATE) - hire_date FROM employees;

-- Date formatting
SELECT TO_CHAR(hire_date, 'YYYY-MM-DD HH24:MI:SS') FROM employees;
SELECT TO_CHAR(hire_date, 'DD/MM/YYYY') FROM employees;

-- Last day of month
SELECT LAST_DAY(hire_date) FROM employees;
```

#### PostgreSQL
```sql
-- Current date/time
SELECT CURRENT_TIMESTAMP;        -- Returns current date and time
SELECT CURRENT_DATE;             -- Returns only date
SELECT CURRENT_TIME;             -- Returns only time
SELECT NOW();                    -- Same as CURRENT_TIMESTAMP

-- Extract parts
SELECT EXTRACT(YEAR FROM hire_date), EXTRACT(MONTH FROM hire_date), EXTRACT(DAY FROM hire_date) FROM employees;

-- Date arithmetic
SELECT hire_date + INTERVAL '30 days' FROM employees;
SELECT hire_date + INTERVAL '6 months' FROM employees;
SELECT EXTRACT(DAY FROM CURRENT_DATE - hire_date) FROM employees;

-- Date formatting
SELECT TO_CHAR(hire_date, 'YYYY-MM-DD HH24:MI:SS') FROM employees;
SELECT TO_CHAR(hire_date, 'DD/MM/YYYY') FROM employees;

-- Last day of month (using DATE_TRUNC)
SELECT DATE_TRUNC('month', hire_date + INTERVAL '1 month') - INTERVAL '1 day' FROM employees;
```

### Conditional Functions

#### MySQL
```sql
-- IF statement
SELECT name, IF(salary > 50000, 'High', 'Low') as salary_level FROM employees;

-- CASE statement
SELECT name,
  CASE 
    WHEN salary >= 80000 THEN 'Executive'
    WHEN salary >= 50000 THEN 'Manager'
    WHEN salary >= 30000 THEN 'Senior'
    ELSE 'Junior'
  END as position_level
FROM employees;

-- COALESCE (return first non-NULL)
SELECT name, COALESCE(middle_name, last_name, 'Unknown') FROM employees;

-- NULLIF (return NULL if equal)
SELECT name, NULLIF(salary, 0) FROM employees;
```

#### SQL Server
```sql
-- CASE statement
SELECT name,
  CASE 
    WHEN salary >= 80000 THEN 'Executive'
    WHEN salary >= 50000 THEN 'Manager'
    WHEN salary >= 30000 THEN 'Senior'
    ELSE 'Junior'
  END as position_level
FROM employees;

-- IIF (SQL Server 2012+)
SELECT name, IIF(salary > 50000, 'High', 'Low') as salary_level FROM employees;

-- COALESCE (return first non-NULL)
SELECT name, COALESCE(middle_name, last_name, 'Unknown') FROM employees;

-- NULLIF (return NULL if equal)
SELECT name, NULLIF(salary, 0) FROM employees;

-- ISNULL (SQL Server specific)
SELECT name, ISNULL(middle_name, 'Unknown') FROM employees;
```

#### Oracle
```sql
-- CASE statement
SELECT name,
  CASE 
    WHEN salary >= 80000 THEN 'Executive'
    WHEN salary >= 50000 THEN 'Manager'
    WHEN salary >= 30000 THEN 'Senior'
    ELSE 'Junior'
  END as position_level
FROM employees;

-- DECODE (Oracle specific, legacy)
SELECT name, DECODE(salary, 50000, 'Fifty K', 80000, 'Eighty K', 'Other') FROM employees;

-- COALESCE (return first non-NULL)
SELECT name, COALESCE(middle_name, last_name, 'Unknown') FROM employees;

-- NULLIF (return NULL if equal)
SELECT name, NULLIF(salary, 0) FROM employees;

-- NVL (Oracle specific)
SELECT name, NVL(middle_name, 'Unknown') FROM employees;
```

#### PostgreSQL
```sql
-- CASE statement
SELECT name,
  CASE 
    WHEN salary >= 80000 THEN 'Executive'
    WHEN salary >= 50000 THEN 'Manager'
    WHEN salary >= 30000 THEN 'Senior'
    ELSE 'Junior'
  END as position_level
FROM employees;

-- COALESCE (return first non-NULL)
SELECT name, COALESCE(middle_name, last_name, 'Unknown') FROM employees;

-- NULLIF (return NULL if equal)
SELECT name, NULLIF(salary, 0) FROM employees;
```

### User-Defined Scalar Functions

#### MySQL
```sql
-- Creating a UDF (requires CREATE FUNCTION privilege)
DELIMITER //
CREATE FUNCTION GetAnnualBonus(salary DECIMAL(10,2))
RETURNS DECIMAL(10,2)
DETERMINISTIC
READS SQL DATA
BEGIN
  DECLARE bonus DECIMAL(10,2);
  IF salary >= 80000 THEN
    SET bonus = salary * 0.20;
  ELSEIF salary >= 50000 THEN
    SET bonus = salary * 0.15;
  ELSE
    SET bonus = salary * 0.10;
  END IF;
  RETURN bonus;
END //
DELIMITER ;

-- Using the function
SELECT name, salary, GetAnnualBonus(salary) as bonus FROM employees;

-- Dropping function
DROP FUNCTION GetAnnualBonus;
```

#### SQL Server
```sql
-- Creating a scalar function
CREATE FUNCTION GetAnnualBonus(@salary DECIMAL(10,2))
RETURNS DECIMAL(10,2)
AS
BEGIN
  DECLARE @bonus DECIMAL(10,2);
  IF @salary >= 80000
    SET @bonus = @salary * 0.20;
  ELSE IF @salary >= 50000
    SET @bonus = @salary * 0.15;
  ELSE
    SET @bonus = @salary * 0.10;
  RETURN @bonus;
END;

-- Using the function
SELECT name, salary, dbo.GetAnnualBonus(salary) as bonus FROM employees;

-- Dropping function
DROP FUNCTION dbo.GetAnnualBonus;
```

#### Oracle
```sql
-- Creating a function
CREATE OR REPLACE FUNCTION GetAnnualBonus(p_salary NUMBER)
RETURN NUMBER
IS
  v_bonus NUMBER;
BEGIN
  IF p_salary >= 80000 THEN
    v_bonus := p_salary * 0.20;
  ELSIF p_salary >= 50000 THEN
    v_bonus := p_salary * 0.15;
  ELSE
    v_bonus := p_salary * 0.10;
  END IF;
  RETURN v_bonus;
END GetAnnualBonus;
/

-- Using the function
SELECT name, salary, GetAnnualBonus(salary) as bonus FROM employees;

-- Dropping function
DROP FUNCTION GetAnnualBonus;
```

#### PostgreSQL
```sql
-- Creating a function
CREATE FUNCTION GetAnnualBonus(p_salary NUMERIC)
RETURNS NUMERIC AS $$
BEGIN
  IF p_salary >= 80000 THEN
    RETURN p_salary * 0.20;
  ELSIF p_salary >= 50000 THEN
    RETURN p_salary * 0.15;
  ELSE
    RETURN p_salary * 0.10;
  END IF;
END;
$$ LANGUAGE plpgsql;

-- Using the function
SELECT name, salary, GetAnnualBonus(salary) as bonus FROM employees;

-- Dropping function
DROP FUNCTION GetAnnualBonus;
```

---

## Stored Procedures

### Basic Procedure Structure

#### MySQL
```sql
-- Create procedure
DELIMITER //
CREATE PROCEDURE InsertEmployee(
  IN p_name VARCHAR(100),
  IN p_salary DECIMAL(10,2),
  IN p_dept_id INT,
  OUT p_emp_id INT
)
BEGIN
  INSERT INTO employees (name, salary, dept_id)
  VALUES (p_name, p_salary, p_dept_id);
  
  SET p_emp_id = LAST_INSERT_ID();
END //
DELIMITER ;

-- Call procedure
CALL InsertEmployee('John Doe', 55000, 1, @new_id);
SELECT @new_id;

-- View procedures
SHOW PROCEDURE STATUS;

-- Drop procedure
DROP PROCEDURE InsertEmployee;
```

#### SQL Server
```sql
-- Create procedure
CREATE PROCEDURE InsertEmployee
  @p_name NVARCHAR(100),
  @p_salary DECIMAL(10,2),
  @p_dept_id INT,
  @p_emp_id INT OUTPUT
AS
BEGIN
  INSERT INTO employees (name, salary, dept_id)
  VALUES (@p_name, @p_salary, @p_dept_id);
  
  SET @p_emp_id = SCOPE_IDENTITY();
END;

-- Call procedure
DECLARE @new_id INT;
EXEC InsertEmployee @p_name = 'John Doe', @p_salary = 55000, @p_dept_id = 1, @p_emp_id = @new_id OUTPUT;
SELECT @new_id;

-- View procedures
SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE';

-- Drop procedure
DROP PROCEDURE InsertEmployee;
```

#### Oracle
```sql
-- Create procedure
CREATE OR REPLACE PROCEDURE InsertEmployee(
  p_name IN VARCHAR2,
  p_salary IN NUMBER,
  p_dept_id IN NUMBER,
  p_emp_id OUT NUMBER
)
IS
BEGIN
  INSERT INTO employees (name, salary, dept_id)
  VALUES (p_name, p_salary, p_dept_id);
  
  SELECT employee_id_seq.CURRVAL INTO p_emp_id FROM DUAL;
  COMMIT;
END InsertEmployee;
/

-- Call procedure
DECLARE
  v_emp_id NUMBER;
BEGIN
  InsertEmployee('John Doe', 55000, 1, v_emp_id);
  DBMS_OUTPUT.PUT_LINE('New Employee ID: ' || v_emp_id);
END;
/

-- View procedures
SELECT object_name FROM user_objects WHERE object_type = 'PROCEDURE';

-- Drop procedure
DROP PROCEDURE InsertEmployee;
```

#### PostgreSQL
```sql
-- Create procedure (PostgreSQL 11+)
CREATE PROCEDURE InsertEmployee(
  p_name VARCHAR,
  p_salary NUMERIC,
  p_dept_id INT,
  OUT p_emp_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
  INSERT INTO employees (name, salary, dept_id)
  VALUES (p_name, p_salary, p_dept_id)
  RETURNING id INTO p_emp_id;
END;
$$;

-- Call procedure
CALL InsertEmployee('John Doe', 55000, 1, NULL);

-- View procedures
SELECT routine_name FROM information_schema.routines WHERE routine_type = 'PROCEDURE';

-- Drop procedure
DROP PROCEDURE InsertEmployee;
```

### Procedures with Control Flow

#### MySQL
```sql
DELIMITER //
CREATE PROCEDURE UpdateEmployeeSalary(
  IN p_emp_id INT,
  IN p_percentage DECIMAL(5,2),
  OUT p_new_salary DECIMAL(10,2),
  OUT p_status VARCHAR(50)
)
BEGIN
  DECLARE v_current_salary DECIMAL(10,2);
  DECLARE v_emp_count INT;
  
  -- Check if employee exists
  SELECT COUNT(*) INTO v_emp_count FROM employees WHERE id = p_emp_id;
  
  IF v_emp_count = 0 THEN
    SET p_status = 'Employee not found';
    SET p_new_salary = 0;
  ELSE
    -- Get current salary
    SELECT salary INTO v_current_salary FROM employees WHERE id = p_emp_id;
    
    -- Calculate new salary
    SET p_new_salary = v_current_salary + (v_current_salary * p_percentage / 100);
    
    -- Update salary
    UPDATE employees SET salary = p_new_salary WHERE id = p_emp_id;
    
    SET p_status = 'Salary updated successfully';
  END IF;
END //
DELIMITER ;

-- Call procedure
CALL UpdateEmployeeSalary(1, 10, @new_salary, @status);
SELECT @new_salary, @status;
```

#### SQL Server
```sql
CREATE PROCEDURE UpdateEmployeeSalary
  @p_emp_id INT,
  @p_percentage DECIMAL(5,2),
  @p_new_salary DECIMAL(10,2) OUTPUT,
  @p_status VARCHAR(50) OUTPUT
AS
BEGIN
  DECLARE @v_current_salary DECIMAL(10,2);
  DECLARE @v_emp_count INT;
  
  -- Check if employee exists
  SELECT @v_emp_count = COUNT(*) FROM employees WHERE id = @p_emp_id;
  
  IF @v_emp_count = 0
  BEGIN
    SET @p_status = 'Employee not found';
    SET @p_new_salary = 0;
  END
  ELSE
  BEGIN
    -- Get current salary
    SELECT @v_current_salary = salary FROM employees WHERE id = @p_emp_id;
    
    -- Calculate new salary
    SET @p_new_salary = @v_current_salary + (@v_current_salary * @p_percentage / 100);
    
    -- Update salary
    UPDATE employees SET salary = @p_new_salary WHERE id = @p_emp_id;
    
    SET @p_status = 'Salary updated successfully';
  END;
END;

-- Call procedure
DECLARE @new_salary DECIMAL(10,2), @status VARCHAR(50);
EXEC UpdateEmployeeSalary @p_emp_id = 1, @p_percentage = 10, @p_new_salary = @new_salary OUTPUT, @p_status = @status OUTPUT;
SELECT @new_salary, @status;
```

#### Oracle
```sql
CREATE OR REPLACE PROCEDURE UpdateEmployeeSalary(
  p_emp_id IN NUMBER,
  p_percentage IN NUMBER,
  p_new_salary OUT NUMBER,
  p_status OUT VARCHAR2
)
IS
  v_current_salary NUMBER;
  v_emp_count NUMBER;
BEGIN
  -- Check if employee exists
  SELECT COUNT(*) INTO v_emp_count FROM employees WHERE id = p_emp_id;
  
  IF v_emp_count = 0 THEN
    p_status := 'Employee not found';
    p_new_salary := 0;
  ELSE
    -- Get current salary
    SELECT salary INTO v_current_salary FROM employees WHERE id = p_emp_id;
    
    -- Calculate new salary
    p_new_salary := v_current_salary + (v_current_salary * p_percentage / 100);
    
    -- Update salary
    UPDATE employees SET salary = p_new_salary WHERE id = p_emp_id;
    
    p_status := 'Salary updated successfully';
    COMMIT;
  END IF;
END UpdateEmployeeSalary;
/

-- Call procedure
DECLARE
  v_new_salary NUMBER;
  v_status VARCHAR2(50);
BEGIN
  UpdateEmployeeSalary(1, 10, v_new_salary, v_status);
  DBMS_OUTPUT.PUT_LINE('New Salary: ' || v_new_salary || ', Status: ' || v_status);
END;
/
```

#### PostgreSQL
```sql
CREATE PROCEDURE UpdateEmployeeSalary(
  p_emp_id INT,
  p_percentage NUMERIC,
  INOUT p_new_salary NUMERIC,
  OUT p_status VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
  v_current_salary NUMERIC;
  v_emp_count INT;
BEGIN
  -- Check if employee exists
  SELECT COUNT(*) INTO v_emp_count FROM employees WHERE id = p_emp_id;
  
  IF v_emp_count = 0 THEN
    p_status := 'Employee not found';
    p_new_salary := 0;
  ELSE
    -- Get current salary
    SELECT salary INTO v_current_salary FROM employees WHERE id = p_emp_id;
    
    -- Calculate new salary
    p_new_salary := v_current_salary + (v_current_salary * p_percentage / 100);
    
    -- Update salary
    UPDATE employees SET salary = p_new_salary WHERE id = p_emp_id;
    
    p_status := 'Salary updated successfully';
    COMMIT;
  END IF;
END;
$$;

-- Call procedure
DO $$
DECLARE
  v_new_salary NUMERIC;
  v_status VARCHAR;
BEGIN
  CALL UpdateEmployeeSalary(1, 10, v_new_salary, v_status);
  RAISE NOTICE 'New Salary: %, Status: %', v_new_salary, v_status;
END;
$$;
```

### Transaction Management in Procedures

#### MySQL
```sql
DELIMITER //
CREATE PROCEDURE TransferBudget(
  IN p_from_dept INT,
  IN p_to_dept INT,
  IN p_amount DECIMAL(10,2)
)
BEGIN
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Transaction failed';
  END;
  
  START TRANSACTION;
  
  UPDATE department_budgets SET budget = budget - p_amount WHERE dept_id = p_from_dept;
  UPDATE department_budgets SET budget = budget + p_amount WHERE dept_id = p_to_dept;
  
  COMMIT;
END //
DELIMITER ;
```

#### SQL Server
```sql
CREATE PROCEDURE TransferBudget
  @p_from_dept INT,
  @p_to_dept INT,
  @p_amount DECIMAL(10,2)
AS
BEGIN
  BEGIN TRY
    BEGIN TRANSACTION;
    
    UPDATE department_budgets SET budget = budget - @p_amount WHERE dept_id = @p_from_dept;
    UPDATE department_budgets SET budget = budget + @p_amount WHERE dept_id = @p_to_dept;
    
    COMMIT TRANSACTION;
  END TRY
  BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
  END CATCH;
END;
```

#### Oracle
```sql
CREATE OR REPLACE PROCEDURE TransferBudget(
  p_from_dept IN NUMBER,
  p_to_dept IN NUMBER,
  p_amount IN NUMBER
)
IS
BEGIN
  UPDATE department_budgets SET budget = budget - p_amount WHERE dept_id = p_from_dept;
  UPDATE department_budgets SET budget = budget + p_amount WHERE dept_id = p_to_dept;
  COMMIT;
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END TransferBudget;
/
```

#### PostgreSQL
```sql
CREATE PROCEDURE TransferBudget(
  p_from_dept INT,
  p_to_dept INT,
  p_amount NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN
  UPDATE department_budgets SET budget = budget - p_amount WHERE dept_id = p_from_dept;
  UPDATE department_budgets SET budget = budget + p_amount WHERE dept_id = p_to_dept;
  COMMIT;
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
$$;
```

---

## Indexes

### Index Types and Concepts

#### MySQL
```sql
-- Show existing indexes
SHOW INDEXES FROM employees;
SHOW INDEX FROM employees;

-- Create index
CREATE INDEX idx_emp_name ON employees(name);

-- Create unique index
CREATE UNIQUE INDEX idx_emp_email ON employees(email);

-- Create composite index
CREATE INDEX idx_emp_dept_salary ON employees(dept_id, salary);

-- Create full-text index
CREATE FULLTEXT INDEX idx_emp_bio ON employees(biography);

-- Drop index
DROP INDEX idx_emp_name ON employees;

-- Analyze index
ANALYZE TABLE employees;
EXPLAIN SELECT * FROM employees WHERE name = 'John';

-- Get index statistics
SELECT * FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_NAME = 'employees';
```

#### SQL Server
```sql
-- Show existing indexes
SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('employees');

-- Create index
CREATE INDEX idx_emp_name ON employees(name);

-- Create unique index
CREATE UNIQUE INDEX idx_emp_email ON employees(email);

-- Create composite index
CREATE INDEX idx_emp_dept_salary ON employees(dept_id, salary);

-- Create clustered index (only one per table)
CREATE CLUSTERED INDEX idx_emp_id ON employees(id);

-- Create non-clustered index with included columns
CREATE NONCLUSTERED INDEX idx_emp_name_salary ON employees(name) INCLUDE (salary);

-- Drop index
DROP INDEX employees.idx_emp_name;

-- Analyze index
DBCC SHOW_STATISTICS (employees, idx_emp_name);
EXEC sp_helpindex 'employees';

-- Index statistics
SELECT * FROM sys.dm_db_index_usage_stats WHERE database_id = DB_ID();
```

#### Oracle
```sql
-- Show existing indexes
SELECT index_name FROM user_indexes WHERE table_name = 'EMPLOYEES';

-- Create index
CREATE INDEX idx_emp_name ON employees(name);

-- Create unique index
CREATE UNIQUE INDEX idx_emp_email ON employees(email);

-- Create composite index
CREATE INDEX idx_emp_dept_salary ON employees(dept_id, salary);

-- Create function-based index
CREATE INDEX idx_emp_upper_name ON employees(UPPER(name));

-- Create bitmap index (for low cardinality columns)
CREATE BITMAP INDEX idx_emp_status ON employees(status);

-- Drop index
DROP INDEX idx_emp_name;

-- Analyze index
ANALYZE INDEX idx_emp_name VALIDATE STRUCTURE;

-- Index statistics
SELECT * FROM user_ind_statistics WHERE table_name = 'EMPLOYEES';
```

#### PostgreSQL
```sql
-- Show existing indexes
\d employees  -- In psql
SELECT indexname FROM pg_indexes WHERE tablename = 'employees';

-- Create index
CREATE INDEX idx_emp_name ON employees(name);

-- Create unique index
CREATE UNIQUE INDEX idx_emp_email ON employees(email);

-- Create composite index
CREATE INDEX idx_emp_dept_salary ON employees(dept_id, salary);

-- Create partial index
CREATE INDEX idx_emp_active ON employees(id) WHERE status = 'active';

-- Create expression-based index
CREATE INDEX idx_emp_upper_name ON employees(UPPER(name));

-- Create BRIN index (for large tables)
CREATE INDEX idx_emp_id_brin ON employees USING BRIN (id);

-- Drop index
DROP INDEX idx_emp_name;

-- Analyze index
ANALYZE employees;

-- Index statistics
SELECT * FROM pg_stat_user_indexes WHERE relname = 'employees';
```

### Index Performance Optimization

#### MySQL
```sql
-- Query analysis with EXPLAIN
EXPLAIN SELECT * FROM employees WHERE name = 'John' AND dept_id = 1;

-- Extended explain format
EXPLAIN FORMAT=JSON SELECT * FROM employees WHERE name = 'John';

-- Check if index is being used
SELECT * FROM INFORMATION_SCHEMA.STATISTICS 
WHERE TABLE_NAME = 'employees' AND SEQ_IN_INDEX = 1;

-- Force index usage
SELECT * FROM employees FORCE INDEX (idx_emp_name) WHERE name = 'John';

-- Index hints
SELECT * FROM employees USE INDEX (idx_emp_dept_salary) 
WHERE dept_id = 1 AND salary > 50000;
```

#### SQL Server
```sql
-- Query analysis with STATISTICS IO
SET STATISTICS IO ON;
SELECT * FROM employees WHERE name = 'John' AND dept_id = 1;
SET STATISTICS IO OFF;

-- Execution plan
SET STATISTICS TIME ON;
SELECT * FROM employees WHERE name = 'John';
SET STATISTICS TIME OFF;

-- Check missing indexes
SELECT * FROM sys.dm_db_missing_index_details;

-- Check unused indexes
SELECT i.name FROM sys.indexes i
WHERE OBJECTPROPERTY(i.object_id, 'IsUserTable') = 1
AND i.index_id > 0  -- Exclude heap
AND NOT EXISTS (
  SELECT 1 FROM sys.dm_db_index_usage_stats s
  WHERE s.index_id = i.index_id
);
```

#### Oracle
```sql
-- Query analysis with EXPLAIN PLAN
EXPLAIN PLAN FOR
SELECT * FROM employees WHERE name = 'John' AND dept_id = 1;
SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);

-- Check index usage
SELECT index_name, used FROM v$object_usage WHERE table_name = 'EMPLOYEES';

-- Monitor index statistics
SELECT index_name, num_rows, sample_size, last_analyzed
FROM user_indexes
WHERE table_name = 'EMPLOYEES';

-- Rebuild index
ALTER INDEX idx_emp_name REBUILD;

-- Rebuild all indexes for a table
ALTER TABLE employees MOVE;
```

#### PostgreSQL
```sql
-- Query analysis with EXPLAIN
EXPLAIN SELECT * FROM employees WHERE name = 'John' AND dept_id = 1;

-- Extended explain format
EXPLAIN (ANALYZE, BUFFERS) SELECT * FROM employees WHERE name = 'John';

-- Check index usage
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
WHERE tablename = 'employees';

-- Check table and index size
SELECT pg_size_pretty(pg_total_relation_size('employees'));
SELECT pg_size_pretty(pg_relation_size('idx_emp_name'));

-- Reindex
REINDEX INDEX idx_emp_name;
REINDEX TABLE employees;

-- Vacuum and analyze
VACUUM ANALYZE employees;
```

### Advanced Index Strategies

#### MySQL
```sql
-- Covering index (using WHERE and SELECT columns)
CREATE INDEX idx_emp_composite ON employees(dept_id, name, salary);

-- Query covered by index (no need to access table)
SELECT dept_id, name, salary FROM employees 
WHERE dept_id = 1 AND name LIKE 'J%';

-- Prefix index for string columns
CREATE INDEX idx_emp_email_prefix ON employees(email(20));

-- Check table/index fragmentation
SELECT OBJECT_SCHEMA_NAME(ps.object_id) AS SchemaName,
       OBJECT_NAME(ps.object_id) AS TableName,
       i.name AS IndexName,
       ps.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
INNER JOIN sys.indexes i ON ps.object_id = i.object_id 
  AND ps.index_id = i.index_id;
```

#### SQL Server
```sql
-- Covering index with INCLUDE clause
CREATE NONCLUSTERED INDEX idx_emp_dept_name 
ON employees(dept_id, name) 
INCLUDE (salary, email);

-- Filtered index
CREATE NONCLUSTERED INDEX idx_emp_active 
ON employees(id) 
WHERE status = 'active';

-- Check index fragmentation
SELECT i.name, ps.avg_fragmentation_in_percent
FROM sys.indexes i
INNER JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
  ON i.object_id = ps.object_id AND i.index_id = ps.index_id
WHERE ps.avg_fragmentation_in_percent > 10
ORDER BY ps.avg_fragmentation_in_percent DESC;

-- Rebuild fragmented index
ALTER INDEX idx_emp_name ON employees REBUILD;

-- Reorganize index
ALTER INDEX idx_emp_name ON employees REORGANIZE;
```

#### Oracle
```sql
-- Create index with specific storage parameters
CREATE INDEX idx_emp_name ON employees(name)
INITRANS 4
STORAGE (INITIAL 1M NEXT 1M PCTINCREASE 0);

-- Local partitioned index
CREATE INDEX idx_emp_date ON employees(hire_date) LOCAL;

-- Global partitioned index
CREATE INDEX idx_emp_sal_global ON employees(salary)
GLOBAL PARTITION BY RANGE(salary) (
  PARTITION p1 VALUES LESS THAN (30000),
  PARTITION p2 VALUES LESS THAN (60000),
  PARTITION p3 VALUES LESS THAN (MAXVALUE)
);

-- Rebuild index with monitoring
ALTER INDEX idx_emp_name REBUILD ONLINE;

-- Monitor index size and fragmentation
SELECT index_name, leaf_blocks, btree_space, used_space
FROM index_stats
WHERE table_name = 'EMPLOYEES';
```

#### PostgreSQL
```sql
-- Partial index for WHERE clause optimization
CREATE INDEX idx_emp_high_sal ON employees(salary)
WHERE salary > 100000;

-- Multicolumn index with DESC
CREATE INDEX idx_emp_dept_sal_desc ON employees(dept_id, salary DESC);

-- Expression-based index
CREATE INDEX idx_emp_lower_name ON employees(LOWER(name));

-- GiST index for range queries
CREATE INDEX idx_emp_date_range ON employees USING GIST (tsrange(hire_date, hire_date + INTERVAL '1 day'));

-- GIN index for array/JSON columns
CREATE INDEX idx_emp_tags ON employees USING GIN (tags);

-- Check index bloat
SELECT schemaname, tablename, indexname, 
       ROUND(100*(CASE WHEN otta>0 THEN sml.relpages::float/otta 
       ELSE 0 END)) AS ratio
FROM pg_stat_user_indexes
WHERE tablename = 'employees';
```

---

## Comparison Matrix

### Feature Comparison

| Feature | MySQL | SQL Server | Oracle | PostgreSQL |
|---------|-------|-----------|--------|-----------|
| **Indexes** |
| Clustered Index | Primary Key only | Yes (one per table) | Primary Key | Yes (via OID) |
| Non-Clustered Index | Yes | Yes | Yes | Yes |
| Unique Index | Yes | Yes | Yes | Yes |
| Full-Text Index | Yes | Yes | Yes | Yes (tsearch2) |
| Partial Index | No | Filtered Index | No | Yes |
| Expression Index | No | No | Function-Based | Yes |
| **Functions** |
| Scalar Functions | Yes | Yes | Yes | Yes |
| Table Functions | Yes | Yes | Yes | Yes |
| Aggregate Functions | Yes | Yes | Yes | Yes |
| Window Functions | Yes (8.0+) | Yes | Yes | Yes |
| **Procedures** |
| Stored Procedures | Yes | Yes | Yes | Yes (11+) |
| Transactions | Yes | Yes | Yes | Yes |
| Exception Handling | Yes | Yes | Yes | Yes |
| **Query Features** |
| CTE (WITH clause) | Yes | Yes | Yes | Yes |
| Window Functions | Yes (8.0+) | Yes | Yes | Yes |
| JSON Support | Yes (5.7+) | Yes | Yes | Yes |
| Full-Text Search | Yes | Yes | Yes | Yes (tsearch2) |
| Recursive CTE | Yes (8.0+) | Yes | Yes | Yes |

### Performance Comparison

| Operation | MySQL | SQL Server | Oracle | PostgreSQL |
|-----------|-------|-----------|--------|-----------|
| Simple SELECT | Fast | Fast | Fast | Fast |
| Complex JOIN | Good | Excellent | Excellent | Good |
| Aggregate Query | Good | Good | Excellent | Good |
| Window Functions | Good | Good | Excellent | Good |
| Large Data Set (1M+) | Good | Excellent | Excellent | Good |
| Transaction Speed | Good | Excellent | Excellent | Good |

### Syntax Differences Summary

| Operation | MySQL | SQL Server | Oracle | PostgreSQL |
|-----------|-------|-----------|--------|-----------|
| **String Concat** | CONCAT() or + | + or CONCAT() | \|\| or CONCAT() | \|\| |
| **Limit Results** | LIMIT | TOP | ROWNUM/FETCH | LIMIT |
| **Date Add** | DATE_ADD() | DATEADD() | ADD_MONTHS() | + INTERVAL |
| **Current Date** | NOW() | GETDATE() | SYSDATE | CURRENT_DATE |
| **NULL Check** | IS NULL | IS NULL | IS NULL | IS NULL |
| **CASE** | CASE | CASE | CASE/DECODE | CASE |
| **Procedure Params** | IN/OUT | @ prefix | IN/OUT | IN/OUT |
| **Transactions** | START TRANSACTION | BEGIN TRANSACTION | Implicit | BEGIN |
| **Outer Join** | Simulated | FULL OUTER | FULL OUTER | FULL OUTER |

---

## Key Takeaways

### Best Practices

1. **Indexes**
   - Always create indexes on PRIMARY KEY columns
   - Create indexes on frequently queried columns in WHERE clauses
   - Use composite indexes for columns queried together
   - Monitor index usage and remove unused indexes
   - Be careful with too many indexes (slow INSERT/UPDATE)

2. **Functions**
   - Use built-in functions before creating custom ones
   - Mark functions as DETERMINISTIC when possible (performance)
   - Be cautious with functions in WHERE clauses (may prevent index usage)
   - Document parameter and return types clearly

3. **Stored Procedures**
   - Use procedures for complex business logic
   - Always implement error handling
   - Use transactions for data consistency
   - Avoid cursors when set-based operations are available
   - Document parameters and expected behavior

4. **Queries**
   - Always use EXPLAIN/EXPLAIN PLAN to analyze query performance
   - Avoid SELECT * in production code
   - Use appropriate JOINs (INNER, LEFT, RIGHT, FULL OUTER)
   - Filter data as early as possible in queries
   - Use window functions for complex analytics

### Common Gotchas

1. **Index Design**
   - Indexes speed up SELECT but slow down INSERT/UPDATE/DELETE
   - Too many indexes can cause problems
   - Index column order matters in composite indexes
   - Non-clustered indexes don't include all columns by default

2. **Functions**
   - Functions in SELECT clause may cause table scans
   - String functions are often database-specific
   - NULL handling differs between functions
   - Some functions are case-sensitive

3. **Procedures**
   - Transaction handling varies by database
   - Parameter passing syntax is different
   - Error handling approaches differ significantly
   - Some databases limit what procedures can do

4. **Syntax Differences**
   - Date/Time functions are highly database-specific
   - String concatenation varies (CONCAT vs +  vs ||)
   - LIMIT/OFFSET vs TOP vs ROWNUM
   - NULL vs ISNULL vs NVL vs COALESCE usage

