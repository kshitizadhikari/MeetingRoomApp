using Microsoft.EntityFrameworkCore;
using RoomApp.DataAccess.DAL;
using RoomApp.DataAccess.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RoomApp.DataAccess.Infrastructure.Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly AppDbContext _db;

        public RepositoryBase(AppDbContext db)
        {
            _db = db;
        }
        public IQueryable<T> FindAll() => _db.Set<T>().AsNoTracking();
        public async Task<T?> FindById(object Id)
        {
            return await _db.Set<T>().FindAsync(Id);
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression) => _db.Set<T>().Where(expression).AsNoTracking();
        public void Create(T entity) => _db.Set<T>().Add(entity);
        public void Update(T entity) => _db.Set<T>().Update(entity);
        public void Delete(T entity) => _db.Set<T>().Remove(entity);

    }
}
