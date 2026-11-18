namespace BookLibraryApi.UnitOfWork;

// UNIT OF WORK: groups repository operations behind one commit boundary.
public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
