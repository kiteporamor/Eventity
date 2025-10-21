TRUNCATE TABLE "Notifications", "Participations", "Events", "Users" RESTART IDENTITY;

INSERT INTO "Users" ("Id", "Name", "Email", "Login", "Password", "Role") VALUES
('a1b2c3d4-1234-5678-9abc-123456789abc', 'Иван Организатор', 'organizer@', 'organizer', 'password123', 0),
('b2c3d4e5-2345-6789-abcd-23456789abcd', 'Петр Участник', 'participant@', 'participant', 'password123', 0),
('c3d4e5f6-3456-789a-bcde-3456789abcde', 'Мария Участник', 'maria@', 'maria', 'password123', 0),
('d4e5f6a7-4567-89ab-cdef-456789abcdef', 'Степан Админ', 'admin@', 'admin', 'password123', 1);

INSERT INTO "Events" ("Id", "Title", "Description", "DateTime", "Address", "OrganizerId") VALUES
('e5f6a7b8-5678-9abc-def0-56789abcdef0', 'День рождения', 'День рождения', '2026-12-15 10:00:00+00', 'Москва', 'a1b2c3d4-1234-5678-9abc-123456789abc'),
('f6a7b8c9-6789-abcd-ef01-6789abcdef01', 'Конференция', 'Конференция', '2026-11-20 14:00:00+00', 'Санкт-Петербург', 'a1b2c3d4-1234-5678-9abc-123456789abc');

INSERT INTO "Participations" ("Id", "UserId", "EventId", "Role", "Status") VALUES
('a7b8c9d0-789a-bcde-f012-789abcdef012', 'a1b2c3d4-1234-5678-9abc-123456789abc', 'e5f6a7b8-5678-9abc-def0-56789abcdef0', 1, 1),
('b8c9d0e1-89ab-cdef-0123-89abcdef0123', 'b2c3d4e5-2345-6789-abcd-23456789abcd', 'e5f6a7b8-5678-9abc-def0-56789abcdef0', 0, 1),
('c9d0e1f2-9abc-def0-1234-9abcdef01234', 'c3d4e5f6-3456-789a-bcde-3456789abcde', 'e5f6a7b8-5678-9abc-def0-56789abcdef0', 0, 0),
('d0e1f2a3-abcd-ef01-2345-abcdef012345', 'a1b2c3d4-1234-5678-9abc-123456789abc', 'f6a7b8c9-6789-abcd-ef01-6789abcdef01', 1, 1),
('e1f2a3b4-bcde-f012-3456-bcdef0123456', 'b2c3d4e5-2345-6789-abcd-23456789abcd', 'f6a7b8c9-6789-abcd-ef01-6789abcdef01', 0, 2);

INSERT INTO "Notifications" ("Id", "ParticipationId", "Text", "SentAt", "Type") VALUES
('f2a3b4c5-cdef-0123-4567-cdef01234567', 'b8c9d0e1-89ab-cdef-0123-89abcdef0123', 'Напоминание', '2026-12-15 08:00:00+00', 0),
('a3b4c5d6-def0-1234-5678-def012345678', 'c9d0e1f2-9abc-def0-1234-9abcdef01234', 'Приглашение', '2026-10-20 09:00:00+00', 1);