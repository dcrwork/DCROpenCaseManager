CREATE VIEW dbo.UserDetail
AS
SELECT        Id, SamAccountName, Name, Title, Department, ManagerId, Acadreorgid, IsManager, DepartmentId
FROM            dbo.[User] AS u
GO



GO


