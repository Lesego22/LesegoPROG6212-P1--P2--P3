
SELECT 'UserRole' AS TableName, COUNT(*) AS RecordCount FROM [dbo].[UserRole]
UNION ALL
SELECT 'User', COUNT(*) FROM [dbo].[User]
UNION ALL
SELECT 'Event', COUNT(*) FROM [dbo].[Event]
UNION ALL
SELECT 'Category', COUNT(*) FROM [dbo].[Category]
UNION ALL
SELECT 'EventCategory', COUNT(*) FROM [dbo].[EventCategory]
UNION ALL
SELECT 'Enrolment', COUNT(*) FROM [dbo].[Enrolment]
UNION ALL
SELECT 'Result', COUNT(*) FROM [dbo].[Result]
ORDER BY TableName;

SELECT 
    u.[FirstName],
    u.[LastName],
    u.[Email],
    r.[RoleName] AS Role
FROM [dbo].[User] u
JOIN [dbo].[UserRole] r ON u.[UserRoleId] = r.[UserRoleId]
ORDER BY u.[FirstName];

SELECT 
    e.[EventName],
    e.[Location],
    e.[Distance],
    e.[EventDate]
FROM [dbo].[Event] e
ORDER BY e.[EventDate];
 
 SELECT 
    [CategoryName],
    [Description],
    [MinAge],
    [MaxAge]
FROM [dbo].[Category]
ORDER BY [CategoryName];
 
 SELECT 
    u.[FirstName] + ' ' + u.[LastName] AS ParticipantName,
    e.[EventName],
    c.[CategoryName],
    en.[EnrolmentStatus]
FROM [dbo].[Enrolment] en
JOIN [dbo].[User] u ON en.[ParticipantId] = u.[UserId]
JOIN [dbo].[EventCategory] ec ON en.[EventCategoryId] = ec.[EventCategoryId]
JOIN [dbo].[Event] e ON ec.[EventId] = e.[EventId]
JOIN [dbo].[Category] c ON ec.[CategoryId] = c.[CategoryId]
ORDER BY u.[FirstName];

SELECT 
    u.[FirstName] + ' ' + u.[LastName] AS ParticipantName,
    e.[EventName],
    r.[Position],
    r.[FinishTime],
    r.[ChipNumber]
FROM [dbo].[Result] r
JOIN [dbo].[Enrolment] en ON r.[EnrolmentId] = en.[EnrolmentId]
JOIN [dbo].[User] u ON en.[ParticipantId] = u.[UserId]
JOIN [dbo].[EventCategory] ec ON en.[EventCategoryId] = ec.[EventCategoryId]
JOIN [dbo].[Event] e ON ec.[EventId] = e.[EventId]
ORDER BY e.[EventName], r.[Position];
 
 