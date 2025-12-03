-- Script để fix NULL values trong bảng Users

-- Update NULL Username values
UPDATE Users 
SET Username = 'user_' + CAST(Id AS VARCHAR(10))
WHERE Username IS NULL;

-- Update NULL Email values  
UPDATE Users 
SET Email = 'user' + CAST(Id AS VARCHAR(10)) + '@example.com'
WHERE Email IS NULL;

-- Update NULL Role values
UPDATE Users 
SET Role = 'Customer'
WHERE Role IS NULL;

-- Update NULL FullName values (optional)
UPDATE Users 
SET FullName = 'User ' + CAST(Id AS VARCHAR(10))
WHERE FullName IS NULL;

-- Check results
SELECT Id, Username, Email, Role, FullName, Phone, Address
FROM Users 
ORDER BY Id;
