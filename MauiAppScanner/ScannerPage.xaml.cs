using ZXing.Net.Maui;
using System.Net;

namespace MauiAppScanner;

public partial class ScannerPage : ContentPage
{
    private bool _isScanning = false;

    public ScannerPage()
    {
        InitializeComponent();
    }

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isScanning)
            return;

        var result = e.Results?.FirstOrDefault()?.Value;

        if (!string.IsNullOrEmpty(result))
        {
            _isScanning = true;
            cameraView.IsDetecting = false;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var apiUrl = $"http://192.168.55.3:5136/api/qrcodevalidation/validate?qrCode={Uri.EscapeDataString(result)}";

                try
                {
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(apiUrl);

                    string message = response.StatusCode switch
                    {
                        HttpStatusCode.OK => "✅ Le code QR est valide.",
                        HttpStatusCode.Conflict => "⚠️ Ce code QR a déjà été scanné.",
                        HttpStatusCode.NotFound => "❌ Le code QR est invalide ou introuvable.",
                        HttpStatusCode.BadRequest => "⚠️ Code QR non valide.",
                        _ => $"⚠️ Erreur : {response.StatusCode}"
                    };

                    await DisplayAlert("Vérification du code QR", message, "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erreur", $"Impossible de se connecter à l’API : {ex.Message}", "OK");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));
                    _isScanning = false;
                    cameraView.IsDetecting = true;
                }
            });
        }
    }
}
