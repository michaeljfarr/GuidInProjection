using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GuidInProjection
{
    public class EntityType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ParentId { get; set; }

        //public virtual ICollection<En>
    }
    class GuidDbContext : DbContext
    {
        public DbSet<EntityType> Entities { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databaseName = "GuidTest2";
            var connection = $@"Server=(localdb)\mssqllocaldb;Database={databaseName};Trusted_Connection=True;ConnectRetryCount=0";
            optionsBuilder.UseSqlServer(connection);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
    public class ProjectionType
    {
        public Guid SomeOtherId { get; set; }
        public string Name { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new GuidDbContext())
            {
                db.Database.EnsureCreated();
                var childId = Guid.NewGuid();
                db.Entities.Add(new EntityType() {Id = childId, Name = "adsf"});
                db.SaveChanges();
                var fromSql = db.Entities.FromSql($@"with cte(id)
as (select top 1 id from Entities where id={childId}
union all 
select e.ParentId as Id from Entities e inner join cte on cte.id=e.Id
)
select d.* from cte inner join Entities d on d.id=cte.id");
                var all = fromSql.ToList();
                var projectionTest = fromSql
                    .Select(a => new ProjectionType { SomeOtherId = a.Id, Name = a.Name }).ToList();
            }
        }

    }
}
