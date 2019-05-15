using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace KeyLineTest.Models.Repository
{
    public class Repository<TEntity,TKey> where TEntity: class

    {
        private readonly KeyLinesTestEntities _ctx = new KeyLinesTestEntities();
        
        public Repository()
        {
            
        }

        public Repository(KeyLinesTestEntities dbContext)
        {
            _ctx = dbContext;
        }

        public TEntity Find(TKey key)
        {
            return _ctx.Set<TEntity>().Find(key);
        }

        public void Insert(TEntity entity)
        {
            _ctx.Set<TEntity>().Add(entity);
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity,bool>> predicate)
        {
            return _ctx.Set<TEntity>().Where(predicate).AsQueryable();

        }
        public int SaveChanges()
        {
            return _ctx.SaveChanges();
        }

    }
}