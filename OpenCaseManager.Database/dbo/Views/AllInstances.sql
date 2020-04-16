CREATE VIEW dbo.AllInstances
AS
SELECT        dbo.Instance.Id, dbo.Instance.Title, dbo.Instance.CaseNoForeign, dbo.Instance.CaseLink, dbo.Instance.IsOpen, dbo.[User].Name AS Responsible, dbo.Instance.GraphId, dbo.Instance.SimulationId, dbo.Instance.CurrentTime, 
                         dbo.Instance.Modified, dbo.Instance.Description, dbo.Instance.InternalCaseID, dbo.Instance.CaseStatus, dbo.Instance.NextDeadline, dbo.Instance.CurrentPhaseNo, dbo.Instance.IsAccepting, dbo.Instance.NextDelay
FROM            dbo.Instance INNER JOIN
                         dbo.[User] ON dbo.Instance.Responsible = dbo.[User].Id
GO



GO



GO


