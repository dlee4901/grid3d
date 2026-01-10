//using Unity.Netcode;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestMultiplayer : MonoBehaviour
{
    [SerializeField] private SessionManager _sessionManager;
    
    [Header("Start")]
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _startClientButton;
    
    [Header("Host Page")]
    [SerializeField] private TMP_Text _codeText;
    [SerializeField] private Button _startGameButton;

    [Header("Client Page")]
    [SerializeField] private TMP_InputField _codeInputField;
    [SerializeField] private Button _joinGameButton;
    
    private enum TestMultiplayerPage {Hide, Start, Host, Client}
    
    void Start()
    {
        _sessionManager.OnCreateJoinCode += SessionManager_OnCreateJoinCode;
        ToggleUI(TestMultiplayerPage.Start);
        _startHostButton.onClick.AddListener(() =>
        {
            ToggleUI(TestMultiplayerPage.Hide);
            _sessionManager.StartSessionAsHost();
        });
        _startClientButton.onClick.AddListener(() => {
            ToggleUI(TestMultiplayerPage.Client);
        });
        _startGameButton.onClick.AddListener(() => {
            EventManager.Singleton.StartGame();
            gameObject.SetActive(false);
        });
        _joinGameButton.onClick.AddListener(() => {
            _sessionManager.JoinSessionAsClient(_codeInputField.text);
            gameObject.SetActive(false);
        });
    }

    private void SessionManager_OnCreateJoinCode(object sender, SessionManager.OnCreateJoinCodeArgs e)
    {
        _codeText.text = e.JoinCode;
        Debug.Log("test");
        ToggleUI(TestMultiplayerPage.Host);
    }
    
    private void ToggleUI(TestMultiplayerPage page)
    {
        if (page == TestMultiplayerPage.Start)
        {
            gameObject.SetActive(true);
            _startClientButton.gameObject.SetActive(true);
            _startHostButton.gameObject.SetActive(true);
            _codeText.gameObject.SetActive(false);
            _startGameButton.gameObject.SetActive(false);
            _codeInputField.gameObject.SetActive(false);
            _joinGameButton.gameObject.SetActive(false);
        }
        else if (page == TestMultiplayerPage.Host)
        {
            _startClientButton.gameObject.SetActive(false);
            _startHostButton.gameObject.SetActive(false);
            _codeText.gameObject.SetActive(true);
            _startGameButton.gameObject.SetActive(true);
            _codeInputField.gameObject.SetActive(false);
            _joinGameButton.gameObject.SetActive(false);
        }
        else if (page == TestMultiplayerPage.Client)
        {
            _startClientButton.gameObject.SetActive(false);
            _startHostButton.gameObject.SetActive(false);
            _codeText.gameObject.SetActive(false);
            _startGameButton.gameObject.SetActive(false);
            _codeInputField.gameObject.SetActive(true);
            _joinGameButton.gameObject.SetActive(true);
        }
        else if (page == TestMultiplayerPage.Hide)
        {
            _startClientButton.gameObject.SetActive(false);
            _startHostButton.gameObject.SetActive(false);
            _codeText.gameObject.SetActive(false);
            _startGameButton.gameObject.SetActive(false);
            _codeInputField.gameObject.SetActive(false);
            _joinGameButton.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
