using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour //민정수정
{
    public TMP_InputField emailInput, passwordInput, userNameInput;
    public Button loginBtn, registerBtn;
    public TextMeshProUGUI errorText;

    private PlayFabManager playFabManager;

    private void Start()
    {
        loginBtn.onClick.AddListener(TryLogin);
        registerBtn.onClick.AddListener(TryRegister);
    }

    private void OnEnable()
    {
        PlayFabManager.Instance.OnLogin += CheckLoginResult;
        PlayFabManager.Instance.OnRegister += CheckRegisterResult;
    }

    public void TryLogin()
    {
        if (string.IsNullOrWhiteSpace(emailInput.text) || string.IsNullOrWhiteSpace(passwordInput.text))
        {
            UpdateMessage("이메일과 비밀번호를 입력해주세요.", false);
            return;
        }

        PlayFabManager.Instance.Login(emailInput.text, passwordInput.text);

    }

    public void TryRegister()
    {
        playFabManager = PlayFabManager.Instance;

        playFabManager.Register(emailInput.text, passwordInput.text, userNameInput.text);
    }

    private void CheckLoginResult()
    {
        var pm = PlayFabManager.Instance;

        if (pm.SuccessLogin)
        {
            gameObject.SetActive(false);
            ReSetField();
        }
        else
            UpdateMessage(pm.Error, false);


    }

    private void CheckRegisterResult()
    {
        var pm = PlayFabManager.Instance;

        if (pm.SuccessRegister)
            UpdateMessage("회원가입이 완료되었습니다!", true);
        else
            UpdateMessage(pm.Error, false);
    }

    private void UpdateMessage(string message, bool isSuccess)
    {
        errorText.text = message;
        errorText.color = isSuccess ? Color.green : Color.red;
    }

    public void ReSetField()
    {
        emailInput.text = "";
        passwordInput.text = "";
        userNameInput.text = "";
    }

}