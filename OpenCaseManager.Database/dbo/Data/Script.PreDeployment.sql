/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

SET IDENTITY_INSERT [dbo].[Employee] ON 

GO
INSERT [dbo].[Employee]
  (
    [Id],
    [cpr],
    [EmployeeId],
    [fullName],
    [cprnr2],
    [department],
    [email]
  )
VALUES
  (
    1,
    N'1234-5678',
    N'UserName',
    N'Full Name',
    N'9876-5432',
    N'Department',
    N'email@some-domain.com'
  )
GO
SET IDENTITY_INSERT [dbo].[Employee] OFF
GO
SET IDENTITY_INSERT [dbo].[User] ON 

GO
INSERT [dbo].[User]
  (
    [Id],
    [SamAccountName],
    [Name],
    [Title],
    [Department],
    [ManagerId]
  )
VALUES
  (
    1,
    N'UserName',
    N'Full Name',
    N'Title',
    N'Department',
    NULL
  )
GO
INSERT [dbo].[User]
  (
    [Id],
    [SamAccountName],
    [Name],
    [Title],
    [Department],
    [ManagerId]
  )
VALUES
  (
    2,
    N'UserName1',
    N'Full Name',
    N'Title',
    N'Department',
    1
  )
GO
SET IDENTITY_INSERT [dbo].[User] OFF
GO
