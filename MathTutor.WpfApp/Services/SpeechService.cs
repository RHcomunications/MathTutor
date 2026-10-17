using System;
using System.Linq;
using System.Speech.Synthesis;

namespace MathTutor.WpfApp.Services;

/// <summary>
/// Servicio de síntesis de voz (TTS) para lectura de pantalla integrada.
/// Permite reproducir voz en tiempo real con control de interrupciones, velocidad y volumen.
/// </summary>
public class SpeechService : IDisposable
{
    private SpeechSynthesizer? _synthesizer;
    private bool _enabled = true;
    private readonly object _lock = new();

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (!_enabled)
            {
                Stop();
            }
        }
    }

    public SpeechService()
    {
        InitializeSynthesizer();
    }

    private void InitializeSynthesizer()
    {
        try
        {
            _synthesizer = new SpeechSynthesizer();

            // Buscar voz en español instalada en Windows (ej. Sabina, Helena, Laura, Raul, etc.)
            var spanishVoice = _synthesizer.GetInstalledVoices()
                .FirstOrDefault(v => v.Enabled && (v.VoiceInfo.Culture.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase) ||
                                                    v.VoiceInfo.Culture.Name.StartsWith("es-", StringComparison.OrdinalIgnoreCase)));

            if (spanishVoice != null)
            {
                _synthesizer.SelectVoice(spanishVoice.VoiceInfo.Name);
            }

            _synthesizer.Rate = -1; // Tono y ritmo amigable para niños pequeños
            _synthesizer.Volume = 100;
        }
        catch
        {
            _synthesizer = null;
        }
    }

    /// <summary>
    /// Lee un texto en voz alta. Si interruptPrevious es true, cancela cualquier lectura previa.
    /// </summary>
    public void Speak(string text, bool interruptPrevious = true)
    {
        if (!_enabled || _synthesizer == null || string.IsNullOrWhiteSpace(text))
            return;

        lock (_lock)
        {
            try
            {
                if (interruptPrevious)
                {
                    _synthesizer.SpeakAsyncCancelAll();
                }

                _synthesizer.SpeakAsync(text);
            }
            catch
            {
                // Manejo de excepción silencioso para evitar fallos de UI
            }
        }
    }

    /// <summary>
    /// Cancela de inmediato cualquier locución en curso.
    /// </summary>
    public void Stop()
    {
        if (_synthesizer == null) return;
        lock (_lock)
        {
            try
            {
                _synthesizer.SpeakAsyncCancelAll();
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// Ajusta la velocidad de habla (-10 a 10).
    /// </summary>
    public void SetRate(int rate)
    {
        if (_synthesizer == null) return;
        try
        {
            _synthesizer.Rate = Math.Clamp(rate, -10, 10);
        }
        catch
        {
        }
    }

    /// <summary>
    /// Ajusta el volumen de habla (0 a 100).
    /// </summary>
    public void SetVolume(int volume)
    {
        if (_synthesizer == null) return;
        try
        {
            _synthesizer.Volume = Math.Clamp(volume, 0, 100);
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        try
        {
            _synthesizer?.Dispose();
        }
        catch
        {
        }
        _synthesizer = null;
    }
}
