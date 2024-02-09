using System.Collections.Generic;
using LSCore.ConfigModule;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MusicEventSystem.Configs;
using UnityEditor;
using UnityEngine;
using static SoundventTypes;

namespace MusicEventSystem.Editor
{
    public class MidiToJsonConverterWindow : EditorWindow
    {
        private string jsonName;
        private string[] soundvents;
        private Object midiFile;

        private Dictionary<FourBitNumber, (string trackName, List<LongNoteData> notes, bool isShort)> tracks = new();

        [MenuItem("Window/MIDI to Json Converter")]
        private static void ShowWindow()
        {
            var window = GetWindow<MidiToJsonConverterWindow>();
            window.titleContent = new GUIContent("MIDI to Json Converter");
            window.Show();
        }

        private void OnEnable()
        {
            midiFile = null;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                var tempMidiFile = EditorGUILayout.ObjectField(midiFile, typeof(Object), true);

                if (tempMidiFile == null)
                {
                    EditorGUILayout.EndVertical();
                    return;
                }

                var path = AssetDatabase.GetAssetPath(tempMidiFile);
                MidiFile midi;

                if (path.EndsWith(".mid") == false)
                {
                    EditorGUILayout.EndVertical();
                    return;
                }
                
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Json Name");
                    jsonName = tempMidiFile.name;
                    jsonName = GUILayout.TextField(jsonName);
                    MusicData.Editor_SetMusicName(jsonName);
                }
                EditorGUILayout.EndHorizontal();
                
                TempoMap tempo;

                if (!Equals(tempMidiFile, midiFile))
                {
                    midiFile = tempMidiFile;
                    tracks.Clear();
                    GetMidiProperties(path, out midi, out tempo);
                        
                    var nameIndex = 0;

                    foreach (var channel in midi.GetChannels())
                    {
                        var trackName = $"Channel_{channel}";
                        tracks.Add(channel, (GUILayout.TextField(trackName), new List<LongNoteData>(), true));
                        nameIndex++;
                    }
                    
                    var soundventsList = new List<string>();
                    soundventsList.AddRange(Sounds);
                    soundventsList.AddRange(Enemies);
                    soundvents = soundventsList.ToArray();
                }
                else
                {
                    GetMidiProperties(path, out midi, out tempo);
                    var tempTracks = new List<(FourBitNumber channel, string trackName, List<LongNoteData> notes, bool isShort)>();
                    var style = new GUIStyle() {alignment = TextAnchor.MiddleCenter};
                    style.normal.textColor = Color.white;
                    style.fontSize = 20;
                    style.richText = true;
                    
                    GUILayout.Label($"<b>Tracks</b>", style);

                    foreach (var (channel, track) in tracks)
                    {
                        var soundventType = soundvents[channel];
                        EditorGUILayout.LabelField($"Track {channel}", soundventType);
                        var attribute = GroupByName[soundventType];
                        var isShort = attribute.isShort;
                        tempTracks.Add((channel, soundventType, track.notes, isShort));
                    }

                    for (int i = 0; i < tempTracks.Count; i++)
                    {
                        var track = tempTracks[i];
                        track.notes.Clear();
                        tracks[track.channel] = (track.trackName, track.notes, track.isShort);
                    }
                }
                
                if (midi != null)
                {
                    EditorGUILayout.Space(10);
                        
                    if (GUILayout.Button("Convert to JSON"))
                    {
                        foreach (var note in midi.GetNotes())
                        {
                            float time = note.TimeAs<MetricTimeSpan>(tempo).TotalMicroseconds / 1000000f;
                            float length = note.LengthAs<MetricTimeSpan>(tempo).TotalMicroseconds / 1000000f;
                            tracks[note.Channel].notes.Add(new LongNoteData(time, time + length));
                        }

                        foreach (var (name, notes, isShort) in tracks.Values)
                        {
                            if (isShort)
                            {
                                var shortNotes = new ShortNoteData[notes.Count];

                                for (int i = 0; i < shortNotes.Length; i++)
                                {
                                    shortNotes[i] = new ShortNoteData(notes[i].startTime);
                                }

                                MusicData.ShortTrackData[name] = new ShortNoteTrackData(name, shortNotes);
                            }
                            else
                            {
                                MusicData.LongTrackData[name] = new LongNoteTrackData(name, notes.ToArray());
                            }
                        }

                        var bpm = midi.GetTempoMap().Tempo.AtTime(1000).BeatsPerMinute;
                        MusicData.Config.bpmStep = 60f / bpm;
                        //ConfigUtils.Save<MusicData>(); //TODO: Refactor
                        AssetDatabase.Refresh();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void GetMidiProperties(string path, out MidiFile midi, out TempoMap tempo)
        {
            midi = MidiFile.Read(path);
            tempo = midi.GetTempoMap();
        }
    }
}