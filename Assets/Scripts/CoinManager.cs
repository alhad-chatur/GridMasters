using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using System.Collections.Generic;

public class CoinManager : MonoBehaviour
{
    //public async void Start()
    //{
    //    await UnityServices.InitializeAsync();
    //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //    await EconomyService.Instance.Configuration.SyncConfigurationAsync();
    //    List<CurrencyDefinition> definitions = EconomyService.Instance.Configuration.GetCurrencies();
    //    foreach (var x in definitions)
    //        print(x.Name);
    //    print("Connection Done");

    //    PlayerBalance playersGoldBarBalance = await definitions[0].GetPlayerBalanceAsync();

    //    string currencyID = definitions[0].Id;
    //    await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(currencyID, 1000);
        
    //    print(playersGoldBarBalance.Balance);
    //}

    // Replace with currency
}