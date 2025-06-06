// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class TestMultiplayer : MonoBehaviour
// {
//     [SerializeField] private Button _startServer;
//     [SerializeField] private Button _startClient;
//     [SerializeField] private Button _startHost;
//     [SerializeField] private Button _startGameButton;
//
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         EnableUI(1);
//         _startServer.onClick.AddListener(() => {
//             NetworkManager.Singleton.StartServer();
//             Debug.Log("TestMultiplayer StartServer");
//             EnableUI(2);
//         });
//         _startClient.onClick.AddListener(() => {
//             NetworkManager.Singleton.StartClient();
//             EnableUI(0);
//         });
//         _startHost.onClick.AddListener(() => {
//             NetworkManager.Singleton.StartHost();
//             EnableUI(2);
//         });
//         _startGameButton.onClick.AddListener(() => {
//             EventManager.Singleton.StartGame();
//             EnableUI(0);
//         });
//     }
//
//     private void EnableUI(int level)
//     {
//         if (level == 1)
//         {
//             _startServer.gameObject.SetActive(true);
//             _startClient.gameObject.SetActive(true);
//             _startHost.gameObject.SetActive(true);
//             _startGameButton.gameObject.SetActive(false);
//         }
//         else if (level == 2)
//         {
//             _startServer.gameObject.SetActive(false);
//             _startClient.gameObject.SetActive(false);
//             _startHost.gameObject.SetActive(false);
//             _startGameButton.gameObject.SetActive(true);
//         }
//         else
//         {
//             _startServer.gameObject.SetActive(false);
//             _startClient.gameObject.SetActive(false);
//             _startHost.gameObject.SetActive(false);
//             _startGameButton.gameObject.SetActive(false);
//         }
//     }  
// }
