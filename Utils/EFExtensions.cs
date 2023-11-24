using Microsoft.EntityFrameworkCore;

namespace IsometricShooterWebApp.Utils
{
    public static class EFExtensions
    {
        public static async Task<bool> TryUpdateAsync(this DbContext dbContext, Func<Task<bool>> action, byte time = 15)
        {
            for (int i = 0; i < time; i++)
            {
                try
                {
                    return await action();
                }
                catch (DbUpdateException)
                {
                    if (time - 1 == i)
                        throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return false;
        }
    }
}
