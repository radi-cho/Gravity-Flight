using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
    public static bool signedIn = false;
    public static string email = "";
    public static bool IsEmailVerified;
    public static string userId = "";
    public GameObject SignedInObject;
    public GameObject SignedOutObject;
    public GameObject EmailVerificationButton;
    public Text SignedInAsEmail;
    public Text Info;
    public InputField EmailInput;
    public InputField PasswordInput;
    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    void Start()
    {
        InitializeFirebase();

        if (signedIn)
        {
            SignedInObject.SetActive(true);
        }
        else
        {
            SignedOutObject.SetActive(true);
        }
    }

    void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser == null)
        {
            // For now we should rely on the Firebase session not expireing because manually logging out breaks
            // the non-profile functionalities. Also if the session expires you will be really fine if you just re- log in.
            // LogOut(false);
        }
        else
        {
            user = auth.CurrentUser;
            AuthStateChanged();
            Sync.SessionSync();
        }
    }

    void AuthStateChanged()
    {
        Info.text = "";
        EmailVerificationButton.SetActive(false);

        if (user == null)
        {
            signedIn = false;
            email = "";
            userId = "";
            IsEmailVerified = false;
            SignedInObject.SetActive(false);
            SignedOutObject.SetActive(true);
        }
        else
        {
            signedIn = true;
            email = user.Email;
            userId = user.UserId;
            IsEmailVerified = user.IsEmailVerified;
            SignedInObject.SetActive(true);
            SignedOutObject.SetActive(false);
            SignedInAsEmail.text = "Signed in as:\n" + email;

            if (!IsEmailVerified)
            {
                Info.text = "Please verify your email address!";
                EmailVerificationButton.SetActive(true);
            }
        }
    }

    public void SignUp()
    {
        if (!ValidateEmail()) return;
        if (!ValidatePassword()) return;

        auth.CreateUserWithEmailAndPasswordAsync(EmailInput.text, PasswordInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Info.text = "Something went wrong: Sign Up Task was canceled.";
                return;
            }
            if (task.IsFaulted)
            {
                Info.text = "Error! If you already own an account please log in instead. Make sure all fields are filled correctly!";
                Debug.Log(task.Exception);
                return;
            }

            user = task.Result;
            AuthStateChanged();
            VerifyEmail();
            Sync.InitializeSync();
            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventSignUp);
        });
    }

    public void LogIn()
    {
        if (!ValidateEmail()) return;
        if (!ValidatePassword()) return;

        Debug.Log(EmailInput.text);
        Debug.Log(PasswordInput.text);

        auth.SignInWithEmailAndPasswordAsync(EmailInput.text, PasswordInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Info.text = "Something went wrong: Log In Task was canceled.";
                return;
            }
            if (task.IsFaulted)
            {
                Info.text = "Ensure stable internet connection! Make sure your email and password are correct or reset them! If you do not have account Sign Up instead!";
                Debug.Log(task.Exception);
                return;
            }

            user = task.Result;
            AuthStateChanged();
            Sync.ExistingSync();
            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
        });
    }

    public void LogOut(bool firebaseSignOut)
    {
        if (firebaseSignOut) auth.SignOut();
        user = null;
        AuthStateChanged();

        bool wasTutorialCompleted = PlayerPrefs.GetInt("tutorial_complete", 0) == 1;
        PlayerPrefs.DeleteAll();

        if (wasTutorialCompleted)
        {
            PlayerPrefs.SetInt("tutorial_complete", 1);
            PlayerPrefs.SetString("lastCompletedLevel", "3");
            PlayerPrefs.SetString("Level1", "Completed");
            PlayerPrefs.SetString("Level2", "Completed");
            PlayerPrefs.SetString("Level3", "Completed");
        }

        PlayerPrefs.SetString("spaceship", "Starman/rocket");
        UpdateStoreItems.UpdateAllItems();
    }

    public void ResetPassword()
    {
        if (!ValidateEmail()) return;

        auth.SendPasswordResetEmailAsync(EmailInput.text).ContinueWith((authTask) =>
        {
            if (authTask.IsCanceled)
            {
                Info.text = "Password reset was canceled.";
            }
            else if (authTask.IsFaulted)
            {
                Info.text = "Password reset encountered an error.";
                Debug.Log(authTask.Exception.ToString());
            }
            else if (authTask.IsCompleted)
            {
                Info.text = "Reset email sent successfully!\nPlease follow the instructions sent to you via email!";
            }
        });
    }

    public void VerifyEmail()
    {
        user.SendEmailVerificationAsync();
        Info.text = "We have sent you a new verification message. If you still see an error after the email is verified, close and reopen the app.";
    }

    public bool ValidateEmail()
    {
        string MatchEmailPattern =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
              + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        bool IsValid = EmailInput.text != null && Regex.IsMatch(EmailInput.text, MatchEmailPattern);
        if (!IsValid) Info.text = "Please enter a valid email addresss!";
        return IsValid;
    }

    public bool ValidatePassword()
    {
        bool IsValid = PasswordInput.text != null && PasswordInput.text.Length >= 6;
        if (!IsValid) Info.text = "Please enter a password with at least 6 characters!";
        return IsValid;
    }

    void OnDestroy()
    {
        auth = null;
    }
}
