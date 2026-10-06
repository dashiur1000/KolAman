# KolAman
## הוראות הרצה
וודא שהנך נמצא בתיקיית הפרוייקט
```
cd NotificationGate
```
```
docker compose up --build -d
```
```
docker run -d --name elasticsearch -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" -e "xpack.security.enabled=false" elasticsearch:8.11.3
```
```
docker run -d --name kibana -p 5601:5601 --link elasticsearch:elasticsearch -e "ELASTICSEARCH_HOSTS=http://elasticsearch:9200" kibana:8.11.3
```
```
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
```
docker run -d --name redis-db -p 6379:6379 redis
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

