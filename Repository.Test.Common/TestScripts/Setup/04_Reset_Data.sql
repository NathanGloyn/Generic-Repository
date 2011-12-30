USE RepositoryTest
GO

DELETE [dbo].[Orders]
WHERE OrderId > 10347
GO

DBCC CHECKIDENT('Orders', Reseed, 10347)
GO