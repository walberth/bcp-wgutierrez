USE master;
GO

DECLARE @DBName NVARCHAR(50) = 'MICROSERVICIO_PEDIDOS';
DECLARE @sql NVARCHAR(MAX) = '';

-- Check if the database exists
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = @DBName)
BEGIN
    SELECT @sql = @sql + 'KILL ' + CAST(spid AS NVARCHAR(10)) + '; '
    FROM master.dbo.sysprocesses
    WHERE dbid = DB_ID(@DBName);

    IF @sql <> ''
    BEGIN
        EXEC sp_executesql @sql;
    END
    ELSE
    BEGIN
        PRINT 'No active connections found for database ' + @DBName + '.';
    END
END
ELSE
BEGIN
    PRINT 'Database ' + @DBName + ' does not exist.';
END
GO

DROP DATABASE IF EXISTS MICROSERVICIO_PEDIDOS;
GO

CREATE DATABASE MICROSERVICIO_PEDIDOS;
GO

USE MICROSERVICIO_PEDIDOS;
GO

DROP TABLE IF EXISTS [dbo].[Cliente];
CREATE TABLE [dbo].[Cliente] (
    IdCliente INT IDENTITY(1,1) PRIMARY KEY,
    NombreCliente VARCHAR(100)
);
GO

insert into [dbo].[Cliente] (NombreCliente) 
values 
	('WALBERTH'), 
	('ANGELA'), 
	('FELIPE'), 
	('DANIELA');
GO

DROP TABLE IF EXISTS [dbo].[Pedido];
CREATE TABLE [dbo].[Pedido] (
    IdPedido INT IDENTITY(1,1) PRIMARY KEY,
    FechaPedido DATETIME,
    IdCliente INT,
    MontoPedido DECIMAL(9,2),
    FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente)
);
GO

insert into [dbo].[Pedido] (FechaPedido, IdCliente, MontoPedido) 
values 
	(CURRENT_TIMESTAMP, 1, 145), 
	(CURRENT_TIMESTAMP, 2, 145), 
	(CURRENT_TIMESTAMP, 3, 145);
GO
--SELECT * FROM [dbo].[Cliente];
--SELECT * FROM [dbo].[Pedido];