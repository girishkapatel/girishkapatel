using AuditManagementCore.Models;

namespace AuditManagementCore.MongoDb.IUnitOfWork
{
    public class UnitOfWork
    {
        #region Variable
        private IMongoDbSettings mongoDbSettings;
        #endregion

        #region Ctor
        public UnitOfWork(IMongoDbSettings dbsettings)
        {
            mongoDbSettings = dbsettings;
        }

        public MongoGenericRepository<Country> CountryRepository
        {
            get
            {
                mongoDbSettings.CollectionName = nameof(Country);
                return new MongoGenericRepository<Country>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<State> StateRepository
        {
            get
            {
                return new MongoGenericRepository<State>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<CityOrTown> CityOrTownRepository
        {
            get
            {
                return new MongoGenericRepository<CityOrTown>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<Settings> SettingRepository
        {
            get
            {
                return new MongoGenericRepository<Settings>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<Templates> TemplateRepository
        {
            get
            {
                return new MongoGenericRepository<Templates>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<Location> LocationRepository
        {
            get
            {
                return new MongoGenericRepository<Location>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<User> UserRepository
        {
            get
            {
                return new MongoGenericRepository<User>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<Role> RoleRepository
        {
            get
            {
                return new MongoGenericRepository<Role>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<BusinessCycle> BusinessCycleRepository
        {
            get
            {
                return new MongoGenericRepository<BusinessCycle>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<Control> ControlRepository
        {
            get
            {
                return new MongoGenericRepository<Control>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<EYBenchmark> EYBenchmarkRepository
        {
            get
            {
                return new MongoGenericRepository<EYBenchmark>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<ProcessL1> ProcessL1Repository
        {
            get
            {
                return new MongoGenericRepository<ProcessL1>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<ProcessL2> ProcessL2Repository
        {
            get
            {
                return new MongoGenericRepository<ProcessL2>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<Risk> RiskRepository
        {
            get
            {
                return new MongoGenericRepository<Risk>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<Sector> SectorRepository
        {
            get
            {
                return new MongoGenericRepository<Sector>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<ImpactMaster> ImpactMaster
        {
            get
            {
                return new MongoGenericRepository<ImpactMaster>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<RiskType> RiskType
        {
            get
            {
                return new MongoGenericRepository<RiskType>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<ReportConsideration> ReportConsideration
        {
            get
            {
                return new MongoGenericRepository<ReportConsideration>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<Recommendation> Recommendation
        {
            get
            {
                return new MongoGenericRepository<Recommendation>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<RootCause> RootCause
        {
            get
            {
                return new MongoGenericRepository<RootCause>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<AuditFiles> AuditFiles
        {
            get
            {
                return new MongoGenericRepository<AuditFiles>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<TrialBalance> TrialBalance
        {
            get
            {
                return new MongoGenericRepository<TrialBalance>(mongoDbSettings);
            }
        }

        public MongoGenericRepository<ObservationGrading> ObservationGrading
        {
            get
            {
                return new MongoGenericRepository<ObservationGrading>(mongoDbSettings);
            }
        }
        #endregion

        #region Public member methods...
        #endregion
    }
}