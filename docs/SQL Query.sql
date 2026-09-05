CREATE DATABASE RaceDay

-- Create Tables
CREATE TABLE [dbo].[UserRole] (
    [UserRoleId] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [RoleName] [nvarchar](50) NOT NULL UNIQUE,
    [Description] [nvarchar](45),
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

--Insert Information
INSERT INTO [dbo].[UserRole] ([RoleName], [Description]) VALUES 
    (N'Organiser', N'Event organizer who creates and manages events'),
    (N'Participant', N'Person who participates in events');
 
 DECLARE @OrgRoleId UNIQUEIDENTIFIER = (SELECT [UserRoleId] FROM [dbo].[UserRole] WHERE [RoleName] = 'Organiser');
DECLARE @PartRoleId UNIQUEIDENTIFIER = (SELECT [UserRoleId] FROM [dbo].[UserRole] WHERE [RoleName] = 'Participant');
 
-- Insert Users (Organisers)
INSERT INTO [dbo].[User] ([Email], [PasswordHash], [FirstName], [LastName], [UserRoleId]) VALUES 
    (N'organiser1@raceday.co.za', N'hashed_password_1', N'Mandla', N'Masemola', @OrgRoleId),
    (N'organiser2@raceday.co.za', N'hashed_password_2', N'Sarah', N'Mahlomuza', @OrgRoleId);

    DECLARE @OrgRoleId UNIQUEIDENTIFIER = (SELECT [UserRoleId] FROM [dbo].[UserRole] WHERE [RoleName] = 'Organiser');
DECLARE @PartRoleId UNIQUEIDENTIFIER = (SELECT [UserRoleId] FROM [dbo].[UserRole] WHERE [RoleName] = 'Participant');
 
-- Insert Users (Participants)
INSERT INTO [dbo].[User] ([Email], [PasswordHash], [FirstName], [LastName], [UserRoleId]) VALUES 
    (N'participant1@raceday.co.za', N'hashed_password_3', N'Nkulu', N'Ntuli', @PartRoleId),
    (N'participant2@raceday.co.za', N'hashed_password_4', N'Lindiwe', N'Mabena', @PartRoleId),
    (N'participant3@raceday.co.za', N'hashed_password_5', N'Michael', N'Sithole', @PartRoleId);
 
 DECLARE @Org1 UNIQUEIDENTIFIER = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'organiser1@raceday.co.za');
DECLARE @Part1 UNIQUEIDENTIFIER = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant1@raceday.co.za');
DECLARE @Part2 UNIQUEIDENTIFIER = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant2@raceday.co.za');
DECLARE @Part3 UNIQUEIDENTIFIER = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant3@raceday.co.za');
 
INSERT INTO [dbo].[Event] ([EventName], [Description], [EventDate], [Location], [Distance], [Capacity], [OrganiserId], [Status]) VALUES 
    (N'Pretoria 10K Run', N'Annual 10 kilometer running event', DATEFROMPARTS(2026, 10, 15), N'Pretoria', 10.0, 500, @Org1, N'Scheduled'),
    (N'Johannesburg Half Marathon', N'Half marathon event for all fitness levels', DATEFROMPARTS(2026, 11, 20), N'Johannesburg', 21.1, 1000, @Org1, N'Scheduled'),
    (N'Cape Town Cycle Challenge', N'50km cycling event', DATEFROMPARTS(2026, 12, 10), N'Cape Town', 50.0, 300, @Org1, N'Scheduled');
 
 DECLARE @Event1 UNIQUEIDENTIFIER = (SELECT [EventId] FROM [dbo].[Event] WHERE [EventName] = 'Pretoria 10K Run');
DECLARE @Event2 UNIQUEIDENTIFIER = (SELECT [EventId] FROM [dbo].[Event] WHERE [EventName] = 'Johannesburg Half Marathon');
DECLARE @Event3 UNIQUEIDENTIFIER = (SELECT [EventId] FROM [dbo].[Event] WHERE [EventName] = 'Cape Town Cycle Challenge');
 
INSERT INTO [dbo].[Category] ([CategoryName], [Description], [MinAge], [MaxAge]) VALUES 
    (N'5K', N'5 kilometer category', NULL, NULL),
    (N'10K', N'10 kilometer category', NULL, NULL),
    (N'Half Marathon', N'21.1 kilometer category', NULL, NULL),
    (N'U18', N'Under 18 years', 0, 18),
    (N'18-35', N'Ages 18 to 35', 18, 35),
    (N'35+', N'35 years and older', 35, 120);
 
 DECLARE @Cat10K UNIQUEIDENTIFIER = (SELECT [CategoryId] FROM [dbo].[Category] WHERE [CategoryName] = '10K');
DECLARE @CatHM UNIQUEIDENTIFIER = (SELECT [CategoryId] FROM [dbo].[Category] WHERE [CategoryName] = 'Half Marathon');
DECLARE @Cat1835 UNIQUEIDENTIFIER = (SELECT [CategoryId] FROM [dbo].[Category] WHERE [CategoryName] = '18-35');
 -- Insert EventCategory
INSERT INTO [dbo].[EventCategory] ([EventId], [CategoryId]) VALUES 
    (@Event1, @Cat10K),
    (@Event2, @CatHM),
    (@Event3, @Cat1835);
 
 DECLARE @EC2 UNIQUEIDENTIFIER = (SELECT [EventCategoryId] FROM [dbo].[EventCategory] WHERE [EventId] = (SELECT [EventId] FROM [dbo].[Event] WHERE [EventName] = 'Johannesburg Half Marathon') AND [CategoryId] = (SELECT [CategoryId] FROM [dbo].[Category] WHERE [CategoryName] = 'Half Marathon'));
DECLARE @EC3 UNIQUEIDENTIFIER = (SELECT [EventCategoryId] FROM [dbo].[EventCategory] WHERE [EventId] = (SELECT [EventId] FROM [dbo].[Event] WHERE [EventName] = 'Cape Town Cycle Challenge') AND [CategoryId] = (SELECT [CategoryId] FROM [dbo].[Category] WHERE [CategoryName] = '18-35'));
 

DECLARE @Part1 UNIQUEIDENTIFIER = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant1@raceday.co.za');
DECLARE @Part2 UNIQUEIDENTIFIER = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant2@raceday.co.za');
DECLARE @Part3 UNIQUEIDENTIFIER = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant3@raceday.co.za');
 
-- Insert Enrolments
INSERT INTO [dbo].[Enrolment] ([ParticipantId], [EventCategoryId], [EmergencyContact], [EnrolmentStatus]) VALUES 
    (@Part1, @EC1, N'555-0001', N'Active'),
    (@Part2, @EC1, N'555-0002', N'Active'),
    (@Part3, @EC2, N'555-0003', N'Active'),
    (@Part1, @EC2, N'555-0001', N'Active');

    DECLARE @Enrol1 UNIQUEIDENTIFIER = (SELECT TOP 1 [EnrolmentId] FROM [dbo].[Enrolment] WHERE [ParticipantId] = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant1@raceday.co.za') AND [EventCategoryId] = (SELECT [EventCategoryId] FROM [dbo].[EventCategory] WHERE [EventId] = (SELECT [EventId] FROM [dbo].[Event] WHERE [EventName] = 'Pretoria 10K Run')));
DECLARE @Enrol2 UNIQUEIDENTIFIER = (SELECT TOP 1 [EnrolmentId] FROM [dbo].[Enrolment] WHERE [ParticipantId] = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant2@raceday.co.za') AND [EventCategoryId] = (SELECT [EventCategoryId] FROM [dbo].[EventCategory] WHERE [EventId] = (SELECT [EventId] FROM [dbo].[Event] WHERE [EventName] = 'Pretoria 10K Run')));
DECLARE @Enrol3 UNIQUEIDENTIFIER = (SELECT TOP 1 [EnrolmentId] FROM [dbo].[Enrolment] WHERE [ParticipantId] = (SELECT [UserId] FROM [dbo].[User] WHERE [Email] = 'participant3@raceday.co.za') AND [EventCategoryId] = (SELECT [EventCategoryId] FROM [dbo].[EventCategory] WHERE [EventId] = (SELECT [EventId] FROM [dbo].[Event] WHERE [EventName] = 'Johannesburg Half Marathon')));
 
-- Insert Results
INSERT INTO [dbo].[Result] ([EnrolmentId], [FinishTime], [Position], [ChipNumber], [Status]) VALUES 
    (@Enrol1, CAST('00:45:32' AS TIME), 1, N'CHIP001', N'Finished'),
    (@Enrol2, CAST('00:48:15' AS TIME), 2, N'CHIP002', N'Finished'),
    (@Enrol3, CAST('01:52:44' AS TIME), 1, N'CHIP003', N'Finished');
