CREATE TABLE [dbo].[StamdataChild] (
    [Id]				INT             IDENTITY (1, 1) NOT NULL,
    [ChildId]			INT				NOT NULL,
    [Sagsnummer]		NVARCHAR(50)    NOT NULL,
	[Addresse]			nvarchar(100)	NULL,
	[Forældremyndighed] nvarchar(50)	NULL,
	[Skole]				nvarchar(100)	NULL,
	[Alder]				INT				NULL,
	[Navn]				nvarchar(200)	NULL,
    CONSTRAINT [PK_StamdataChild] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StamdataChild_ChildId] FOREIGN KEY ([ChildId]) REFERENCES [dbo].[Child] ([Id]) ON DELETE CASCADE
);