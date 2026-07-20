PRAGMA foreign_keys = ON;

INSERT OR IGNORE INTO Books (Id, Isbn, Title, Author, PageCount, TotalCopies) VALUES
(1, '978-0201616224', 'The Pragmatic Programmer', 'Andrew Hunt and David Thomas', 352, 3),
(2, '978-0132350884', 'Clean Code', 'Robert C. Martin', 464, 4),
(3, '978-1449373320', 'Designing Data-Intensive Applications', 'Martin Kleppmann', 616, 2),
(4, '978-0134757599', 'Refactoring', 'Martin Fowler', 448, 2),
(5, '978-0321125217', 'Domain-Driven Design', 'Eric Evans', 560, 2),
(6, '978-0547928227', 'The Hobbit', 'J.R.R. Tolkien', 310, 5);

INSERT OR IGNORE INTO Borrowers (Id, Name, Email) VALUES
(11, 'Ada Lovelace', 'ada@example.org'),
(12, 'Grace Hopper', 'grace@example.org'),
(13, 'Alan Turing', 'alan@example.org'),
(14, 'Katherine Johnson', 'katherine@example.org'),
(15, 'Margaret Hamilton', 'margaret@example.org');

INSERT OR IGNORE INTO Loans (Id, BookId, BorrowerId, BorrowedOn, ReturnedOn) VALUES
(101, 1, 11, '2026-01-01', '2026-01-08'),
(102, 2, 11, '2026-02-01', '2026-02-13'),
(103, 3, 11, '2026-03-01', '2026-03-20'),
(104, 1, 12, '2026-01-05', '2026-01-15'),
(105, 4, 12, '2026-02-05', '2026-02-12'),
(106, 6, 12, '2026-04-01', '2026-04-08'),
(107, 2, 12, '2026-05-01', '2026-05-11'),
(108, 3, 13, '2026-01-10', '2026-02-01'),
(109, 1, 13, '2026-03-05', '2026-03-12'),
(110, 2, 14, '2026-01-20', '2026-01-28'),
(111, 4, 14, '2026-03-02', '2026-03-16'),
(112, 5, 14, '2026-04-03', '2026-04-20'),
(113, 1, 15, '2026-04-05', '2026-04-15'),
(114, 5, 15, '2026-05-01', NULL),
(115, 6, 15, '2026-06-01', '2026-06-11');
