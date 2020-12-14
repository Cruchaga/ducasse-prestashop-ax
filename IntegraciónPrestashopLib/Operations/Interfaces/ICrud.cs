using IntegracionPrestashopLib.Schema.dto;
using System.Collections;
using System.Collections.Generic;

namespace IntegracionPrestashopLib.Operations
{
    internal interface ICrud<T>
    {
        OperacionResponse<T> Get(Hashtable p);
        OperacionResponse<T> Create(T o);
        OperacionResponse<T> Update(T o);
        OperacionResponse<T> Delete(T o);
    }
}