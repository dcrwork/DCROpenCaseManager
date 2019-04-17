CREATE VIEW [dbo].[ChildInstances] AS
SELECT I.[Id], I.[Title], U.[Name], I.[NextDeadline], I.[IsOpen], I.[Description], P.[Title] AS Process, C.[Id] AS ChildId, (SELECT MAX(CreationDate)
																											 FROM [dbo].[JournalHistory]
																											 WHERE I.[Id] = [JournalHistory].[InstanceId]) AS LastUpdated
FROM [dbo].[Instance] AS I, [dbo].[InstanceExtension] AS IE, [dbo].[Child] AS C, [dbo].[User] AS U, [dbo].[Process] AS P
WHERE I.[Id] = IE.[InstanceId]
	AND IE.[ChildId] = C.[Id]
	AND I.[Responsible] = U.[Id]
	AND I.[GraphId] = P.[GraphId]
GO
GO