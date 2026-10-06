using DbConsumer.Services;

string rabbitHost = "localhost";
string mongoConn = "mongodb://localhost:27017";
string dbName = "AlertsDB";

IElasticService elasticService = new ElasticService();

var mongoRepo = new MongoRepository(mongoConn, dbName);
string logLevel2 = "Info";
string logMessage2 = "Connection to MongoDb";
await elasticService.SendLogAsync(logMessage2, logLevel2);

var rabbitService = new RabbitMQConsumerService(rabbitHost, mongoRepo);
string logLevel1 = "Info";
string logMessage1 = "Connection to RabbitMQ";
await elasticService.SendLogAsync(logMessage1, logLevel1);

rabbitService.StartConsuming();