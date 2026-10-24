using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using MathTutor.Core.Models;
using MathTutor.Core.Services;

namespace MathTutor.WinUI.Services;

/// <summary>
/// Reproductor de pistas sonoras accesibles generadas proceduralmente por
/// <see cref="AudioSynthesizer"/> (Core). Todas las señales pasan por un único
/// canal de reproducción serializado de latencia cero:
/// - Reutiliza una instancia única de SoundPlayer para no reiniciar el driver de audio en cada nota.
/// - Cancela y purga el audio activo al cambiar de pantalla o lección.
/// - Deduplica sonidos de navegación para evitar colas rezagadas al pulsar flechas rápidamente.
/// - Despierta al hilo de fondo de inmediato con Monitor.Pulse.
/// </summary>
public class AccessibleAudioCueService
{
    private const int MaxQueuedCues = 32;

    private static readonly object ChannelGate = new();
    private static readonly Queue<CueSpec> Channel = new();
    private static readonly SoundPlayer ReusablePlayer = new();
    private static bool _workerRunning;

    private bool _enabled = true;
    private bool _musicEnabled = true;

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public bool MusicEnabled
    {
        get => _musicEnabled;
        set => _musicEnabled = value;
    }

    /// <summary>
    /// Descarta las pistas aún no reproducidas e interrumpe de inmediato cualquier nota
    /// en curso (p. ej. al cambiar de pantalla o volver al menú).
    /// </summary>
    public void ClearPendingCues()
    {
        lock (ChannelGate)
        {
            Channel.Clear();
            try
            {
                ReusablePlayer.Stop();
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// Reproduce una nota musical específica (utilizada para el conteo sensorial paso a paso).
    /// </summary>
    public void PlayNote(int frequencyHz, int durationMs = 250)
    {
        if (!_enabled || !_musicEnabled) return;
        Enqueue(new CueSpec(
            AudioSynthesizer.GetNoteWav(frequencyHz, durationMs, 0.4),
            gapMs: 0));
    }

    /// <summary>
    /// Reproduce el timbre correspondiente al objeto manipulable (campana, tambor, flauta o palmada).
    /// </summary>
    public void PlayItemCue(ManipulableItem item)
    {
        if (!_enabled || !_musicEnabled || item is null) return;
        Enqueue(new CueSpec(AudioSynthesizer.GetItemCueWav(item), gapMs: 0));
    }

    /// <summary>
    /// Reproduce en orden el timbre de cada elemento para anticipar un patrón o secuencia.
    /// </summary>
    public void PlayItemSequence(IReadOnlyList<ManipulableItem> items, int startDelayMs = 0)
    {
        if (!_enabled || !_musicEnabled || items is null || items.Count == 0) return;

        ClearPendingCues();

        if (startDelayMs > 0)
        {
            Enqueue(new CueSpec(SilenceWav(Math.Min(startDelayMs, 3000)), gapMs: 0));
        }

        foreach (var item in items)
        {
            Enqueue(new CueSpec(AudioSynthesizer.GetItemCueWav(item), gapMs: 30));
        }
    }

    /// <summary>
    /// Reproduce acorde armónico de felicitación (Do-Mi-Sol-Do ascendente ágil y sin demoras).
    /// </summary>
    public void PlaySuccessChord()
    {
        if (!_enabled || !_musicEnabled) return;
        int[] chordNotes = { 523, 659, 784, 1046 }; // C5, E5, G5, C6
        foreach (var note in chordNotes)
        {
            Enqueue(new CueSpec(AudioSynthesizer.GetNoteWav(note, 80, 0.45), gapMs: 25));
        }
    }

    /// <summary>
    /// Reproduce un tono suave y cálido para ofrecer una pista o animar al alumno.
    /// </summary>
    public void PlayHintTone()
    {
        if (!_enabled || !_musicEnabled) return;
        int[] hintNotes = { 440, 554 }; // A4, C#5
        foreach (var note in hintNotes)
        {
            Enqueue(new CueSpec(AudioSynthesizer.GetNoteWav(note, 90, 0.35), gapMs: 30));
        }
    }

    /// <summary>
    /// Reproduce un acorde diagnóstico descendente suave cuando una respuesta es incorrecta.
    /// </summary>
    public void PlayErrorChord()
    {
        if (!_enabled || !_musicEnabled) return;
        int[] errorNotes = { 392, 330 }; // G4 -> E4
        foreach (var note in errorNotes)
        {
            Enqueue(new CueSpec(AudioSynthesizer.GetNoteWav(note, 100, 0.38), gapMs: 35));
        }
    }

    /// <summary>
    /// Sonido sutil al mover el foco de teclado para orientación espacial.
    /// Deduplica clics si el usuario navega rápidamente con las flechas.
    /// </summary>
    public void PlayNavigationClick()
    {
        if (!_enabled) return;
        lock (ChannelGate)
        {
            // Si ya hay un clic de navegación pendiente, no acumular más
            if (Channel.Any(c => c.IsNavigation))
                return;
        }

        Enqueue(new CueSpec(AudioSynthesizer.GetNoteWav(880, 25, 0.15), gapMs: 0, isNavigation: true));
    }

    private static void Enqueue(CueSpec cue)
    {
        lock (ChannelGate)
        {
            if (Channel.Count >= MaxQueuedCues)
            {
                Channel.Dequeue();
            }

            Channel.Enqueue(cue);
            if (!_workerRunning)
            {
                _workerRunning = true;
                var worker = new Thread(PlaybackLoop)
                {
                    IsBackground = true,
                    Name = "AccessibleAudioCueService.Playback"
                };
                worker.Start();
            }
            else
            {
                Monitor.Pulse(ChannelGate);
            }
        }
    }

    private static void PlaybackLoop()
    {
        while (true)
        {
            CueSpec cue;
            lock (ChannelGate)
            {
                while (Channel.Count == 0)
                {
                    Monitor.Wait(ChannelGate, 15000);
                    if (Channel.Count == 0)
                    {
                        _workerRunning = false;
                        return;
                    }
                }

                cue = Channel.Dequeue();
            }

            try
            {
                if (cue.GapMs > 0 || cue.WavBytes.Length == 0)
                {
                    Thread.Sleep(cue.GapMs);
                }

                using var wavStream = new MemoryStream(cue.WavBytes, writable: false);
                ReusablePlayer.Stream = wavStream;
                ReusablePlayer.PlaySync();
            }
            catch
            {
                // Manejo seguro si no hay dispositivo de audio disponible
            }
        }
    }

    private static byte[] SilenceWav(int durationMs)
    {
        int numSamples = Math.Max(1, 44100 * durationMs / 1000);
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: false);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + (numSamples * 2));
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(44100);
        writer.Write(44100 * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8.ToArray());
        writer.Write(numSamples * 2);
        for (int i = 0; i < numSamples; i++)
        {
            writer.Write((short)0);
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    private readonly struct CueSpec
    {
        public CueSpec(byte[] wavBytes, int gapMs, bool isNavigation = false)
        {
            WavBytes = wavBytes;
            GapMs = gapMs;
            IsNavigation = isNavigation;
        }

        public byte[] WavBytes { get; }
        public int GapMs { get; }
        public bool IsNavigation { get; }
    }
}
