USE MASTER
GO

IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'RepositoryTest')
    CREATE DATABASE RepositoryTest
GO