using RoomApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebPages.Html;

namespace RoomApp.DataAccess.Infrastructure.Interfaces
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> FindAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
        Task<T?> FindById(object id);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        void RemoveMultiple(List<T> entitiesToRemove);


    }
}
