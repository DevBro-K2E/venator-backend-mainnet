
using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Managers.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Model;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using NuGet.ContentModel;
using System.Numerics;
using System.Transactions;
using Account = Nethereum.Web3.Accounts.Account;

namespace IsometricShooterWebApp.Managers
{
    public class BlockchainManager : IBlockchainManager
    {
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<IBlockchainManager> logger;

        public BlockchainManager(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<IBlockchainManager> logger)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public string GetAddress()
            => configuration.GetValue<string>("blockchain:wallet:address");
        private int GetChainId()
            => configuration.GetValue<int>("blockchain:chainId");

        public string GetBlockchainApiUrl()
            => configuration.GetValue<string>("blockchain:provider");

        //public async Task CreateBill(decimal count, string data)
        //{
        //    var web3 = GetWeb3();
        //}

        #region bindingTransactions

        private Timer bindingTimer;

        public void InitializeBinding()
        {
            if (bindingTimer == null)
                bindingTimer = new Timer((e) => bindingTransactions(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
        }

        private SemaphoreSlim bindingLocker = new SemaphoreSlim(1);

        private Nethereum.Hex.HexConvertors.HexUTF8StringConvertor hexValueConverter = new Nethereum.Hex.HexConvertors.HexUTF8StringConvertor();

        private async void bindingTransactions()
        {
            if (!await bindingLocker.WaitAsync(1000))
                return;
            // todo: check this - https://docs.infura.io/infura/tutorials/ethereum/subscribe-to-pending-transactions
            try
            {
                var web3 = GetWeb3();

                await using var scope = serviceProvider.CreateAsyncScope();

                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var newBlockId = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                var prevValue = await db.WalletBlocks.AnyAsync() ? await db.WalletBlocks.MaxAsync(x => x.BlockId) : (long)newBlockId.Value - 100;

                for (long blockId = prevValue; blockId <= newBlockId.Value; blockId++)
                {
                    var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new BlockParameter((ulong)blockId));

                    if (block == null)
                        break;

                    if (block.TransactionCount() == 0) // m.b
                        continue;

                    var dbBlock = await db.WalletBlocks.FindAsync(blockId);

                    if (dbBlock == null)
                    {
                        dbBlock = new Data.Models.WalletBlockModel() { BlockId = blockId };

                        dbBlock = db.WalletBlocks.Add(dbBlock).Entity;
                    }

                    using var dbTransaction = await db.Database.BeginTransactionAsync();

                    foreach (var transaction in block.Transactions)
                    {
                        if (!transaction.IsTo(GetAddress()))
                            continue;

                        var data = hexValueConverter.ConvertFromHex(transaction.Input);

                        if (!Guid.TryParse(data, out var userIdGuid))
                            continue;

                        if (await db.WalletTransactions.AnyAsync(x => x.TransactionId.Equals(transaction.TransactionHash)))
                            continue;

                        string userId = userIdGuid.ToString();

                        var etherValue = Web3.Convert.FromWei(transaction.Value);

                        var user = await db.Users.FindAsync(userId);

                        if (user == null)
                            continue;

                        user.Balance += (double)etherValue;

                        db.WalletTransactions.Add(new Data.Models.WalletTransactionModel()
                        {
                            Accepted = true,
                            Amount = (double)etherValue,
                            TransactionId = transaction.TransactionHash,
                            UserId = userId
                        });

                        ++dbBlock.ReceiveCount;
                    }

                    await db.SaveChangesAsync();

                    await dbTransaction.CommitAsync();
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.ToString());
#if DEBUG
                throw;
#endif
            }
            finally
            {
                bindingLocker.Release();
            }
        }

        #endregion

        public async Task<string?> SendTransactionAsync(string address, decimal count)
        {
            //https://github.com/Nethereum/Nethereum/issues/703 this help
            var web3 = GetWeb3();

            web3.TransactionManager.UseLegacyAsDefault = true;
            var txnInput = EtherTransferTransactionInputBuilder.CreateTransactionInput(web3.TransactionManager?.Account?.Address, address, count);
            txnInput.ChainId = await web3.Eth.ChainId.SendRequestAsync();

            var signedTx = await web3.TransactionManager.SignTransactionAsync(txnInput); 

            var result = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTx);

            return result;
        }

        private Web3 GetWeb3()
        {
            return new Web3(GetAccount(), url: GetBlockchainApiUrl());
        }

        private Account GetAccount()
        {
            //https://docs.nethereum.com/en/latest/nethereum-transferring-ether/
            return new Account(configuration.GetValue<string>("blockchain:wallet:privateKey"));
        }
    }
}
