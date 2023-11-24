namespace IsometricShooterWebApp.Managers.Abstraction
{
    public interface IBlockchainManager
    {
        string GetAddress();

        string GetBlockchainApiUrl();

        void InitializeBinding();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="count"></param>
        /// <returns>transaction id. null if failed</returns>
        Task<string?> SendTransactionAsync(string address, decimal count);
    }
}
