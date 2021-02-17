CREATE VIEW [dbo].[InstanceAIRoboticEvents]
AS
SELECT        dbo.Event.EventId, dbo.Event.Title AS EventTitle, dbo.Event.isOpen AS EventOpen, dbo.Event.IsEnabled, dbo.Event.IsPending, dbo.Event.IsIncluded, dbo.Event.IsExecuted, dbo.Event.EventType, dbo.Event.InstanceId, 
                         dbo.Event.Responsible, dbo.Event.EventTypeData, i.Modified, i.GraphId, i.SimulationId, dbo.Event.Id TrueEventId, dbo.Event.Description, i.NextDelay, i.NextDeadline, CASE 
            WHEN i.NextDelay < GETUTCDATE() THEN 1
            WHEN i.NextDeadline < GETUTCDATE() and datediff(minute,i.nextdeadline,getutcdate())<5 THEN 1
            ELSE 0
       END AS NeedToSetTime
FROM            dbo.Event INNER JOIN
                         dbo.Instance i ON dbo.Event.InstanceId = i.Id
WHERE        (dbo.Event.Roles = 'AIRobot') AND (dbo.Event.IsPending = 1) AND (dbo.Event.IsEnabled = 1)
GO
