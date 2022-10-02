using Envelope.Extensions;
using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Envelope.EntityFrameworkCore.PostgreSql.Internal;

internal class DbContextTransactionBehaviorObserver : ITransactionBehaviorObserver
{
	private bool _disposed;
	private readonly IDbContextTransaction _dbContextTransaction;
	private readonly int _waitForConnectionExecutingInMilliseconds;
	private readonly int _waitForConnectionExecutingCount;

	public DbContextTransactionBehaviorObserver(IDbContextTransaction dbContextTransaction, int waitForConnectionExecutingInMilliseconds = 50, int waitForConnectionExecutingCount = 40)
	{
		_dbContextTransaction = dbContextTransaction ?? throw new ArgumentNullException(nameof(dbContextTransaction));
		_waitForConnectionExecutingInMilliseconds = waitForConnectionExecutingInMilliseconds;
		_waitForConnectionExecutingCount = waitForConnectionExecutingCount;
	}

	public void Commit(ITransactionCoordinator transactionCoordinator)
	{
		var transaction = _dbContextTransaction.GetDbTransaction();
		if (transaction is not Npgsql.NpgsqlTransaction npgsqlTransaction)
			throw new InvalidOperationException($"Invalid transaction type '{transaction?.GetType().FullName}' | Expected {typeof(Npgsql.NpgsqlTransaction).FullName}");

		if (npgsqlTransaction.IsCompletedTransaction() || npgsqlTransaction.IsDisposedTransaction())
			return;

		var iterations = 0;
		while (npgsqlTransaction.Connection?.State == System.Data.ConnectionState.Executing)
		{
			iterations++;
			Thread.Sleep(_waitForConnectionExecutingInMilliseconds);
			if (_waitForConnectionExecutingCount < iterations)
				break;
		}

		if (!npgsqlTransaction.IsCompletedTransaction() && !npgsqlTransaction.IsDisposedTransaction())
			_dbContextTransaction.Commit();
	}

	public async Task CommitAsync(ITransactionCoordinator transactionCoordinator, CancellationToken cancellationToken)
	{
		var transaction = _dbContextTransaction.GetDbTransaction();
		if (transaction is not Npgsql.NpgsqlTransaction npgsqlTransaction)
			throw new InvalidOperationException($"Invalid transaction type '{transaction?.GetType().FullName}' | Expected {typeof(Npgsql.NpgsqlTransaction).FullName}");

		if (npgsqlTransaction.IsCompletedTransaction() || npgsqlTransaction.IsDisposedTransaction())
			return;

		var iterations = 0;
		while (npgsqlTransaction.Connection?.State == System.Data.ConnectionState.Executing)
		{
			iterations++;
			await Task.Delay(_waitForConnectionExecutingInMilliseconds, cancellationToken);
			if (_waitForConnectionExecutingCount < iterations)
				break;
		}

		if (!npgsqlTransaction.IsCompletedTransaction() && !npgsqlTransaction.IsDisposedTransaction())
			await _dbContextTransaction.CommitAsync(cancellationToken);
	}

	public void Rollback(ITransactionCoordinator transactionCoordinator, Exception? exception)
	{
		var transaction = _dbContextTransaction.GetDbTransaction();
		if (transaction is not Npgsql.NpgsqlTransaction npgsqlTransaction)
			throw new InvalidOperationException($"Invalid transaction type '{transaction?.GetType().FullName}' | Expected {typeof(Npgsql.NpgsqlTransaction).FullName}");

		if (npgsqlTransaction.IsCompletedTransaction() || npgsqlTransaction.IsDisposedTransaction())
			return;

		var iterations = 0;
		while (npgsqlTransaction.Connection?.State == System.Data.ConnectionState.Executing)
		{
			iterations++;
			Thread.Sleep(_waitForConnectionExecutingInMilliseconds);
			if (_waitForConnectionExecutingCount < iterations)
				break;
		}

		if (!npgsqlTransaction.IsCompletedTransaction() && !npgsqlTransaction.IsDisposedTransaction())
			_dbContextTransaction.Rollback();
	}

	public async Task RollbackAsync(ITransactionCoordinator transactionCoordinator, Exception? exception, CancellationToken cancellationToken)
	{
		var transaction = _dbContextTransaction.GetDbTransaction();
		if (transaction is not Npgsql.NpgsqlTransaction npgsqlTransaction)
			throw new InvalidOperationException($"Invalid transaction type '{transaction?.GetType().FullName}' | Expected {typeof(Npgsql.NpgsqlTransaction).FullName}");

		if (npgsqlTransaction.IsCompletedTransaction() || npgsqlTransaction.IsDisposedTransaction())
			return;

		var iterations = 0;
		while (npgsqlTransaction.Connection?.State == System.Data.ConnectionState.Executing)
		{
			iterations++;
			await Task.Delay(_waitForConnectionExecutingInMilliseconds, cancellationToken);
			if (_waitForConnectionExecutingCount < iterations)
				break;
		}

		if (!npgsqlTransaction.IsCompletedTransaction() && !npgsqlTransaction.IsDisposedTransaction())
			await _dbContextTransaction.RollbackAsync(cancellationToken);
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual ValueTask DisposeAsyncCoreAsync()
		=> _dbContextTransaction.DisposeAsync();

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
			_dbContextTransaction.Dispose();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
