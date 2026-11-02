using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Starlight_Decay
{
    /// <summary>Sound effects and the two looping music tracks.</summary>
    public static class Sfx
    {
        // The same sound can't restart more often than this, which stops stacked explosions clipping.
        private const float MinimumGap = 0.06f;
        private const float MusicVolume = 0.35f;

        private static readonly string[] Names =
        {
            "laser", "explosion", "start", "invi", "enemy_shot", "pickup", "powerup", "hit", "shield_hit",
            "bomb", "buy", "denied", "dash", "warning", "boss_die", "sector_clear", "beam", "ion"
        };

        private static readonly Dictionary<string, SoundEffect> Sounds = new Dictionary<string, SoundEffect>();
        private static readonly Dictionary<string, float> LastPlayed = new Dictionary<string, float>();
        private static SoundEffectInstance _sectorMusic;
        private static SoundEffectInstance _bossMusic;
        private static SoundEffectInstance _currentMusic;
        private static float _clock;

        public static bool Muted { get; private set; }

        public static void Load(ContentManager content)
        {
            foreach (string name in Names)
            {
                Sounds[name] = content.Load<SoundEffect>(name);
            }

            _sectorMusic = content.Load<SoundEffect>("music_sector").CreateInstance();
            _sectorMusic.IsLooped = true;
            _sectorMusic.Volume = MusicVolume;
            _bossMusic = content.Load<SoundEffect>("music_boss").CreateInstance();
            _bossMusic.IsLooped = true;
            _bossMusic.Volume = MusicVolume;
        }

        public static void Tick(float dt)
        {
            _clock += dt;
        }

        public static void Play(string name, float volume = 1f, float pitch = 0f)
        {
            if (Muted)
            {
                return;
            }
            if (LastPlayed.TryGetValue(name, out float last) && _clock - last < MinimumGap)
            {
                return;
            }
            LastPlayed[name] = _clock;
            Sounds[name].Play(volume, pitch, 0f);
        }

        public static void PlaySectorMusic()
        {
            StartMusic(_sectorMusic);
        }

        public static void PlayBossMusic()
        {
            StartMusic(_bossMusic);
        }

        public static void StopMusic()
        {
            _sectorMusic.Stop();
            _bossMusic.Stop();
            _currentMusic = null;
        }

        public static void ToggleMute()
        {
            Muted = !Muted;
            if (_currentMusic == null)
            {
                return;
            }
            if (Muted)
            {
                _currentMusic.Stop();
            }
            else
            {
                _currentMusic.Play();
            }
        }

        private static void StartMusic(SoundEffectInstance track)
        {
            if (_currentMusic != null && _currentMusic != track)
            {
                _currentMusic.Stop();
            }
            _currentMusic = track;
            if (!Muted && track.State != SoundState.Playing)
            {
                track.Play();
            }
        }
    }
}
