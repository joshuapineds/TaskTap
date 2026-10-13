CREATE DATABASE IF NOT EXISTS tasktap_db;
USE tasktap_db;

-- Users Table (with hashed passwords)
CREATE TABLE IF NOT EXISTS Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Phone VARCHAR(30) DEFAULT NULL,
    Barangay VARCHAR(100) DEFAULT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) DEFAULT 'User', -- 'Admin', 'User'
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tasks Table
CREATE TABLE IF NOT EXISTS Tasks (
    TaskId INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(100) NOT NULL,
    Description TEXT,
    CreatorId INT,
    PerformerId INT DEFAULT NULL,
    Status VARCHAR(20) DEFAULT 'Open', -- 'Open', 'In Progress', 'Completed', 'Archived'
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CreatorId) REFERENCES Users(UserId),
    FOREIGN KEY (PerformerId) REFERENCES Users(UserId)
);

-- Requests / Performer Applications
CREATE TABLE IF NOT EXISTS TaskRequests (
    RequestId INT AUTO_INCREMENT PRIMARY KEY,
    TaskId INT,
    ApplicantId INT,
    Status VARCHAR(20) DEFAULT 'Pending', -- 'Pending', 'Accepted', 'Rejected'
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (ApplicantId) REFERENCES Users(UserId)
);

-- Messages Table
CREATE TABLE IF NOT EXISTS Messages (
    MessageId INT AUTO_INCREMENT PRIMARY KEY,
    TaskId INT,
    SenderId INT,
    ReceiverId INT,
    MessageText TEXT,
    SentAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (SenderId) REFERENCES Users(UserId),
    FOREIGN KEY (ReceiverId) REFERENCES Users(UserId)
);