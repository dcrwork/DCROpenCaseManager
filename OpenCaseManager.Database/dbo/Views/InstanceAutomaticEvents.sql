CREATE VIEW [dbo].[InstanceAutomaticEvents]
AS
SELECT        dbo.Event.EventId, dbo.Event.Title AS EventTitle, dbo.Event.isOpen AS EventOpen, dbo.Event.IsEnabled, dbo.Event.IsPending, dbo.Event.IsIncluded, dbo.Event.IsExecuted, dbo.Event.EventType, dbo.Event.InstanceId, 
                         dbo.Event.Responsible, dbo.Event.EventTypeData
FROM            dbo.Event INNER JOIN
                         dbo.Instance ON dbo.Event.InstanceId = dbo.Instance.Id
WHERE        ((dbo.Event.Responsible = N'-1') AND (dbo.Event.IsPending = 1) AND (dbo.Event.IsEnabled = 1) AND dbo.Event.Roles != 'AIRobot') 
GO