CREATE TABLE [dbo].[MenuItemAccess] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [MenuItemId]   INT NULL,
    [DepartmentId] INT NULL,
    [IsManager]    BIT NULL,
    CONSTRAINT [PK__MenuItem__3214EC0772C2815F] PRIMARY KEY CLUSTERED ([Id] ASC)
);


