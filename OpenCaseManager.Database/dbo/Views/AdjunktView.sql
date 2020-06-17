CREATE VIEW [dbo].[AdjunktView] AS
SELECT A.[Id]
      ,A.[Name]
      ,U.[Name] AS ResponsibleName
	  ,A.[Responsible]
FROM [dbo].[Adjunkt] AS A LEFT JOIN [dbo].[User] AS U ON U.Id = A.Responsible
GO


