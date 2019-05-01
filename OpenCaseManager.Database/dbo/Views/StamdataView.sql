
CREATE VIEW [dbo].[StamdataView]
AS
SELECT        dbo.Child.Name, dbo.StamdataChild.Sagsnummer, dbo.StamdataChild.Addresse, dbo.StamdataChild.Alder, dbo.StamdataChild.Forældremyndighed, dbo.StamdataChild.Skole, dbo.StamdataDummyData.CPR, 
                         dbo.StamdataDummyData.Name AS StamdataName, dbo.StamdataDummyData.Address, dbo.StamdataDummyData.City, dbo.StamdataDummyData.Postcode, dbo.StamdataDummyDataExtension.Relation, 
                         dbo.Child.Id AS ChildId
FROM            dbo.StamdataChild RIGHT OUTER JOIN
                         dbo.Child ON dbo.StamdataChild.ChildId = dbo.Child.Id LEFT OUTER JOIN
                         dbo.StamdataDummyData INNER JOIN
                         dbo.StamdataDummyDataExtension ON dbo.StamdataDummyData.Id = dbo.StamdataDummyDataExtension.StamdataId ON dbo.Child.Id = dbo.StamdataDummyDataExtension.ChildId