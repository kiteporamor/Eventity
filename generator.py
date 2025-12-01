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
        conn = psycopg2.connect(
            dbname="EventityBench",
            user="postgres",
            password="postgres",
            host="postgres-bench",
            port=5432
        )
        
        with conn.cursor() as cur:
            cur.execute("""
                CREATE TABLE IF NOT EXISTS users (
                    Id UUID PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Login TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL,
                    Role INTEGER NOT NULL
                );
                
                CREATE TABLE IF NOT EXISTS events (
                    Id UUID PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    DateTime TIMESTAMP NOT NULL,
                    Address TEXT,
                    OrganizerId UUID
                );
                
                CREATE TABLE IF NOT EXISTS participations (
                    Id UUID PRIMARY KEY,
                    UserId UUID,
                    EventId UUID,
                    Role INTEGER NOT NULL,
                    Status INTEGER NOT NULL
                );
                
                CREATE TABLE IF NOT EXISTS notifications (
                    Id UUID PRIMARY KEY,
                    ParticipationId UUID,
                    Text TEXT,
                    SentAt TIMESTAMP NOT NULL,
                    Type INTEGER NOT NULL
                );
            """)
            conn.commit()
        
        users = generate_users(10)
        events = generate_events(users, 20)
        participations = generate_participations(users, events, 50)
        notifications = generate_notifications(participations, 100)
        
        with conn.cursor() as cur:
            for user in users:
                cur.execute("""
                    INSERT INTO users (Id, Name, Email, Login, Password, Role) 
                    VALUES (%s, %s, %s, %s, %s, %s)
                    ON CONFLICT (Id) DO UPDATE SET
                        Name = EXCLUDED.Name,
                        Email = EXCLUDED.Email,
                        Login = EXCLUDED.Login,
                        Password = EXCLUDED.Password,
                        Role = EXCLUDED.Role
                """, (user["Id"], user["Name"], user["Email"], user["Login"], 
                      user["Password"], user["Role"]))
            
            for event in events:
                cur.execute("""
                    INSERT INTO events (Id, Title, Description, DateTime, Address, OrganizerId) 
                    VALUES (%s, %s, %s, %s, %s, %s)
                    ON CONFLICT (Id) DO UPDATE SET
                        Title = EXCLUDED.Title,
                        Description = EXCLUDED.Description,
                        DateTime = EXCLUDED.DateTime,
                        Address = EXCLUDED.Address,
                        OrganizerId = EXCLUDED.OrganizerId
                """, (event["Id"], event["Title"], event["Description"], 
                      event["DateTime"], event["Address"], event["OrganizerId"]))
            
            for part in participations:
                cur.execute("""
                    INSERT INTO participations (Id, UserId, EventId, Role, Status) 
                    VALUES (%s, %s, %s, %s, %s)
                    ON CONFLICT (Id) DO UPDATE SET
                        UserId = EXCLUDED.UserId,
                        EventId = EXCLUDED.EventId,
                        Role = EXCLUDED.Role,
                        Status = EXCLUDED.Status
                """, (part["Id"], part["UserId"], part["EventId"], 
                      part["Role"], part["Status"]))
            
            for notif in notifications:
                cur.execute("""
                    INSERT INTO notifications (Id, ParticipationId, Text, SentAt, Type) 
                    VALUES (%s, %s, %s, %s, %s)
                    ON CONFLICT (Id) DO UPDATE SET
                        ParticipationId = EXCLUDED.ParticipationId,
                        Text = EXCLUDED.Text,
                        SentAt = EXCLUDED.SentAt,
                        Type = EXCLUDED.Type
                """, (notif["Id"], notif["ParticipationId"], notif["Text"], 
                      notif["SentAt"], notif["Type"]))
            
            conn.commit()
        
        print("PostgreSQL: Data generated")
        print(f"Users: {len(users)}, Events: {len(events)}, Participations: {len(participations)}, Notifications: {len(notifications)}")
        
        conn.close()
        
    except Exception as e:
        print(f"PostgreSQL error: {e}")

def generate_mongo():
    try:
        client = MongoClient("mongodb://mongo-bench:27017/", serverSelectionTimeoutMS=5000)
        db = client["EventityBench"]
        
        client.server_info()
        print("Successfully connected to MongoDB")
        
        db.users.delete_many({})
        db.events.delete_many({})
        db.participations.delete_many({})
        db.notifications.delete_many({})
        
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
        
        if users_mongo:
            db.users.insert_many(users_mongo)
            print(f"Inserted {len(users_mongo)} users with fields: Login, Password, Role")
        
        if events_mongo:
            db.events.insert_many(events_mongo)
        
        if participations_mongo:
            db.participations.insert_many(participations_mongo)
        
        if notifications_mongo:
            db.notifications.insert_many(notifications_mongo)
        
        print("MongoDB: Data generated")
        
        sample = db.users.find_one()
        print(f"\nSample user in MongoDB: {sample}")
        
        print(f"\nMongoDB counts:")
        print(f"Users: {db.users.count_documents({})}")
        print(f"Events: {db.events.count_documents({})}")
        print(f"Participations: {db.participations.count_documents({})}")
        print(f"Notifications: {db.notifications.count_documents({})}")
        
        client.close()
        
    except Exception as e:
        print(f"MongoDB error: {e}")

def main():
    generate_postgres()
    generate_mongo()
    print("Test users: admin/adminpass, testuser/testpass")

if __name__ == "__main__":
    main()