# KolAman
## הוראות הרצה
וודא שהנך נמצא בתיקיית הפרוייקט
```
cd NotificationGate
```
העלאת קפקא
```
docker compose up --build -d
```
העלאת אלסטיק
```
docker run -d --name elasticsearch -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" -e "xpack.security.enabled=false" elasticsearch:8.11.3
```
העלאת קיבאנה
```
docker run -d --name kibana -p 5601:5601 --link elasticsearch:elasticsearch -e "ELASTICSEARCH_HOSTS=http://elasticsearch:9200" kibana:8.11.3
```
העלאת ראביט
```
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
העלאת רדיס
```
docker run -d --name redis-db -p 6379:6379 redis
```
העלאת מונגו
```
cd ..
```
```
cd DbConsumer
```
```
docker compose up --build -d
```
```
docker ps
```
וודא שכל הקונטנטיינרים עלו עלה

כנס לתיקיית alert-simulator

הפעל את הסקריפט בלחיצה כפולה על הקובץ run.bat

תוודא שההתראות נשלחות בטרמינל נפרד
```
dotnet run
```
```
cd ..
```
```
cd AlertsAPI
```
```
dotnet run
```
```
http://localhost:5186/swagger/index.html
```
ההתראות נשלחות לקפקא!
לצפייה בלוגים באלסטיק דרך קיבאנה
```
http://localhost:5601/app/dev_tools#/console
```
לצפייה בראביט:
```
http://localhost:15672/#/
```

## פירוט המערכת
### NotificationGate
סורק את תיקיית הסימולטור בו נוצרים הקבצים החדשים, מזהה בעזרת ספריית FileSystemWatcher כאשר נוצר קובץ חדש, לאחר שקובץ הביטחון "redi" מוכן - קובץ הג'ייסון נקר ונשלח לקפקא, לוגים נשלחים לאלסטיקסרץ' במקביל.
### Classification
קורא מקפקא, עושה ולידציה בסיסית על הנתונים, מגלה את הפיקוד האחראי על הודעה ומכניס לראביט כאשר כל הודעה משוייכת לתור בשם הפיקוד שלה לצורך שיוך בשירות הבא לטבלת הפיקוד המסויים בבסיס הנתונים.
עך אף הנכתב להעביר לוגים לאלסטיק בהם נכתב איזו הודעה הועברה לאיזה פיקוד 
- לא הגעתי לזה ולתוצאותיו - והעדפתי להתמקד בבסיס הנתונים ובAPI שלו.
התמקדות היא בולדיציה המזהה תקינות של הפרמטרים בג'ייסון וכן בנקודות האורך והרוחב שיהיו תקינות.
### DbConsumer
מתחבר לראביט ולמונגו, יוצר בסיס נתונים בשם AlertsDB ומשייך כל הודעה לטבלה שלה לפי התור ממנה הגיעה. לפני ההכנסה עושה שוב ולידציה בסיסית, לוגים נשלחים לאלסטיק במקביל.
### AlertsAPI
שירות API המספק נתונים מבסיס הנתונים בשאילתות מסויימות.
```
http://localhost:5186/Alerts/ByCommand?command=NorthAlerts
```
הנותנת לצפות בהודעות שנשלחו לכל פיקוד.


## מבנה התיקיות
```
KolAman/
├── NotificationGate/
|   ├── alert-simulator/
|   ├── Models/
|   |   ├── Alert.cs
|   |   └── AppLog.cs
|   ├── Services/
|   |   ├── DataLoader.cs
|   |   ├── ElasticService.cs
|   |   ├── FileSystemWatch.cs
|   |   ├── IElasticService.cs
|   |   └── SendsToKafka.cs
|   ├── docker-compose.yaml
|   └── Program.cs
├── Classification/
|   ├── main.py
|   ├── requirements.txt
|   └── regions.geojson
├── DbConsumer/
|   ├── Models/
|   |   ├── AlertMessage.cs
|   |   └── AppLog.cs
|   ├── Services/
|   |   ├── IElasticService.cs
|   |   ├── ElasticService.cs
|   |   ├── MongoRepository.cs
|   |   └── RabbitMQConsumerService.cs
|   ├── docker-compose.yaml
|   └── Program.cs
└── AlertsAPI/
|   ├── Models/
|   |   └── AlertModel.cs
|   ├── Exceptions/
|   |   └── GlobalExceptionMiddleware.cs
|   ├── Repositories/
|   |   ├── IAlertRepository.cs
|   |   └── AlertRepository.cs
|   ├── Controllers/
|   |   └── AlertsController.cs
|   └── Program.cs
├── .gitignore
└── 
```
.