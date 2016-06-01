using System;
using System.Collections.Generic;

namespace WebApplication.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        T GetById(string id);
        IEnumerable<T> Find(Func<T, bool> predicate);
        void Create(T photo);
        void Update(T photo);
        void Delete(long id);
	}
}