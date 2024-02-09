namespace Solution.Data.Provider;

/// <summary>
/// Classe per la gestione della transazione.
/// </summary>
public class cTransaction : System.Data.IDbTransaction
{
    private IDbTransaction _Transaction;
    private cConnection oCn;

    public System.Data.IDbConnection Connection
    {
        get { return oCn.Connection; }
    }

    public void Dispose()
    {
    }

    public cTransaction(cConnection oCn)
    {
        this.oCn = oCn;
        _Transaction = oCn.BeginTransaction();
    }

    public cTransaction(cConnection oCn, System.Data.IsolationLevel il)
    {
        this.oCn = oCn;
        _Transaction = oCn.BeginTransaction(il);
    }

    public void Commit()
    {
        _Transaction.Commit();
    }

    public void Rollback()
    {
        _Transaction.Rollback();
    }

    public System.Data.IDbTransaction IDbTransaction
    {
        get { return _Transaction; }
    }

    public System.Data.IsolationLevel IsolationLevel
    {
        get { return _Transaction.IsolationLevel; }
    }

    #region IDbTransaction Members

    void IDbTransaction.Commit()
    {
        throw new NotImplementedException();
    }

    IDbConnection IDbTransaction.Connection
    {
        get { throw new NotImplementedException(); }
    }

    IsolationLevel IDbTransaction.IsolationLevel
    {
        get { throw new NotImplementedException(); }
    }

    void IDbTransaction.Rollback()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IDisposable Members

    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }

    #endregion
}
