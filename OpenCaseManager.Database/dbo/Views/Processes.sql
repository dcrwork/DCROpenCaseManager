CREATE VIEW [dbo].[Processes]
AS
SELECT ph.Id,
       ph.GraphId,
       ISNULL(p.Title, ph.Title)    AS Title,
       ISNULL(p.DCRXML, ph.DCRXML)  AS DCRXML,
       p.MajorVersionId,
       p.MajorVersionTitle,
       p.MajorVerisonDate,
       ph.State                     AS ProcessApprovalState,
       p.OnFrontPage,
       ISNULL(
           (
               SELECT NAME
               FROM   dbo.[User] AS u
               WHERE  (Id = ISNULL(p.Owner, ph.Owner))
           ),
           ''
       )                            AS ProcessOwner,
       ph.ReleaseDate,
       ph.InstanceId,
       p.Id                         AS ProcessId
FROM   dbo.ProcessHistory           AS ph
       LEFT OUTER JOIN dbo.Process  AS p
            ON  p.GraphId = ph.GraphId
WHERE  (
           ph.GraphId IN (SELECT GraphId
                          FROM   dbo.ProcessHistory AS ph
                          GROUP BY
                                 GraphId)
       )
       AND (
               ph.MajorVersionId IS NULL
               OR ph.MajorVersionId IN (SELECT MAX(MajorVersionId) AS 
                                               MaxMajorRevisionId
                                        FROM   dbo.ProcessHistory AS ph
                                        GROUP BY
                                               GraphId)
           )
       AND (ph.State <> - 1)
       AND (
               ph.Id IN (SELECT MAX(Id) AS MaxId
                         FROM   dbo.ProcessHistory AS ph2
                         GROUP BY
                                GraphId)
           )
       AND (ISNULL(p.Status, ph.Status) = 1)