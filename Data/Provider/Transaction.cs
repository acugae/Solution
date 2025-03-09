namespace Solution.Data.Provider;

/// <summary>
/// Classe per la gestione della transazione.
/// </summary>
public class Transaction : DbTransaction
{
    private readonly DbTransaction _transaction;
    private bool _isDisposed;

    public Transaction(DbTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        _isDisposed = false;
    }

    public override IsolationLevel IsolationLevel => _transaction.IsolationLevel;

    protected override DbConnection DbConnection => _transaction.Connection;

    public override void Commit()
    {
        ThrowIfDisposed();
        _transaction.Commit();
    }

    public override void Rollback()
    {
        ThrowIfDisposed();
        _transaction.Rollback();
    }

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _transaction.Dispose();
            }

            _isDisposed = true;
            base.Dispose(disposing);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(Transaction));
        }
    }
}
