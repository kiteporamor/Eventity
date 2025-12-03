import uuid
import random
import psycopg2
from pymongo import MongoClient
from datetime import datetime, timedelta

def generate_users(count=10):
    users = []
    for i in range(count):
        role = 1 if i == 0 else 0
        user_id = ""
        login = ""
        password = ""
        
        if i == 0:
            user_id = "11111111-1111-1111-1111-111111111111"
            login = "admin"
            password = "adminpass"
        elif i == 1:
            user_id = "22222222-2222-2222-2222-222222222222"
            login = "testuser"
            password = "testpass"
        else:
            user_id = str(uuid.uuid4())
            login = f"user{i}"
            password = "password123"
        
        user = {
            "Id": user_id,
            "Name": f"User {i+1}",
            "Email": f"user{i+1}@eventity.com",
            "Login": login,
            "Password": password,
            "Role": role
        }
        users.append(user)
    return users

def generate_events(users, count=20):
    events = []
    now = datetime.now()
    for i in range(count):
        organizer = users[0] if i < 5 else random.choice(users)
        event = {
            "Id": str(uuid.uuid4()),
            "Title": f"Event {i+1}",
            "Description": f"Description for event {i+1}",
            "DateTime": (now + timedelta(days=i + 1)),
            "Address": f"Address {i+1}",
            "OrganizerId": organizer["Id"]
        }
        events.append(event)
    return events

def generate_participations(users, events, count=50):
    participations = []
    for i in range(count):
        user = users[i % len(users)]
        event = events[i % len(events)]
        participation = {
            "Id": str(uuid.uuid4()),
            "UserId": user["Id"],
            "EventId": event["Id"],
            "Role": random.randint(0, 2),
            "Status": random.randint(0, 2)
        }
        participations.append(participation)
    return participations

def generate_notifications(participations, count=100):
    notifications = []
    for i in range(count):
        p = participations[i % len(participations)]
        notif = {
            "Id": str(uuid.uuid4()),
            "ParticipationId": p["Id"],
            "Text": f"Notification {i+1}",
            "SentAt": datetime.now(),
            "Type": random.randint(0, 1)
        }
        notifications.append(notif)
    return notifications

def generate_postgres():
    try:
        print("=== DEBUG: Connecting to PostgreSQL ===")
        conn = psycopg2.connect(
            dbname="EventityBench",
            user="postgres",
            password="postgres",
            host="postgres-bench",
            port=5432
        )
        print("=== DEBUG: Connected successfully ===")
        
        # УДАЛЯЕМ ЭТОТ БЛОК КОДА - не удаляем таблицы!
        # with conn.cursor() as cur:
        #     cur.execute("""
        #         DROP TABLE IF EXISTS users, events, participations, notifications;
        #     """)
        #     conn.commit()
        
        with conn.cursor() as cur:
            print("=== DEBUG: Creating tables ===")
            # Создаём таблицы с правильными типами данных и ограничениями
            cur.execute("""
                CREATE TABLE IF NOT EXISTS "Users" (
                    "Id" uuid NOT NULL,
                    "Name" text NOT NULL,
                    "Email" text NOT NULL,
                    "Login" text NOT NULL,
                    "Password" text NOT NULL,
                    "Role" integer NOT NULL,
                    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
                );
                
                CREATE TABLE IF NOT EXISTS "Events" (
                    "Id" uuid NOT NULL,
                    "Title" character varying(100) NOT NULL,
                    "Description" text NOT NULL,
                    "DateTime" timestamp with time zone NOT NULL,
                    "Address" text NOT NULL,
                    "OrganizerId" uuid NOT NULL,
                    CONSTRAINT "PK_Events" PRIMARY KEY ("Id")
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
            """)
            
            # Создаём индексы
            cur.execute("""
                CREATE INDEX IF NOT EXISTS "IX_Notifications_ParticipationId" 
                ON "Notifications" ("ParticipationId");
                
                CREATE INDEX IF NOT EXISTS "IX_Participations_EventId" 
                ON "Participations" ("EventId");
                
                CREATE INDEX IF NOT EXISTS "IX_Participations_UserId" 
                ON "Participations" ("UserId");
            """)
            
            conn.commit()
            print("=== DEBUG: Tables created with proper constraints ===")
        
        # Очищаем существующие данные перед вставкой новых
        with conn.cursor() as cur:
            cur.execute('TRUNCATE TABLE "Notifications" CASCADE;')
            cur.execute('TRUNCATE TABLE "Participations" CASCADE;')
            cur.execute('TRUNCATE TABLE "Events" CASCADE;')
            cur.execute('TRUNCATE TABLE "Users" CASCADE;')
            conn.commit()
            print("=== DEBUG: Old data truncated ===")
        
        # Генерируем данные
        users = generate_users(10)
        events = generate_events(users, 20)
        participations = generate_participations(users, events, 50)
        notifications = generate_notifications(participations, 100)
        
        with conn.cursor() as cur:
            print(f"=== DEBUG: Inserting {len(users)} users ===")
            for user in users:
                cur.execute("""
                    INSERT INTO "Users" ("Id", "Name", "Email", "Login", "Password", "Role") 
                    VALUES (%s, %s, %s, %s, %s, %s)
                    ON CONFLICT ("Id") DO UPDATE SET
                        "Name" = EXCLUDED."Name",
                        "Email" = EXCLUDED."Email",
                        "Login" = EXCLUDED."Login",
                        "Password" = EXCLUDED."Password",
                        "Role" = EXCLUDED."Role"
                """, (user["Id"], user["Name"], user["Email"], user["Login"], 
                      user["Password"], user["Role"]))
            
            print(f"=== DEBUG: Inserting {len(events)} events ===")
            for event in events:
                cur.execute("""
                    INSERT INTO "Events" ("Id", "Title", "Description", "DateTime", "Address", "OrganizerId") 
                    VALUES (%s, %s, %s, %s, %s, %s)
                    ON CONFLICT ("Id") DO UPDATE SET
                        "Title" = EXCLUDED."Title",
                        "Description" = EXCLUDED."Description",
                        "DateTime" = EXCLUDED."DateTime",
                        "Address" = EXCLUDED."Address",
                        "OrganizerId" = EXCLUDED."OrganizerId"
                """, (event["Id"], event["Title"], event["Description"], 
                      event["DateTime"], event["Address"], event["OrganizerId"]))
            
            print(f"=== DEBUG: Inserting {len(participations)} participations ===")
            for part in participations:
                cur.execute("""
                    INSERT INTO "Participations" ("Id", "UserId", "EventId", "Role", "Status") 
                    VALUES (%s, %s, %s, %s, %s)
                    ON CONFLICT ("Id") DO UPDATE SET
                        "UserId" = EXCLUDED."UserId",
                        "EventId" = EXCLUDED."EventId",
                        "Role" = EXCLUDED."Role",
                        "Status" = EXCLUDED."Status"
                """, (part["Id"], part["UserId"], part["EventId"], 
                      part["Role"], part["Status"]))
            
            print(f"=== DEBUG: Inserting {len(notifications)} notifications ===")
            for notif in notifications:
                cur.execute("""
                    INSERT INTO "Notifications" ("Id", "ParticipationId", "Text", "SentAt", "Type") 
                    VALUES (%s, %s, %s, %s, %s)
                    ON CONFLICT ("Id") DO UPDATE SET
                        "ParticipationId" = EXCLUDED."ParticipationId",
                        "Text" = EXCLUDED."Text",
                        "SentAt" = EXCLUDED."SentAt",
                        "Type" = EXCLUDED."Type"
                """, (notif["Id"], notif["ParticipationId"], notif["Text"], 
                      notif["SentAt"], notif["Type"]))
            
            conn.commit()
        
        # Проверим, что данные вставлены
        with conn.cursor() as cur:
            cur.execute('SELECT COUNT(*) FROM "Users";')
            count = cur.fetchone()[0]
            print(f"=== DEBUG: Users in database: {count} ===")
            
            cur.execute('SELECT "Login", "Password", "Role" FROM "Users" WHERE "Login" IN (\'admin\', \'testuser\');')
            users_check = cur.fetchall()
            print(f"=== DEBUG: Test users: {users_check} ===")
            
            # Проверим структуру таблиц
            cur.execute("""
                SELECT table_name, column_name, data_type, is_nullable 
                FROM information_schema.columns 
                WHERE table_schema = 'public' 
                ORDER BY table_name, ordinal_position;
            """)
            columns = cur.fetchall()
            print(f"=== DEBUG: Table columns: {len(columns)} total ===")
        
        print("PostgreSQL: Data generated successfully")
        print(f"Users: {len(users)}, Events: {len(events)}, Participations: {len(participations)}, Notifications: {len(notifications)}")
        
        conn.close()
        
    except Exception as e:
        print(f"PostgreSQL error: {e}")
        import traceback
        traceback.print_exc()

def generate_mongo():
    try:
        client = MongoClient("mongodb://mongo-bench:27017/", serverSelectionTimeoutMS=5000)
        db = client["EventityBench"]
        
        client.server_info()
        print("Successfully connected to MongoDB")
        
        # Очищаем коллекции
        db.users.delete_many({})
        db.events.delete_many({})
        db.participations.delete_many({})
        db.notifications.delete_many({})
        
        # Генерируем данные
        users = generate_users(10)
        events = generate_events(users, 20)
        participations = generate_participations(users, events, 50)
        notifications = generate_notifications(participations, 100)
        
        users_mongo = []
        for user in users:
            user_mongo = {
                "_id": user["Id"],
                "Login": user["Login"], 
                "Password": user["Password"], 
                "Role": user["Role"],     
                "Name": user["Name"],     
                "Email": user["Email"]    
            }
            users_mongo.append(user_mongo)
        
        events_mongo = []
        for event in events:
            event_mongo = {
                "_id": event["Id"],
                "Title": event["Title"],
                "Description": event["Description"],
                "DateTime": event["DateTime"],
                "Address": event["Address"],
                "OrganizerId": event["OrganizerId"]
            }
            events_mongo.append(event_mongo)
        
        participations_mongo = []
        for part in participations:
            part_mongo = {
                "_id": part["Id"],
                "UserId": part["UserId"],
                "EventId": part["EventId"],
                "Role": part["Role"],
                "Status": part["Status"]
            }
            participations_mongo.append(part_mongo)
        
        notifications_mongo = []
        for notif in notifications:
            notif_mongo = {
                "_id": notif["Id"],
                "ParticipationId": notif["ParticipationId"],
                "Text": notif["Text"],
                "SentAt": notif["SentAt"],
                "Type": notif["Type"]
            }
            notifications_mongo.append(notif_mongo)
        
        # Вставляем данные
        if users_mongo:
            db.users.insert_many(users_mongo)
            print(f"Inserted {len(users_mongo)} users")
        
        if events_mongo:
            db.events.insert_many(events_mongo)
        
        if participations_mongo:
            db.participations.insert_many(participations_mongo)
        
        if notifications_mongo:
            db.notifications.insert_many(notifications_mongo)
        
        print("MongoDB: Data generated")
        
        # Проверяем данные
        sample = db.users.find_one({"Login": "testuser"})
        print(f"\nTest user in MongoDB: Login={sample.get('Login') if sample else 'Not found'}, Role={sample.get('Role') if sample else 'N/A'}")
        
        print(f"\nMongoDB counts:")
        print(f"Users: {db.users.count_documents({})}")
        print(f"Events: {db.events.count_documents({})}")
        print(f"Participations: {db.participations.count_documents({})}")
        print(f"Notifications: {db.notifications.count_documents({})}")
        
        # Создаём индексы для MongoDB
        db.users.create_index("Login", unique=True)
        db.participations.create_index("UserId")
        db.participations.create_index("EventId")
        db.notifications.create_index("ParticipationId")
        print("MongoDB indexes created")
        
        client.close()
        
    except Exception as e:
        print(f"MongoDB error: {e}")

def main():
    print("=== Starting data generation ===")
    generate_postgres()
    generate_mongo()
    print("\nTest users: admin/adminpass (Role=1), testuser/testpass (Role=0)")
    print("=== Data generation complete ===")

if __name__ == "__main__":
    main()