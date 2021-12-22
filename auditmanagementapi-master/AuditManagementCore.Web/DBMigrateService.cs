using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AuditManagementCore.Models.AuditConstants;

namespace AuditManagementCore.Web
{
    public class DBMigrateService : IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundEmailSending> logger;
        public int number = 0;
        private Timer timer;
        IMongoDbSettings _dbsetting;
        IEmailUtility _IEmailUtility;
        IWebHostEnvironment _IWebHostEnvironment;
        //IGlobalConfiguration _globalConfig;
        public string serviceName = "DBMigration Service";

        public DBMigrateService(ILogger<BackgroundEmailSending> logger, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, IEmailUtility emailUtility)
        {
            this.logger = logger;
            _dbsetting = mongoDbSettings;
            _IEmailUtility = emailUtility;
            _IWebHostEnvironment = webHostEnvironment;
            //_globalConfig = config; , IGlobalConfiguration config
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(TaskRoutine, cancellationToken);
            return Task.CompletedTask;
        }
        public Task TaskRoutine()
        {
            while (true)
            {
                var _repoServiceConfiguration = new MongoGenericRepository<ServiceConfiguration>(_dbsetting);
                var lstService = _repoServiceConfiguration.GetAll();
                if (lstService != null)
                {
                    if (lstService.Count() > 0)
                    {
                        foreach (var item in lstService)
                        {
                            if (item.IsMigrate)
                            {
                                bool isConnection = false;
                                isConnection = WebRequestTest();
                                if (isConnection)
                                {
                                    MigrateDB(item);
                                    string setHour = item.SetHours;
                                    //Wait  8 am  daily service till next execution
                                    //if (DateTime.Now.Hour == setHour)
                                    //{
                                    DateTime nextStop = DateTime.Now.AddMinutes(30);
                                    var timeToWait = nextStop - DateTime.Now;
                                    var millisToWait = timeToWait.TotalMilliseconds;
                                    Thread.Sleep((int)millisToWait);
                                    //}
                                }
                            }
                        }
                    }
                }
            }
        }
        public static bool WebRequestTest()
        {
            string url = "http://www.google.com";
            try
            {
                System.Net.WebRequest myRequest = System.Net.WebRequest.Create(url);
                System.Net.WebResponse myResponse = myRequest.GetResponse();
            }
            catch (System.Net.WebException)
            {
                return false;
            }
            return true;
        }
        public void MigrateDB(ServiceConfiguration objServiceConfiguration)
        {
            CommonServices obj = new CommonServices(_dbsetting);
            try
            {
                //obj.MigrateDBLog("Start");
                obj.ServiceLog(serviceName, "Start", "", "", "MigrateDB()");

                MongoClient dbClient = new MongoClient(_dbsetting.ConnectionString);
                var db = dbClient.GetDatabase(_dbsetting.DatabaseName);

                //Migrate db
                MongoClient Client = new MongoClient(objServiceConfiguration.MigrateConnectionString);
                var dbMigrate = Client.GetDatabase(objServiceConfiguration.MigrateDatabaseName);
                //end Migrate db
                var lstCollection = db.ListCollectionNames().ToList();

                foreach (var collect in lstCollection)
                {
                    var collection = db.GetCollection<BsonDocument>(collect);
                    var documents = collection.Find(new BsonDocument()).ToList();

                    foreach (BsonDocument doc in documents)
                    {
                        #region Migrate db

                        var collectionMigrate = dbMigrate.GetCollection<BsonDocument>(collect);
                        var builder = Builders<BsonDocument>.Filter;
                        var filter = builder.Eq("_id", doc["_id"]);

                        var docs = collectionMigrate.Find(filter).ToList();
                        if (docs.Count == 0)
                            collectionMigrate.InsertOne(doc);
                        else
                            collectionMigrate.ReplaceOne(filter, doc);

                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                obj.ServiceLog(serviceName, "Error", e.Message, e.StackTrace, "MigrateDB()");
            }
            obj.ServiceLog(serviceName, "End", "", "", "MigrateDB()");

        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Sending mail");
            return Task.CompletedTask;
        }


    }
}
