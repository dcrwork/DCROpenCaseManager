/****** Object:  View [dbo].[Timeline]    Script Date: 03-05-2019 11:21:03 ******/
CREATE VIEW [dbo].[Timeline]
AS
SELECT        dbo.JournalHistory.InstanceId, dbo.JournalHistory.EventId, dbo.JournalHistory.Id, dbo.JournalHistory.DocumentId, dbo.JournalHistory.Type, dbo.JournalHistory.Title, dbo.JournalHistory.CreationDate, 
                         dbo.JournalHistory.EventDate, dbo.JournalHistory.IsLocked, dbo.InstanceExtension.ChildId, dbo.Event.Responsible AS ResponsibleId, dbo.[Document].Title AS DocumentTitle, dbo.[Document].Type AS DocumentType, 
                         dbo.[Document].Link, dbo.Instance.Title AS InstanceTitle, User_2.Name AS EventResponsible, User_1.Name AS DocumentResponsible
FROM            dbo.[Document] INNER JOIN
                         dbo.[User] AS User_1 ON dbo.[Document].Responsible = User_1.SamAccountName FULL OUTER JOIN
                         dbo.[User] AS User_2 INNER JOIN
                         dbo.Event ON User_2.Id = dbo.Event.Responsible RIGHT OUTER JOIN
                         dbo.Instance INNER JOIN
                         dbo.InstanceExtension INNER JOIN
                         dbo.JournalHistory ON dbo.InstanceExtension.InstanceId = dbo.JournalHistory.InstanceId ON dbo.Instance.Id = dbo.InstanceExtension.InstanceId ON dbo.Event.Id = dbo.JournalHistory.EventId ON 
                         dbo.[Document].Id = dbo.JournalHistory.DocumentId
GO

