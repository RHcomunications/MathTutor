using System;
using System.Collections.Generic;
using System.IO;
using MathTutor.Core.Models;

namespace MathTutor.Core.Services;

/// <summary>
/// Sintetizador procedural de pistas sonoras accesibles en formato WAV PCM
/// (44100 Hz, 16-bit, mono) con envolvente de atenuación para evitar chasquidos.
/// La síntesis es determinista y se sirve desde una caché acotada para no
/// regenerar buffers idénticos en cada disparo.
/// </summary>
public static class AudioSynthesizer
{
    private const int SampleRate = 44100;

    private static readonly object CacheGate = new();
    private static readonly Dictionary<string, byte[]> Cache = new();

    /// <summary>
    /// Devuelve (generando y memorizando en caché) un tono sinusoidal limpio.
    /// </summary>
    public static byte[] GetNoteWav(int frequencyHz, int durationMs, double volume)
    {
        return GetFromCache($"tone|{frequencyHz}|{durationMs}|{volume:0.000}", () => SynthesizeTone(frequencyHz, durationMs, volume));
    }

    /// <summary>
    /// Devuelve (generando y memorizando en caché) el timbre del objeto
    /// manipulable según su tipo de sonido (campana, tambor, flauta o palmada).
    /// </summary>
    public static byte[] GetItemCueWav(ManipulableItem item)
    {
        int frequency = Math.Clamp(item.MusicalPitchHz, 120, 1200);
        return item.SoundKind switch
        {
            ManipulableSoundKind.Bell => GetFromCache($"bell|{frequency}", () => SynthesizeTimbre(
                frequency, 500, 0.32,
                t => Math.Sin(2 * Math.PI * frequency * t) +
                     0.45 * Math.Sin(2 * Math.PI * frequency * 2.4 * t),
                2.2)),
            ManipulableSoundKind.Drum => GetFromCache($"drum|{frequency}", () => SynthesizeTimbre(
                Math.Clamp(frequency, 110, 220), 220, 0.5,
                t => Math.Sin(2 * Math.PI * (frequency + 40 * Math.Exp(-t * 28)) * t),
                4.5)),
            ManipulableSoundKind.Flute => GetFromCache($"flute|{frequency}", () => SynthesizeTimbre(
                frequency, 480, 0.28,
                t => Math.Sin(2 * Math.PI * frequency * t) +
                     0.12 * Math.Sin(2 * Math.PI * frequency * 2 * t),
                0.5)),
            ManipulableSoundKind.Clap => GetFromCache($"clap|{frequency}", () => SynthesizeTimbre(
                frequency, 140, 0.45,
                t => Math.Sin(2 * Math.PI * 873 * t) * Math.Sin(2 * Math.PI * 1487 * t) +
                     Math.Sin(2 * Math.PI * 2341 * t) * 0.5,
                7.0)),
            _ => GetFromCache($"plain|{frequency}", () => SynthesizeTone(frequency, 300, 0.4))
        };
    }

    /// <summary>
    /// Vuelca la caché de buffers (acotación simple; se regeneran bajo demanda).
    /// </summary>
    internal static void ClearCache()
    {
        lock (CacheGate)
        {
            Cache.Clear();
        }
    }

    private static byte[] GetFromCache(string key, Func<byte[]> factory)
    {
        lock (CacheGate)
        {
            var cached = Cache.GetValueOrDefault(key);
            if (cached != null)
            {
                return cached;
            }

            var bytes = factory();

            // Acotación: si la caché crece demasiado, se reinicia desde cero.
            if (Cache.Count >= 96)
            {
                Cache.Clear();
            }

            Cache[key] = bytes;
            return bytes;
        }
    }

    /// <summary>
    /// Genera un tono sinusoidal con fade-in/fade-out lineal para evitar chasquidos.
    /// </summary>
    private static byte[] SynthesizeTone(int frequencyHz, int durationMs, double volume)
    {
        int numSamples = (SampleRate * durationMs) / 1000;
        var samples = new short[numSamples];

        int fadeSamples = Math.Min(numSamples / 6, 441);
        double amplitude = short.MaxValue * Math.Clamp(volume, 0.05, 0.95);

        for (int i = 0; i < numSamples; i++)
        {
            double t = (double)i / SampleRate;
            double wave = Math.Sin(2.0 * Math.PI * frequencyHz * t);

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

        return BuildWavBytes(samples, SampleRate);
    }

    private static byte[] SynthesizeTimbre(
        int frequencyHz,
        int durationMs,
        double volume,
        Func<double, double> wave,
        double decayPower)
    {
        int numSamples = SampleRate * durationMs / 1000;
        var samples = new short[numSamples];
        int fadeSamples = Math.Min(numSamples / 8, 441);
        double amplitude = short.MaxValue * Math.Clamp(volume, 0.05, 0.95);

        for (int i = 0; i < numSamples; i++)
        {
            double t = (double)i / SampleRate;
            double progress = (double)i / numSamples;
            double decay = Math.Pow(Math.Max(0, 1 - progress), decayPower);
            double envelope = Math.Min(1, (double)i / fadeSamples);
            double normalizedWave = Math.Clamp(wave(t) * 0.75, -1, 1);
            samples[i] = (short)(normalizedWave * amplitude * decay * envelope);
        }

        return BuildWavBytes(samples, SampleRate);
    }

    /// <summary>
    /// Empaqueta muestras PCM 16-bit mono en un contenedor WAV RIFF estándar.
    /// </summary>
    private static byte[] BuildWavBytes(short[] samples, int sampleRate)
    {
        int numSamples = samples.Length;
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: false);

        // Cabecera RIFF / WAVE
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + (numSamples * 2));
        writer.Write("WAVE"u8.ToArray());

        // Subfragmento fmt
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);    // AudioFormat PCM = 1
        writer.Write((short)1);    // Canales: 1 (Mono)
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2); // ByteRate (44100 * Mono * 16-bit / 8)
        writer.Write((short)2);    // BlockAlign
        writer.Write((short)16);   // BitsPorMuestra

        // Subfragmento data
        writer.Write("data"u8.ToArray());
        writer.Write(numSamples * 2);
        for (int i = 0; i < numSamples; i++)
        {
            writer.Write(samples[i]);
        }

        writer.Flush();
        return memoryStream.ToArray();
    }
}
