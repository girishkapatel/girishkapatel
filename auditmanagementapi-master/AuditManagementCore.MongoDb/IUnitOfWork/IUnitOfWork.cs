using AuditManagementCore.Models;

namespace AuditManagementCore.MongoDb.IUnitOfWork
{
    public interface IUnitOfWork
    {
        MongoGenericRepository<Country> CountryRepository { get; }
        MongoGenericRepository<State> StateRepository { get; }
        MongoGenericRepository<CityOrTown> CityOrTownRepository { get; }
        MongoGenericRepository<Settings> SettingRepository { get; }
        MongoGenericRepository<Templates> TemplateRepository { get; }
        MongoGenericRepository<Location> LocationRepository { get; }
        MongoGenericRepository<User> UserRepository { get; }
        MongoGenericRepository<Role> RoleRepository { get; }
        MongoGenericRepository<BusinessCycle> BusinessCycleRepository { get; }
        MongoGenericRepository<Control> ControlRepository { get; }
        MongoGenericRepository<EYBenchmark> EYBenchmarkRepository { get; }
        MongoGenericRepository<ProcessL1> ProcessL1Repository { get; }
        MongoGenericRepository<ProcessL2> ProcessL2Repository { get; }
        MongoGenericRepository<Risk> RiskRepository { get; }
        MongoGenericRepository<Sector> SectorRepository { get; }
        MongoGenericRepository<ImpactMaster> ImpactMaster { get; }
        MongoGenericRepository<RiskType> RiskType { get; }
        MongoGenericRepository<ReportConsideration> ReportConsideration { get; }
        MongoGenericRepository<Recommendation> Recommendation { get; }
        MongoGenericRepository<RootCause> RootCause { get; }
        MongoGenericRepository<AuditFiles> AuditFiles { get; }
        MongoGenericRepository<TrialBalance> TrialBalance { get; }
        MongoGenericRepository<ObservationGrading> ObservationGrading { get; }
    }
}