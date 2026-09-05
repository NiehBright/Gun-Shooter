using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AddressablesUpdater : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Button downloadButton;
    [SerializeField] private GameObject updatePanel;

    [Header("Settings")]
    [SerializeField] private string nextSceneName = "Game";

    private AsyncOperationHandle downloadHandle;
    private long totalDownloadSize = 0;
    private List<string> catalogsToUpdate = new List<string>();

    private void Start()
    {
        // Ẩn panel tải xuống ban đầu
        updatePanel.SetActive(false);
        progressBar.gameObject.SetActive(false);
        downloadButton.gameObject.SetActive(false);

        statusText.text = "Đang kiểm tra máy chủ...";
        StartCoroutine(CheckForUpdates());
    }

    private IEnumerator CheckForUpdates()
    {
        // 1. Khởi tạo Addressables
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        // 2. Kiểm tra danh mục cập nhật
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            catalogsToUpdate = checkHandle.Result;
            if (catalogsToUpdate.Count > 0)
            {
                // Có bản cập nhật mới -> Cập nhật Catalog
                var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
                yield return updateHandle;

                // Lấy dung lượng cần tải
                var sizeHandle = Addressables.GetDownloadSizeAsync(catalogsToUpdate);
                yield return sizeHandle;

                totalDownloadSize = sizeHandle.Result;

                if (totalDownloadSize > 0)
                {
                    // Chuyển sang MB
                    float sizeMB = totalDownloadSize / (1024f * 1024f);
                    statusText.text = $"Có bản cập nhật mới: {sizeMB:F1} MB.\nBạn có muốn tải ngay?";
                    
                    updatePanel.SetActive(true);
                    downloadButton.gameObject.SetActive(true);
                    
                    // Gán sự kiện nút Tải
                    downloadButton.onClick.RemoveAllListeners();
                    downloadButton.onClick.AddListener(StartDownload);
                }
                else
                {
                    // Catalog mới nhưng không cần tải file (đã cache)
                    StartGame();
                }
                Addressables.Release(sizeHandle);
            }
            else
            {
                // Không có cập nhật
                statusText.text = "Dữ liệu đã ở phiên bản mới nhất!";
                yield return new WaitForSeconds(1f);
                StartGame();
            }
        }
        else
        {
            // Lỗi mạng hoặc server
            statusText.text = "Không thể kết nối máy chủ. Bỏ qua kiểm tra...";
            yield return new WaitForSeconds(1f);
            StartGame();
        }

        Addressables.Release(checkHandle);
    }

    private void StartDownload()
    {
        downloadButton.gameObject.SetActive(false);
        progressBar.gameObject.SetActive(true);
        statusText.text = "Đang tải dữ liệu...";

        StartCoroutine(DownloadDependencies());
    }

    private IEnumerator DownloadDependencies()
    {
        downloadHandle = Addressables.DownloadDependenciesAsync(catalogsToUpdate, Addressables.MergeMode.Union);

        while (!downloadHandle.IsDone)
        {
            if (downloadHandle.Status == AsyncOperationStatus.Failed)
            {
                statusText.text = "Tải xuống thất bại. Vui lòng thử lại.";
                downloadButton.gameObject.SetActive(true);
                yield break;
            }

            // Cập nhật thanh tiến trình
            progressBar.value = downloadHandle.PercentComplete;
            float downloadedMB = (downloadHandle.PercentComplete * totalDownloadSize) / (1024f * 1024f);
            float sizeMB = totalDownloadSize / (1024f * 1024f);
            statusText.text = $"Đang tải: {downloadedMB:F1} / {sizeMB:F1} MB";

            yield return null;
        }

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            statusText.text = "Tải thành công! Đang vào game...";
            progressBar.value = 1f;
            yield return new WaitForSeconds(1f);
            StartGame();
        }

        Addressables.Release(downloadHandle);
    }

    private void StartGame()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
