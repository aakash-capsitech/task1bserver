

using MongoDB.Driver;
using MyMongoApp.Models;

namespace MyMongoApp.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            _database = client.GetDatabase("MyMongoAppDb");
        }

        // Main app users
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public IMongoCollection<LogEntry> Logs => _database.GetCollection<LogEntry>("Logs");

        public IMongoCollection<LoginRule> LoginRules => _database.GetCollection<LoginRule>("LoginRules");

        public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("AuditLogs");

        public IMongoCollection<Business> Businesses => _database.GetCollection<Business>("Businesses");

        public IMongoCollection<Quote> Quotes =>
        _database.GetCollection<Quote>("Quotes");



        // Auth-specific users
        // public IMongoCollection<AUser> AUsers => _database.GetCollection<AUser>("auth_users");
    }
}
