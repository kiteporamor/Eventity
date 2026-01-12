CREATE TABLE IF NOT EXISTS "Events" (
    "Id" uuid NOT NULL,
    "Title" character varying(100) NOT NULL,
    "Description" text NOT NULL,
    "DateTime" timestamp with time zone NOT NULL,
    "Address" text NOT NULL,
    "OrganizerId" uuid NOT NULL,
    CONSTRAINT "PK_Events" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Email" text NOT NULL,
    "Login" text NOT NULL,
    "Password" text NOT NULL,
    "Role" integer NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Participations" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "EventId" uuid NOT NULL,
    "Role" integer NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "PK_Participations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Participations_Events_EventId" 
        FOREIGN KEY ("EventId") 
        REFERENCES "Events" ("Id") 
        ON DELETE RESTRICT,
    CONSTRAINT "FK_Participations_Users_UserId" 
        FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") 
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS "Notifications" (
    "Id" uuid NOT NULL,
    "ParticipationId" uuid NOT NULL,
    "Text" text NOT NULL,
    "SentAt" timestamp with time zone NOT NULL,
    "Type" integer NOT NULL,
    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Notifications_Participations_ParticipationId" 
        FOREIGN KEY ("ParticipationId") 
        REFERENCES "Participations" ("Id") 
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Notifications_ParticipationId" ON "Notifications" ("ParticipationId");
CREATE INDEX IF NOT EXISTS "IX_Participations_EventId" ON "Participations" ("EventId");
CREATE INDEX IF NOT EXISTS "IX_Participations_UserId" ON "Participations" ("UserId");