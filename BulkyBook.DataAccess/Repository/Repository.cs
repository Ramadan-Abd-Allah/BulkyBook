using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
           this.dbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            if (entity != null)
            {
                dbSet.Add(entity);
            }
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable <T> query = dbSet;
            if(filter !=null)
                query = query.Where(filter);

            IncludePropertiesToQuery(includeProperties,ref query);
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            query = query.Where(filter);
            IncludePropertiesToQuery(includeProperties,ref query);
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            if (entity != null)
            {
                dbSet.Remove(entity);
            }
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities != null)
            {
                dbSet.RemoveRange(entities);
            }
        }

        private void IncludePropertiesToQuery(string includedPropertiesString,ref IQueryable<T> query)
        {
            if (!string.IsNullOrEmpty(includedPropertiesString))
            {
                foreach (var includeProp in includedPropertiesString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
        }
    }
}
