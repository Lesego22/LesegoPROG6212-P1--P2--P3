# RaceDay Event Management System

## Project Description
RaceDay is a comprehensive event management system designed for South African road running, walking, and cycling events. The system allows organisers to create and manage events, define categories, capture results, and track participant enrollments.

## Database Design
The system uses a relational database with 7 entities:
- **User** - Stores organiser and participant information
- **UserRole** - Reference table for user roles (Organiser, Participant)
- **Event** - Race events with date, location, distance
- **Category** - Race types and age groups
- **EventCategory** - Links events to categories (many-to-many)
- **Enrolment** - Participant registrations
- **Result** - Race results and finish times

## System Roles

### Organiser
- Create and manage events
- Define race categories
- Record race results
- View all enrollments

### Participant
- Browse available events
- Enroll in events
- Track personal results
- View performance history

## API Endpoints
The system includes 17 RESTful API endpoints covering:
- Authentication (register, login)
- User profiles
- Event management
- Category management
- Enrollments
- Results and leaderboards

## Files in /docs
- `RaceDay_ERD.png` - Entity Relationship Diagram
- `API_ENDPOINT_PLAN.md` - Complete API documentation
- `CREATE DATABASE RaceDay.sql` - Database creation script
- `CREATE TABLES.sql` - Table creation script
- `INSERT SAMPLE DATA.sql` - Sample data insertion
- `VERIFY DATA.sql` - Data verification queries

## GitHub Repository
All planning documents, SQL scripts, and ERD are available in the `/docs` folder.

##
