using System;
using System.Diagnostics;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.Core.Services;
using Xunit;

namespace MathTutor.Tests;

public class LoadAndPerformanceTests
{
    [Fact]
    public void AudioSynthesizer_GeneratesNotesAndTimbres_WithSubMillisecondLatency()
    {
        // 1. Latencia de síntesis en frío
        var sw = Stopwatch.StartNew();
        byte[] noteCold = AudioSynthesizer.GetNoteWav(440, 250, 0.4);
        sw.Stop();
        double coldMs = sw.Elapsed.TotalMilliseconds;

        Assert.NotNull(noteCold);
        Assert.True(noteCold.Length > 44, "El archivo WAV debe contener datos de audio válidos");

        // 2. Latencia de acceso en caliente (caché)
        sw.Restart();
        byte[] noteWarm = AudioSynthesizer.GetNoteWav(440, 250, 0.4);
        sw.Stop();
        double warmMs = sw.Elapsed.TotalMilliseconds;

        Assert.Same(noteCold, noteWarm); // La caché devuelve exactamente la misma referencia en memoria
        Assert.True(warmMs < 1.0, $"El acceso a la caché de audio debe ser sub-milisegundo (fue {warmMs:F3} ms)");

        // 3. Síntesis de timbres orquestales
        var bellItem = new ManipulableItem { SoundKind = ManipulableSoundKind.Bell, MusicalPitchHz = 523 };
        sw.Restart();
        byte[] bellWav = AudioSynthesizer.GetItemCueWav(bellItem);
        sw.Stop();
        Assert.NotNull(bellWav);
        Assert.True(sw.Elapsed.TotalMilliseconds < 25.0, $"La síntesis de campana debe tardar menos de 25 ms (fue {sw.Elapsed.TotalMilliseconds:F2} ms)");
    }

    [Fact]
    public void CurriculumService_LoadTimeAndMemoryUsage_AreWithinReasonableBounds()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        long memBefore = GC.GetTotalMemory(true);

        var sw = Stopwatch.StartNew();
        var service = new CurriculumService();
        var topics = service.GetAllTopics();
        sw.Stop();

        long memAfter = GC.GetTotalMemory(false);
        double deltaMb = (memAfter - memBefore) / (1024.0 * 1024.0);

        Assert.Equal(74, topics.Count);
        Assert.True(sw.ElapsedMilliseconds < 500, $"La instanciación del catálogo completo de 74 temas debe tomar menos de 500 ms (tomó {sw.ElapsedMilliseconds} ms)");
        Assert.True(deltaMb < 15.0, $"La carga del catálogo completo no debe superar 15 MB de Heap (fue {deltaMb:F2} MB)");
    }

    [Fact]
    public void ExerciseGenerator_ProceduralBatchGeneration_IsFastAndScalable()
    {
        var service = new CurriculumService();
        var generator = new ExerciseGenerator();
        var topics = service.GetAllTopics();

        var sw = Stopwatch.StartNew();
        int totalGenerated = 0;
        foreach (var topic in topics)
        {
            var exercises = generator.GenerateExercisesForTopic(topic.Id, new UserProfile { Age = topic.TargetAgeMin }, count: 4);
            totalGenerated += exercises.Count;
        }
        sw.Stop();

        Assert.Equal(74 * 4, totalGenerated);
        double avgPerExerciseMs = sw.Elapsed.TotalMilliseconds / totalGenerated;
        Assert.True(avgPerExerciseMs < 2.0, $"La generación procedimental debe promediar menos de 2 ms por ejercicio (fue {avgPerExerciseMs:F3} ms)");
    }
}
