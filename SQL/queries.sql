-- ============================================
-- E-Learning Platform - SQL Scripts
-- ============================================

-- ── TABLE CREATION ──
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(200) NOT NULL UNIQUE,
    PasswordHash VARCHAR(500) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Courses (
    CourseId INT IDENTITY(1,1) PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    CreatedBy INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Lessons (
    LessonId INT IDENTITY(1,1) PRIMARY KEY,
    CourseId INT NOT NULL FOREIGN KEY REFERENCES Courses(CourseId) ON DELETE CASCADE,
    Title VARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    OrderIndex INT NOT NULL DEFAULT 0
);

CREATE TABLE Quizzes (
    QuizId INT IDENTITY(1,1) PRIMARY KEY,
    CourseId INT NOT NULL FOREIGN KEY REFERENCES Courses(CourseId) ON DELETE CASCADE,
    Title VARCHAR(200) NOT NULL
);

CREATE TABLE Questions (
    QuestionId INT IDENTITY(1,1) PRIMARY KEY,
    QuizId INT NOT NULL FOREIGN KEY REFERENCES Quizzes(QuizId) ON DELETE CASCADE,
    QuestionText NVARCHAR(MAX) NOT NULL,
    OptionA NVARCHAR(500) NOT NULL,
    OptionB NVARCHAR(500) NOT NULL,
    OptionC NVARCHAR(500) NOT NULL,
    OptionD NVARCHAR(500) NOT NULL,
    CorrectAnswer VARCHAR(1) NOT NULL
);

CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId) ON DELETE CASCADE,
    QuizId INT NOT NULL FOREIGN KEY REFERENCES Quizzes(QuizId) ON DELETE CASCADE,
    Score INT NOT NULL,
    AttemptDate DATETIME NOT NULL DEFAULT GETUTCDATE()
);

-- ── DML: INSERT SEED DATA ──
INSERT INTO Users (FullName, Email, PasswordHash) VALUES
('Alice Johnson', 'alice@example.com', 'hashed_pw_1'),
('Bob Smith', 'bob@example.com', 'hashed_pw_2'),
('Charlie Brown', 'charlie@example.com', 'hashed_pw_3');

INSERT INTO Courses (Title, Description, CreatedBy) VALUES
('C# Fundamentals', 'Learn C# from scratch', 1),
('ASP.NET Core', 'Build web APIs with .NET', 1),
('SQL Mastery', 'Master SQL Server', 2);

INSERT INTO Lessons (CourseId, Title, Content, OrderIndex) VALUES
(1, 'Variables & Types', 'C# is a strongly typed language...', 1),
(1, 'Control Flow', 'If statements, loops...', 2),
(1, 'OOP Basics', 'Classes, objects, inheritance...', 3),
(2, 'Intro to ASP.NET', 'ASP.NET Core is a framework...', 1),
(2, 'REST APIs', 'Building RESTful services...', 2);

INSERT INTO Quizzes (CourseId, Title) VALUES
(1, 'C# Basics Quiz'),
(1, 'C# OOP Quiz'),
(2, 'ASP.NET Quiz');

INSERT INTO Questions (QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer) VALUES
(1, 'What is C#?', 'A language', 'A database', 'An OS', 'A browser', 'A'),
(1, 'Which keyword declares a class?', 'class', 'struct', 'def', 'func', 'A'),
(1, 'What is int?', 'Integer type', 'String type', 'Boolean', 'Float', 'A'),
(2, 'What is polymorphism?', 'Many forms', 'One form', 'No form', 'Two forms', 'A'),
(3, 'What is middleware?', 'Request pipeline component', 'A database', 'A CSS framework', 'A JS library', 'A');

INSERT INTO Results (UserId, QuizId, Score, AttemptDate) VALUES
(1, 1, 3, '2025-01-15'),
(1, 2, 1, '2025-01-16'),
(2, 1, 2, '2025-01-15'),
(2, 3, 1, '2025-01-17'),
(3, 1, 1, '2025-01-18'),
(3, 1, 3, '2025-01-19');

-- ══════════════════════════════════════════
-- BASIC QUERIES: SELECT, WHERE, ORDER BY
-- ══════════════════════════════════════════

-- All courses ordered by creation date
SELECT * FROM Courses ORDER BY CreatedAt DESC;

-- Users registered after a date
SELECT FullName, Email, CreatedAt FROM Users WHERE CreatedAt >= '2025-01-01' ORDER BY FullName;

-- Lessons for a specific course ordered by index
SELECT Title, Content, OrderIndex FROM Lessons WHERE CourseId = 1 ORDER BY OrderIndex;

-- ══════════════════════════════════════════
-- JOINS: INNER JOIN, LEFT JOIN
-- ══════════════════════════════════════════

-- Courses with creator name (INNER JOIN)
SELECT c.CourseId, c.Title, u.FullName AS Creator
FROM Courses c
INNER JOIN Users u ON c.CreatedBy = u.UserId;

-- All users with their results (LEFT JOIN — includes users with no results)
SELECT u.FullName, r.QuizId, r.Score, r.AttemptDate
FROM Users u
LEFT JOIN Results r ON u.UserId = r.UserId
ORDER BY u.FullName;

-- Quiz details with course name and question count
SELECT q.QuizId, q.Title AS QuizTitle, c.Title AS CourseName, COUNT(qn.QuestionId) AS QuestionCount
FROM Quizzes q
INNER JOIN Courses c ON q.CourseId = c.CourseId
LEFT JOIN Questions qn ON q.QuizId = qn.QuizId
GROUP BY q.QuizId, q.Title, c.Title;

-- ══════════════════════════════════════════
-- AGGREGATION: GROUP BY, COUNT, AVG
-- ══════════════════════════════════════════

-- Average score per quiz
SELECT q.Title, AVG(CAST(r.Score AS FLOAT)) AS AvgScore, COUNT(r.ResultId) AS Attempts
FROM Quizzes q
INNER JOIN Results r ON q.QuizId = r.QuizId
GROUP BY q.QuizId, q.Title;

-- Number of courses per user
SELECT u.FullName, COUNT(c.CourseId) AS CourseCount
FROM Users u
LEFT JOIN Courses c ON u.UserId = c.CreatedBy
GROUP BY u.UserId, u.FullName;

-- Total lessons per course
SELECT c.Title, COUNT(l.LessonId) AS LessonCount
FROM Courses c
LEFT JOIN Lessons l ON c.CourseId = l.CourseId
GROUP BY c.CourseId, c.Title;

-- ══════════════════════════════════════════
-- SUBQUERIES: Users scoring above average
-- ══════════════════════════════════════════

-- Users whose best score on any quiz is above the overall average score
SELECT DISTINCT u.FullName, u.Email
FROM Users u
INNER JOIN Results r ON u.UserId = r.UserId
WHERE r.Score > (SELECT AVG(CAST(Score AS FLOAT)) FROM Results);

-- Courses that have more lessons than the average lesson count per course
SELECT Title FROM Courses
WHERE CourseId IN (
    SELECT CourseId FROM Lessons
    GROUP BY CourseId
    HAVING COUNT(*) > (SELECT AVG(cnt) FROM (SELECT COUNT(*) AS cnt FROM Lessons GROUP BY CourseId) AS sub)
);

-- ══════════════════════════════════════════
-- SET OPERATORS: UNION
-- ══════════════════════════════════════════

-- All users who are either course creators OR have quiz results
SELECT u.UserId, u.FullName, 'Course Creator' AS Role
FROM Users u INNER JOIN Courses c ON u.UserId = c.CreatedBy
UNION
SELECT u.UserId, u.FullName, 'Quiz Taker' AS Role
FROM Users u INNER JOIN Results r ON u.UserId = r.UserId;

-- ══════════════════════════════════════════
-- DML: UPDATE, DELETE
-- ══════════════════════════════════════════

-- Update a course title
UPDATE Courses SET Title = 'C# Fundamentals (Updated)' WHERE CourseId = 1;

-- Delete a result
DELETE FROM Results WHERE ResultId = 1;
