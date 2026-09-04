-- Create Tables
CREATE TABLE [dbo].[UserRole] (
    [UserRoleId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [RoleName] [nvarchar](50) NOT NULL UNIQUE,
    [Description] [nvarchar](255),
    [CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE [dbo].[User] (
    [UserId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Email] [nvarchar](100) NOT NULL UNIQUE,
    [PasswordHash] [nvarchar](255) NOT NULL,
    [FirstName] [nvarchar](50) NOT NULL,
    [LastName] [nvarchar](50) NOT NULL,
    [PhoneNumber] [nvarchar](20),
    [UserRoleId] [uniqueidentifier] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedDate] [datetime] DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_User_UserRole] FOREIGN KEY ([UserRoleId]) REFERENCES [dbo].[UserRole]([UserRoleId])
);
 
 CREATE TABLE [dbo].[Event] (
    [EventId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [EventName] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](max),
    [EventDate] [datetime] NOT NULL,
    [Location] [nvarchar](100) NOT NULL,
    [Distance] [decimal](5,2) NOT NULL,
    [Capacity] [int] NOT NULL,
    [OrganiserId] [uniqueidentifier] NOT NULL,
    [Status] [nvarchar](20) NOT NULL DEFAULT 'Scheduled',
    [CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedDate] [datetime] DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Event_Organiser] FOREIGN KEY ([OrganiserId]) REFERENCES [dbo].[User]([UserId])
);

CREATE TABLE [dbo].[Category] (
    [CategoryId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [CategoryName] [nvarchar](50) NOT NULL,
    [Description] [nvarchar](45),
    [MinAge] [int],
    [MaxAge] [int],
    [CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE [dbo].[EventCategory] (
    [EventCategoryId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [EventId] [uniqueidentifier] NOT NULL,
    [CategoryId] [uniqueidentifier] NOT NULL,
    [CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_EventCategory_Event] FOREIGN KEY ([EventId]) REFERENCES [dbo].[Event]([EventId]),
    CONSTRAINT [FK_EventCategory_Category] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category]([CategoryId]),
    CONSTRAINT [UQ_EventCategory] UNIQUE ([EventId], [CategoryId])
);
CREATE TABLE [dbo].[Enrolment] (
    [EnrolmentId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [ParticipantId] [uniqueidentifier] NOT NULL,
    [EventCategoryId] [uniqueidentifier] NOT NULL,
    [EmergencyContact] [nvarchar](100),
    [EnrolmentStatus] [nvarchar](20) NOT NULL DEFAULT 'Active',
    [EnrolmentDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
    [CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedDate] [datetime] DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Enrolment_Participant] FOREIGN KEY ([ParticipantId]) REFERENCES [dbo].[User]([UserId]),
    CONSTRAINT [FK_Enrolment_EventCategory] FOREIGN KEY ([EventCategoryId]) REFERENCES [dbo].[EventCategory]([EventCategoryId])
);

CREATE TABLE [dbo].[Result] (
    [ResultId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [EnrolmentId] [uniqueidentifier] NOT NULL UNIQUE,
    [FinishTime] [time] NOT NULL,
    [Position] [int] NOT NULL,
    [ChipNumber] [nvarchar](20),
    [Status] [nvarchar](20) NOT NULL DEFAULT 'Finished',
    [CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Result_Enrolment] FOREIGN KEY ([EnrolmentId]) REFERENCES [dbo].[Enrolment]([EnrolmentId])
);
 