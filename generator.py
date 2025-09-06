import uuid
import random
import io
from datetime import datetime, timedelta
import psycopg2
from faker import Faker

fake = Faker()

# Настройки подключения к БД
DB_NAME = "Eventity"
DB_USER = "postgres"
DB_PASSWORD = "postgres"
DB_HOST = "localhost"
DB_PORT = 5432

# Генерация пользователей
def generate_users(count=10):
    users = []
    for _ in range(count):
        user = {
            "Id": str(uuid.uuid4()),
            "Name": fake.name(),
            "Email": fake.email(),
            "Login": fake.user_name(),
            "Password": fake.password(),
            "Role": random.choice([0, 1])
        }
        users.append(user)
    return users

# Генерация событий
def generate_events(users, count=10):
    events = []
    now = datetime.now()
    for i in range(count):
        organizer = random.choice(users)
        event = {
            "Id": str(uuid.uuid4()),
            "Title": f"Event {i}",
            "Description": f"Description {i}",
            "DateTime": (now + timedelta(days=i + 1)).isoformat(),
            "Address": fake.address().replace("\n", ", "),
            "OrganizerId": organizer["Id"]
        }
        events.append(event)
    return events

# Генерация участий
def generate_participations(users, events, count=20):
    participations = []
    for _ in range(count):
        user = random.choice(users)
        event = random.choice(events)
        participation = {
            "Id": str(uuid.uuid4()),
            "UserId": user["Id"],
            "EventId": event["Id"],
            "Role": random.choice([0, 1, 2]),
            "Status": random.choice([0, 1, 2])
        }
        participations.append(participation)
    return participations

# Генерация уведомлений
def generate_notifications(participations, count=30):
    notifications = []
    for _ in range(count):
        p = random.choice(participations)
        notif = {
            "Id": str(uuid.uuid4()),
            "ParticipationId": p["Id"],
            "Text": fake.sentence(),
            "SentAt": datetime.now().isoformat()
        }
        notifications.append(notif)
    return notifications

# Преобразуем в CSV-формат в памяти
def to_csv_buffer(records, fields):
    buffer = io.StringIO()
    for rec in records:
        row = [str(rec[field]) for field in fields]
        buffer.write("\t".join(row) + "\n")
    buffer.seek(0)
    return buffer

# Вставка в БД через COPY
def copy_to_db(table, data, fields, conn):
    with conn.cursor() as cur:
        buffer = to_csv_buffer(data, fields)
        cur.copy_from(buffer, table, sep="\t", columns=fields)
    conn.commit()

# Главная функция
def main():
    users = generate_users()
    events = generate_events(users)
    participations = generate_participations(users, events)
    notifications = generate_notifications(participations)

    conn = psycopg2.connect(
        dbname=DB_NAME, user=DB_USER, password=DB_PASSWORD,
        host=DB_HOST, port=DB_PORT
    )

    try:
        copy_to_db("Users", users, ["Id", "Name", "Email", "Login", "Password", "Role"], conn)
        copy_to_db("Events", events, ["Id", "Title", "Description", "DateTime", "Address", "OrganizerId"], conn)
        copy_to_db("Participations", participations, ["Id", "UserId", "EventId", "Role", "Status"], conn)
        copy_to_db("Notifications", notifications, ["Id", "ParticipationId", "Text", "SentAt"], conn)
        print("✅ Данные успешно вставлены в базу данных!")
    finally:
        conn.close()

if __name__ == "__main__":
    main()
