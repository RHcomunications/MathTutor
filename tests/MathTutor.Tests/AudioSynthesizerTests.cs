using System;
using System.Linq;
using MathTutor.Core.Models;
using MathTutor.Core.Services;
using Xunit;

namespace MathTutor.Tests;

public class AudioSynthesizerTests
{
    [Fact]
    public void GetNoteWav_ProducesWellFormedPcmWavContainer()
    {
        var wav = AudioSynthesizer.GetNoteWav(440, 250, 0.4);

        Assert.True(wav.Length >= 44);
        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(wav, 0, 4));
        Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(wav, 8, 4));
        Assert.Equal("fmt ", System.Text.Encoding.ASCII.GetString(wav, 12, 4));
        Assert.Equal("data", System.Text.Encoding.ASCII.GetString(wav, 36, 4));

        int pcmAudioFormat = BitConverter.ToInt16(wav, 20);
        int channelCount = BitConverter.ToInt16(wav, 22);
        int sampleRate = BitConverter.ToInt32(wav, 24);
        int bitsPerSample = BitConverter.ToInt16(wav, 34);
        int dataSize = BitConverter.ToInt32(wav, 40);

        Assert.Equal(1, pcmAudioFormat);
        Assert.Equal(1, channelCount);
        Assert.Equal(44100, sampleRate);
        Assert.Equal(16, bitsPerSample);
        Assert.Equal(44100 * 250 / 1000 * 2, dataSize);
        Assert.Equal(44 + dataSize, wav.Length);
    }

    [Fact]
    public void GetNoteWav_AppliesFadeEnvelope_AtBothEnds()
    {
        var wav = AudioSynthesizer.GetNoteWav(523, 250, 0.4);
        int firstSample = BitConverter.ToInt16(wav, 44);
        int lastSample = BitConverter.ToInt16(wav, wav.Length - 2);

        Assert.Equal(0, firstSample);
        // El último muestra puede conservar un residuo mínimo dentro del tramo de fade-out
        Assert.True(Math.Abs(lastSample) < 500, $"El fade-out no se aplicó: último muestra = {lastSample}");
    }

    [Fact]
    public void GetNoteWav_IsDeterministic_AndDistinguishesFrequencies()
    {
        var tone440 = AudioSynthesizer.GetNoteWav(440, 250, 0.4);
        var tone440Again = AudioSynthesizer.GetNoteWav(440, 250, 0.4);
        var tone659 = AudioSynthesizer.GetNoteWav(659, 250, 0.4);

        Assert.Equal(tone440, tone440Again);
        Assert.NotEqual(tone440, tone659);
    }

    [Fact]
    public void GetItemCueWav_DistinguishesTimbres_AndIsDeterministicPerKind()
    {
        var bell = AudioSynthesizer.GetItemCueWav(new ManipulableItem { Id = "b1", MusicalPitchHz = 440, SoundKind = ManipulableSoundKind.Bell });
        var bellAgain = AudioSynthesizer.GetItemCueWav(new ManipulableItem { Id = "b2", MusicalPitchHz = 440, SoundKind = ManipulableSoundKind.Bell });
        var flute = AudioSynthesizer.GetItemCueWav(new ManipulableItem { Id = "f1", MusicalPitchHz = 440, SoundKind = ManipulableSoundKind.Flute });

        Assert.Equal(bell, bellAgain);
        Assert.NotEqual(bell, flute);
    }

    [Fact]
    public void GetItemCueWav_ClampsExtremeFrequencies()
    {
        var tooLow = AudioSynthesizer.GetItemCueWav(new ManipulableItem { Id = "x", MusicalPitchHz = 10, SoundKind = ManipulableSoundKind.Flute });
        var clampedLow = AudioSynthesizer.GetItemCueWav(new ManipulableItem { Id = "y", MusicalPitchHz = 120, SoundKind = ManipulableSoundKind.Flute });

        Assert.Equal(clampedLow, tooLow);
    }

    [Fact]
    public void GetItemCueWav_FabricUsesItemFrequencyOnlyAsAmplitudeCarrier()
    {
        // La palmada es un timbre percusivo neutral: su espectro no depende de
        // la frecuencia de nota, así dos palmas suenan igual de "palma".
        var clapLow = AudioSynthesizer.GetItemCueWav(new ManipulableItem { Id = "c1", MusicalPitchHz = 300, SoundKind = ManipulableSoundKind.Clap });
        var clapHigh = AudioSynthesizer.GetItemCueWav(new ManipulableItem { Id = "c2", MusicalPitchHz = 880, SoundKind = ManipulableSoundKind.Clap });

        Assert.Equal(clapLow, clapHigh);
    }
}
