CREATE VIEW dbo.ContextInstances
AS
SELECT        dbo.InstanceExtension.InstanceId, dbo.InstanceExtension.ChildId, dbo.Instance.SimulationId, dbo.Instance.GraphId, dbo.Instance.Title, dbo.Instance.DCRXML
FROM            dbo.Instance INNER JOIN
                         dbo.InstanceExtension ON dbo.Instance.Id = dbo.InstanceExtension.InstanceId