CREATE VIEW [dbo].[DocumentTimes]
AS
SELECT        
dbo.Document.Id, 
dbo.Document.Title, 
dbo.Document.Type, 
dbo.Document.Link, 
dbo.Document.Responsible,
dbo.Document.InstanceId,
dbo.Document.UploadDate,  
dbo.Document.IsLocked,  
dbo.Document.IsDraft,
dbo.JournalHistory.EventDate 
FROM dbo.Document, dbo.JournalHistory 
WHERE dbo.Document.Id = dbo.JournalHistory.DocumentId
GO



GO


