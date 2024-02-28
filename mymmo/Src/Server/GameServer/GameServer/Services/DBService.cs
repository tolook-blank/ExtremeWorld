
using Common;

namespace GameServer.Services
{
    class DBService : Singleton<DBService> //DB服务是要全局存在的，适合做成单例
    {
        ExtremeWorldEntities entities;

        public ExtremeWorldEntities Entities
        {
            get { return this.entities; }
        }

        public void Init()
        {
            entities = new ExtremeWorldEntities();
        }

        //用一个定时器 timer，在数据库存储Save()前判断一下，当前时间 DateTime.Now.Ticks - timer > delta（如1分钟）, 来按间隔时间存储DB，以优化性能
        //数据库操作受 I/O 限制:网络速度不快，SQL 查询需要时间来处理
        public void Save(bool async = false)// async Task + await SaveChangesAsync  async：将方法标记为异步方法，Task表示一个异步操作,
        {
            if (async) //注意EF不支持同时保存多个，使用 “await” 搭配 SaveChangesAsync，确保在此 context 上调用另一个方法之前已完成所有异步操作。
                entities.SaveChangesAsync();//await SaveChangesAsync是异步保存，不阻塞，Save操作延后执行，可能会导致回档
            else
                entities.SaveChanges(); //默认是SaveChanges同步保存，在执行数据库 I/O 时阻塞线程，直到Save完成后才返回
        }
    }
}
