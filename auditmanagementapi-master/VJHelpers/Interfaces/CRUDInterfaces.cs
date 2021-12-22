using System;
using System.Collections.Generic;
using System.Text;
using VJLiabraries.Wrappers;

namespace VJLiabraries.Interfaces
{
    public interface IUpdateObject<T>
    {
        VJResult<bool> Update(T obj);
    }

    public interface ICreateObject<T>
    {
        VJResult<bool> Create(T obj);

    }
    public interface ICreateManyObject<T> : ICreateObject<T>
    {
        VJResult<bool> Create(IEnumerable<T> obj);
    }
    public interface IReadById<T>
    {
        T Get(int Id);

        IEnumerable<T> GetAll();
    }

    public interface IDeleteObject<T>
    {
        VJResult<bool> Delete(object obj);

        VJResult<bool> Delete(T obj);
    }

    public interface IDeleteById<T>
    {
        VJResult<bool> Delete(int id);

    }
    public interface IObjectActiveDeactive<T>
    {
        VJResult<bool> Active(T obj);

        VJResult<bool> Deactive(T obj);

    }

    public interface IActiveDeactiveById<T>
    {
        VJResult<bool> Active(int Id);

        VJResult<bool> Deactive(int id);

    }
    public interface IModifyById<T> : IActiveDeactiveById<T>, IDeleteById<T>
    {

    }
    public interface IModifyObject<T> : IObjectActiveDeactive<T>, IDeleteObject<T>
    {

    }

}
