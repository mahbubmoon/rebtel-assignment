PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Books (
    Id TEXT PRIMARY KEY NOT NULL,
    Isbn TEXT NOT NULL,
    Title TEXT NOT NULL,
    Author TEXT NOT NULL,
    PageCount INTEGER NOT NULL CHECK (PageCount > 0),
    TotalCopies INTEGER NOT NULL CHECK (TotalCopies >= 0)
);

CREATE TABLE IF NOT EXISTS Borrowers (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Loans (
    Id TEXT PRIMARY KEY NOT NULL,
    BookId TEXT NOT NULL,
    BorrowerId TEXT NOT NULL,
    BorrowedOn TEXT NOT NULL,
    ReturnedOn TEXT NULL,
    FOREIGN KEY (BookId) REFERENCES Books (Id),
    FOREIGN KEY (BorrowerId) REFERENCES Borrowers (Id),
    CHECK (ReturnedOn IS NULL OR ReturnedOn >= BorrowedOn)
);

CREATE INDEX IF NOT EXISTS IX_Loans_BookId ON Loans (BookId);
CREATE INDEX IF NOT EXISTS IX_Loans_BorrowerId ON Loans (BorrowerId);
CREATE INDEX IF NOT EXISTS IX_Loans_BorrowedOn ON Loans (BorrowedOn);
