using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace OptimaLibray.Db.Operations.Infrastracture
{
    public interface  IDatabase<TContext>: IDisposable
    {
        Connection Connection { get; }
    }
}
