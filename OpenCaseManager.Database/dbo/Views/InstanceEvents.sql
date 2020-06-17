CREATE VIEW [dbo].[InstanceEvents]
AS
SELECT e.EventId,
       CASE 
            WHEN CHARINDEX('[', e.EventId) > 0 AND CHARINDEX(']', e.eventid) = 
                 LEN(e.EventId) THEN e.Title + SUBSTRING(e.eventid, CHARINDEX('[', e.EventId), LEN(e.eventid))
            ELSE e.Title
       END              AS EventTitle,
       u.Id             AS Responsible,
       u.Name,
       e.Due,
       e.isOpen         AS EventIsOpen,
       e.IsEnabled,
       e.IsPending,
       e.IsIncluded,
       e.IsExecuted,
       e.EventType,
       e.InstanceId,
       i.SimulationId,
       p.GraphId,
       e.Description,
       i.CaseNoForeign  AS [Case],
       i.CaseLink,
       i.Title          AS CaseTitle,
       i.IsOpen         AS InstanceIsOpen,
       CASE ISNULL(
                e.[EventTypeData].value('(/parameter[@title="UIEvent"]/@value)[1]', 'varchar(500)'),
                ''
            )
            WHEN '' THEN 0
            WHEN '0' THEN 0
            ELSE 1
       END AS IsUIEvent,
       e.EventTypeData.value('(/parameter[@title="UIEvent"]/@value)[1]', 'varchar(500)') AS 
       UIEventValue,
       e.EventTypeData.value(
           '(/parameter[@title="UIEventClass"]/@value)[1]',
           'varchar(500)'
       ) AS UIEventCssClass,
       ISNULL(e.Type, N'') AS TYPE,
       CASE 
            WHEN e.Due < GETUTCDATE() THEN 1
            ELSE 0
       END AS IsOverDue,
       DATEDIFF(DAY, e.Due, GETUTCDATE()) AS DaysPassedDue,
       i.Modified,
       i.NextDelay,
       i.NextDeadline,
       CASE 
            WHEN i.NextDelay < GETUTCDATE() THEN 1
            WHEN i.NextDeadline < GETUTCDATE() and datediff(minute,i.nextdeadline,getutcdate())<5 THEN 1
            ELSE 0
       END AS NeedToSetTime,
       e.Id AS TrueEventId,
       ISNULL(e.NotApplicable, 0) AS NotApplicable,
       ISNULL(e2.IsPending, e.IsPending) AS ActualIsPending,
       ISNULL(e2.IsEnabled, e.IsEnabled) AS ActualIsEnabled,
       ISNULL(e2.IsExecuted, e.IsExecuted) AS ActualIsExecuted,
       CASE 
            WHEN e.ParentId IS NOT NULL THEN - 1
            WHEN (
                     SELECT COUNT(*)
                     FROM   [Event] AS e3
                     WHERE  e3.ParentId = e.Id
                 ) > 0 THEN 0
            ELSE 1
       END AS Preference,
       e.ParentId,
	   e.Roles
FROM   dbo.Event AS e
       INNER JOIN dbo.Instance AS i
            ON  e.InstanceId = i.Id
       INNER JOIN dbo.Process AS p
            ON  p.GraphId = i.GraphId
       INNER JOIN dbo.[User] AS u
            ON  e.Responsible = u.Id
       LEFT OUTER JOIN dbo.Event AS e2
            ON  e2.Id = e.ParentId
GO



GO





GO



GO


