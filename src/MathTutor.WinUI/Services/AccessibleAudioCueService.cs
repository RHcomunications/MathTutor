using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using MathTutor.Core.Models;

namespace MathTutor.WinUI.Services;

/// <summary>
/// Generador procedural de pistas sonoras armónicas y efectos de audio en memoria.
/// Proporciona retroalimentación auditiva inmediata sin depender de archivos de audio externos.
/// </summary>
public class AccessibleAudioCueService
{
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
    /// Reproduce una nota musical específica (utilizada para el conteo sensorial paso a paso).
    /// </summary>
    public void PlayNote(int frequencyHz, int durationMs = 250)
    {
        if (!_enabled || !_musicEnabled) return;
        Task.Run(() =>
        {
            try
            {
                using var wavStream = GenerateToneStream(frequencyHz, durationMs, 0.4);
                using var player = new SoundPlayer(wavStream);
                player.Play();
            }
            catch
            {
                // Manejo seguro si no hay dispositivo de audio
            }
        });
    }

    /// <summary>
    /// Reproduce el timbre correspondiente al objeto manipulable (campana, tambor, flauta o palmada).
    /// </summary>
    public void PlayItemCue(ManipulableItem item)
    {
        if (!_enabled || !_musicEnabled || item is null) return;
        Task.Run(() =>
        {
            try
            {
                using var wavStream = GenerateItemCueStream(item);
                using var player = new SoundPlayer(wavStream);
                player.PlaySync();
            }
            catch
            {
                // Manejo seguro si no hay dispositivo de audio
            }
        });
    }

    /// <summary>
    /// Reproduce en orden el timbre de cada elemento para anticipar un patrón o secuencia.
    /// </summary>
    public void PlayItemSequence(IReadOnlyList<ManipulableItem> items, int startDelayMs = 0)
    {
        if (!_enabled || !_musicEnabled || items is null || items.Count == 0) return;
        Task.Run(async () =>
        {
            try
            {
                if (startDelayMs > 0)
                {
                    await Task.Delay(startDelayMs);
                }

                foreach (var item in items)
                {
                    using var wavStream = GenerateItemCueStream(item);
                    using var player = new SoundPlayer(wavStream);
                    player.PlaySync();
                    await Task.Delay(60);
                }
            }
            catch
            {
                // Manejo seguro si no hay dispositivo de audio
            }
        });
    }

    /// <summary>
    /// Reproduce acorde armónico de felicitación (Do-Mi-Sol-Do ascendente rápido).
    /// </summary>
    public void PlaySuccessChord()
    {
        if (!_enabled || !_musicEnabled) return;
        Task.Run(async () =>
        {
            try
            {
                int[] chordNotes = { 523, 659, 784, 1046 }; // C5, E5, G5, C6
                foreach (var note in chordNotes)
                {
                    using var wavStream = GenerateToneStream(note, 130, 0.45);
                    using var player = new SoundPlayer(wavStream);
                    player.Play();
                    await Task.Delay(90);
                }
            }
            catch
            {
                // Fallback seguro
            }
        });
    }

    /// <summary>
    /// Reproduce un tono suave y cálido para ofrecer una pista o animar al alumno.
    /// </summary>
    public void PlayHintTone()
    {
        if (!_enabled || !_musicEnabled) return;
        Task.Run(async () =>
        {
            try
            {
                int[] hintNotes = { 440, 554 }; // A4, C#5
                foreach (var note in hintNotes)
                {
                    using var wavStream = GenerateToneStream(note, 160, 0.35);
                    using var player = new SoundPlayer(wavStream);
                    player.Play();
                    await Task.Delay(110);
                }
            }
            catch
            {
                // Fallback seguro
            }
        });
    }

    /// <summary>
    /// Reproduce un acorde diagnóstico descendente suave cuando una respuesta es incorrecta (Tarea 4).
    /// </summary>
    public void PlayErrorChord()
    {
        if (!_enabled || !_musicEnabled) return;
        Task.Run(async () =>
        {
            try
            {
                int[] errorNotes = { 392, 330 }; // G4 -> E4 (intervalo diagnóstico descendente)
                foreach (var note in errorNotes)
                {
                    using var wavStream = GenerateToneStream(note, 170, 0.38);
                    using var player = new SoundPlayer(wavStream);
                    player.Play();
                    await Task.Delay(120);
                }
            }
            catch
            {
                // Fallback seguro
            }
        });
    }

    /// <summary>
    /// Sonido sutil al mover el foco de teclado para orientación espacial.
    /// </summary>
    public void PlayNavigationClick()
    {
        if (!_enabled) return;
        Task.Run(() =>
        {
            try
            {
                using var wavStream = GenerateToneStream(880, 35, 0.15);
                using var player = new SoundPlayer(wavStream);
                player.Play();
            }
            catch
            {
                // Fallback silencioso
            }
        });
    }

    private static MemoryStream GenerateItemCueStream(ManipulableItem item)
    {
        int frequency = Math.Clamp(item.MusicalPitchHz, 120, 1200);
        return item.SoundKind switch
        {
            ManipulableSoundKind.Bell => GenerateTimbreStream(
                frequency, 500, 0.32,
                t => Math.Sin(2 * Math.PI * frequency * t) +
                     0.45 * Math.Sin(2 * Math.PI * frequency * 2.4 * t),
                2.2),
            ManipulableSoundKind.Drum => GenerateTimbreStream(
                Math.Clamp(frequency, 110, 220), 220, 0.5,
                t => Math.Sin(2 * Math.PI * (frequency + 40 * Math.Exp(-t * 28)) * t),
                4.5),
            ManipulableSoundKind.Flute => GenerateTimbreStream(
                frequency, 480, 0.28,
                t => Math.Sin(2 * Math.PI * frequency * t) +
                     0.12 * Math.Sin(2 * Math.PI * frequency * 2 * t),
                0.5),
            ManipulableSoundKind.Clap => GenerateTimbreStream(
                frequency, 140, 0.45,
                t => Math.Sin(2 * Math.PI * 873 * t) * Math.Sin(2 * Math.PI * 1487 * t) +
                     Math.Sin(2 * Math.PI * 2341 * t) * 0.5,
                7.0),
            _ => GenerateToneStream(frequency, 300, 0.4)
        };
    }

    private static MemoryStream GenerateTimbreStream(
        int frequencyHz,
        int durationMs,
        double volume,
        Func<double, double> wave,
        double decayPower)
    {
        const int sampleRate = 44100;
        int numSamples = sampleRate * durationMs / 1000;
        var samples = new short[numSamples];
        int fadeSamples = Math.Min(numSamples / 8, 441);
        double amplitude = short.MaxValue * Math.Clamp(volume, 0.05, 0.95);

        for (int i = 0; i < numSamples; i++)
        {
            double t = (double)i / sampleRate;
            double progress = (double)i / numSamples;
            double decay = Math.Pow(Math.Max(0, 1 - progress), decayPower);
            double envelope = Math.Min(1, (double)i / fadeSamples);
            double normalizedWave = Math.Clamp(wave(t) * 0.75, -1, 1);
            samples[i] = (short)(normalizedWave * amplitude * decay * envelope);
        }

        return BuildWavStream(samples, sampleRate);
    }

    /// <summary>
    /// Genera un MemoryStream con formato WAV PCM estándar a 44100Hz 16-bit Mono
    /// con envolvente (Fade-In y Fade-Out) para evitar clics o chasquidos acústicos.
    /// </summary>
    private static MemoryStream GenerateToneStream(int frequencyHz, int durationMs, double volume)
    {
        int sampleRate = 44100;
        int numSamples = (sampleRate * durationMs) / 1000;
        short[] samples = new short[numSamples];

        int fadeSamples = Math.Min(numSamples / 6, 441); // Pequeño fade in/out
        double amplitude = short.MaxValue * Math.Clamp(volume, 0.05, 0.95);

        for (int i = 0; i < numSamples; i++)
        {
            double t = (double)i / sampleRate;
            double wave = Math.Sin(2.0 * Math.PI * frequencyHz * t);

            // Envolvente de volumen lineal para suavidad
            double envelope = 1.0;
            if (i < fadeSamples)
            {
                envelope = (double)i / fadeSamples;
            }
            else if (i > numSamples - fadeSamples)
            {
                envelope = (double)(numSamples - i) / fadeSamples;
            }

            samples[i] = (short)(wave * amplitude * envelope);
        }

        return BuildWavStream(samples, sampleRate);
    }

    private static MemoryStream BuildWavStream(short[] samples, int sampleRate)
    {
        int numSamples = samples.Length;
        var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true);

        // Cabecera RIFF / WAVE
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + (numSamples * 2));
        writer.Write("WAVE"u8.ToArray());

        // Subfragmento fmt
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);          // Tamaño del subfragmento fmt
        writer.Write((short)1);    // AudioFormat PCM = 1
        writer.Write((short)1);    // Canales: 1 (Mono)
        writer.Write(sampleRate);  // SampleRate: 44100
        writer.Write(sampleRate * 2); // ByteRate (SampleRate * Canales * BitsPorMuestra / 8)
        writer.Write((short)2);    // BlockAlign
        writer.Write((short)16);   // BitsPorMuestra

        // Subfragmento data
        writer.Write("data"u8.ToArray());
        writer.Write(numSamples * 2);
        for (int i = 0; i < numSamples; i++)
        {
            writer.Write(samples[i]);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }
}
