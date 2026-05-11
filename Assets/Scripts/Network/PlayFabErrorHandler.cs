using System.Collections.Generic;
using UnityEngine;
using PlayFab;

public class PlayFabErrorHandler : MonoBehaviour
{
    private static readonly Dictionary<PlayFabErrorCode, string> ErrorMessages = new()
    {
        { PlayFabErrorCode.InvalidEmailAddress, "The email address entered is invalid." },
        { PlayFabErrorCode.EmailAddressNotAvailable, "This email address is already in use." },
        { PlayFabErrorCode.InvalidPassword, "Password must be between 6 and 100 characters." },
        { PlayFabErrorCode.UsernameNotAvailable, "This username is already taken." },
        { PlayFabErrorCode.InvalidParams, "Required information is missing or incorrect." },
        { PlayFabErrorCode.InvalidUsername, "The username is invalid or contains forbidden characters." },
        { PlayFabErrorCode.AccountNotFound, " Account not found. Please check your credentials."},
        { PlayFabErrorCode.InvalidUsernameOrPassword ," Invalid username or password." }

    };

    public static string GetErrorMessage(PlayFabErrorCode errorCode)
    {
        return ErrorMessages.TryGetValue(errorCode, out string message)
               ? message
               : $"알 수 없는 에러 발생 (코드: {errorCode})";
    }
}
