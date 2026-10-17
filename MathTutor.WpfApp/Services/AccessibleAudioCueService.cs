using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace MathTutor.WpfApp.Services;

/// <summary>
/// Generador procedural de pistas sonoras armónicas y efectos de audio en memoria.
/// Proporciona retroalimentación auditiva inmediata sin depender de archivos de audio externos.
/// </summary>
public class AccessibleAudioCueService
{
    private bool _enabled = true;

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Reproduce una nota musical específica (utilizada para el conteo sensorial paso a paso).
    /// </summary>
    public void PlayNote(int frequencyHz, int durationMs = 250)
    {
        if (!_enabled) return;
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
    /// Reproduce acorde armónico de felicitación (Do-Mi-Sol-Do ascendente rápido).
    /// </summary>
    public void PlaySuccessChord()
    {
        if (!_enabled) return;
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
        if (!_enabled) return;
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
