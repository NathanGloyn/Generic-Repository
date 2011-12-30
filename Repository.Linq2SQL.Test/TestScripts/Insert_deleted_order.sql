USE RepositoryTest
GO

SET IDENTITY_INSERT [dbo].[Orders] ON
INSERT [dbo].[Orders] ([OrderID], [CustomerID], [EmployeeID], [OrderDate], [RequiredDate], [ShippedDate], [ShipVia], [Freight], [ShipName], [ShipAddress], [ShipCity], [ShipRegion], [ShipPostalCode], [ShipCountry]) VALUES (10287, N'RICAR', 8, CAST(0x000089E100000000 AS DateTime), CAST(0x000089FD00000000 AS DateTime), CAST(0x000089E700000000 AS DateTime), 3, 12.7600, N'Ricardo Adocicados', N'Av. Copacabana, 267', N'Rio de Janeiro', N'RJ', N'02389-890', N'Brazil')
GO