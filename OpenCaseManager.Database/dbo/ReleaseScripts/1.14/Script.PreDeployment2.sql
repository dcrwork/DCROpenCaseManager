/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
Update Data to work with Governance of Process
--------------------------------------------------------------------------------------
*/

INSERT INTO ProcessHistory
(
	-- Id -- this column value is auto-generated
	GraphId,
	Title,
	ForeignIntegration,
	DCRXML,
	[Status],
	Created,
	Modified,
	OnFrontPage,
	Guid,
	CreateInstance,
	EventId,
	InstanceGuid,
	MajorVersionId,
	MajorVersionTitle,
	MajorVerisonDate,
	ReleaseDate,
	[Owner],
	InstanceId,
	[State]
)
SELECT
	-- Id -- this column value is auto-generated
	GraphId,
	Title,
	ForeignIntegration,
	DCRXML,
	[Status],
	Created,
	Modified,
	OnFrontPage,
	Guid,
	CreateInstance,
	EventId,
	InstanceGuid,
	MajorVersionId,
	MajorVersionTitle,
	MajorVerisonDate,
	ReleaseDate,
	[Owner],
	NULL,
	1
FROM Process AS p 