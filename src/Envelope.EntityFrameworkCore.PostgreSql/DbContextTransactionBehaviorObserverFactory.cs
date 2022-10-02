using Envelope.Database.PostgreSql;
using Envelope.EntityFrameworkCore.PostgreSql.Internal;
using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Envelope.EntityFrameworkCore.PostgreSql;

public class DbContextTransactionBehaviorObserverFactory : IDbContextTransactionBehaviorObserverFactory
{
	public ITransactionBehaviorObserver Create(IDbContextTransaction dbContextTransaction, int waitForConnectionExecutingInMilliseconds = 50, int waitForConnectionExecutingCount = 40)
		=> new DbContextTransactionBehaviorObserver(dbContextTransaction, waitForConnectionExecutingInMilliseconds, waitForConnectionExecutingCount);
}
