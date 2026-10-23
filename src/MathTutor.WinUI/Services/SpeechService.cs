using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MathTutor.WinUI.Services;

/// <summary>
/// Puente de comunicación nativa con el lector de pantalla instalado en el sistema
/// (NVDA, JAWS o Narrador de Windows). Elimina por completo el uso de SAPI5 para respetar
/// la voz, el sintetizador y la velocidad configurados por el usuario en su lector de pantalla.
/// </summary>
public class SpeechService : IDisposable
{
    private const uint SPI_GETSCREENREADER = 0x0046;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);

    [DllImport("nvdaControllerClient64.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int nvdaController_testIfRunning();

    [DllImport("nvdaControllerClient64.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int nvdaController_speakText(string text);

    [DllImport("nvdaControllerClient64.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int nvdaController_cancelSpeech();

    private bool _enabled = true;
    private object? _jawsComObject;
    private bool _jawsInitialized;
    private bool _jawsFailed;
    private bool _jawsDetected;
    private bool _nvdaClientDllAvailable = true;
    private bool _nvdaControllerActive;

    /// <summary>
    /// Canal B: Integración con el lector de pantalla del sistema (NVDA, JAWS, Narrador).
    /// Activo por defecto y configurable de forma independiente al audio interno (F2).
    /// </summary>
    public bool IsScreenReaderIntegrationEnabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Acción delegada conectada por la ventana principal para disparar una notificación
    /// de Microsoft UI Automation (UIA), compatible de forma nativa con NVDA, JAWS y el
    /// Narrador de Windows a la velocidad exacta del usuario.
    /// El segundo parámetro indica si el anuncio es asertivo inmediato (true) o tras asentar foco (false).
    /// </summary>
    public Action<string, bool>? UiAutomationAnnouncer { get; set; }

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Detecta cuál lector de pantalla se encuentra activo en el sistema operativo.
    /// </summary>
    public string GetActiveScreenReaderName()
    {
        try
        {
            if (Process.GetProcessesByName("nvda").Length > 0)
                return "NVDA";
            if (Process.GetProcessesByName("jfw").Length > 0)
                return "JAWS";
            if (Process.GetProcessesByName("Narrator").Length > 0)
                return "Narrador de Windows";

            bool screenReaderFlag = false;
            if (SystemParametersInfo(SPI_GETSCREENREADER, 0, ref screenReaderFlag, 0) && screenReaderFlag)
                return "Lector de Pantalla del Sistema";
        }
        catch
        {
        }

        return "UI Automation (NVDA / JAWS / Narrador)";
    }

    /// <summary>
    /// Envía el texto directamente al lector de pantalla del sistema (NVDA, JAWS o Narrador)
    /// respetando el sintetizador y la velocidad del usuario.
    /// </summary>
    public void Speak(string text, bool interruptPrevious = true, bool immediateLiveRegion = false)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(text))
            return;

        // 1. Si NVDA Controller Client DLL está presente y NVDA responde por RPC directo:
        if (TrySpeakViaNvdaController(text, interruptPrevious))
            return;

        // 2. Si JAWS está en ejecución y responde por su API COM nativa (FreedomScientific.JawsApi):
        if (TrySpeakViaJawsCom(text, interruptPrevious))
            return;

        // 3. Canal nativo universal de Windows (Microsoft UI Automation LiveRegionChanged):
        // Soportado nativamente por NVDA, JAWS y Narrador de Windows en WinUI 3 sin requerir DLLs externas.
        UiAutomationAnnouncer?.Invoke(text, immediateLiveRegion);
    }

    /// <summary>
    /// Emite un anuncio prioritario asertivo (Tarea 4) que interrumpe locuciones previas y notifica de inmediato.
    /// </summary>
    public void SpeakAssertive(string text)
    {
        Speak(text, interruptPrevious: true, immediateLiveRegion: true);
    }

    /// <summary>
    /// Detiene la locución activa si el lector de pantalla lo permite por API directa.
    /// </summary>
    public void Stop()
    {
        if (_nvdaClientDllAvailable)
        {
            try
            {
                if (nvdaController_testIfRunning() == 0)
                {
                    nvdaController_cancelSpeech();
                }
            }
            catch (DllNotFoundException)
            {
                _nvdaClientDllAvailable = false;
            }
            catch
            {
            }
        }

        if (_jawsComObject != null)
        {
            try
            {
                _jawsComObject.GetType().InvokeMember("StopSpeech", BindingFlags.InvokeMethod, null, _jawsComObject, null);
            }
            catch
            {
            }
        }
    }

    private bool TrySpeakViaNvdaController(string text, bool interrupt)
    {
        if (!_nvdaClientDllAvailable)
            return false;

        try
        {
            // Handshake RPC latched: tras una primera respuesta positiva se evita
            // el ping por cada locución (honestidad: si el latch está activo,
            // la locución por NVDA se intenta de forma directa).
            if (!(_nvdaControllerActive || nvdaController_testIfRunning() == 0))
                return false;

            _nvdaControllerActive = true;

            if (interrupt)
            {
                nvdaController_cancelSpeech();
            }
            return nvdaController_speakText(text) == 0;
        }
        catch (DllNotFoundException)
        {
            _nvdaClientDllAvailable = false;
        }
        catch (EntryPointNotFoundException)
        {
            _nvdaClientDllAvailable = false;
        }
        catch
        {
        }

        return false;
    }

    private bool TrySpeakViaJawsCom(string text, bool interrupt)
    {
        try
        {
            // La falla de vinculación COM (JAWS sin soporte de workspace COM moderno,
            // p. ej. licencia de pantalla inferior o DLL no registrada) se latching para
            // no repetir la creación del objeto COM en cada locución.
            if (_jawsFailed)
                return false;

            if (!_jawsDetected && Process.GetProcessesByName("jfw").Length == 0)
                return false;

            _jawsDetected = true;

            if (!_jawsInitialized)
            {
                _jawsInitialized = true;
                var jawsType = Type.GetTypeFromProgID("FreedomScientific.JawsApi");
                _jawsComObject = jawsType != null ? Activator.CreateInstance(jawsType) : null;
                if (_jawsComObject == null)
                {
                    _jawsFailed = true;
                    return false;
                }
            }

            if (_jawsComObject != null)
            {
                object? result = _jawsComObject.GetType().InvokeMember(
                    "SayString",
                    BindingFlags.InvokeMethod,
                    null,
                    _jawsComObject,
                    new object[] { text, interrupt });

                return result is true;
            }
        }
        catch (COMException)
        {
            _jawsFailed = true;
        }
        catch
        {
        }

        return false;
    }

    public void SetRate(int rate)
    {
        // El lector de pantalla del sistema (NVDA, JAWS o Narrador) controla directamente la velocidad del usuario.
    }

    public void SetVolume(int volume)
    {
        // El lector de pantalla del sistema controla directamente el volumen del usuario.
    }

    public void Dispose()
    {
        _jawsComObject = null;
    }
}
