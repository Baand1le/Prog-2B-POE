--Create Database
CREATE DATABASE RaceDay;

USE RaceDay;

--Create user role table
--Stores the different roles available in the system
CREATE TABLE userRole (
	roleID INT PRIMARY KEY,
	roleType VARCHAR(30)
);

SELECT * FROM userRole;

--Create the users table to store the details of all users
CREATE TABLE Users (
	userID VARCHAR(20) PRIMARY KEY,
	roleID INT NOT NULL REFERENCES userRole(roleID),
	firstName VARCHAR(50),
	Surname VARCHAR(50),
	email VARCHAR(100)
);

SELECT * FROM Users;

--Create the event table
--Stores information about events, the organiserID identifies the user who organised the event
CREATE TABLE Event (
	eventID VARCHAR(20) PRIMARY KEY,
	organizerID VARCHAR(20) NOT NULL REFERENCES Users(userID),
	Name VARCHAR(100),
	Description VARCHAR(255),
	[Date] DATE,
	Location VARCHAR(255),
	Distance VARCHAR(20),
	eventType VARCHAR(30)
);

SELECT * FROM Event;

--this table stores the categories available for each event
CREATE TABLE Category (
	categoryID VARCHAR(20) PRIMARY KEY,
	eventID VARCHAR(20) NOT NULL REFERENCES Event(eventID),
	categoryName VARCHAR(50),
	ageGroup VARCHAR(30)
);

SELECT * FROM Category;

--This table records participants who enroll in events and links a participant to an event and a category
CREATE TABLE Enrollment(
    enrollmentID VARCHAR(20) PRIMARY KEY,
    participantID VARCHAR(20) NOT NULL,
    eventID VARCHAR(20) NOT NULL,
    categoryID VARCHAR(20) NOT NULL,
    enrollmentDate DATE,

	--Links the participant to a user
    FOREIGN KEY (participantID) REFERENCES Users(userID),

	--Links the enrollment to an event
    FOREIGN KEY (eventID) REFERENCES Event(eventID),

	--Links the enrollment to a category
    FOREIGN KEY (categoryID) REFERENCES Category(categoryID)
);

SELECT * FROM Enrollment;

CREATE TABLE Result (
    resultID VARCHAR(20) PRIMARY KEY,
    enrollmentID VARCHAR(20) NOT NULL,
    finishTime TIME,
    finishPosition INT,

    FOREIGN KEY (enrollmentID) REFERENCES Enrollment(enrollmentID)
);

SELECT * FROM Result;

INSERT INTO userRole (roleID, roleType)
VALUES
	(1, 'Organiser'),
	(2, 'Participant');

SELECT * FROM userRole;


INSERT INTO Users (userID, roleID, firstName, surname, email)
VALUES
	('U001', 1, 'Sarah', 'Mokoena', 'sarah.mokoena@email.com'),
	('U002', 1, 'David', 'Naidoo', 'david.naidoo@email.com'),
	('U003', 2, 'Thabo', 'Dlamini', 'thabo.dlamini@email.com'),
	('U004', 2, 'Lerato', 'Molefe', 'lerato.molefe@email.com');

SELECT * FROM Users;

INSERT INTO Event
(eventID, organizerID, Name, Description, [Date], Location, Distance, eventType)
VALUES
	('E001', 'U001', 'Johannesburg City Run',
	 'Annual city running event',
	 '2026-10-10', 'Johannesburg', '10 km', 'Run'),

	('E002', 'U002', 'Soweto Community Walk',
	 'Community fitness and wellness walk',
	 '2026-10-24', 'Soweto', '5 km', 'Walk'),

	('E003', 'U001', 'Pretoria Cycle Challenge',
	 'Cycling challenge for recreational cyclists',
	 '2026-11-07', 'Pretoria', '20 km', 'Cycle');

SELECT * FROM Event;

INSERT INTO Category
(categoryID, eventID, categoryName, ageGroup)
VALUES
	('C001', 'E001', 'Junior Run', 'Under 18'),
	('C002', 'E001', 'Senior Run', '18 and Over'),

	('C003', 'E002', 'Junior Walk', 'Under 18'),
	('C004', 'E002', 'Senior Walk', '18 and Over'),

	('C005', 'E003', 'Junior Cycle', 'Under 18'),
	('C006', 'E003', 'Senior Cycle', '18 and Over');

SELECT * FROM Category;

INSERT INTO Enrollment
(enrollmentID, participantID, eventID, categoryID, enrollmentDate)
VALUES
	('EN001', 'U003', 'E001', 'C002', '2026-09-01'),
	('EN002', 'U004', 'E001', 'C002', '2026-09-02'),

	('EN003', 'U003', 'E002', 'C004', '2026-09-03'),
	('EN004', 'U004', 'E002', 'C004', '2026-09-03'),

	('EN005', 'U003', 'E003', 'C006', '2026-09-04');

SELECT * FROM Enrollment;
