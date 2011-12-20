USE RepositoryTest
GO

UPDATE	[dbo].[Orders]
SET	ShipName = 'Frankenversand'
WHERE	OrderId = 10337
GO