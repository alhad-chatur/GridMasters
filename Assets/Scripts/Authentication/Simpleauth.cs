using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Simpleauth : MonoBehaviour
{
    [SerializeField] GameObject signupUI, signinUI;
    int mode;
    string id;
    
    async void Start()
    {
        mode = 0;
        signupUI.SetActive(true);
        signinUI.SetActive(false);
        await UnityServices.InitializeAsync();
    }

    public void changemode(int imode)
    {
        mode = imode;
        if(mode == 0)
        {
            signupUI.SetActive(true);
            signinUI.SetActive(false);
        }
        else if(mode == 1)
        {
            signupUI.SetActive(false);
            signinUI.SetActive(true);
        }
    }

    public async void submit()
    {

        if (mode == 0)
        {
            string username = signupUI.transform.Find("username").GetComponent<TMP_InputField>().text;
            string password = signupUI.transform.Find("password").GetComponent<TMP_InputField>().text;
            if (!ValidateUsername(username))
            {
                Debug.Log("Username does not meets the criteria");
                return;
            }
            if (!ValidatePassword(password))
            {
                Debug.Log("Password does not meets the criteria");
                return;
            }
            await SignUpWithUsernamePassword(username, password);
        }
        else if(mode == 1)
        {
            string username = signinUI.transform.Find("username").GetComponent<TMP_InputField>().text;
            string password = signinUI.transform.Find("password").GetComponent<TMP_InputField>().text;
            
            //if (!ValidateUsername(username))
            //{
            //    Debug.Log("Username does not meets the criteria");
            //    return;
            //}
            //if (!ValidatePassword(password))
            //{
            //    Debug.Log("Password does not meets the criteria");
            //    return;
            //}

            await SignInWithUsernamePassword(username, password);
        }

    }

    async Task SignInWithUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            print(AuthenticationService.Instance.PlayerId);
            Debug.Log("SignIn is successful.");
            //var data = new Dictionary<string, object> { { "MySaveKey", "HelloWorld" } };
            //await CloudSaveService.Instance.Data.ForceSaveAsync(data);


            await EconomyManager.instance.RefreshEconomyConfiguration();
            if (this == null) return;

            await UpdateEconomy();
            if (this == null) return;

            SceneManager.LoadScene("Puzzle Level");

            //id = await startmatchmake();
            //StartCoroutine(SendRequest());
            //ForegroundServiceController.StartForegroundService(AuthenticationService.Instance.AccessToken, id);
            //await pollmatchmake(id);

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    //private void OnApplicationPause(bool pause)
    //{
    //    if (pause)
    //    {
    //        ForegroundServiceController.StartForegroundService(AuthenticationService.Instance.AccessToken, id);
    //    }
    //}

  
    IEnumerator SendRequest()
    {
        // URL of the API endpoint you want to send the GET request to
        string url = "https://matchmaker.services.api.unity.com/v2/tickets/status";

        string accessToken = AuthenticationService.Instance.AccessToken;

        url += "?id=" + id;

        // Create a UnityWebRequest object for GET request
        UnityWebRequest request = UnityWebRequest.Get(url);

        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        // Send the request
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Request succeeded, handle the response
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);

        }
        else
        {
            // Request failed
            Debug.Log("Error: " + request.error);
        }
    }


    async Task UpdateEconomy()
    {
        await Task.WhenAll(EconomyManager.instance.RefreshCurrencyBalances());
            //EconomyManager.instance.RefreshInventory());
    }

    async Task SignUpWithUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            print(AuthenticationService.Instance.PlayerId);
            Debug.Log("SignUp is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }


    private static Regex AllowedChars = new Regex(@"^[a-zA-Z0-9.,\-@]+$");
    private static Regex PasswordRequirements = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,30}$");
    public bool ValidatePassword(string password)
    {
        // Check if the password meets the requirements
        if (!PasswordRequirements.IsMatch(password))
        {
            return false;
        }

        return true;
    }
    public bool ValidateUsername(string username)
    {
        // Convert the username to lower case to make the validation case-insensitive
        string lowerCaseUsername = username.ToLower();

        // Check if the length of the username is within the allowed range
        if (lowerCaseUsername.Length < 3 || lowerCaseUsername.Length > 20)
        {
            return false;
        }

        // Check if the username contains only the allowed characters
        if (!AllowedChars.IsMatch(lowerCaseUsername))
        {
            return false;
        }

        return true;
    }

}
