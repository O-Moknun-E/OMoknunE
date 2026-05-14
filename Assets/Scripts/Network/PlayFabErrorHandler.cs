using System.Collections.Generic;
using UnityEngine;
using PlayFab;

public class PlayFabErrorHandler : MonoBehaviour
{
    private static readonly Dictionary<PlayFabErrorCode, string> ErrorMessages = new()
    {
        { PlayFabErrorCode.InvalidEmailAddress, "유효하지 않은 이메일 형식입니다." },
        { PlayFabErrorCode.EmailAddressNotAvailable, "이미 사용 중인 이메일 주소입니다." },
        { PlayFabErrorCode.InvalidPassword, "비밀번호는 6자에서 100자 사이여야 합니다." },
        { PlayFabErrorCode.UsernameNotAvailable, "이미 존재하는 사용자 이름입니다." },
        { PlayFabErrorCode.InvalidParams, "필수 정보가 누락되었거나 입력값이 올바르지 않습니다." },
        { PlayFabErrorCode.InvalidUsername, "사용자 이름이 유효하지 않거나 금지된 문자가 포함되어 있습니다." },
        { PlayFabErrorCode.AccountNotFound, " 계정을 찾을 수 없습니다. 입력 정보를 다시 확인해주세요."},
        { PlayFabErrorCode.InvalidUsernameOrPassword ," 아이디 또는 비밀번호가 일치하지 않습니다." },
        { PlayFabErrorCode.NameNotAvailable ," 사용할 수 없는 이름입니다." },
        { PlayFabErrorCode.InvalidEmailOrPassword ," 이메일 주소 또는 비밀번호가 올바르지 않습니다." },

    };

    public static string GetErrorMessage(PlayFabErrorCode errorCode)
    {
        return ErrorMessages.TryGetValue(errorCode, out string message)
               ? message
               : $"알 수 없는 에러 발생 (코드: {errorCode})";
    }
}
