CREATE TABLE [dbo].[User] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [SamAccountName]    NVARCHAR (500) NOT NULL,
    [Name]              NVARCHAR (500) NOT NULL,
    [Title]             NVARCHAR (100) NULL,
    [ManagerId]         INT            NULL,
    [Acadreorgid]       INT            NULL,
    [DepartmentId]      INT            NULL,
    [IsManager]         BIT            NULL,
    [Familieafdelingen] INT            NULL,
    [Department]        NVARCHAR (500) NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);







