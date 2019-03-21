using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PCG.Terrain.Common.Extensions;
using PCG.Terrain.Core.DataTypes;
using PCG.Terrain.Settings;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PCG.Terrain.Core
{
    public sealed class TerrainController : MonoBehaviour
    {
        #region DefaultRangeValues

        private const int ThrottleOverallChangeSliderValue = 250;

        // ReSharper disable once InconsistentNaming
        private const int ThrottlePanelUI = 500;

        #endregion

        #region SerializeField

#pragma warning disable 649
        [SerializeField] private KeyCode toggleActiveKeyCode = KeyCode.F1;
        [SerializeField, Required] private TerrainSettings terrainSettings;
#if UNITY_EDITOR
        // ReSharper disable once InconsistentNaming
        [SerializeField, BoxGroup("UI")] private bool toggleUI;
#endif
        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue resolutionUI;

        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue chunkCountUI;

        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue chunksPerFrameUI;

        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue chunkThresholdUI;

        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue noiseFrequencyUI;

        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue noiseOctavecUI;

        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue noiseLacunarityUI;

        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue noisePersistanceUI;

        [SerializeField, BoxGroup("UI")
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        // ReSharper disable once InconsistentNaming
        private SliderValue noiseAmplitudeUI;

        [SerializeField, BoxGroup("UI"), Required
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        private Toggle autoUpdateToggle;

        [SerializeField, BoxGroup("UI"), Required
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        private Button generateButton;

        [SerializeField, BoxGroup("UI"), Required
#if UNITY_EDITOR
         , ShowIf(nameof(toggleUI))
#endif
        ]
        private GameObject optionsPanel;
#pragma warning restore 649

        #endregion

        public TerrainSettings TerrainSettings => terrainSettings;

        private bool _activeOptionsPanel = true;
        private IObservable<IEnumerable<SliderValueMessage>> _allSliderValueObservable;
        private IDisposable _autoUpdateSubscription;

        private void Awake()
        {
            #region LocalFunctions

            void SliderValueValidation()
            {
                chunkCountUI.Slider.onValueChanged.AsObservable().Subscribe(
                    value => { chunksPerFrameUI.Slider.maxValue = value; }
                );
            }

            void DefaultSliderValueSettings()
            {
                resolutionUI.OnValueChangedSetText(
                    0, TerrainSettings.PossibleResolutions.Length - 1,
                    value => TerrainSettings.PossibleResolutions[(int) value].ToString(),
                    true);
                chunkCountUI.OnValueChangedSetText(
                    TerrainSettings.MinChunkCount, TerrainSettings.MaxChunkCount,
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    value => value.ToString(),
                    true);
                chunksPerFrameUI.OnValueChangedSetText(
                    TerrainSettings.MinChunksPerFrame, TerrainSettings.MaxChunksPerFrame,
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    value => value.ToString(),
                    true);
                chunkThresholdUI.OnValueChangedSetText(
                    TerrainSettings.MinThreshold, TerrainSettings.MaxThreshold,
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    value => value.ToString(),
                    true);
                noiseFrequencyUI.OnValueChangedSetText(
                    NoiseSettings.MinFrequency, NoiseSettings.MaxFrequency,
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    value => value.ToString()
                );
                noiseOctavecUI.OnValueChangedSetText(
                    NoiseSettings.MinOctaves, NoiseSettings.MaxOctaves,
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    value => value.ToString(),
                    true
                );
                noiseLacunarityUI.OnValueChangedSetText(
                    NoiseSettings.MinLacunarity, NoiseSettings.MaxLacunarity,
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    value => value.ToString()
                );
                noisePersistanceUI.OnValueChangedSetText(
                    NoiseSettings.MinPersistence, NoiseSettings.MaxPersistence,
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    value => value.ToString()
                );
                noiseAmplitudeUI.OnValueChangedSetText(
                    NoiseSettings.MinAmplitude, NoiseSettings.MaxAmplitude,
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    value => value.ToString()
                );
            }

            void LoadDefaultTerrainSettings()
            {
                resolutionUI.SetValue(Array.IndexOf(TerrainSettings.PossibleResolutions, terrainSettings.Resolution));
                chunkCountUI.SetValue(terrainSettings.ChunkCount);
                chunksPerFrameUI.SetValue(terrainSettings.ChunksPerFrame);
                chunkThresholdUI.SetValue(terrainSettings.ChangeThreshold);
                noiseFrequencyUI.SetValue(terrainSettings.NoiseSettings.Frequency);
                noiseOctavecUI.SetValue(terrainSettings.NoiseSettings.Octaves);
                noiseLacunarityUI.SetValue(terrainSettings.NoiseSettings.Lacunarity);
                noisePersistanceUI.SetValue(terrainSettings.NoiseSettings.Persistence);
                noiseAmplitudeUI.SetValue(terrainSettings.NoiseSettings.Amplitude);
            }

            void ObservableSubscribe()
            {
                #region HidePanelUI

                Observable.EveryUpdate()
                    .Where(_ => Input.GetKeyDown(toggleActiveKeyCode))
                    .ThrottleFirst(TimeSpan.FromMilliseconds(ThrottlePanelUI))
                    .Subscribe(_ => ToggleActive());

                #endregion

                #region AllSliderValue

                _allSliderValueObservable = Observable.Merge(
                        resolutionUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = TerrainSettings.PossibleResolutions[(int) value],
                                SliderValueCode = SliderValueCode.Resolution
                            }
                        ),
                        chunkCountUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = value, SliderValueCode = SliderValueCode.ChunkCount
                            }
                        ),
                        chunksPerFrameUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = value, SliderValueCode = SliderValueCode.ChunksPerFrame
                            }
                        ),
                        chunkThresholdUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = value, SliderValueCode = SliderValueCode.ChunkThreshold
                            }
                        ),
                        noiseFrequencyUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = value, SliderValueCode = SliderValueCode.NoiseFrequency
                            }
                        ),
                        noiseOctavecUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = value, SliderValueCode = SliderValueCode.NoiseOctavec
                            }
                        ),
                        noiseLacunarityUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = value, SliderValueCode = SliderValueCode.NoiseLacunarity
                            }
                        ),
                        noisePersistanceUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = value, SliderValueCode = SliderValueCode.NoisePersistance
                            }
                        ),
                        noiseAmplitudeUI.Slider.AsObservable(
                            value => new SliderValueMessage
                            {
                                Value = value, SliderValueCode = SliderValueCode.NoiseAmplitude
                            }
                        )
                    ).Buffer(TimeSpan.FromMilliseconds(ThrottleOverallChangeSliderValue))
                    .Where(messages => messages.Count > 0)
                    .Select(
                        messages => messages.GroupBy(message => message.SliderValueCode).Select(x => x.Last())
                    );

                _allSliderValueObservable.Subscribe(HandleChangedSliderValue);

                #endregion

                #region AutoUpdateToggle

                var autoUpdateObservable = autoUpdateToggle.onValueChanged.AsObservable();
                autoUpdateObservable.Select(value => !value).SubscribeToInteractable(generateButton);
                autoUpdateObservable.Subscribe(value =>
                {
                    if (value)
                    {
                        _autoUpdateSubscription =
                            _allSliderValueObservable.Subscribe(_ => LoadNewSystemConfiguration());
                    }
                    else
                    {
                        _autoUpdateSubscription?.Dispose();
                    }
                });

                #endregion

                generateButton.onClick.AsObservable().Subscribe(_ => LoadNewSystemConfiguration());
            }

            #endregion

            SliderValueValidation();
            DefaultSliderValueSettings();
            LoadDefaultTerrainSettings();
            ObservableSubscribe();
        }

        private void ToggleActive()
        {
            if (_activeOptionsPanel)
            {
                _activeOptionsPanel = false;
                optionsPanel.SetActive(_activeOptionsPanel);
            }
            else
            {
                _activeOptionsPanel = true;
                optionsPanel.SetActive(_activeOptionsPanel);
            }
        }

        private void HandleChangedSliderValue(IEnumerable<SliderValueMessage> messages)
        {
            #region HandleSingleItem

            void HandleSingleItem(SliderValueMessage message)
            {
                switch (message.SliderValueCode)
                {
                    case SliderValueCode.Resolution:
                    {
                        terrainSettings.Resolution = (int) message.Value;
                        break;
                    }
                    case SliderValueCode.ChunkCount:
                    {
                        terrainSettings.ChunkCount = (int) message.Value;
                        break;
                    }
                    case SliderValueCode.ChunksPerFrame:
                    {
                        terrainSettings.ChunksPerFrame = (int) message.Value;
                        break;
                    }
                    case SliderValueCode.ChunkThreshold:
                    {
                        terrainSettings.ChangeThreshold = (int) message.Value;
                        break;
                    }
                    case SliderValueCode.NoiseFrequency:
                    {
                        terrainSettings.NoiseSettingsImpostor.frequency = message.Value;
                        break;
                    }
                    case SliderValueCode.NoiseOctavec:
                    {
                        terrainSettings.NoiseSettingsImpostor.octaves = (int) message.Value;
                        break;
                    }
                    case SliderValueCode.NoiseLacunarity:
                    {
                        terrainSettings.NoiseSettingsImpostor.lacunarity = message.Value;
                        break;
                    }
                    case SliderValueCode.NoisePersistance:
                    {
                        terrainSettings.NoiseSettingsImpostor.persistence = message.Value;
                        break;
                    }
                    case SliderValueCode.NoiseAmplitude:
                    {
                        terrainSettings.NoiseSettingsImpostor.amplitude = message.Value;
                        break;
                    }
                    default:
                    {
                        throw new Exception($"Unsupported {nameof(SliderValueMessage)} -> {message.SliderValueCode}");
                    }
                }
            }

            #endregion

            using (var message = messages.GetEnumerator())
            {
                while (message.MoveNext())
                {
                    HandleSingleItem(message.Current);
                }
            }
        }

        private void LoadNewSystemConfiguration()
        {
            Bootstrap.LoadNewConfiguration(terrainSettings);
        }
    }
}