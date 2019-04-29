CREATE TABLE [dbo].[StamdataDummyData] (
    [Id]				INT             IDENTITY (1, 1) NOT NULL,
    [CPR]				nvarchar(11)	NOT NULL,
    [Name]				nvarchar(100)   NOT NULL,
    [Address]			nvarchar(100)   NULL,
    [City]				NVARCHAR (100)  NULL,
    [Postcode]			int				NULL,
    CONSTRAINT [PK_StamdataDummyData] PRIMARY KEY CLUSTERED ([Id] ASC)
);

drop table StamdataDummyData;