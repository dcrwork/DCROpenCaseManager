﻿CREATE FUNCTION [dbo].[ChildInstances]
(
	@UserId INT
)
RETURNS TABLE
AS

            RETURN (
                       SELECT I.Id,
                              I.Title,
                              I.InternalCaseId,
                              I.CaseNoForeign,
                              I.CaseLink,
                              U.Name      AS CaseManagerName,
                              I.NextDeadline,
                              I.IsOpen,
                              CASE 
                                   WHEN I.IsOpen = 1 THEN 0
                                   ELSE 1
                              END         AS IsClosed,
                              I.Description,
                              P.Title     AS Process,
                              IE.ChildId  AS ChildId,
                              (
                                  SELECT MAX(CreationDate)
                                  FROM   dbo.JournalHistory
                                  WHERE  I.Id = JournalHistory.InstanceId
                              )           AS LastUpdated,
                              (
                                  SELECT COUNT(*)
                                  FROM   dbo.Event
                                  WHERE  IsEnabled = 1
                                         AND IsPending = 1
                                         AND InstanceId = I.Id
                                         AND Responsible = @UserId
                              )           AS PendingAndEnabled,
                              (
                                  SELECT CASE 
                                              WHEN COUNT(*) > 0 THEN 'true'
                                              ELSE 'false'
                                         END AS PendingBool
                                  FROM   dbo.Event
                                  WHERE  IsPending = 1
                                         AND InstanceId = I.Id
                                         AND Responsible = @UserId
                              )           AS Pending
                       FROM   dbo.Instance AS I
                              INNER JOIN dbo.InstanceExtension AS IE
                                   ON  I.Id = IE.InstanceId
                              INNER JOIN dbo.[User] AS U
                                   ON  I.Responsible = U.Id
                              LEFT OUTER JOIN dbo.Process AS P
                                   ON  I.GraphId = P.GraphId
                   )