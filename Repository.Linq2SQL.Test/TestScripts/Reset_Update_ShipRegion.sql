USE RepositoryTest
GO

UPDATE	[dbo].[Orders]
SET	ShipRegion = NULL
WHERE	OrderId = 10337
GO